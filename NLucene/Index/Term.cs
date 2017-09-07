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

namespace DotnetPark.NLucene.Index
{       
	/// <summary>
	/// A Term represents a word from text. This is the unit of search.
	/// It is composed of two elements, the text of the word, as a string,
	/// and the name of the field that the text occured in, an interned string.
	/// Note that terms may represent more than words from text fields, but also
	/// things like dates, email addresses, urls, etc.
	/// </summary>
	public sealed class Term
	{
		String field;
		String text;
  
		///<summary> Constructs a Term with the given field and text. </summary>
		public Term(String fld, String txt) 
			: this(fld, txt, true)
		{
			// use this
		}

		/// <summary>
		/// Constructs a Term with the given field and text.
		/// </summary>
		public Term(String fld, String txt, bool intern) 
		{
			field = intern ? string.Intern(fld) : fld;    // field names are interned
			text = txt;           // unless already known to be
		}

		///<summary>
		///Returns the field of this term, an interned string.   The field indicates
		///the part of a document which this term came from.
		///</summary>
		public string Field
		{
			get			
			{return field;}
		}

		///<summary>
		///Returns the text of this term.  In the case of words, this is simply the
		///text of the word.  In the case of dates and other types, this is an
		///encoding of the object as a string.
		///</summary>
		public string Text 
		{
			get 
			{ return text; }
			set 
			{
				text= value;
			}
		}

		///<summary>
		///Compares two terms, returning true iff they have the same
		///field and text.
		///</summary>
		public override bool Equals(Object o) 
		{
			if (o == null)
				return false;

      Term other = o as Term;
      if (other == null) return false;
			return field == other.field && text.Equals(other.text);
		}

		///<summary> Combines the HashCode() of the field and the text. </summary>
		public override int GetHashCode() 
		{
			return field.GetHashCode() + text.GetHashCode();
		}

		///<summary>
		///Compares two terms, returning an integer which is less than zero iff this
		///term belongs after the argument, equal zero iff this term is equal to the
		///argument, and greater than zero iff this term belongs after the argument.
		///The ordering of terms is first by field, then by text.
		///</summary>
		public int CompareTo(Term other) 
		{
			if (field == other.field)       // fields are interned
				return text.CompareTo(other.text);
			else
				return field.CompareTo(other.field);
		}

		///<summary> Resets the field and text of a Term. </summary>
		public void Set(String fld, String txt) 
		{
			field = fld;
			text = txt;
		}

		/// <summary>
		/// Returns a string representation of the term.
		/// </summary>
		/// <returns>A string representing a term.</returns>
		public override string ToString() 
		{
			return "Term<" + field + ":" + text + ">";
		}
	}
}
