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
	///This stores a monotonically increasing set of &lt;Term, TermInfo&gt; pairs in a
	///Directory.  A TermInfos can be written once, in order.
	///</summary>
	internal sealed class TermInfosWriter 
	{
		FieldInfos fieldInfos;
		OutputStream output;
		Term lastTerm = new Term("", "");
		TermInfo lastTi = new TermInfo();
		int size = 0;
  
		internal static int INDEX_INTERVAL = 128;
		long lastIndexPointer = 0;
		bool isIndex = false;

		TermInfosWriter other = null;

		internal TermInfosWriter(Directory directory, string segment, FieldInfos fis) : this (directory, segment, fis, false)
		{
			other = new TermInfosWriter(directory, segment, fis, true);
			other.other = this;
		}

		TermInfosWriter(Directory directory, string segment, FieldInfos fis,
			bool isIndex) 
		{
			initialize(directory, segment, fis, isIndex);
		}

		void initialize(Directory directory, string segment, FieldInfos fis,
			bool isi) 
		{
			fieldInfos = fis;
			isIndex = isi;
			output = directory.CreateFile(segment + (isIndex ? ".tii" : ".tis"));
			output.WriteInt(0);         // leave space for size
		}

		///<summary> Adds a new &lt;Term, TermInfo&gt; pair to the set.
		///Term must be lexicographically greater than all previous Terms added.
		///TermInfo pointers must be positive and greater than all previous.</summary>
		internal void Add(Term term, TermInfo ti)
	
		{
			if (!isIndex && term.CompareTo(lastTerm) <= 0)
				throw new System.IO.IOException("term out of order");
			if (ti.FreqPointer < lastTi.FreqPointer)
				throw new System.IO.IOException("freqPointer out of order");
			if (ti.ProxPointer < lastTi.ProxPointer)
				throw new System.IO.IOException("proxPointer out of order");

			if (!isIndex && size % INDEX_INTERVAL == 0)
				other.Add(lastTerm, lastTi);      // add an index term

			WriteTerm(term);          // write term
			output.WriteVInt(ti.DocFreq);     // write doc freq
			output.WriteVLong(ti.FreqPointer - lastTi.FreqPointer); // write pointers
			output.WriteVLong(ti.ProxPointer - lastTi.ProxPointer);

			if (isIndex) 
			{
				output.WriteVLong(other.output.GetFilePointer() - lastIndexPointer);
				lastIndexPointer = other.output.GetFilePointer(); // write pointer
			}

			lastTi.Set(ti);
			size++;
		}

		void WriteTerm(Term term)
		{
			int start = StringDifference(lastTerm.Text, term.Text);
			int length = term.Text.Length - start;
    
			output.WriteVInt(start);        // write shared prefix length
			output.WriteVInt(length);       // write delta length
			output.WriteChars(term.Text, start, length);  // write delta chars

			output.WriteVInt(fieldInfos.FieldNumber(term.Field)); // write field num

			lastTerm = term;
		}

		static int StringDifference(String s1, String s2) 
		{
			int len1 = s1.Length;
			int len2 = s2.Length;
			int len = len1 < len2 ? len1 : len2;
			for (int i = 0; i < len; i++)
				if (s1[i] != s2[i])
					return i;
			return len;
		}

		///<summary> Called to complete TermInfos creation. </summary>
		internal void Close()
		{
			output.Seek(0);         // write size at start
			output.WriteInt(size);
			output.Close();
    
			if (!isIndex)
				other.Close();
		}
	}
}
