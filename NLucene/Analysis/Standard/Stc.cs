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

namespace DotnetPark.NLucene.Analysis.Standard
{
	/// <summary>
	/// Standard Tokenizer Constants.
	/// </summary>
	public class Stc
	{
		/// <summary> EOF </summary>
		public static int EOF = 0;

		/// <summary> ALPHANUM </summary>
		public static int ALPHANUM = 1;

		/// <summary> APOSTROPHE </summary>
		public static int APOSTROPHE = 2;

		/// <summary> ACRONYM </summary>
		public static int ACRONYM = 3;

		/// <summary> COMPANY </summary>
		public static int COMPANY = 4;

		/// <summary> EMAIL </summary>
		public static int EMAIL = 5;

		/// <summary> HOST </summary>
		public static int HOST = 6;

		/// <summary> NUM </summary>
		public static int NUM = 7;

		/// <summary> P </summary>
		public static int P = 8;

		/// <summary> HAS_DIGIT </summary>
		public static int HAS_DIGIT = 9;

		/// <summary> ALPHA </summary>
		public static int ALPHA = 10;

		/// <summary> LETTER </summary>
		public static int LETTER = 11;

		/// <summary> DIGIT </summary>
		public static int DIGIT = 12;

		/// <summary> NOISE </summary>
		public static int NOISE = 13;
		
		/// <summary> DEFAULT </summary>
		public static int DEFAULT = 0;

		/// <summary> Token images. </summary>
		public static String[] tokenImage;

		static Stc()
		{
			tokenImage = new String[14];
			tokenImage[0] = "<EOF>";
			tokenImage[1] = "<ALPHANUM>";
			tokenImage[2] = "<APOSTROPHE>";
			tokenImage[3] = "<ACRONYM>";
			tokenImage[4] = "<COMPANY>";
			tokenImage[5] = "<EMAIL>";
			tokenImage[6] = "<HOST>";
			tokenImage[7] = "<NUM>";
			tokenImage[8] = "<P>";
			tokenImage[9] = "<HAS_DIGIT>";
			tokenImage[10] = "<ALPHA>";
			tokenImage[11] = "<LETTER>";
			tokenImage[12] = "<DIGIT>";
			tokenImage[13] = "<NOISE>";
		}

	}
}
