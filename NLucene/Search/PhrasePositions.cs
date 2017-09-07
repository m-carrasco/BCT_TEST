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

namespace DotnetPark.NLucene.Search
{
	internal class PhrasePositions 
	{
		private int doc;					  // current doc
		private int position;					  // position in doc
		private int count;					  // remaining pos in this doc
		private int offset;					  // position in phrase
		private ITermPositions tp;				  // stream of positions
		private PhrasePositions next;				  // used to make lists

		public int Doc
		{
			get { return doc; }
			set { doc = value; }
		}

		public int Position
		{
			get { return position; }
			set { position = value; }
		}

		public int Count
		{
			get { return count; }
			set { count = value; }
		}

		public int Offset
		{
			get { return offset; }
			set { offset = value; }
		}

		public ITermPositions Tp
		{
			get { return tp; }
			set { tp = value; }
		}

		public PhrasePositions Next
		{
			get { return next; }
			set { next = value; }
		}

		internal PhrasePositions(ITermPositions t, int o) 
		{
			tp = t;
			offset = o;
			GetNext();
		}

		internal void GetNext() 
		{	  // increments to next doc
			if (!tp.Next()) 
			{
				tp.Close();				  // close stream
				doc = Int32.MaxValue;			  // sentinel value
				return;
			}
			doc = tp.Doc();
			position = 0;
		}

		public void FirstPosition()
		{
			count = tp.Freq();				  // read first pos
			NextPosition();
		}

		public bool NextPosition() 
		{
			if (count-- > 0) 
			{				  // read subsequent pos's
				position = tp.NextPosition() - offset;
				return true;
			} 
			else
				return false;
		}
	}
}
