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

namespace DotnetPark.NLucene.Index
{

	///<summary>
	/// This stores a monotonically increasing set of &lt;Term, TermInfo&gt; pairs in a
	/// Directory.  Pairs are accessed either by Term or by ordinal position the
	/// set.
	/// </summary>
	internal sealed class TermInfosReader 
	{
		Directory directory;
		string segment;
		FieldInfos fieldInfos;

		SegmentTermEnum enm;
		int size;
		

		internal TermInfosReader(Directory dir, string seg, FieldInfos fis)
		{
			directory = dir;
			segment = seg;
			fieldInfos = fis;

			enm = new SegmentTermEnum(directory.OpenFile(segment + ".tis"),
				fieldInfos, false);
			size = enm.Size;
			ReadIndex();
		}

		internal void Close()
		{
			if (enm != null)
				enm.Close();
		}

		///<summary> Returns the number of term/value pairs in the set. </summary>
		int Size() 
		{
			return size;
		}

		Term[] indexTerms = null;
		TermInfo[] indexInfos;
		long[] indexPointers;

		void ReadIndex() 
		{
			SegmentTermEnum indexEnum =
				new SegmentTermEnum(directory.OpenFile(segment + ".tii"),
				fieldInfos, true);
			try 
			{
				int indexSize = indexEnum.Size;

				indexTerms = new Term[indexSize];
				indexInfos = new TermInfo[indexSize];
				indexPointers = new long[indexSize];

				for (int i = 0; indexEnum.Next(); i++) 
				{
					indexTerms[i] = indexEnum.Term();
					indexInfos[i] = indexEnum.TermInfo();
					indexPointers[i] = indexEnum.IndexPointer;
				}
			} 
			finally 
			{
				indexEnum.Close();
			}
		}

		///<summary> Returns the offset of the greatest index entry which is less than term.</summary>
		int GetIndexOffset(Term term) 
		{
			int lo = 0;           // binary search indexTerms[]
			int hi = indexTerms.Length - 1;

			while (hi >= lo) 
			{
				int mid = (lo + hi) >> 1;
				int delta = term.CompareTo(indexTerms[mid]);
				if (delta < 0)
					hi = mid - 1;
				else if (delta > 0)
					lo = mid + 1;
				else
					return mid;
			}
			return hi;
		}

		void SeekEnum(int indexOffset) 
		{
			enm.Seek(indexPointers[indexOffset],
				(indexOffset * TermInfosWriter.INDEX_INTERVAL) - 1,
				indexTerms[indexOffset], indexInfos[indexOffset]);
		}

		///<summary> Returns the TermInfo for a Term in the set, or null. </summary>
		internal TermInfo Get(Term term) 
		{
			lock (this)
			{
				if (size == 0) return null;
    
				// optimize sequential access: first try scanning cached enm w/o seeking
				if (enm.Term() != null       // term is at or past current
					&& ((enm.Prev != null && term.CompareTo(enm.Prev) > 0)
					|| term.CompareTo(enm.Term()) >= 0)) 
				{ 
					int enumOffset = (enm.Position/TermInfosWriter.INDEX_INTERVAL)+1;
					if (indexTerms.Length == enumOffset   // but before end of block
						|| term.CompareTo(indexTerms[enumOffset]) < 0)
						return ScanEnum(term);        // no need to seek
				}
    
				// random-access: must seek
				SeekEnum(GetIndexOffset(term));
				return ScanEnum(term);
			}
		}
  
		///<summary> Scans within block for matching term. </summary>
		TermInfo ScanEnum(Term term) 
		{
			while (term.CompareTo(enm.Term()) > 0 && enm.Next()) {}
			if (enm.Term() != null && term.CompareTo(enm.Term()) == 0)
				return enm.TermInfo();
			else
				return null;
		}

		///<summary> Returns the nth term in the set. </summary>
		Term Get(int position)
		{
			lock (this) 
			{
				if (size == 0) return null;

				if (enm != null && enm.Term() != null && position >= enm.Position &&
					position < (enm.Position + TermInfosWriter.INDEX_INTERVAL))
					return ScanEnum(position);      // can avoid seek

				SeekEnum(position / TermInfosWriter.INDEX_INTERVAL); // must seek
				return ScanEnum(position);
			}
		}

		Term ScanEnum(int position) 
		{
			while(enm.Position < position)
				if (!enm.Next())
					return null;

			return enm.Term();
		}

		///<summary> Returns the position of a Term in the set or -1. </summary>
		int GetPosition(Term term) 
		{
			lock (this) 
			{
				if (size == 0) return -1;

				int indexOffset = GetIndexOffset(term);
				SeekEnum(indexOffset);

				while(term.CompareTo(enm.Term()) > 0 && enm.Next()) {}

				if (term.CompareTo(enm.Term()) == 0)
					return enm.Position;
				else
					return -1;
			}
		}

		///<summary> Returns an enumeration of all the Terms and TermInfos in the set. </summary>
		internal SegmentTermEnum Terms()
		{
			lock (this) 
			{
				if (enm.Position != -1)        // if not at start
					SeekEnum(0);               // reset to start
				return (SegmentTermEnum) enm.Clone();
			}
		}

		///<summary> Returns an enumeration of terms starting at or after the named term. </summary>
		internal SegmentTermEnum Terms(Term term)
		{
			lock (this) 
			{
				Get(term);            // seek enm to term
				return (SegmentTermEnum) enm.Clone();
			}
		}


	}
}
