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

namespace DotnetPark.NLucene.Analysis
{
	/// <summary>
	/// <p>An Analyzer builds TokenStreams, which analyze text.
	/// It thus represents a policy for extracting index terms from text.</p>
	///
	/// <p>Typical implementations first build a Tokenizer, which breaks 
	/// the stream of characters from the Reader into raw Tokens.
	/// One or more TokenFilters may then be applied to the output of the Tokenizer.</p> 
	///
	///	<p><b>WARNING:</b> You must override one of the methods defined by this class
	///	in your subclass or the Analyzer will enter an infinite loop.</p>
	///
	/// </summary>
	abstract public class Analyzer 
	{
		///<summary>
		/// Creates a TokenStream which tokenizes all the text in the provided
		/// Reader.  Default implementation forwards to tokenStream(Reader) for 
		/// compatibility with older version.  Override to allow Analyzer to choose 
		/// strategy based on document and/or field.  Must be able to handle null
		/// field name for backward compatibility.
		/// </summary>
		public virtual TokenStream TokenStream(string fieldName, TextReader reader)
		{
			// implemented for backward compatibility
			return TokenStream(reader);
		}
  
		///<summary>
		/// Creates a TokenStream which tokenizes all the text in the provided
		/// Reader.  Provided for backward compatibility only.
		///</summary>
		public virtual TokenStream TokenStream(TextReader reader)
		{
			return TokenStream(null, reader);
		}
	}
}
