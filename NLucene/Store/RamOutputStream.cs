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

namespace DotnetPark.NLucene.Store
{
	internal class RamOutputStream : OutputStream 
	{
		RamFile file;
		int pointer = 0;

		public RamOutputStream(RamFile f) 
		{
			file = f;
		}

		///<summary> output methods: </summary>
		public override void FlushBuffer(byte[] src, int len) 
		{
			int bufferNumber = pointer/OutputStream.BUFFER_SIZE;
			int bufferOffset = pointer%OutputStream.BUFFER_SIZE;
			int bytesInBuffer = OutputStream.BUFFER_SIZE - bufferOffset;
			int bytesToCopy = bytesInBuffer >= len ? len : bytesInBuffer;

			if (bufferNumber == file.Buffers.Count)
				file.Buffers.Add(new byte[OutputStream.BUFFER_SIZE]);

			byte[] buffer = file.Buffers[bufferNumber];
			for(int i = 0; i < bytesToCopy; ++i)
				buffer[bufferOffset + i] = src[i];
			//Array.Copy(src, 0, buffer, bufferOffset, bytesToCopy);

			if (bytesToCopy < len) 
			{			  // not all in one buffer
				int srcOffset = bytesToCopy;
				bytesToCopy = len - bytesToCopy;		  // remaining bytes
				bufferNumber++;
				if (bufferNumber == file.Buffers.Count)
					file.Buffers.Add(new byte[OutputStream.BUFFER_SIZE]);
				buffer = file.Buffers[bufferNumber];

				for(int i = 0; i < bytesToCopy; ++i)
					buffer[i] = src[srcOffset + i];
				//Array.Copy(src, srcOffset, buffer, 0, bytesToCopy);
			}
			pointer += len;
			if (pointer > file.Length)
				file.Length = pointer;

			file.LastModified = DateTime.Now.ToFileTime();
		}

		public override void Close()
		{
			base.Close();
		}

		///<summary> Random-access methods </summary>
		public override void Seek(long pos)  
		{
			base.Seek(pos);
			pointer = (int)pos;
		}
		public override long Length
		{
			get 
			{
				return file.Length;
			}
		}
	}
}
