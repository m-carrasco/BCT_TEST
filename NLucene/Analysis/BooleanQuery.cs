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

using DotnetPark.NLucene.Index;

namespace DotnetPark.NLucene.Search
{
	///<summary>
	///A Query that matches documents matching bool combinations of other
	///queries, typically TermQueries or PhraseQueries.
	///</summary>
	public class BooleanQuery : Query 
	{
		private List<BooleanClause> clauses = new List<BooleanClause>();

		///<summary> Constructs an empty bool query. </summary>
		public BooleanQuery() {}

		///<summary>
		///Adds a clause to a bool query.  Clauses may be:
		///<ul>
		///<li><c>required</c> which means that documents which <i>do not</i>
		///match this sub-query will <i>not</i> match the bool query;</li>
		///<li><c>prohibited</c> which means that documents which <i>do</i>
		///match this sub-query will <i>not</i> match the bool query; or</li>
		///<li>neither, in which case matched documents are neither prohibited from
		///nor required to match the sub-query.</li>
		///</ul>
		///It is an error to specify a clause as both <c>required</c> and
		///<c>prohibited</c>.
		///</summary>
		public void Add(Query query, bool required, bool prohibited) 
		{
			clauses.Add(new BooleanClause(query, required, prohibited));
		}

		///<summary> Adds a clause to a bool query. </summary>
		public void Add(BooleanClause clause) 
		{
			clauses.Add(clause);
		}

		/// <summary>
		/// Prepares a boolean query.
		/// </summary>
		public override void Prepare(IndexReader reader) 
		{
			for (int i = 0 ; i < clauses.Count; i++) 
			{
				BooleanClause c = (BooleanClause)clauses[i];
				c.Query.Prepare(reader);
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public override float SumOfSquaredWeights(Searcher searcher)	
		{
			float sum = 0.0f;

			for (int i = 0 ; i < clauses.Count; i++) 
			{
				BooleanClause c = (BooleanClause)clauses[i];
				if (!c.Prohibited)
					sum += c.Query.SumOfSquaredWeights(searcher); // sum sub-query weights
			}

			return sum;
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public override void Normalize(float norm) 
		{
			for (int i = 0 ; i < clauses.Count; i++) 
			{
				BooleanClause c = (BooleanClause)clauses[i];
				if (!c.Prohibited)
					c.Query.Normalize(norm);
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public override Scorer Scorer(IndexReader reader)
		{

			if (clauses.Count == 1) 
			{			  // optimize 1-term queries
				BooleanClause c = (BooleanClause)clauses[0];
				if (!c.Prohibited)			  // just return term scorer
					return c.Query.Scorer(reader);
			}

			BooleanScorer result = new BooleanScorer();

			for (int i = 0 ; i < clauses.Count; i++) 
			{
				BooleanClause c = (BooleanClause)clauses[i];
				Scorer subScorer = c.Query.Scorer(reader);
				if (subScorer != null)
					result.Add(subScorer, c.Required, c.Prohibited);
				else if (c.Required)
					return null;
			}

			return result;
		}

		///<summary> Prints a user-readable version of this query. </summary>
		public override String ToString(String field) 
		{
			StringBuilder buffer = new StringBuilder();
			for (int i = 0 ; i < clauses.Count; i++) 
			{
				BooleanClause c = (BooleanClause)clauses[i];
				if (c.Prohibited)
					buffer.Append("-");
				else if (c.Required)
					buffer.Append("+");

				Query subQuery = c.Query;
				if (subQuery is BooleanQuery) 
				{	  // wrap sub-bools in parens
					BooleanQuery bq = (BooleanQuery)subQuery;
					buffer.Append("(");
					buffer.Append(c.Query.ToString(field));
					buffer.Append(")");
				} 
				else
					buffer.Append(c.Query.ToString(field));

				if (i != clauses.Count-1)
					buffer.Append(" ");
			}
			return buffer.ToString();
		}
	}
}
