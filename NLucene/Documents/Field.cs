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

namespace DotnetPark.NLucene.Documents
{
	/// <summary>
	/// A field is a section of a Document. Each field has two parts,
	/// a name and a value. Values may be free text, provided as a
	/// String or as a TextReader, or they may be atomic keywords,
	/// which are not further processed. Such keywords may be used to
	/// represent dates, urls, etc. Fields are optionally stored in the index,
	/// so that they may be returned with hits on the document.
	/// </summary>
	public sealed class Field 
	{
		private string name = "body";
		private string stringValue = null;
		private TextReader readerValue = null;
		private bool isStored = false;
		private bool isIndexed = true;
		private bool isTokenized = true;

		/// <summary>
		/// Initializes a new instance of the Field class with specified parameters.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="str">A string value of the field.</param>
		/// <param name="store">Indicates whether to store this field or not.</param>
		/// <param name="index">Indicates that contents of this field must be indexed.</param>
		/// <param name="token">Indicates that this field instance is a token.</param>
		public Field(string name, string str,
			bool store, bool index, bool token) 
		{
			if (name == null)
				throw new ArgumentNullException("name argument cannot be null");
			if (str == null)
				throw new ArgumentNullException("str argument cannot be null");

			this.name = string.Intern(name);        // todo: field names are interned!!!
			this.stringValue = str;
			this.isStored = store;
			this.isIndexed = index;
			this.isTokenized = token;
		}

		///<summary>
		/// Constructs a String-valued Field that is not tokenized, but is indexed
		/// and stored.  Useful for non-text fields, e.g. date or url.
		///</summary>
		public static Field Keyword(string name, string value) 
		{
			return new Field(name, value, true, true, false);
		}

		///<summary>
		/// Constructs a String-valued Field that is not tokenized nor indexed,
		/// but is stored in the index, for return with hits.
		/// </summary>
		public static Field UnIndexed(string name, string value) 
		{
			return new Field(name, value, true, false, false);
		}

		///<summary>
		/// Constructs a String-valued Field that is tokenized and indexed,
		/// and is stored in the index, for return with hits.  Useful for short text
		/// fields, like &quot;title&quot; or &quot;subject&quot;.
		///</summary>
		public static Field Text(string name, string value) 
		{
			return new Field(name, value, true, true, true);
		}

		///<summary>
		/// Constructs a String-valued Field that is tokenized and indexed,
		/// but that is not stored in the index.
		///</summary>
		public static Field UnStored(string name, string value) 
		{
			return new Field(name, value, false, true, true);
		}

		///<summary>
		/// Constructs a TextReader-valued Field that is tokenized and indexed, but is
		/// not stored in the index verbatim.  Useful for longer text fields, like
		/// &quot;body&quot;.
		/// </summary>
		public static Field Text(String name, TextReader value) 
		{
			return new Field(name, value);
		}


		/// <summary>
		/// Private constructor.
		/// </summary>
		Field(string name, TextReader reader) 
		{
			if (name == null)
				throw new ArgumentNullException("name argument cannot be null");
			if (reader == null)
				throw new ArgumentNullException("reader argument cannot be null");

			this.name = string.Intern(name);        // todo: field names are interned
			this.readerValue = reader;
		}

		///<summary>
		/// Gets the name of the field (e.g., &quot;date&quot;, &quot;subject&quot;,
		/// &quot;title&quot;, &quot;body&quot;, etc.)
		/// as an interned string.
		/// </summary>
		public string Name
		{ 
			get
			{
				return name; 
			}
		}

		///<summary>
		/// Gets the value of the field as a String, or null.  If null, the TextReader value
		/// is used.  Exactly one of StringValue and ReaderValue must be set.
		/// </summary>
		public string Value
		{ 
			get
			{
				return stringValue; 
			}
		}


		///<summary>
		/// Gets the value of the field as a TextReader, or null.  If null, the String value
		/// is used.  Exactly one of StringValue and ReaderValue must be set.
		/// </summary>
		public TextReader ReaderValue
		{
			get 
			{
				return readerValue; 
			}
		}

		///<summary>
		///Returns True if the value of the field is to be stored in the index for return
		///with search hits.  It is an error for this to be true if a field is
		///Reader-valued.
		///</summary>
		public bool  IsStored  
		{
			get { return isStored; }
		}

		///<summary>
		///Returns True if the value of the field is to be indexed, so that it may be
		///searched on.
		///</summary>
		public bool  IsIndexed
		{ 
			get { return isIndexed; }
		}

		///<summary>
		///Returns True iff the value of the field should be tokenized as text prior to
		///indexing.  Un-tokenized fields are indexed as a single word and may not be
		///Reader-valued.
		///</summary>
		public bool  IsTokenized   
		{ 
			get { return isTokenized; }
		}

		///<summary> Prints a Field for human consumption. </summary>
		public override string ToString() 
		{
			if (isStored && isIndexed && !isTokenized)
				return "Keyword<" + name + ":" + stringValue + ">";
			else if (isStored && !isIndexed && !isTokenized)
				return "Unindexed<" + name + ":" + stringValue + ">";
			else if (isStored && isIndexed && isTokenized && stringValue!=null)
				return "Text<" + name + ":" + stringValue + ">";
			else if (!isStored && isIndexed && isTokenized && readerValue!=null)
				return "Text<" + name + ":" + readerValue + ">";
			else
				return base.ToString();
		}

	}
}
