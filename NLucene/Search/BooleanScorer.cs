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

namespace DotnetPark.NLucene.Search
{
	internal class BooleanScorer : Scorer 
	{
		private int currentDoc;

		private SubScorer scorers = null;
		private BucketTable bucketTable;

		private int maxCoord = 1;
		private float[] coordFactors = null;

		private int requiredMask = 0;
		private int prohibitedMask = 0;
		private int nextMask = 1;

		public int MaxCoord
		{
			get { return maxCoord; }
			set { maxCoord = value; }
		}

		public float[] CoordFactors
		{
			get { return coordFactors; }
			set { coordFactors = value; }
		}

		public int RequiredMask
		{
			get { return requiredMask; }
			set { requiredMask = value; }
		}

		public int ProhibitedMask
		{
			get { return prohibitedMask; }
			set { prohibitedMask = value; }
		}

		public int NextMask
		{
			get { return nextMask; }
			set { nextMask = value; }
		}

		public BooleanScorer()
		{
			bucketTable = new BucketTable(this);
		}

		public class SubScorer 
		{
			public Scorer scorer;
			public bool required = false;
			public bool prohibited = false;
			public HitCollector collector;
			public SubScorer next;

			public Scorer Scorer
			{
				get { return scorer; }
				set { scorer = value; }
			}

			public bool Required
			{
				get { return required; }
				set { required = value; }
			}

			public bool Prohibited
			{
				get { return prohibited; }
				set { prohibited = value; }
			}

			public HitCollector Collector
			{
				get { return collector; }
				set { collector = value; }
			}

			public SubScorer Next
			{
				get { return next; }
				set { next = value; }
			}

			public SubScorer(Scorer scorer, bool required, bool prohibited,
				HitCollector collector, SubScorer next) 
			{
				this.scorer = scorer;
				this.required = required;
				this.prohibited = prohibited;
				this.collector = collector;
				this.next = next;
			}
		}

		public void Add(Scorer scorer, bool required, bool prohibited) 
		{
			int mask = 0;
			if (required || prohibited) 
			{
				if (nextMask == 0)
					throw new ArgumentOutOfRangeException("More than 32 required/prohibited clauses in query.");
				mask = nextMask;
				nextMask = nextMask << 1;
			} 
			else
				mask = 0;

			if (!prohibited)
				maxCoord++;

			if (prohibited)
				prohibitedMask |= mask;			  // update prohibited mask
			else if (required)
				requiredMask |= mask;			  // update required mask

			scorers = new SubScorer(scorer, required, prohibited,
				bucketTable.NewCollector(mask), scorers);
		}

		private void ComputeCoordFactors() 
		{
			coordFactors = new float[maxCoord];
			for (int i = 0; i < maxCoord; i++)
				coordFactors[i] = Similarity.Coord(i, maxCoord);
		}

		public override void Score(HitCollector results, int maxDoc) 
		{
			if (coordFactors == null)
				ComputeCoordFactors();

			while (currentDoc < maxDoc) 
			{
				currentDoc = Math.Min(currentDoc+BucketTable.SIZE, maxDoc);
				for (SubScorer t = scorers; t != null; t = t.Next)
					t.Scorer.Score(t.Collector, currentDoc);
				bucketTable.CollectHits(results);
			}
		}

		public class Bucket 
		{
			public int Doc = -1;				  // tells if bucket is valid
			public float Score;				  // incremental score
			public int Bits;					  // used for bool constraints
			public int Coord;					  // count of terms in score
			public Bucket Next;				  // next valid bucket
		}

		///<summary> A simple hash table of document scores within a range. </summary>
		public class BucketTable 
		{
			public static int SIZE = 1 << 10;
			public static int MASK = (1 << 10) - 1;

			private Bucket[] buckets = new Bucket[SIZE];
			private Bucket first = null;			  // head of valid list
  
			private BooleanScorer scorer;

			public Bucket[] Buckets
			{
				get { return buckets; }
				set { buckets = value; }
			}

			public Bucket First
			{
				get { return first; }
				set { first = value; }
			}

			public BucketTable(BooleanScorer scorer) 
			{
				this.scorer = scorer;
			}

			public void CollectHits(HitCollector results) 
			{
				int required = scorer.RequiredMask;
				int prohibited = scorer.ProhibitedMask;
				float[] coord = scorer.CoordFactors;

				for (Bucket bucket = first; bucket!=null; bucket = bucket.Next) 
				{
					if ((bucket.Bits & prohibited) == 0 &&	  // check prohibited
						(bucket.Bits & required) == required)
					{// check required
						results.Collect(bucket.Doc,		  // add to results
							bucket.Score * coord[bucket.Coord]);
					}
				}
				first = null;				  // reset for next round
			}

			public int Size
			{
				get { return SIZE; }
			}

			public HitCollector NewCollector(int mask) 
			{
				return new Collector(mask, this);
			}
		}

		public class Collector : HitCollector 
		{
			private BucketTable bucketTable;
			private int mask;
			public Collector(int mask, BucketTable bucketTable) 
			{
				this.mask = mask;
				this.bucketTable = bucketTable;
			}
			public override void Collect(int doc, float score) 
			{
				BucketTable table = bucketTable;
				int i = doc & BucketTable.MASK;
				Bucket bucket = table.Buckets[i];
				if (bucket == null)
					table.Buckets[i] = bucket = new Bucket();
      
				if (bucket.Doc != doc) 
				{			  // invalid bucket
					bucket.Doc = doc;			  // set doc
					bucket.Score = score;			  // initialize score
					bucket.Bits = mask;			  // initialize mask
					bucket.Coord = 1;			  // initialize coord
	
					bucket.Next = table.First;		  // push onto valid list
					table.First = bucket;
				} 
				else 
				{					  // valid bucket
					bucket.Score += score;			  // increment score
					bucket.Bits |= mask;			  // add bits in mask
					bucket.Coord++;				  // increment coord
				}
			}
		}
	}
}
