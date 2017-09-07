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

using DotnetPark.NLucene.Util;

namespace DotnetPark.NLucene.Documents
{
	///<summary> Provides support for converting dates to strings and vice-versa.  The
	/// strings are structured so that lexicographic sorting orders by date.  This
	/// makes them suitable for use as field values and search terms.  </summary>
	public class DateField 
	{
		private DateField() {}

		// make date strings long enough to last a millenium
		private static int DATE_LEN
		{
			get
			{
				return Number.ToString(1000L*365*24*60*60*1000, Number.MAX_RADIX).Length;
			}
		}

		/// <summary>
		/// The minimal date string.
		/// </summary>
		public static string MIN_DATE_STRING 
		{
			get { return TimeToString(0); }
		}

		/// <summary>
		/// The maximum date string.
		/// </summary>
		public static string MAX_DATE_STRING
		{
			get
			{
				char[] buffer = new char[DATE_LEN];
				char c = 'F';// Convert.ToChar( Character.forDigit(Character.MAX_RADIX-1, Character.MAX_RADIX);
				for (int i = 0 ; i < DATE_LEN; i++)
					buffer[i] = c;
				return new string(buffer);
			}
		}
  
		///<summary> Converts a Date to a string suitable for indexing. </summary>
		public static string DateToString(DateTime date) 
		{
			TimeSpan ts = date.Subtract(new DateTime(1970, 1, 1));
			long ms = ts.Ticks / 10000;
			return TimeToString(ms);
		}
		///<summary> Converts a millisecond time to a string suitable for indexing. </summary>
		internal static string TimeToString(long time)
		{
			if (time < 0)
				throw new Exception("time too early");

			string s = Number.ToString(time, Number.MAX_RADIX);// Long.toString(time, Character.MAX_RADIX);

			if (s.Length > DATE_LEN)
				throw new Exception("time too late");

			while (s.Length < DATE_LEN)
				s = "0" + s;				  // pad with leading zeros

			return s;
		}

		///<summary> Converts a string-encoded date into a millisecond time. </summary>
		internal static long StringToTime(string value) 
		{
			return Number.Parse(value, Number.MAX_RADIX);
		}
		///<summary> Converts a string-encoded date into a Date object. </summary>
		public static DateTime StringToDate(string value) 
		{
			long ticks = StringToTime(value) * 10000;
			return (new DateTime(ticks)).AddYears(1969);
		}
	}
}
