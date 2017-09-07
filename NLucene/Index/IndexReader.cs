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
using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Documents;  

namespace DotnetPark.NLucene.Index
{

	///<summary>
	///<p>IndexReader is an abstract class, providing an interface for accessing an
	///index.  Search of an index is done entirely through this abstract interface,
	///so that any subclass which implements it is searchable.</p>
	///
	///<p> Concrete subclasses of IndexReader are usually constructed with a call to
	///the static method Open.</p>
	///
	///<p> For efficiency, in this API documents are often referred to via
	///<i>document numbers</i>, non-negative integers which each name a unique
	///document in the index.  These document numbers are ephemeral--they may change
	///as documents are added to and deleted from an index.  Clients should thus not
	///rely on a given document having the same number between sessions.</p>
	///</summary>
	public abstract class IndexReader 
	{
		/// <summary>
		/// Initializes a new instance of the IndexReader class.
		/// </summary>
		/// <param name="directory">A directory instance where an index must be created.</param>
		protected IndexReader(Directory directory) 
		{
			this.directory = directory;
		}

		Directory directory;

		internal Directory Directory 
		{
			get 
			{
				return directory;
			}
		}

		private Lock writeLock;

		///<summary>
		///Returns an IndexReader reading the index in an FSDirectory in the named
		///path.
		///</summary>
		public static IndexReader Open(string path)
		{
			return Open(FsDirectory.GetDirectory(path, false));
		}

		///<summary>
		///Returns an IndexReader reading the index in an FSDirectory in the named
		///path.
		///</summary>
		public static IndexReader Open(System.IO.DirectoryInfo path) 
		{
			return Open(FsDirectory.GetDirectory(path, false));
		}

		///<summary> Returns an IndexReader reading the index in the given Directory. </summary>
		public static IndexReader Open(Directory directory)
		{
			lock (directory) 
			{        // in- & inter-process sync
				IndexReaderLock lck = new IndexReaderLock(directory.MakeLock("commit.lock"), directory);
				return 	(IndexReader) lck.Run();
			}
		}


		private class IndexReaderLock : Lock.With
		{
			Directory directory;

			internal IndexReaderLock (Lock lck, Directory directory) : base(lck)
			{
				this.directory = directory;
			}

			protected override Object DoBody()
			{
				SegmentInfos infos = new SegmentInfos();
				infos.Read(directory);
				if (infos.Count == 1)      // index is optimized
					return new SegmentReader(infos.Info(0), true);

				SegmentReader[] readers = new SegmentReader[infos.Count];
				for (int i = 0; i < infos.Count; i++)
					readers[i] = new SegmentReader(infos.Info(i), i==infos.Count-1);
				return new SegmentsReader(directory, readers);
			}
		}

		///<summary> Returns the time the index in the named directory was last modified. </summary>
		public static long LastModified(string directory) 
		{
			return LastModified(new System.IO.FileInfo(directory));
		}

		///<summary> Returns the time the index in the named directory was last modified. </summary>
		public static long LastModified(System.IO.FileInfo directory) 
		{
			return FsDirectory.GetFileModified(directory.FullName + "/" + "segments");
		}

		///<summary> Returns the time the index in this directory was last modified. </summary>
		public static long LastModified(Directory directory) 
		{
			return directory.FileModified("segments");
		}

		/// <summary>
		/// Returns <c>true</c> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// <c>false</c> is returned.
		/// </summary>
		/// <param name="directory">The directory to check for an index.</param>
		/// <returns><c>True</c> if an index exists; <c>false</c> otherwise.</returns>
		public static bool IndexExists(string directory) 
		{
			return (new System.IO.FileInfo(directory + "/" + "segments")).Exists;
		}

		/// <summary>
		/// Returns <c>true</c> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// </summary>
		/// <param name="directory">The directory to check for an index.</param>
		/// <returns><c>True</c> if an index exists; <c>false</c> otherwise.</returns>
		public static bool IndexExists(System.IO.FileInfo directory) 
		{
			return IndexExists(directory.FullName);
		}

		/// <summary>
		/// Returns <c>true</c> if an index exists at the specified directory.
		/// If the directory does not exist or if there is no index in it.
		/// </summary>
		/// <param name="directory">The directory to check for an index.</param>
		/// <returns><c>True</c> if an index exists; <c>false</c> otherwise.</returns>
		public static bool IndexExists(Directory directory) 
		{
			return directory.FileExists("segments");
		}

		///<summary> Returns the number of documents in this index. </summary>
		abstract public int NumDocs();

		///<summary>
		///Returns one greater than the largest possible document number.
		///This may be used to, e.g., determine how big to allocate an array which
		///will have an element for every document number in an index.
		///</summary>
		abstract public int MaxDoc();

		///<summary>
		///Returns the stored fields of the <c>n</c><sup>th</sup>
		///<c>Document</c> in this index.
		///</summary>
		abstract public Document Document(int n);

		///<summary> Returns true if document <i>n</i> has been deleted </summary>
		abstract public bool IsDeleted(int n);

		///<summary>
		///Returns the byte-encoded normalization factor for the named field of
		///every document.  This is used by the search code to score documents.
		///</summary>
		abstract public byte[] Norms(string field);

		///<summary>
		///Returns an enumeration of all the terms in the index.
		///The enumeration is ordered by Term.CompareTo(). Each term
		///is greater than all that precede it in the enumeration.
		///</summary>
		abstract public ITermEnum Terms();

		///<summary>
		///Returns an enumeration of all terms after a given term.
		///The enumeration is ordered by Term.CompareTo(). Each term
		///is greater than all that precede it in the enumeration.
		///</summary>
		abstract public ITermEnum Terms(Term t);

		///<summary>
		///Returns the number of documents containing the term <c>t</c>.
		///</summary>
		abstract public int DocFreq(Term t);

		///<summary><p>Returns an enumeration of all the documents which contain
		///<c>term</c>. For each document, the document number, the frequency of
		///the term in that document is also provided, for use in search scoring.
		///Thus, this method implements the mapping:</p>
		///<p>
		///<ul>
		///Term   =&gt;   &lt;docNum, freq&gt;<sup>*</sup>
		///</ul>
		///</p>
		///<p>The enumeration is ordered by document number.  Each document number
		///is greater than all that precede it in the enumeration.</p>
		///</summary>
		public ITermDocs TermDocs(Term term) 
		{
			ITermDocs termDocs = TermDocs();
			termDocs.Seek(term);
			return termDocs;
		}

		///<summary> Returns an unpositioned TermDocs enumerator. </summary>
		abstract public ITermDocs TermDocs();

		///<summary>
		///<p>Returns an enumeration of all the documents which contain
		///<c>term</c>.  For each document, in addition to the document number
		///and frequency of the term in that document, a list of all of the ordinal
		///positions of the term in the document is available.  Thus, this method
		///implements the mapping:</p>
		///<p><ul>
		///Term    =&gt;    &lt;docNum, freq,
		///&lt;pos<sub>1</sub>, pos<sub>2</sub>, ...
		///pos<sub>freq-1</sub>&gt;
		///&gt;<sup>*</sup>
		///</ul>
		///</p>
		///<p> This positional information faciliates phrase and proximity searching.</p>
		///<p>The enumeration is ordered by document number.  Each document number is
		///greater than all that precede it in the enumeration.</p>
		///</summary>
		public ITermPositions TermPositions(Term term)
		{
			ITermPositions termPositions = TermPositions();
			termPositions.Seek(term);
			return termPositions;
		}

		///<summary> Returns an unpositioned TermPositions enumerator. </summary>
		abstract public ITermPositions TermPositions();

		///<summary>
		///Deletes the document numbered <c>DocNum</c>. Once a document is
		///deleted it will not appear in TermDocs or TermPostitions enumerations.
		///Attempts to read its field with the Document
		///method will result in an error.  The presence of this document may still be
		///reflected in the DocFreq statistic, though
		///this will be corrected eventually as the index is further modified.
		///</summary>
		public void Delete(int docNum)
		{
			lock (this) 
			{
				if (writeLock == null) 
				{
					Lock writeLockAttempt = directory.MakeLock("write.lock");
					if (!writeLockAttempt.Obtain())        // obtain write lock
						throw new System.IO.IOException("Index locked for write: " + writeLockAttempt);
					this.writeLock = writeLockAttempt;
				}
				DoDelete(docNum);
			}
		}
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public abstract void DoDelete(int docNum);

		///<summary>
		///Deletes all documents containing <c>term</c>.
		///This is useful if one uses a document field to hold a unique ID string for
		///the document.  Then to delete such a document, one merely constructs a
		///term with the appropriate field and the unique ID string as its text and
		///passes it to this method.  Returns the number of documents deleted.
		///</summary>
		public int Delete(Term term)
		{
			ITermDocs docs = TermDocs(term);
			if ( docs == null ) return 0;
			int n = 0;
			try 
			{
				while (docs.Next()) 
				{
					Delete(docs.Doc());
					n++;
				}
			} 
			finally 
			{
				docs.Close();
			}
			return n;
		}

		///<summary>
		/// Closes files associated with this index.
		/// Also saves any new deletions to disk.
		/// No other methods should be called after this has been called.
		///</summary>
		public void Close()
		{
			lock (this) 
			{    
				DoClose();
				if (writeLock != null) 
				{
					writeLock.Release();  // release write lock
					writeLock = null;
				}
			}
		}

		///<summary> Implements close. </summary>
		protected abstract void DoClose();

		/// <summary>
		/// Destructor.
		/// </summary>
		~ IndexReader () 
		{
			if (writeLock != null) 
			{
				writeLock.Release();                        // release write lock
				writeLock = null;
			}
		}

		/// <summary>
		/// Returns <c>true</c> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">The directory to check for a lock.</param>
		/// <returns>True if directory is locked, otherwise false.</returns>
		public static bool IsLocked(Directory directory)
		{
			return directory.FileExists("write.lock");
		}

		/// <summary>
		/// Returns <c>true</c> iff the index in the named directory is
		/// currently locked.
		/// </summary>
		/// <param name="directory">The directory to check for a lock.</param>
		/// <returns>True if directory is locked, otherwise false.</returns>
		public static bool IsLocked(string directory) 
		{
			return (new System.IO.DirectoryInfo(directory + "/" + "write.lock")).Exists;
		}

		///<summary>
		///<p>
		/// Forcibly unlocks the index in the named directory.</p>
		/// <p>
		/// Caution: this should only be used by failure recovery code,
		/// when it is known that no other process nor thread is in fact
		/// currently accessing this index.</p>
		///</summary>
		public static void Unlock(Directory directory) 
		{
			directory.DeleteFile("write.lock");
			directory.DeleteFile("commit.lock");
		}
	}
}
