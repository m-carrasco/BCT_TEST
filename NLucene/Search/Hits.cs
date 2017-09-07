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
using System.Collections.Generic;

using DotnetPark.NLucene.Documents;

namespace DotnetPark.NLucene.Search
{
	///<summary>
	///A ranked list of documents, used to hold search results.
	///</summary>
	public class Hits : IEnumerable<Hit>
	{
		private Query query;
		private Searcher searcher;
		private Filter filter = null;

		private int length;				  // the total number of hits
		private List<Hit> hitDocs = new List<Hit>();	  // cache of hits retrieved

		private Hit first;				  // head of LRU cache
		private Hit last;				  // tail of LRU cache
		private int numDocs = 0;			  // number cached
		private int maxDocs = 200;			  // max to cache

		internal Hits(Searcher s, Query q, Filter f) 
		{
			query = q;
			searcher = s;
			filter = f;
			GetMoreDocs(50);				  // retrieve 100 initially
		}

		///<summary>
		/// Returns the total number of hits available in this set.
		/// </summary>
		public int Length 
		{
			get { return length; }
		}

		/// <summary>
		/// Gets a hit with the specified index.
		/// </summary>
		public Hit this[int i]
		{
			get { return GetHit(i); }
		}

		/// <summary>
		/// A simple enumerator for hits.
		/// </summary>
		public class HitsEnumerator : IEnumerator<Hit>
		{
			private Hits hits;
			private int idx = -1;

			/// <summary>
			/// Creates an enumerator.
			/// </summary>
			public HitsEnumerator(Hits hits)
			{
				this.hits = hits;
			}

			/// <summary>
			/// Gets the current object.
			/// </summary>
			public Hit Current
			{
				get
				{
					return hits[idx];
				}
			}

			/// <summary>
			/// Moves to the next object.
			/// </summary>
			public bool MoveNext()
			{
				idx++;
				return (idx < hits.Length);
			}

			/// <summary>
			/// Resets the state of the enumerator.
			/// </summary>
			public void Reset()
			{
				idx = -1;
			}

			/// <summary>
			/// Dispose of the enumerator
			/// </summary>
			public void Dispose()
			{ ;}

			object System.Collections.IEnumerator.Current
			{
				get { throw new NotImplementedException("Use the Generic One Instead!!"); }
			}
		}

		/// <summary>
		/// Returns an enumerator that allows to iterate through hits collection.
		/// </summary>
		/// <returns>An enumerator that allows to iterate through hits collection.</returns>
		public IEnumerator<Hit> GetEnumerator()
		{
			return new HitsEnumerator(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException("Use the Generic One Instead!!");
		}

		// Tries to add new documents to hitDocs.
		// Ensures that the hit numbered <code>min</code> has been retrieved.
		private void GetMoreDocs(int min)
		{
			if (hitDocs.Count> min)
				min = hitDocs.Count;

			int n = min * 2;				  // double # retrieved
			TopDocs topDocs = searcher.Search(query, filter, n);
			length = topDocs.TotalHits;
			ScoreDoc[] scoreDocs = topDocs.ScoreDocs;

			float scoreNorm = 1.0f;
			if (length > 0 && scoreDocs[0].Score > 1.0f)
				scoreNorm = 1.0f / scoreDocs[0].Score;

			int end = scoreDocs.Length < length ? scoreDocs.Length : length;
			for (int i = hitDocs.Count; i < end; i++)
				hitDocs.Add(new Hit(scoreDocs[i].Score*scoreNorm,
					scoreDocs[i].Doc));
		}

		/*private Document Doc(int n)
		{
			Hit hitDoc = GetHit(n);

			return hitDoc.Doc;
		}*/

		///<summary> Returns the score for the nth document in this set. </summary> 
		/*private float Score(int n) 
		{
			return GetHit(n).Score;
		}*/

		private Hit GetHit(int n) 
		{
			if (n >= length)
				throw new ArgumentOutOfRangeException("Not a valid hit number: " + n);
			if (n >= hitDocs.Count)
				GetMoreDocs(n);

			Hit hitDoc = (Hit)hitDocs[n];

			// Update LRU cache of documents
			Remove(hitDoc);				  // remove from list, if there
			AddToFront(hitDoc);				  // add to front of list
			if (numDocs > maxDocs) 
			{			  // if cache is full
				Hit oldLast = last;
				Remove(last);				  // flush last
				oldLast.Document = null;	  // let doc get gc'd
			}

			if (hitDoc.Document == null)
				hitDoc.Document = searcher.Doc(hitDoc.Id);	  // cache miss: read document

			return hitDoc;
		}

		private void AddToFront(Hit hitDoc) 
		{  // insert at front of cache
			if (first == null)
				last = hitDoc;
			else
				first.Prev = hitDoc;
    
			hitDoc.Next = first;
			first = hitDoc;
			hitDoc.Prev = null;

			numDocs++;
		}

		private void Remove(Hit hitDoc) 
		{	  // remove from cache
			if (hitDoc.Document == null)			  // it's not in the list
				return;					  // abort

			if (hitDoc.Next == null)
				last = hitDoc.Prev;
			else
				hitDoc.Next.Prev = hitDoc.Prev;
    
			if (hitDoc.Prev == null)
				first = hitDoc.Next;
			else
				hitDoc.Prev.Next = hitDoc.Next;

			numDocs--;
		}
	}
}
