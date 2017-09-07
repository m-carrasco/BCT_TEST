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

namespace DotnetPark.NLucene.Index
{
	internal class SegmentsTermDocs : ITermDocs 
	{
		protected SegmentReader[] readers;
		protected int[] starts;
		protected Term term;

		protected int bs = 0;
		protected int pointer = 0;

		private SegmentTermDocs[] segTermDocs;
		protected SegmentTermDocs current;              // == segTermDocs[pointer]
  
		internal SegmentsTermDocs(SegmentReader[] r, int[] s) 
		{
			readers = r;
			starts = s;

			segTermDocs = new SegmentTermDocs[r.Length];
		}

		public int Doc() 
		{
			return bs + current.Doc();
		}
		public int Freq() 
		{
			return current.Freq();
		}

		public void Seek(Term term) 
		{
			this.term = term;
			this.bs = 0;
			this.pointer = 0;
			this.current = null;
		}

		public bool Next() 
		{
			if (current != null && current.Next()) 
			{
				return true;
			} 
			else if (pointer < readers.Length) 
			{
				bs = starts[pointer];
				current = TermDocs(pointer++);
				return Next();
			} 
			else
				return false;
		}

		///<summary> Optimized implementation. </summary>
		public int Read(int[] docs, int[] freqs)
		{
			while (true) 
			{
				while (current == null) 
				{
					if (pointer < readers.Length) 
					{		  // try next segment
						bs = starts[pointer];
						current = TermDocs(pointer++);
					} 
					else 
					{
						return 0;
					}
				}
				int end = current.Read(docs, freqs);
				if (end == 0) 
				{				  // none left in segment
					current = null;
				} 
				else 
				{					  // got some
					int b = bs;			  // adjust doc numbers
					for (int i = 0; i < end; i++)
						docs[i] += b;
					return end;
				}
			}
		}

		///<summary> As yet unoptimized implementation. </summary>
		public bool SkipTo(int target) 
		{
			do 
			{
				if (!Next())
					return false;
			} while (target > Doc());
			return true;
		}

		private SegmentTermDocs TermDocs(int i) 
		{
			if (term == null)
				return null;
			SegmentTermDocs result = segTermDocs[i];
			if (result == null)
				result = segTermDocs[i] = TermDocs(readers[i]);
			result.Seek(term);
			return result;
		}

		protected virtual SegmentTermDocs TermDocs(SegmentReader reader)
		{
			return (SegmentTermDocs)reader.TermDocs();
		}

		public void Close() 
		{
			for (int i = 0; i < segTermDocs.Length; i++) 
			{
				if (segTermDocs[i] != null)
					segTermDocs[i].Close();
			}
		}
	}
}
