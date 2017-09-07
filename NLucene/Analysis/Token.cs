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

using DotnetPark.NLucene.Analysis.Standard;

namespace DotnetPark.NLucene.Analysis
{
	/// <summary>
	/// A Token is an occurence of a term from the text of a field.
	/// It consists of a term's text, the start and end offset of the
	/// term in the text of the field, and a type string.
	/// The start and end offsets permit applications to re-associate a
	/// token with its source text, e.g., to display highlighted query
	/// terms in a document browser, or to show matching text fragments
	/// in a KWIC (KeyWord In Context) display, etc.
	/// The type is an interned string, assigned by a lexical analyzer
	/// (a.k.a. tokenizer), naming the lexical or syntactic class that the
	/// token belongs to. For example an end of sentence marker token might
	/// be implemented with type &quot;eos&quot;. The default token type is &quot;word&quot;.
	/// </summary>
	public class Token 
	{
		private string termText;				// the text of the term
		private int startOffset;				// start in source text
		private int endOffset;					// end in source text
		private string type = "word";			// lexical type

		///<summary> Constructs a Token with the given term text, and start &amp; end offsets.
		///The type defaults to "word." </summary>
		public Token(string text, int start, int end)
		{
			termText = text;
			startOffset = start;
			endOffset = end;
		}

		///<summary> Constructs a Token with the given text, start and end offsets, &amp; type. </summary>
		public Token(string text, int start, int end, string typ) 
		{
			termText = text;
			startOffset = start;
			endOffset = end;
			type = typ;
		}

		/// <summary>
		/// ///<summary> Constructs a Token with the given text, start and end offsets, &amp; type. </summary>
		/// </summary>
		public Token(string termText, int startOffset, int endOffset, TokenTypes type) 
		{
			this.termText = termText;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
			this.type = "<" + type.ToString() + ">";
		}

		/// <summary>
		/// For debug.
		/// </summary>
		/// <returns>A string representing current token.</returns>
		public override string ToString()
		{
			return "(" + type + ") " + termText;
		}

		///<summary> Returns the Token's term text. </summary>
		public string TermText 
		{ 
			get 
			{
				return termText; 
			}
			set
			{
				termText = value;
			}
		}

		///<summary> Returns this Token's starting offset, the position of the first character
		///corresponding to this token in the source text.
		///Note that the difference between endOffset() and startOffset() may not be
		///equal to termText.length(), as the term text may have been altered by a
		///stemmer or some other filter. </summary>
		public int StartOffset 
		{
			get
			{
				return startOffset; 
			}
			set
			{
				startOffset = value;
			}
		}

		///<summary> Returns this Token's ending offset, one greater than the position of the
		///last character corresponding to this token in the source text. </summary>
		public int EndOffset 
		{
			get
			{
				return endOffset;
			}
			set
			{
				endOffset = value;
			}
		}

		///<summary> Returns this Token's lexical type.  Defaults to &quot;word&quot;.</summary>
		public string Type
		{
			get
			{
				return type; 
			}
			set
			{
				type = value;
			}
		}

	}
}
