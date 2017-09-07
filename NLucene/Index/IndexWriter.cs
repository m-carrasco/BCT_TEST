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

using DotnetPark.NLucene.Analysis;
using DotnetPark.NLucene.Documents;
using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Util;

namespace DotnetPark.NLucene.Index
{
	/// <summary>
	/// An IndexWriter creates and maintains an index.
	/// The third argument to the constructor determines whether a new index
	/// is created, or whether an existing index is opened for the addition of
	/// new documents. In either case, documents are added with the addDocument
	/// method. When finished adding documents, close should be called.
	/// If an index will not have more documents added for a while and optimal
	/// search performance is desired, then the optimize method should be called
	/// before the index is closed.
	/// </summary>
	public class IndexWriter 
	{
		private Directory directory;			  // where this index resides
		private Analyzer analyzer;			  // how to analyze text

		private SegmentInfos segmentInfos = new SegmentInfos(); // the segments
		private Directory ramDirectory = new RamDirectory(); // for temp segs

		private Lock writeLock;

		///<summary>
		///Constructs an IndexWriter for the index in <c>path</c>. 
		///Text will be analyzed with <c>a</c>.  If <c>create</c> is true, then a
		///new, empty index will be created in <c>path</c>, replacing the index
		///already there, if any.
		///</summary>
		public IndexWriter(String path, Analyzer a, bool create)
			: this(FsDirectory.GetDirectory(path, create), a, create)
		{
			// use this
		}

		///<summary>
		///Constructs an IndexWriter for the index in <c>path</c>.  Text will
		///be analyzed with <c>a</c>.  If <c>create</c> is true, then a
		///new, empty index will be created in <c>path</c>, replacing the index
		///already there, if any.
		///</summary>
		public IndexWriter(System.IO.FileInfo path, Analyzer a, bool create)
			: this(FsDirectory.GetDirectory(path.FullName, create), a, create)
		{
			//
		}

		/// <summary>
		/// Internal writer lock.
		/// </summary>
		internal class IndexWriterLock : Lock.With
		{
			internal SegmentInfos segmentInfos;
			/// <summary> [To be supplied.] </summary>
			public bool create;
			/// <summary> [To be supplied.] </summary>
			public Directory directory;

			/// <summary>
			/// Builds a lock.
			/// </summary>
			public IndexWriterLock(Lock lck) 
				: base(lck)
			{
				// use base
			}

			/// <summary>
			/// Body method.
			/// </summary>
			protected override Object DoBody()
			{
				if (create)
					segmentInfos.Write(directory);
				else
					segmentInfos.Read(directory);
				return null;
			}
		}

		///<summary>
		///Constructs an IndexWriter for the index in <c>d</c>.  Text will be
		///analyzed with <c>a</c>.  If <c>create</c> is true, then a new,
		///empty index will be created in <c>d</c>, replacing the index already
		///there, if any.
		///</summary>
		public IndexWriter(Directory d, Analyzer a, bool create)
		{
			directory = d;
			analyzer = a;

			Lock writeLock = directory.MakeLock("write.lock");
			if (!writeLock.Obtain())                      // obtain write lock
				throw new System.IO.IOException("Index locked for write: " + writeLock);
			this.writeLock = writeLock;                   // save it

			lock(directory) 
			{			  // in- & inter-process sync
				IndexWriterLock lck = new IndexWriterLock(directory.MakeLock("commit.lock"));
				lck.create = create;
				lck.directory = directory;
				lck.segmentInfos = segmentInfos;
				lck.Run();
			}
		}

		///<summary>
		///Flushes all changes to an index, closes all associated files, and closes
		///the directory that the index is stored in.
		///</summary>
		public void Close() 
		{
			lock(this)
			{
				FlushRamSegments();
				ramDirectory.Close();
				writeLock.Release();                          // release write lock
				writeLock = null;
				directory.Close();
			}
		}

		///<summary>
		///Release the write lock, if needed.
		///</summary>
		~IndexWriter() 
		{
			if (writeLock != null) 
			{
				writeLock.Release();                        // release write lock
				writeLock = null;
			}
		}

		///<summary> Returns the number of documents currently in this index. </summary>
		public int DocCount() 
		{
			lock(this)
			{
				int count = 0;
				for (int i = 0; i < segmentInfos.Count; i++) 
				{
					SegmentInfo si = segmentInfos.Info(i);
					count += si.DocCount;
				}
				return count;
			}
		}

		///<summary>
		///<p>The maximum number of terms that will be indexed for a single field in a
		///document.  This limits the amount of memory required for indexing, so that
		///collections with very large files will not crash the indexing process by
		///running out of memory.</p>
		///
		///<p>By default, no more than 10,000 terms will be indexed for a field.</p>
		///</summary>
		public int maxFieldLength = 10000;

		///<summary> Adds a document to this index.</summary>
		public void AddDocument(Document doc) 
		{
			DocumentWriter dw =
				new DocumentWriter(ramDirectory, analyzer, maxFieldLength);
			String segmentName = NewSegmentName();
			dw.AddDocument(segmentName, doc);
			lock (this) 
			{
				segmentInfos.Add(new SegmentInfo(segmentName, 1, ramDirectory));
				MaybeMergeSegments();
			}
		}

		private String NewSegmentName() 
		{
			lock(this)
			{
				return "_" + String.Format("{0:x}", segmentInfos.counter++);
			}
		}

		///<summary>
		/// <p>Determines how often segment indexes are merged by AddDocument().  With
		/// smaller values, less RAM is used while indexing, and searches on
		/// unoptimized indexes are faster, but indexing speed is slower.  With larger
		/// values more RAM is used while indexing and searches on unoptimized indexes
		/// are slower, but indexing is faster.  Thus larger values (&gt; 10) are best
		/// for batched index creation, and smaller values (&lt; 10) for indexes that are
		/// interactively maintained.</p>
		///
		/// <p>This must never be less than 2.  The default value is 10.</p>
		/// </summary>
		public int mergeFactor = 10;

		///<summary>
		/// <p>Determines the largest number of documents ever merged by addDocument().
		/// Small values (e.g., less than 10,000) are best for interactive indexing,
		/// as this limits the length of pauses while indexing to a few seconds.
		/// Larger values are best for batched indexing and speedier searches.</p>
		///
		/// <p>The default value is Int32.MaxValue.</p></summary>
		public int maxMergeDocs = Int32.MaxValue;

		///<summary> If non-null, information about merges will be printed to this. </summary>
		public System.IO.TextWriter infoStream = null;

		///<summary>
		/// Merges all segments together into a single segment, optimizing an index
		/// for search.
		///</summary>
		public void Optimize() 
		{
			lock(this)
			{
				FlushRamSegments();
				while (segmentInfos.Count > 1 ||
					(segmentInfos.Count == 1 &&
					(SegmentReader.HasDeletions(segmentInfos.Info(0)) ||
					segmentInfos.Info(0).Dir != directory))) 
				{
					int minSegment = segmentInfos.Count - mergeFactor;
					MergeSegments(minSegment < 0 ? 0 : minSegment);
				}
			}
		}

		///<summary>
		/// <p>Merges all segments from an array of indexes into this index.</p>
		///
		/// <p>This may be used to parallelize batch indexing.  A large document
		/// collection can be broken into sub-collections.  Each sub-collection can be
		/// indexed in parallel, on a different thread, process or machine.  The
		/// complete index can then be created by merging sub-collection indexes
		/// with this method.</p>
		///
		/// <p>After this completes, the index is optimized.</p>
		/// </summary>
		public void AddIndexes(Directory[] dirs)
		{
			lock(this)
			{
				Optimize();					  // start with zero or 1 seg
				for (int i = 0; i < dirs.Length; i++) 
				{
					SegmentInfos sis = new SegmentInfos();	  // read infos from dir
					sis.Read(dirs[i]);
					for (int j = 0; j < sis.Count; j++) 
					{
						segmentInfos.Add(sis.Info(j));	  // add each info
					}
				}
				Optimize();					  // final cleanup
			}
		}

		///<summary> Merges all RAM-resident segments. </summary>
		private void FlushRamSegments() 
		{
			int minSegment = segmentInfos.Count-1;
			int docCount = 0;
			while (minSegment >= 0 &&
				(segmentInfos.Info(minSegment)).Dir == ramDirectory) 
			{
				docCount += segmentInfos.Info(minSegment).DocCount;
				minSegment--;
			}
			if (minSegment < 0 ||			  // add one FS segment?
				(docCount + segmentInfos.Info(minSegment).DocCount) > mergeFactor ||
				!(segmentInfos.Info(segmentInfos.Count -1).Dir == ramDirectory))
				minSegment++;
			if (minSegment >= segmentInfos.Count)
				return;					  // none to merge
			MergeSegments(minSegment);
		}

		///<summary> Incremental segment merger.  </summary>
		private void MaybeMergeSegments() 
		{
			long targetMergeDocs = mergeFactor;
			while (targetMergeDocs <= maxMergeDocs) 
			{
				// find segments smaller than current target size
				int minSegment = segmentInfos.Count;
				int mergeDocs = 0;
				while (--minSegment >= 0) 
				{
					SegmentInfo si = segmentInfos.Info(minSegment);
					if (si.DocCount >= targetMergeDocs)
						break;
					mergeDocs += si.DocCount;
				}

				if (mergeDocs >= targetMergeDocs)		  // found a merge to do
					MergeSegments(minSegment+1);
				else
					break;
      
				targetMergeDocs *= mergeFactor;		  // increase target size
			}
		}

		/// <summary>
		/// Internal lock.
		/// </summary>
		internal class IndexWriterLock2 : Lock.With
		{
			public IndexWriter owner;
			internal SegmentInfos segmentInfos;
			public List<SegmentReader> segmentsToDelete;
			public Directory directory;

			/// <summary>
			/// Builds lock.
			/// </summary>
			public IndexWriterLock2(Lock lck) : base(lck)
			{
				//
			}

			/// <summary>
			/// Declares a body of the lock.
			/// </summary>
			protected override Object DoBody()
			{
				segmentInfos.Write(directory);	  // commit before deleting
				owner.DeleteSegments(segmentsToDelete);	  // delete now-unused segments
				return null;
			}
		}

		///<summary>
		///Pops segments off of segmentInfos stack down to minSegment, merges them,
		///and pushes the merged index onto the top of the segmentInfos stack.
		///</summary>
		private void MergeSegments(int minSegment)
		{
			String mergedName = NewSegmentName();
			int mergedDocCount = 0;
			if (infoStream != null) infoStream.Write("merging segments");
			SegmentMerger merger = new SegmentMerger(directory, mergedName);
			List<SegmentReader> segmentsToDelete = new List<SegmentReader>();
			for (int i = minSegment; i < segmentInfos.Count; i++) 
			{
				SegmentInfo si = segmentInfos.Info(i);
				if (infoStream != null)
					infoStream.Write(" " + si.Name + " (" + si.DocCount + " docs)");
				SegmentReader reader = new SegmentReader(si);
				merger.Add(reader);
				if ((reader.Directory == this.directory) || // if we own the directory
					(reader.Directory == this.ramDirectory))
					segmentsToDelete.Add(reader);	  // queue segment for deletion
				mergedDocCount += si.DocCount;
			}
			if (infoStream != null) 
			{
				infoStream.WriteLine();
				infoStream.WriteLine(" into "+mergedName+" ("+mergedDocCount+" docs)");
			}
			merger.Merge();

			segmentInfos.RemoveRange(minSegment,segmentInfos.Count - minSegment);		  // pop old infos & add new
			segmentInfos.Add(new SegmentInfo(mergedName, mergedDocCount,
				directory));
    
			lock (directory) 
			{			  // in- & inter-process sync
				IndexWriterLock2 lck = new IndexWriterLock2(directory.MakeLock("commit.lock"));
				lck.owner = this;
				lck.directory = directory;
				lck.segmentInfos = segmentInfos;
				lck.segmentsToDelete = segmentsToDelete;
				lck.Run();
			}
		}

		/// <summary>
		/// Some operating systems (e.g. Windows) don't permit a file to be deleted
		/// while it is opened for read (e.g. by another process or thread).  So we
		/// assume that when a delete fails it is because the file is open in another
		/// process, and queue the file for subsequent deletion.
		/// </summary>
		/// <param name="segments">A list of segments.</param>
		private void DeleteSegments(List<SegmentReader> segments) 
		{
			List<string> deletable = new List<string>();

			DeleteFiles(ReadDeleteableFiles(), deletable); // try to delete deleteable
    
			for (int i = 0; i < segments.Count; i++) 
			{
				SegmentReader reader = (SegmentReader)segments[i];
				if (reader.Directory == this.directory)
					DeleteFiles(reader.Files(), deletable);	  // try to delete our files
				else
					DeleteFiles(reader.Files(), reader.Directory); // delete, eg, RAM files
			}

			WriteDeleteableFiles(deletable);		  // note files we can't delete
		}

		private void DeleteFiles(List<string> files, Directory directory)
		{
			for (int i = 0; i < files.Count; i++)
				directory.DeleteFile(files[i]);
		}

		private void DeleteFiles(List<string> files, List<string> deletable)
		{
			for (int i = 0; i < files.Count; i++) 
			{
				String file = files[i];
				try 
				{
					directory.DeleteFile(file);		  // try to delete each file
				} 
				catch (System.IO.IOException e) 
				{			  // if delete fails
					if (directory.FileExists(file)) 
					{
						if (infoStream != null)
							infoStream.WriteLine(e.Message + "; Will re-try later.");
						deletable.Add(file);		  // add to deletable
					}
				}
			}
		}

		private List<string> ReadDeleteableFiles() 
		{
			List<string> result = new List<string>();
			if (!directory.FileExists("deletable"))
				return result;

			InputStream input = directory.OpenFile("deletable");
			try 
			{
				for (int i = input.ReadInt(); i > 0; i--)	  // read file names
					result.Add(input.ReadString());
			} 
			finally 
			{
				input.Close();
			}
			return result;
		}

		private void WriteDeleteableFiles(List<string> files) 
		{
			OutputStream output = directory.CreateFile("deleteable.new");
			try 
			{
				output.WriteInt(files.Count);
				for (int i = 0; i < files.Count; i++)
					output.WriteString((String)files[i]);
			} 
			finally 
			{
				output.Close();
			}
			directory.RenameFile("deleteable.new", "deletable");
		}
	}
}
