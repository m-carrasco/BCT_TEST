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
	/// <summary>
	/// Contains utils for diferent array elements operations.
	/// </summary>
	public class Arrays
	{
		/// <summary>
		/// Sorts an array of strings.
		/// </summary>
		/// <param name="a">An array of strings to sort.</param>
		public static void Sort(string[] a) 
		{
			string[] aux = new string[a.Length];
			for(int i = 0; i < a.Length; ++i)
				aux[i] = a[i];
			//string[] aux = (string[]) a.Clone();
			MergeSort(aux, a, 0, a.Length);
		}

		private static void MergeSort(string[] src, string[] dest,
			int low, int high) 
		{
			int length = high - low;

			// Insertion sort on smallest arrays
			if (length < 7) 
			{
				for (int i=low; i<high; i++)
					for (int j=i; j>low && (dest[j-1]).CompareTo(dest[j])>0; j--)
						Swap(dest, j, j-1);
				return;
			}

			// Recursively sort halves of dest into src
			int mid = (low + high)/2;
			MergeSort(dest, src, low, mid);
			MergeSort(dest, src, mid, high);

			// If list is already sorted, just copy from src to dest.  This is an
			// optimization that results in faster sorts for nearly ordered lists.
			if ((src[mid-1]).CompareTo(src[mid]) <= 0) 
			{
				for(int i = 0; i < length; ++i)
					dest[low + i] = src[low + i];
				//Array.Copy(src, low, dest, low, length);
				
				return;
			}

			// Merge sorted halves (now in src) into dest
			for(int i = low, p = low, q = mid; i < high; i++) 
			{
				if (q>=high || p<mid && (src[p]).CompareTo(src[q])<=0)
					dest[i] = src[p++];
				else
					dest[i] = src[q++];
			}
		}

		private static void Swap(string[] x, int a, int b) 
		{
			string t = x[a];
			x[a] = x[b];
			x[b] = t;
		}
	}


}
