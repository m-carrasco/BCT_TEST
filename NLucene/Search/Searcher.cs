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

using DotnetPark.NLucene.Index;
using DotnetPark.NLucene.Documents;

namespace DotnetPark.NLucene.Search
{
	///<summary>
	///The abstract base class for search implementations.
	///<p>Subclasses implement search over a single index, over multiple indices,
	///and over indices on remote servers.</p>
	///</summary>
	public abstract class Searcher 
	{

		///<summary> Returns the documents matching <c>query</c>.</summary>
		public Hits Search(Query query) 
		{
			return Search(query, (Filter)null);
		}

		///<summary>
		///Returns the documents matching <c>query</c> and
		///<c>filter</c>.
		///</summary>
		public Hits Search(Query query, Filter filter) 
		{
			return new Hits(this, query, filter);
		}

		///<summary> Lower-level search API.
		///
		/// <p>HitCollector.Collect(int,float) is called for every non-zero
		/// scoring document.</p>
		///
		/// <p>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API (
		/// Searcher.Search(Query)) is usually more efficient, as it skips
		/// non-high-scoring hits.</p>
		/// </summary>
		public void Search(Query query, HitCollector results)
		{
			Search(query, (Filter)null, results);
		}    

		///<summary> Lower-level search API.
		///
		/// <p>HitCollector.Collect(int,float) is called for every non-zero
		/// scoring document.</p>
		///
		/// <p>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API (
		/// Searcher.Search(Query)) is usually more efficient, as it skips
		/// non-high-scoring hits.</p>
		///</summary>
		///<param name="query">To match documents.</param>
		///<param name="filter">If non-null, a bitset used to eliminate some documents.</param>
		///<param name="results">To receive hits.</param>
		public abstract void Search(Query query, Filter filter, HitCollector results);

		///<summary> Frees resources associated with this Searcher. </summary>
		public abstract void Close();

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public abstract int DocFreq(Term term);

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public abstract int MaxDoc();

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public abstract TopDocs Search(Query query, Filter filter, int n);

		///<summary> For use by HitCollector implementations. </summary>
		public abstract Document Doc(int i);
	}
}
