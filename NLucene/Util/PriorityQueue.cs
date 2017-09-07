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

namespace DotnetPark.NLucene.Util
{
	/*
	/// <summary>
	/// Represents a simple priority queue.
	/// </summary>
	public abstract class PriorityQueue<T0>
	{
		private T0[] heap;
		private int size;

	
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		abstract protected bool LessThan(T0 a, T0 b);

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		protected void Initialize(int maxSize) 
		{
			size = 0;
			int heapSize = (maxSize * 2) + 1;
			heap = new T0[heapSize];
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public void Put(T0 element) 
		{
			size++; 
			heap[size] = element;
			UpHeap();
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public T0 Top() 
		{
			if (size > 0)
				return heap[1];
			else
				return default(T0);
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public T0 Pop() 
		{
			if (size > 0) 
			{
				T0 result = heap[1];        // save first value
				heap[1] = heap[size];       // move last to first
				heap[size] = default(T0);        // permit GC of objects
				size--;
				DownHeap();         // adjust heap
				return result;
			} 
			else
				return default(T0);
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public void AdjustTop() 
		{
			DownHeap();
		}
    

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public int Size() 
		{
			return size;
		}
  
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public void Clear() 
		{
			for (int i = 0; i < size; i++)
				heap[i] = default(T0);
			size = 0;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		private void UpHeap() 
		{
			uint i = (uint) size;
			T0 node = heap[i];        // save bottom node
			uint j = i >> 1;
			while (j > 0 && LessThan(node, heap[j])) 
			{
				heap[i] = heap[j];        // shift parents down
				i = j;
				j = j >> 1;
			}
			heap[i] = node;         // install saved node
		}
  
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		private void DownHeap() 
		{
			int i = 1;
			T0 node = heap[i];        // save top node
			int j = i << 1;         // find smaller child
			int k = j + 1;
			if (k <= size && LessThan(heap[k], heap[j])) 
			{
				j = k;
			}
			while (j <= size && LessThan(heap[j], node)) 
			{
				heap[i] = heap[j];        // shift up child
				i = j;
				j = i << 1;
				k = j + 1;
				if (k <= size && LessThan(heap[k], heap[j])) 
				{
					j = k;
				}
			}
			heap[i] = node;         // install saved node
		}
	}
	*/ 
}
