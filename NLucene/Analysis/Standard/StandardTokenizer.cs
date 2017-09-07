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
using System.IO;

using DotnetPark.NLucene.Analysis;

namespace DotnetPark.NLucene.Analysis.Standard
{
	/// <summary>
	/// Summary description for StandardTokenizer.
	/// </summary>
	public class StandardTokenizer : Tokenizer
	{
		FastCharStream rd;

		int start = 1;

		/// <summary>
		/// Initializes a new instance of the StandardTokenizer class.
		/// </summary>
		/// <param name="textReader">TextReader instance to read from.</param>
		public StandardTokenizer(TextReader textReader)
		{
			rd = new FastCharStream(textReader);
		}

		/// <summary>
		/// Close the current tokenizer.
		/// </summary>
		public override void Close()
		{
			// does nothing
		}

		/// <summary>
		/// Returns the next token.
		/// </summary>
		/// <returns>The next token.</returns>
		public override Token Next()
		{
			while(!rd.Eos())
			{
				char ch = rd.GetNext();
				
				// For skipping whitespaces
				if(Char.IsWhiteSpace(ch))
				{
					continue;
				}

				// Read for Alpha-Nums
				if(Char.IsLetterOrDigit(ch))
				{
					start = rd.Column;
					return ReadAlphaNum(ch);
				}

				//throw new Exception("Error in input stream!");
				continue;
			}

			return null;
		}

		/// <summary>
		/// Reads for an ALPHA NUM value.
		/// </summary>
		/// <param name="ch">The next char to read.</param>
		/// <returns>A token containing ALPHA NUM value.</returns>
		private Token ReadAlphaNum(char ch)
		{
			maybeAcronym = true;
			maybeHost = true;
			maybeNumber = true;

			string str = ch.ToString();

			while(!rd.Eos() && !Char.IsWhiteSpace(ch))
			{
				ch = rd.GetNext();

				if(Char.IsLetterOrDigit(ch))
				{
					if(Char.IsDigit(ch))
						prevHasDigit = true;

					str += ch.ToString();
				}

				switch(ch)
				{
					case '\'':
							return ReadApostrophe(str, ch);
					case '.':
							return ReadNumber(str, ch);
					case '&':
							return ReadCompany(str, ch);
					case '@':
							return ReadAt(str, ch);
					case '-':
					case ',':
					case '_':
					case '/':
							return ReadNumber(str, ch);
					default:
						break;
				}
			}

			return new Token(str, start, rd.Column, TokenTypes.ALPHANUM);
		}

		/// <summary>
		/// Reads for apostrophe.
		/// </summary>
		/// <param name="str">Previous grabbed string.</param>
		/// <param name="ch">The next char.</param>
		/// <returns>A token containing apostrophe value.</returns>
		private Token ReadApostrophe(string str, char ch)
		{
			str += ch;

			while(!Char.IsWhiteSpace(rd.Peek()) && Char.IsLetterOrDigit(rd.Peek()) || rd.Peek() == '\'')
			{
				str += rd.GetNext();
			}

			if(rd.Peek() == '\'')
				str.Substring(0, str.Length-1);
			return new Token(str, start, rd.Column, TokenTypes.APOSTROPHE);
		}

		/// <summary>
		/// Reads for something@... 
		/// it may be a COMPANY name or a EMAIL address
		/// </summary>
		/// <param name="str">Previous grabbed string.</param>
		/// <param name="ch">The next char.</param>
		/// <returns>A token containing company name or email address.</returns>
		private Token ReadAt(string str, char ch)
		{
			string val = "";
			bool append = true;

			while(!rd.Eos() && !Char.IsWhiteSpace(ch))
			{
				ch = rd.GetNext();

				if(Char.IsLetterOrDigit(ch) && append)
					val += ch.ToString();
				else if(ch == '.' && val.Length > 0)
					return ReadEmail(str + "@" + val, ch);
				else
					append = false;
			}

			if(val.Length > 0)
				return new Token(str + "@" + val, start, rd.Column, TokenTypes.COMPANY);
			else
				return new Token(str, start, rd.Column, TokenTypes.ALPHANUM);
		}

		/// <summary>
		/// Reads for COMPANYs in format some&amp;home, at&amp;t.
		/// </summary>
		/// <param name="str">Previous grabbed string.</param>
		/// <param name="ch">The next char.</param>
		/// <returns>A token containing company name.</returns>
		private Token ReadCompany(string str, char ch)
		{
			bool append = true;
			string val = "";

			while(!rd.Eos() && !Char.IsWhiteSpace(ch))
			{
				ch = rd.GetNext();

				if(Char.IsLetterOrDigit(ch) && append)
					val += ch.ToString();
				else
					append = false;
			}

			if(val.Length > 0)
				return new Token(str + "&" + val, start, rd.Column, TokenTypes.COMPANY);
			else
				return new Token(str, start, rd.Column, TokenTypes.ALPHANUM);
		}

		/// <summary>
		/// Reads for EMAILs somebody@somewhere.else.com.
		/// </summary>
		/// <param name="str">Previous grabbed string.</param>
		/// <param name="ch">The next char.</param>
		/// <returns>A token containing email address.</returns>
		private Token ReadEmail(string str, char ch)
		{
			str += ch;

			while(!Char.IsWhiteSpace(rd.Peek()) && Char.IsLetterOrDigit(rd.Peek()) || rd.Peek() == '.')
			{
				str += rd.GetNext();
			}

			if(rd.Peek() == '.')
				str.Substring(0, str.Length-1);
			return new Token(str, start, rd.Column, TokenTypes.EMAIL);
		}

		private bool maybeAcronym = false;
		private bool maybeHost = false;
		private bool maybeNumber = false;
		private bool prevHasDigit = false;

		/// <summary>
		/// Reads for some.
		/// It may be a NUMBER like 12.3, an ACRONYM like U.S.A., or a HOST www.som.com.
		/// </summary>
		/// <param name="str">Previous grabbed string.</param>
		/// <param name="ch">The next char.</param>
		/// <returns>A token containing a number, acronym or a host address.</returns>
		private Token ReadNumber(string str, char ch)
		{
			str += ch.ToString();
			string val = "";
			bool append = true;
			bool hasDigit = false;

			if(ch != '.')
			{
				maybeHost = false;
				maybeAcronym = false;
			}

			while(!rd.Eos() && !Char.IsWhiteSpace(ch))
			{
				ch = rd.GetNext();

				if(Char.IsLetter(ch) && append)
				{
					val += ch.ToString();
				}
				else if(Char.IsDigit(ch) && append)
				{
					// acronyms can't contain numbers
					maybeAcronym = false;

					// check for number
					hasDigit = true;

					val += ch.ToString();
				}
				else if(ch == '_' || ch == '-' || ch == '/' || ch == ',' || ch == '.' && append)
				{
					if(ch != '.')
					{
						maybeHost = false;
						maybeAcronym = false;
					}
					if(val.Length == 0)
					{
						append = false;
						continue;
					} 
					else 
					{
						maybeNumber = (prevHasDigit != hasDigit);
						prevHasDigit = hasDigit;
						return ReadNumber(str + val, ch);
					}
				}
				else
					append = false;
			} // end of while

			if(maybeAcronym && val.Length == 0)
				return new Token(str + val, start, rd.Column, TokenTypes.ACRONYM);
			else if(maybeHost)
				return new Token(str + val, start, rd.Column, TokenTypes.HOST);
			else if(maybeNumber)
				return new Token(str + val, start, rd.Column, TokenTypes.NUM);
			else
			{
				char[] idxcheck = new char[5];
				idxcheck[0] = '_';
				idxcheck[1] = '-';
				idxcheck[2] = '.';
				idxcheck[3] = '/';
				idxcheck[4] = ',';

				int idx = str.IndexOfAny(idxcheck);  //str.IndexOfAny(new char[] { '_', '-', '.', '/', ',' });

				if(idx != -1)
					str = str.Substring(0, idx);
				return new Token(str, start, rd.Column, TokenTypes.ALPHANUM);
			}
		}
	}
}
