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

namespace DotnetPark.NLucene.Index
{
	internal class SegmentsReader : IndexReader 
	{
		internal SegmentReader[] readers;
		protected int[] starts;			  // 1st docno for each segment
		private Dictionary<string, byte[]> normsCache = new Dictionary<string, byte[]>();
		private int maxDoc = 0;
		private int numDocs = -1;

		internal SegmentsReader(Directory directory, SegmentReader[] r) : base(directory) 
		{
			//super(directory);
			readers = r;
			starts = new int[readers.Length + 1];	  // build starts array
			for (int i = 0; i < readers.Length; i++) 
			{
				starts[i] = maxDoc;
				maxDoc += readers[i].MaxDoc();		  // compute maxDocs
			}
			starts[readers.Length] = maxDoc;
		}

		public override int NumDocs() 
		{
			lock(this)
			{
				if (numDocs == -1) 
				{			  // check cache
					int n = 0;				  // cache miss--recompute
					for (int i = 0; i < readers.Length; i++)
						n += readers[i].NumDocs();		  // sum from readers
					numDocs = n;
				}
				return numDocs;
			}
		}

		public override int MaxDoc() 
		{
			return maxDoc;
		}

		public override Document Document(int n) 
		{
			int i = ReaderIndex(n);			  // find segment num
			return readers[i].Document(n - starts[i]);	  // dispatch to segment reader
		}

		public override bool IsDeleted(int n) 
		{
			int i = ReaderIndex(n);			  // find segment num
			return readers[i].IsDeleted(n - starts[i]);	  // dispatch to segment reader
		}

		public override void DoDelete(int n) 
		{
			lock(this)
			{
				numDocs = -1;				  // invalidate cache
				int i = ReaderIndex(n);			  // find segment num
				readers[i].DoDelete(n - starts[i]);		  // dispatch to segment reader
			}
		}

		private int ReaderIndex(int n) 
		{	  // find reader for doc n:
			int lo = 0;					  // search starts array
			int hi = readers.Length - 1;		  // for first element less
			// than n, return its index
			while (hi >= lo) 
			{
				int mid = (lo + hi) >> 1;
				int midValue = starts[mid];
				if (n < midValue)
					hi = mid - 1;
				else if (n > midValue)
					lo = mid + 1;
				else
					return mid;
			}
			return hi;
		}

		public override byte[] Norms(String field) 
		{
			lock(this)
			{
				byte[] bytes = (byte[])normsCache[field];
				if (bytes != null)
					return bytes;				  // cache hit

				bytes = new byte[MaxDoc()];
				for (int i = 0; i < readers.Length; i++)
					readers[i].Norms(field, bytes, starts[i]);
				normsCache[field] = bytes;		  // update cache
				return bytes;
			}
		}

		public override ITermEnum Terms() 
		{
			return new SegmentsTermEnum(readers, starts, null);
		}

		public override ITermEnum Terms(Term term)  
		{
			return new SegmentsTermEnum(readers, starts, term);
		}

		public override int DocFreq(Term t) 
		{
			int total = 0;				  // sum freqs in segments
			for (int i = 0; i < readers.Length; i++)
				total += readers[i].DocFreq(t);
			return total;
		}

		public override ITermDocs TermDocs() 
		{
			return new SegmentsTermDocs(readers, starts);
		}

		public override ITermPositions TermPositions() 
		{
			return new SegmentsTermPositions(readers, starts);
		}

		protected override void DoClose() 
		{
			lock(this)
			{
				for (int i = 0; i < readers.Length; i++)
					readers[i].Close();
			}
		}
	}
}
