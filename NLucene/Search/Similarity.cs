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

using DotnetPark.NLucene.Index;

namespace DotnetPark.NLucene.Search
{
	///<summary>
	///Internal class used for scoring.
	/// <p>Public only so that the indexing code can compute and store the
	/// normalization byte for each document.</p>
	/// </summary>
	public class Similarity 
	{
		private Similarity() {}			  // no public constructor

		///<summary> Computes the normalization byte for a document given the total number of
		/// terms contained in the document.  These values are stored in an index and
		/// used by the search code. </summary>
		public static byte Norm(int numTerms) 
		{
			// Scales 1/sqrt(numTerms) into a byte, i.e. 256/sqrt(numTerms).
			// Math.ceil is used to ensure that even very long documents don't get a
			// zero norm byte, as that is reserved for zero-lengthed documents and
			// deleted documents.
			return (byte) Math.Ceiling(255.0 / Math.Sqrt(numTerms));
		}


		private static float[] MakeNormTable() 
		{
			float[] result = new float[256];
			for (int i = 0; i < 256; i++)
				result[i] = i / 255.0F;
			return result;
		}

		internal static float[] NORM_TABLE = MakeNormTable();
    
		internal static float Norm(byte normByte) 
		{
			// Un-scales from the byte encoding of a norm into a float, i.e.,
			// approximately 1/sqrt(numTerms).
			return NORM_TABLE[normByte & 0xFF];
		}

		internal static float Tf(int freq) 
		{
			return (float)Math.Sqrt(freq);
		}

		internal static float Tf(float freq) 
		{
			return (float)Math.Sqrt(freq);
		}
    
		internal static float Idf(Term term, Searcher searcher)
		{
			// Use maxDoc() instead of numDocs() because its proportional to docFreq(),
			// i.e., when one is inaccurate, so is the other, and in the same way.
			return Idf(searcher.DocFreq(term), searcher.MaxDoc());
		}

		internal static float Idf(int docFreq, int numDocs) 
		{
			return (float)(Math.Log(numDocs/(double)(docFreq+1)) + 1.0);
		}
    
		internal static float Coord(int overlap, int maxOverlap) 
		{
			return overlap / (float)maxOverlap;
		}
	}
}
