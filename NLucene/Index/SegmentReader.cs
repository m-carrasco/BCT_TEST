/* ====================================================================
 * The Apache Software License, Version 1.1
 *
 * Copyright (c) 2001 The Apache Software Foundation.  All rights
 * reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 *
 * 3. The end-user documentation included with the redistribution,
 *    if any, must include the following acknowledgment:
 *       "This product includes software developed by the
 *        Apache Software Foundation (http://www.apache.org/)."
 *    Alternately, this acknowledgment may appear in the software itself,
 *    if and wherever such third-party acknowledgments normally appear.
 *
 * 4. The names "Apache" and "Apache Software Foundation" and
 *    "Apache Lucene" must not be used to endorse or promote products
 *    derived from this software without prior written permission. For
 *    written permission, please contact apache@apache.org.
 *
 * 5. Products derived from this software may not be called "Apache",
 *    "Apache Lucene", nor may "Apache" appear in their name, without
 *    prior written permission of the Apache Software Foundation.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL THE APACHE SOFTWARE FOUNDATION OR
 * ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 * ====================================================================
 *
 * This software consists of voluntary contributions made by many
 * individuals on behalf of the Apache Software Foundation.  For more
 * information on the Apache Software Foundation, please see
 * <http://www.apache.org/>.
 */

using System;
using System.Collections.Generic;

using DotnetPark.NLucene.Documents;
using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Util;

namespace DotnetPark.NLucene.Index
{
	internal class SegmentReader : IndexReader
	{
		private bool closeDirectory = false;
		private String segment;

		FieldInfos fieldInfos;
		private FieldsReader fieldsReader;

		TermInfosReader tis;
  
		BitVector deletedDocs = null;
		private bool deletedDocsDirty = false;

		InputStream freqStream;
		InputStream proxStream;


		private class Norm 
		{
			public Norm(InputStream input)
			{
				this.Input = input;
			}
			public InputStream Input;
			public byte[] Bytes;
		}

		private Dictionary<string, Norm> norms = new Dictionary<string, Norm>();

		public SegmentReader(SegmentInfo si, bool closeDir) : this(si)
		{
			closeDirectory = closeDir;
		}

		public SegmentReader(SegmentInfo si) : base(si.Dir)
		{
			segment = si.Name;

			fieldInfos = new FieldInfos(this.Directory, segment + ".fnm");
			fieldsReader = new FieldsReader(this.Directory, segment, fieldInfos);

			tis = new TermInfosReader(this.Directory, segment, fieldInfos);

			if (HasDeletions(si))
				deletedDocs = new BitVector(this.Directory, segment + ".del");

			// make sure that all index files have been read or are kept open
			// so that if an index update removes them we'll still have them
			freqStream = this.Directory.OpenFile(segment + ".frq");
			proxStream = this.Directory.OpenFile(segment + ".prx");
			OpenNorms();
		}

		internal class SegmentReaderLock : Lock.With
		{
			public BitVector deletedDocs;
			public Directory directory;
			public string segment;

			internal SegmentReaderLock(Lock lck): base(lck)
			{
				//
			}

			protected override Object DoBody()
			{
				deletedDocs.Write(directory, segment + ".tmp");
				directory.RenameFile(segment + ".tmp", segment + ".del");
				return null;
			}
		}
  
		protected override void DoClose() 
		{
			lock(this)
			{
				if (deletedDocsDirty) 
				{
					lock (this.Directory) 
					{		  // in- & inter-process sync
						SegmentReaderLock lck = new SegmentReaderLock(this.Directory.MakeLock("commit.lock"));
						lck.deletedDocs = DeletedDocs;
						lck.directory = this.Directory;
						lck.segment = segment;
						lck.Run();
						/*new Lock.With(directory.makeLock("commit.lock")) {
							public Object doBody() {
							  deletedDocs.write(directory, segment + ".tmp");
							  directory.renameFile(segment + ".tmp", segment + ".del");
							  return null;
							}
						  }.run();
						  }*/
						deletedDocsDirty = false;
					}
				}
				fieldsReader.Close();
				tis.Close();

				if (freqStream != null)
					freqStream.Close();
				if (proxStream != null)
					proxStream.Close();

				CloseNorms();

				if (closeDirectory)
					this.Directory.Close();
				
			}
		}

		internal static bool HasDeletions(SegmentInfo si) 
		{
			return si.Dir.FileExists(si.Name + ".del");
		}

		public override void DoDelete(int docNum) 
		{
			lock(this)
			{
				if (deletedDocs == null)
					deletedDocs = new BitVector(MaxDoc());
				deletedDocsDirty = true;
				deletedDocs.Set(docNum);
			}
		}

		internal List<string> Files() 
		{
			List<string> files = new List<string>(16);
			files.Add(segment + ".fnm");
			files.Add(segment + ".fdx");
			files.Add(segment + ".fdt");
			files.Add(segment + ".tii");
			files.Add(segment + ".tis");
			files.Add(segment + ".frq");
			files.Add(segment + ".prx");

			if (this.Directory.FileExists(segment + ".del"))
				files.Add(segment + ".del");

			for (int i = 0; i < fieldInfos.Size(); i++) 
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.IsIndexed)
					files.Add(segment + ".f" + i);
			}
			return files;
		}

		public override ITermEnum Terms() 
		{
			return tis.Terms();
		}

		public override ITermEnum Terms(Term t) 
		{
			return tis.Terms(t);
		}

		public override Document Document(int n) 
		{
			lock(this)
			{
				if (IsDeleted(n))
					throw new ArgumentException
						("attempt to access a deleted document");
				return fieldsReader.Doc(n);
			}
		}

		public override bool IsDeleted(int n) 
		{
			lock(this)
			{
				return (deletedDocs != null && deletedDocs.Get(n));
			}
		}

		public override ITermDocs TermDocs() 
		{
			return new SegmentTermDocs(this);
		}

		public override ITermPositions TermPositions() 
		{
			return new SegmentTermPositions(this);
		}

		public override int DocFreq(Term t) 
		{
			TermInfo ti = tis.Get(t);
			if (ti != null)
				return ti.DocFreq;
			else
				return 0;
		}

		public override int NumDocs() 
		{
			int n = MaxDoc();
			if (deletedDocs != null)
				n -= deletedDocs.Count;
			return n;
		}

		public override int MaxDoc() 
		{
			return fieldsReader.Size();
		}

		public override byte[] Norms(String field) 
		{
			Norm norm = (Norm) norms[field];
			if (norm == null)
				return null;
			if (norm.Bytes == null) 
			{
				byte[] bytes = new byte[MaxDoc()];
				Norms(field, bytes, 0);
				norm.Bytes = bytes;
			}
			return norm.Bytes;
		}

		internal void Norms(String field, byte[] bytes, int offset) 
		{
			InputStream normStream = NormStream(field);
			if (normStream == null)
				return;					  // use zeros in array
			try 
			{
				normStream.ReadBytes(bytes, offset, MaxDoc());
			} 
			finally 
			{
				normStream.Close();
			}
		}

		internal InputStream NormStream(String field) 
		{
			Norm norm = norms[field];
			if (norm == null)
				return null;
			InputStream result = (InputStream)norm.Input.Clone();
			result.Seek(0);
			return result;
		}

		private void OpenNorms() 
		{
			for (int i = 0; i < fieldInfos.Size(); i++) 
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if (fi.IsIndexed) 
					norms[fi.Name] = new Norm(this.Directory.OpenFile(segment + ".f" + fi.Number));
			}
		}


		private void CloseNorms() 
		{
			lock (norms) 
			{
				foreach(Norm norm in norms.Values)
				{
					norm.Input.Close();
				}
			}
		}

		internal FieldInfos FieldInfos
		{
			get
			{
				return fieldInfos;
			}
		}

		internal InputStream FreqStream
		{
			get
			{
				return freqStream;
			}
			set
			{
				freqStream = value;
			}
		}

		internal InputStream ProxStream
		{
			get
			{
				return proxStream;
			}
			set
			{
				proxStream = value;
			}
		}

		internal BitVector DeletedDocs
		{
			get
			{
				return deletedDocs;
			}
			set
			{
				deletedDocs = value;
			}
		}

		internal TermInfosReader Tis
		{
			get
			{
				return tis;
			}
			set
			{
				tis = value;
			}
		}
	}
}
