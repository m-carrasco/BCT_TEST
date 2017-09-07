using System;
using System.Collections.Generic;
using System.Text;
using DotnetPark.NLucene.Index;
using System.IO;
using DotnetPark.NLucene.Store;
using DotnetPark.NLucene.Analysis.Standard;
using DotnetPark.NLucene.Search;
using DotnetPark.NLucene.Analysis;
using NLuceneClient;

namespace NLucene
{
	/// <summary>
	/// Main class to drive search and indexing.
	/// </summary>
	public class Lusearch
	{
		private static readonly string wordsFolder = @".\input\words\";
		private static readonly string indexFolder = @".\input\index\";
		
		/// <summary>
		/// Perform indexing on a file or directory.
		/// </summary>
		public static void PerformIndexing(string fileName)
		{
			DateTime start = DateTime.Now;

			IndexWriter writer = new IndexWriter(indexFolder, new StandardAnalyzer(), true);
			writer.mergeFactor = 20;

			IndexDoc(writer, fileName);

			writer.Optimize();
			writer.Close();

			DateTime end = DateTime.Now;
		}

		private static void IndexDoc(IndexWriter writer, string path)
		{
			if(File.Exists(path))
			{
				// This path is a file
				ProcessFile(writer, new FileInfo(path));
			}
			else if(System.IO.Directory.Exists(path))
			{
				// This path is a directory
				ProcessDirectory(writer, path);
			}
			else
			{
				throw new Exception("'" + path + "' is not a valid file or directory.");
			}
		}

		// Process all files in the directory passed in, and recurse on any directories 
		// that are found to process the files they contain
		private static void ProcessDirectory(IndexWriter writer, string targetDirectory)
		{
			// Process the list of files found in the directory
			string[] fileEntries = System.IO.Directory.GetFiles(targetDirectory, "*.txt");
			foreach(string fileName in fileEntries)
				ProcessFile(writer, new FileInfo(fileName));

			// Recurse into subdirectories of this directory
			string[] subdirectoryEntries = System.IO.Directory.GetDirectories(targetDirectory);
			foreach(string subdirectory in subdirectoryEntries)
				ProcessDirectory(writer, subdirectory);
		}

		// Real logic for processing found files would go here.
		private static void ProcessFile(IndexWriter writer, FileInfo file)
		{
			writer.AddDocument(FileDocument.Document(file));
		}

        /// <summary>
        /// Run the tests
        /// </summary>
        /// Unstacker.cs  (cci)  Contract.Assume(stackAfterTrue.Count == this.locals.Count); fails line 228 
        public static void Main(string[] args)
		{
			PerformIndexing(wordsFolder);
		}
	}
}
