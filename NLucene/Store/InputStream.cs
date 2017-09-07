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

namespace DotnetPark.NLucene.Store
{
	/// <summary>
	/// A random-access input stream.
	/// </summary>
	public abstract class InputStream : ICloneable
	{
		/// <summary>
		/// The maximum size of the buffer.
		/// </summary>
		public static int BUFFER_SIZE = 1024;

		private byte[] buffer;
		private char[] chars;

		private long bufferStart = 0;			  // position in file of buffer
		private int bufferLength = 0;			  // end of valid bytes
		private int bufferPosition = 0;		  // next byte to Read

		/// <summary>
		/// Set by subclasses.
		/// </summary>
		protected long length;			  // set by subclasses

		///<summary> InputStream-like methods.</summary>
		public byte ReadByte() 
		{
			if (bufferPosition >= bufferLength)
				Refill();
			return buffer[bufferPosition++];
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="b">[To be supplied.]</param>
		/// <param name="offset">[To be supplied.]</param>
		/// <param name="len">[To be supplied.]</param>
		public void ReadBytes(byte[] b, int offset, int len) 
		{
			if (len < BUFFER_SIZE) 
			{
				for (int i = 0; i < len; i++)		  // Read byte-by-byte
					b[i + offset] = (byte)ReadByte();
			} 
			else 
			{					  // Read all-at-once
				long start = GetFilePointer();
				SeekInternal(start);
				ReadInternal(b, offset, len);

				bufferStart = start + len;		  // adjust stream variables
				bufferPosition = 0;
				bufferLength = 0;				  // trigger refill() on Read
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public int ReadInt() 
		{
			return ((ReadByte() & 0xFF) << 24) | ((ReadByte() & 0xFF) << 16)
				| ((ReadByte() & 0xFF) <<  8) |  (ReadByte() & 0xFF);
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public int ReadVInt() 
		{
			byte b = ReadByte();
			int i = b & 0x7F;
			for (int shift = 7; (b & 0x80) != 0; shift += 7) 
			{
				b = ReadByte();
				i |= (b & 0x7F) << shift;
			}
			return i;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public long ReadLong() 
		{
			return (((long)ReadInt()) << 32) | (ReadInt() & 0xFFFFFFFFL);
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public long ReadVLong() 
		{
			byte b = ReadByte();
			long i = b & 0x7F;
			for (int shift = 7; (b & 0x80) != 0; shift += 7) 
			{
				b = ReadByte();
				i |= (b & 0x7FL) << shift;
			}
			return i;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public String ReadString() 
		{
			int length = ReadVInt();
			if (chars == null || length > chars.Length)
				chars = new char[length];
			ReadChars(chars, 0, length);
			return new String(chars, 0, length);
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="buffer">[To be supplied.]</param>
		/// <param name="start">[To be supplied.]</param>
		/// <param name="length">[To be supplied.]</param>
		public void ReadChars(char[] buffer, int start, int length) 
		{
			int end = start + length;
			for (int i = start; i < end; i++) 
			{
				byte b = ReadByte();
				if ((b & 0x80) == 0)
					buffer[i] = (char)(b & 0x7F);
				else if ((b & 0xE0) != 0xE0) 
				{
					buffer[i] = (char)(((b & 0x1F) << 6)
						| (ReadByte() & 0x3F));
				} 
				else 
					buffer[i] = (char)(((b & 0x0F) << 12)
						| ((ReadByte() & 0x3F) << 6)
						|  (ReadByte() & 0x3F));
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		protected void Refill() 
		{
			long start = bufferStart + bufferPosition;
			long end = start + BUFFER_SIZE;
			if (end > length)				  // don't Read past EOF
				end = length;
			bufferLength = (int)(end - start);
			if (bufferLength == 0)
				throw new IOException("Read past EOF");

			if (buffer == null)
				buffer = new byte[BUFFER_SIZE];		  // allocate buffer lazily
			ReadInternal(buffer, 0, bufferLength);

			bufferStart = start;
			bufferPosition = 0;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="b">[To be supplied.]</param>
		/// <param name="offset">[To be supplied.]</param>
		/// <param name="length">[To be supplied.]</param>
		abstract public void ReadInternal(byte[] b, int offset, int length);

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		abstract public void Close();

		///<summary> RandomAccessFile-like methods @see java.io.RandomAccessFile </summary>
		public long GetFilePointer() 
		{
			return bufferStart + bufferPosition;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="pos">[To be supplied.]</param>
		public void Seek(long pos) 
		{
			if (pos >= bufferStart && pos < (bufferStart + bufferLength))
				bufferPosition = (int)(pos - bufferStart);  // seek within buffer
			else 
			{
				bufferStart = pos;
				bufferPosition = 0;
				bufferLength = 0;				  // trigger refill() on Read()
				SeekInternal(pos);
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="pos">[To be supplied.]</param>
		abstract public void SeekInternal(long pos);

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public long Length 
		{
			get
			{
				return length;
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <returns>[To be supplied.]</returns>
		public virtual Object Clone() 
		{
			InputStream clone = null;
			try 
			{
				clone = (InputStream) this.MemberwiseClone();
			} 
			catch (Exception e)
			{
				throw new Exception("Can't clone InputStream." + e);
			}

			if (buffer != null) 
			{
				clone.buffer = new byte[BUFFER_SIZE];
				for(int i = 0; i < bufferLength; ++i)
					clone.buffer[i] = buffer[i];
				
				//Array.Copy(buffer, 0, clone.buffer, 0, bufferLength);
			}

			clone.chars = null;

			return clone;
		}
	}
}
