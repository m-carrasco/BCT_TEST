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

using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Util;

namespace DotnetPark.NLucene.Index
{
	internal class SegmentMerger
	{
		private Directory directory;
		private string segment;

		private List<SegmentReader> readers = new List<SegmentReader>();
		private FieldInfos fieldInfos;

		public SegmentMerger(Directory dir, string name)
		{
			directory = dir;
			segment = name;
		}

		public void Add(SegmentReader reader)
		{
			readers.Add(reader);
		}

		internal SegmentReader SegmentReader(int i)
		{
			return readers[i];
		}

		public void Merge()
		{
			try
			{
				MergeFields();
				MergeTerms();
				MergeNorms();

			}
			finally
			{
				for(int i = 0; i < readers.Count; i++)
				{  // close readers
					SegmentReader reader = (SegmentReader)readers[i];
					reader.Close();
				}
			}
		}

		private void MergeFields()
		{
			fieldInfos = new FieldInfos();		  // merge field names
			for(int i = 0; i < readers.Count; i++)
			{
				SegmentReader reader = (SegmentReader)readers[i];
				fieldInfos.Add(reader.FieldInfos);
			}
			fieldInfos.Write(directory, segment + ".fnm");

			FieldsWriter fieldsWriter =			  // merge field values
				new FieldsWriter(directory, segment, fieldInfos);
			try
			{
				for(int i = 0; i < readers.Count; i++)
				{
					SegmentReader reader = (SegmentReader)readers[i];
					BitVector deletedDocs = reader.DeletedDocs;
					int maxDoc = reader.MaxDoc();
					for(int j = 0; j < maxDoc; j++)
						if(deletedDocs == null || !deletedDocs.Get(j)) // skip deleted docs
							fieldsWriter.AddDocument(reader.Document(j));
				}
			}
			finally
			{
				fieldsWriter.Close();
			}
		}

		private OutputStream freqOutput = null;
		private OutputStream proxOutput = null;
		private TermInfosWriter termInfosWriter = null;
		private SegmentMergeQueue queue = null;

		private void MergeTerms()
		{
			try
			{
				freqOutput = directory.CreateFile(segment + ".frq");
				proxOutput = directory.CreateFile(segment + ".prx");
				termInfosWriter =
					new TermInfosWriter(directory, segment, fieldInfos);

				MergeTermInfos();

			}
			finally
			{
				if(freqOutput != null) freqOutput.Close();
				if(proxOutput != null) proxOutput.Close();
				if(termInfosWriter != null) termInfosWriter.Close();
				if(queue != null) queue.Close();
			}
		}

		private void MergeTermInfos()
		{
			queue = new SegmentMergeQueue(readers.Count);
			int bs = 0;
			for(int i = 0; i < readers.Count; i++)
			{
				SegmentReader reader = readers[i];
				SegmentTermEnum termEnum = (SegmentTermEnum)reader.Terms();
				SegmentMergeInfo smi = new SegmentMergeInfo(bs, termEnum, reader);
				bs += reader.NumDocs();
				if(smi.Next())
					queue.Put(smi);				  // initialize queue
				else
					smi.Close();
			}

			SegmentMergeInfo[] match = new SegmentMergeInfo[readers.Count];

			while(queue.Size() > 0)
			{
				int matchSize = 0;			  // pop matching terms
				match[matchSize++] = (SegmentMergeInfo)queue.Pop();
				Term term = match[0].Term;
				SegmentMergeInfo top = (SegmentMergeInfo)queue.Top();

				while(top != null && term.CompareTo(top.Term) == 0)
				{
					match[matchSize++] = (SegmentMergeInfo)queue.Pop();
					top = (SegmentMergeInfo)queue.Top();
				}

				MergeTermInfo(match, matchSize);		  // add new TermInfo

				while(matchSize > 0)
				{
					SegmentMergeInfo smi = match[--matchSize];
					if(smi.Next())
						queue.Put(smi);			  // restore queue
					else
						smi.Close();				  // done with a segment
				}
			}
		}

		private TermInfo termInfo = new TermInfo(); // minimize consing

		private void MergeTermInfo(SegmentMergeInfo[] smis, int n)
		{
			long freqPointer = freqOutput.GetFilePointer();
			long proxPointer = proxOutput.GetFilePointer();

			int df = AppendPostings(smis, n);		  // append posting data

			if(df > 0)
			{
				// add an entry to the dictionary with pointers to prox and freq files
				termInfo.Set(df, freqPointer, proxPointer);
				termInfosWriter.Add(smis[0].Term, termInfo);
			}
		}

		private int AppendPostings(SegmentMergeInfo[] smis, int n)
		{
			int lastDoc = 0;
			int df = 0;					  // number of docs w/ term
			for(int i = 0; i < n; i++)
			{
				SegmentMergeInfo smi = smis[i];
				SegmentTermPositions postings = smi.Postings;
				int bs = smi.Base;
				int[] docMap = smi.DocMap;
				smi.TermEnum.TermInfo(termInfo);
				postings.Seek(termInfo);
				while(postings.Next())
				{
					int doc;
					if(docMap == null)
						doc = bs + postings.Doc();		  // no deletions
					else
						doc = bs + docMap[postings.Doc()];	  // re-map around deletions

					if(doc < lastDoc)
						throw new Exception("docs out of order");

					int docCode = (doc - lastDoc) << 1;	  // use low bit to flag freq=1
					lastDoc = doc;

					int freq = postings.Freq();
					if(freq == 1)
					{
						freqOutput.WriteVInt(docCode | 1);	  // write doc & freq=1
					}
					else
					{
						freqOutput.WriteVInt(docCode);	  // write doc
						freqOutput.WriteVInt(freq);		  // write frequency in doc
					}

					int lastPosition = 0;			  // write position deltas
					for(int j = 0; j < freq; j++)
					{
						int position = postings.NextPosition();
						proxOutput.WriteVInt(position - lastPosition);
						lastPosition = position;
					}

					df++;
				}
			}
			return df;
		}

		private void MergeNorms()
		{
			for(int i = 0; i < fieldInfos.Size(); i++)
			{
				FieldInfo fi = fieldInfos.FieldInfo(i);
				if(fi.IsIndexed)
				{
					OutputStream output = directory.CreateFile(segment + ".f" + i);
					try
					{
						for(int j = 0; j < readers.Count; j++)
						{
							SegmentReader reader = (SegmentReader)readers[j];
							BitVector deletedDocs = reader.DeletedDocs;
							InputStream input = reader.NormStream(fi.Name);
							int maxDoc = reader.MaxDoc();
							try
							{
								for(int k = 0; k < maxDoc; k++)
								{
									byte norm = input != null ? input.ReadByte() : (byte)0;
									if(deletedDocs == null || !deletedDocs.Get(k))
										output.WriteByte(norm);
								}
							}
							finally
							{
								if(input != null)
									input.Close();
							}
						}
					}
					finally
					{
						output.Close();
					}
				}
			}
		}
	}
}
