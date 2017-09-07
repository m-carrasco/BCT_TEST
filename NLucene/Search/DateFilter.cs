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
using System.Collections;

using DotnetPark.NLucene.Documents;
using DotnetPark.NLucene.Index;

namespace DotnetPark.NLucene.Search
{
	///<summary>
	///<p>A Filter that restricts search results to a range of time.</p>
	///<p>For this to work, documents must have been indexed with a DateField.</p>
	///</summary>
	public class DateFilter : Filter 
	{
		private string field;

		private string start = DateField.MIN_DATE_STRING;
		private string end = DateField.MAX_DATE_STRING;

		/// <summary>
		/// Gets or sets a field.
		/// </summary>
		public string Field
		{
			get { return field; }
			set { field = value; }
		}

		/// <summary>
		/// Gets or sets a start Date.
		/// </summary>
		public string Start
		{
			get { return start; }
			set { start = value; }
		}

		/// <summary>
		/// Gets or sets a start Date.
		/// </summary>
		public string End
		{
			get { return end; }
			set { end = value; }
		}

		/// <summary>
		/// Private constructor.
		/// </summary>
		private DateFilter(string f) 
		{
			field = f;
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching dates between
		///<c>from</c> and <c>to</c>.
		///</summary>
		public DateFilter(string f, DateTime from, DateTime to) 
		{
			field = f;
			start = DateField.DateToString(from);
			end = DateField.DateToString(to);
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching times between
		///<c>from</c> and <c>to</c>.
		///</summary>
		public DateFilter(string f, long from, long to) 
		{
			field = f;
			start = DateField.TimeToString(from);
			end = DateField.TimeToString(to);
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching dates before
		///<c>date</c>.
		///</summary>
		public static DateFilter Before(string field, DateTime date) 
		{
			DateFilter result = new DateFilter(field);
			result.End = DateField.DateToString(date);
			return result;
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching times before
		///<c>time</c>.
		///</summary>
		public static DateFilter Before(string field, long time) 
		{
			DateFilter result = new DateFilter(field);
			result.End = DateField.TimeToString(time);
			return result;
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching dates after
		///<c>date</c>.
		///</summary>
		public static DateFilter After(string field, DateTime date) 
		{
			DateFilter result = new DateFilter(field);
			result.Start = DateField.DateToString(date);
			return result;
		}

		///<summary>
		///Constructs a filter for field <c>f</c> matching times after
		///<c>time</c>.
		///</summary>
		public static DateFilter After(string field, long time) 
		{
			DateFilter result = new DateFilter(field);
			result.Start = DateField.TimeToString(time);
			return result;
		}

		///<summary>
		///Returns a BitSet with true for documents which should be permitted in
		///search results, and false for those that should not.
		///</summary>
		public override BitArray Bits(IndexReader reader) 
		{
			BitArray bits = new BitArray(reader.MaxDoc());
			ITermEnum enm = reader.Terms(new Term(field, start));
			ITermDocs termDocs = reader.TermDocs();
			if (enm.Term() == null)
				return bits;

			try 
			{
				Term stop = new Term(field, end);
				while (enm.Term().CompareTo(stop) <= 0) 
				{
					termDocs.Seek(enm.Term());
					try 
					{
						while (termDocs.Next())
							bits[termDocs.Doc()] = true;
					} 
					finally 
					{
						termDocs.Close();
					}
					if (!enm.Next()) 
					{
						break;
					}
				}
			} 
			finally 
			{
				enm.Close();
			}
			return bits;
		}

		/// <summary>
		/// Returns a string representation of this DateFilter.
		/// </summary>
		public override string ToString() 
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append(field);
			buffer.Append(":");
			buffer.Append(DateField.StringToDate(start).ToString());
			buffer.Append("-");
			buffer.Append(DateField.StringToDate(end).ToString());
			return buffer.ToString();
		}
	}
}
