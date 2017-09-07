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
using System.Text;
using System.Collections.Generic;

namespace DotnetPark.NLucene.Documents
{
	///<summary>
	/// <p>Documents are the unit of indexing and search.</p>
	///
	/// <p>A Document is a set of fields.  Each field has a name and a textual value.
	/// A field may be stored with the document, in which case it is returned with
	/// search hits on the document.  Thus each document should typically contain
	/// stored fields which uniquely identify it.</p>
	/// </summary>
	public sealed class Document 
	{
		internal DocumentFieldList fieldList = null;

		///<summary> Constructs a new document with no fields. </summary>
		public Document()
		{
			// nothing to do
		}

		///<summary> Adds a field to a document.  Several fields may be added with
		/// the same name.  In this case, if the fields are indexed, their text is
		/// treated as though appended for the purposes of search. </summary>
		public void Add(Field field) 
		{
			fieldList = new DocumentFieldList(field, fieldList);
		}

		/// <summary>
		/// Gets a field with the specified name.
		/// </summary>
		public Field this[string fieldName]
		{
			get
			{
				return GetField(fieldName);
			}
		}

		///<summary>
		///Returns a field with the given name if any exist in this document, or
		///null. If multiple fields may exist with this name, this method returns the
		///last added such added. </summary>
		private Field GetField(string name) 
		{
			for (DocumentFieldList list = fieldList; list != null; list = list.Next)
				if (list.Field.Name.Equals(name))
					return list.Field;
			return null;
		}

		/*public string Get(string name)
		{
			Field field = GetField(name);
			if (field != null)
				return field.StringValue;
			else
				return null;
		}*/

		///<summary> Returns an Enumeration of all the fields in a document. </summary>
		public IEnumerable<Field> Fields
		{
			get
			{
				return new DocumentFields(this);
			}
		}

		///<summary> Prints the fields of a document for human consumption. </summary>
		public  override string ToString() 
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append("Document<");
			for (DocumentFieldList list = fieldList; list != null; list = list.Next) 
			{
				buffer.Append(list.Field.ToString());
				if (list.Next != null)
					buffer.Append(" ");
			}
			buffer.Append(">");
			return buffer.ToString();
		}

	}

}
