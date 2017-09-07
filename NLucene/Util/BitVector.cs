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

namespace DotnetPark.NLucene.Util
{
	/// <summary>
	/// A simple implementation of the bit vector.
	/// </summary>
	public class BitVector
	{
		///<summary> This is public just so that methods will inline.  Please don't touch.</summary>
		public byte[] bits;
		private int size;
		private int count = -1;

		///<summary> Constructs a vector capable of holding <c>n</c> bits. </summary>
		public BitVector(int n) 
		{
			size = n;
			bits = new byte[(size >> 3) + 1];
		}

		///<summary> Constructs a bit vector from the file <c>name</c> in Directory
		///<c>d</c>, as written by the {@link #write} method.
		///</summary>
		public BitVector(Directory d, String name) 
		{
			InputStream input = d.OpenFile(name);
			try 
			{
				size = input.ReadInt();			  // read size
				count = input.ReadInt();			  // read count
				bits = new byte[(size >> 3) + 1];		  // allocate bits
				input.ReadBytes(bits, 0, bits.Length);	  // read bits
			} 
			finally 
			{
				input.Close();
			}
		}

		///<summary> Sets the value of <c>bit</c> to one. </summary>
		public void Set(int bit) 
		{
			bits[bit >> 3] |= (byte)( 1 << (bit & 7));
			count = -1;
		}

		///<summary> Sets the value of <c>bit</c> to zero. </summary>
		public void Clear(int bit) 
		{
			bits[bit >> 3] &= (byte)( ~(1 << (bit & 7)));
			count = -1;
		}

		///<summary> Returns <c>true</c> if <c>bit</c> is one and
		///<c>false</c> if it is zero. </summary>
		public bool Get(int bit) 
		{
			return (bits[bit >> 3] & (1 << (bit & 7))) != 0;
		}

		///<summary> Returns the number of bits in this vector.  This is also one greater than
		///the number of the largest valid bit number. </summary>
		public int Size 
		{
			get 
			{
				return size;
			}
		}

		///<summary> Returns the total number of one bits in this vector.  This is efficiently
		///computed and cached, so that, if the vector is not changed, no
		///recomputation is done for repeated calls. </summary>
		public int Count 
		{
			get
			{
				if (count == -1) 
				{
					int c = 0;
					int end = bits.Length;
					for (int i = 0; i < end; i++)
						c += BYTE_COUNTS[bits[i] & 0xFF];	  // sum bits per byte
					count = c;
				}
				return count;
			}
		}

		private static byte[] BYTE_COUNTS = null;

		static BitVector()
		{

			Console.WriteLine("Ok we don't do the weird init thing so we commented it out and this will crash");
			BYTE_COUNTS = new byte[0];

			/*
			BYTE_COUNTS = {	  // table of bits/byte
													0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
													1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
													1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
													1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
													2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
													3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
													3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
													4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
												};
			 */
		}


		///<summary> Writes this vector to the file <c>name</c> in Directory
		///<c>d</c>, in a format that can be read by the constructor 
		///BitVector(Directory, String).  </summary>
		public void Write(Directory d, String name)
		{
			OutputStream output = d.CreateFile(name);
			try 
			{
				output.WriteInt(this.Size);			  // write size
				output.WriteInt(this.Count);			  // write count
				output.WriteBytes(bits, bits.Length);	  // write bits
			} 
			finally 
			{
				output.Close();
			}
		}
	}
}
