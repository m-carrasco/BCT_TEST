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
	internal sealed class SegmentTermEnum : ITermEnum, ICloneable 
	{
		private InputStream input;
		private FieldInfos fieldInfos;
		int size;
		int position = -1;

		Term term = new Term("", "");
		TermInfo termInfo = new TermInfo();

		bool isIndex = false;
		long indexPointer = 0;
		Term prev;

		private char[] buffer = {};

		internal int Position 
		{
			get
			{
				return position;
			}
		}

		internal Term Prev 
		{
			get
			{
				return prev;
			}
		}
		
		internal long IndexPointer 
		{
			get
			{
				return indexPointer;
			}
			/*
						set 
						{
							indexPointer = value;
						}
			*/	
		}

		internal int Size 
		{
			get
			{
				return size;
			}
			/*
						set 
						{
							size = value;
						}
			*/			
		}

		internal SegmentTermEnum(InputStream i, FieldInfos fis, bool isi)
		{
			input = i;
			fieldInfos = fis; 
			size = input.ReadInt();
			isIndex = isi;
		}
  
		public Object Clone() 
		{
			SegmentTermEnum clone = null;
			clone = (SegmentTermEnum) base.MemberwiseClone();

			clone.input = (InputStream)input.Clone();
			clone.termInfo = new TermInfo(termInfo);
			if (term != null) clone.GrowBuffer(term.Text.Length);

			return clone;
		}

		internal void Seek(long pointer, int p, Term t, TermInfo ti)
		{
			input.Seek(pointer);
			position = p;
			term = t;
			prev = null;
			termInfo.Set(ti);
			GrowBuffer(term.Text.Length);     // copy term text into buffer
		}

		///<summary> Increments the enumeration to the next element.  True if one exists.</summary>
		public bool Next()
		{
			if (position++ >= size-1) 
			{
				term = null;
				return false;
			}

			prev = term;
			term = ReadTerm();

			termInfo.DocFreq = input.ReadVInt();    // read doc freq
			termInfo.FreqPointer += input.ReadVLong();    // read freq pointer
			termInfo.ProxPointer += input.ReadVLong();    // read prox pointer
    
			if (isIndex)
				indexPointer += input.ReadVLong();    // read index pointer

			return true;
		}

		Term ReadTerm() 
		{
			int start = input.ReadVInt();
			int length = input.ReadVInt();
			int totalLength = start + length;
			if (buffer.Length < totalLength)
				GrowBuffer(totalLength);
    
			input.ReadChars(buffer, start, length);
			return new Term(fieldInfos.FieldName(input.ReadVInt()),
				new string(buffer, 0, totalLength), false);
		}

		void GrowBuffer(int length) 
		{
			buffer = new char[length];
			for (int i = 0; i < term.Text.Length; i++)  // copy contents
				buffer[i] = term.Text[i];
		}

		///<summary> Returns the current Term in the enumeration.
		  ///Initially invalid, valid after next() called for the first time.</summary>
		public Term Term() 
		{
			return term;
		}

		///<summary> Returns the current TermInfo in the enumeration.
		  ///Initially invalid, valid after next() called for the first time.</summary>
		internal TermInfo TermInfo() 
		{
			return new TermInfo(termInfo);
		}

		///<summary> Sets the argument to the current TermInfo in the enumeration.
		  ///Initially invalid, valid after next() called for the first time.</summary>
		internal void TermInfo(TermInfo ti) 
		{
			ti.Set(termInfo);
		}

		///<summary> Returns the docFreq from the current TermInfo in the enumeration.
		  ///Initially invalid, valid after next() called for the first time.</summary>
		public int DocFreq() 
		{
			return termInfo.DocFreq;
		}

		/// <summary>
		///  Returns the freqPointer from the current TermInfo in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		long FreqPointer() 
		{
			return termInfo.FreqPointer;
		}

		/// <summary>
		///  Returns the proxPointer from the current TermInfo in the enumeration.
		/// Initially invalid, valid after next() called for the first time.
		/// </summary>
		long ProxPointer() 
		{
			return termInfo.ProxPointer;
		}

		///<summary> Closes the enumeration to further activity, freeing resources. </summary>
		public void Close()
		{
			input.Close();
		}
	}
}
