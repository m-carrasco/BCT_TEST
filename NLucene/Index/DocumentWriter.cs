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

using DotnetPark.NLucene.Analysis;
using DotnetPark.NLucene.Documents;
using DotnetPark.NLucene.Search;
using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Util;

namespace DotnetPark.NLucene.Index
{
	/// <summary>
	/// 
	/// </summary>
	internal class DocumentWriter
	{
		private Analyzer analyzer;
		private Directory directory;
		private FieldInfos fieldInfos;
		private int maxFieldLength;

		public DocumentWriter(Directory d, Analyzer a, int mfl)
		{
			directory = d;
			analyzer = a;
			maxFieldLength = mfl;
		}

		public void AddDocument(String segment, Document doc)
		{
			// write field names
			fieldInfos = new FieldInfos();
			fieldInfos.Add(doc);
			fieldInfos.Write(directory, segment + ".fnm");

			// write field values
			FieldsWriter fieldsWriter =
				new FieldsWriter(directory, segment, fieldInfos);
			try
			{
				fieldsWriter.AddDocument(doc);
			}
			finally
			{
				fieldsWriter.Close();
			}

			// invert doc into postingTable
			postingTable.Clear();			  // clear postingTable
			fieldLengths = new int[fieldInfos.Size()];	  // init fieldLengths
			InvertDocument(doc);

			// sort postingTable into an array
			Posting[] postings = SortPostingTable();

			/*
			for (int i = 0; i < postings.length; i++) {
			  Posting posting = postings[i];
			  System.out.print(posting.term);
			  System.out.print(" freq=" + posting.freq);
			  System.out.print(" pos=");
			  System.out.print(posting.positions[0]);
			  for (int j = 1; j < posting.freq; j++)
			System.out.print("," + posting.positions[j]);
			  System.out.println("");
			}
			*/

			// write postings
			WritePostings(postings, segment);

			// write norms of indexed fields
			WriteNorms(doc, segment);

		}

		// Keys are Terms, values are Postings.
		// Used to buffer a document before it is written to the index.
		private Dictionary<Term, Posting> postingTable = new Dictionary<Term, Posting>();
		private int[] fieldLengths;

		// Tokenizes the fields of a document into Postings.
		private void InvertDocument(Document doc)
		{
			foreach(Field field in doc.Fields)
			{
				String fieldName = field.Name;
				int fieldNumber = fieldInfos.FieldNumber(fieldName);

				int position = fieldLengths[fieldNumber];	  // position in field

				if(field.IsIndexed)
				{
					if(!field.IsTokenized)
					{		  // un-tokenized field
						AddPosition(fieldName, field.Value, position++);
					}
					else
					{
						System.IO.TextReader reader;			  // find or make Reader
						if(field.ReaderValue != null)
							reader = field.ReaderValue;
						else if(field.Value != null)
							reader = new System.IO.StringReader(field.Value);
						else
							throw new ArgumentException("field must have either String or Reader value");

						// Tokenize field and add to postingTable
						TokenStream stream = analyzer.TokenStream(fieldName, reader);
						try
						{
							for(Token t = stream.Next(); t != null; t = stream.Next())
							{
								AddPosition(fieldName, t.TermText, position++);
								if(position > maxFieldLength) break;
							}
						}
						finally
						{
							stream.Close();
						}
					}

					fieldLengths[fieldNumber] = position;	  // save field length
				}
			}
		}

		private Term termBuffer = new Term("", ""); // avoid consing

		private void AddPosition(String field, String text, int position)
		{
			termBuffer.Set(field, text);
			
			if(postingTable.ContainsKey(termBuffer))
			{	
				// word seen before
				Posting ti = postingTable[termBuffer];
				int freq = ti.Freq;
				if(ti.Positions.Length == freq)
				{	  // positions array is full
					int[] newPositions = new int[freq * 2];	  // double size
					int[] positions = ti.Positions;
					for(int i = 0; i < freq; i++)		  // copy old positions to new
						newPositions[i] = positions[i];
					ti.Positions = newPositions;
				}
				ti.Positions[freq] = position;		  // add new position
				ti.Freq = freq + 1;			  // update frequency
			}
			else
			{					  // word not seen before
				Term term = new Term(field, text, false);
				postingTable[term] = new Posting(term, position);
			}
		}

		private Posting[] SortPostingTable()
		{
			// copy postingTable into an array
			Posting[] array = new Posting[postingTable.Count];

			int i = 0;
			foreach(Posting posting in postingTable.Values)
			{
				array[i] = posting;
				i++;
			}

			// sort the array
			QuickSort(array, 0, array.Length - 1);

			return array;
		}

		static private void QuickSort(Posting[] postings, int lo, int hi)
		{
			if(lo >= hi)
				return;

			int mid = (lo + hi) / 2;

			if(postings[lo].Term.CompareTo(postings[mid].Term) > 0)
			{
				Posting tmp = postings[lo];
				postings[lo] = postings[mid];
				postings[mid] = tmp;
			}

			if(postings[mid].Term.CompareTo(postings[hi].Term) > 0)
			{
				Posting tmp = postings[mid];
				postings[mid] = postings[hi];
				postings[hi] = tmp;

				if(postings[lo].Term.CompareTo(postings[mid].Term) > 0)
				{
					Posting tmp2 = postings[lo];
					postings[lo] = postings[mid];
					postings[mid] = tmp2;
				}
			}

			int left = lo + 1;
			int right = hi - 1;

			if(left >= right)
				return;

			Term partition = postings[mid].Term;

			for(; ; )
			{
				while(postings[right].Term.CompareTo(partition) > 0)
					--right;

				while(left < right && postings[left].Term.CompareTo(partition) <= 0)
					++left;

				if(left < right)
				{
					Posting tmp = postings[left];
					postings[left] = postings[right];
					postings[right] = tmp;
					--right;
				}
				else
				{
					break;
				}
			}

			QuickSort(postings, lo, left);
			QuickSort(postings, left + 1, hi);
		}

		private void WritePostings(Posting[] postings, String segment)
		{
			OutputStream freq = null, prox = null;
			TermInfosWriter tis = null;

			try
			{
				freq = directory.CreateFile(segment + ".frq");
				prox = directory.CreateFile(segment + ".prx");
				tis = new TermInfosWriter(directory, segment, fieldInfos);
				TermInfo ti = new TermInfo();

				for(int i = 0; i < postings.Length; i++)
				{
					Posting posting = postings[i];

					// add an entry to the dictionary with pointers to prox and freq files
					ti.Set(1, freq.GetFilePointer(), prox.GetFilePointer());
					tis.Add(posting.Term, ti);

					// add an entry to the freq file
					int f = posting.Freq;
					if(f == 1)				  // optimize freq=1
						freq.WriteVInt(1);			  // set low bit of doc num.
					else
					{
						freq.WriteVInt(0);			  // the document number
						freq.WriteVInt(f);			  // frequency in doc
					}

					int lastPosition = 0;			  // write positions
					int[] positions = posting.Positions;
					for(int j = 0; j < f; j++)
					{		  // use delta-encoding
						int position = positions[j];
						prox.WriteVInt(position - lastPosition);
						lastPosition = position;
					}
				}
			}
			finally
			{
				if(freq != null) freq.Close();
				if(prox != null) prox.Close();
				if(tis != null) tis.Close();
			}
		}

		private void WriteNorms(Document doc, String segment)
		{
			foreach(Field field in doc.Fields)
			{
				if(field.IsIndexed)
				{
					int fieldNumber = fieldInfos.FieldNumber(field.Name);
					OutputStream norm = directory.CreateFile(segment + ".f" + fieldNumber);
					try
					{
						norm.WriteByte(Similarity.Norm(fieldLengths[fieldNumber]));
					}
					finally
					{
						norm.Close();
					}
				}
			}
		}
	}
}
