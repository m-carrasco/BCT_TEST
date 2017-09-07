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
using System.IO;

namespace DotnetPark.NLucene.Analysis.Standard
{
	/// <summary>
	/// Ported implementation of the FastCharStream class.
	/// </summary>
	public class FastCharStream
	{
		string file = null;
		int    ptr  = 0;

		int col = 1;
		int line = 1;

		/// <summary>
		/// Initializes a new instance of the FastCharStream class.
		/// </summary>
		/// <param name="textReader">TextReader to scan for.</param>
		public FastCharStream(TextReader textReader)
		{
			file = textReader.ReadToEnd();
			textReader.Close();
		}
		
		/// <summary>
		/// Returns the next char from the stream.
		/// </summary>
		/// <returns>The next read char.</returns>
		public char GetNext()
		{
			if (Eos()) 
			{
				Console.WriteLine("warning : FileReader.GetNext : Read char over eos.");
				return '\0';
			}
			char ch = file[ptr++];
			if(ch == '\n') 
			{
				line++;
				col = 1;
			}
			return ch;
		}
		
		/// <summary>
		/// Returns the current top char from the input stream without removing it.
		/// </summary>
		/// <returns>Top char.</returns>
		public char Peek()
		{
			if (Eos()) 
			{
				Console.WriteLine("warning : FileReader.Peek : Read char over eos.");
				return '\0';
			}
			return file[ptr];
		}
		
		/// <summary>
		/// Ungets the last returned char.
		/// </summary>
		public void UnGet()
		{
			--ptr;
			if (ptr < 0) 
				throw new Exception("error : FileReader.UnGet : ungetted first char");
		}
		
		/// <summary>
		/// Returns <b>True</b> if the end of stream was reached.
		/// </summary>
		/// <returns>Returns <b>True</b> if the end of stream was reached.</returns>
		public bool Eos()
		{
			return ptr >= file.Length;
		}

		/// <summary>
		/// Gets the current column.
		/// </summary>
		public int Column 
		{
			get
			{
				return col;
			}
		}

		/// <summary>
		/// Gets the current line.
		/// </summary>
		public int Line
		{
			get
			{
				return line;
			}
		}
	}
}
