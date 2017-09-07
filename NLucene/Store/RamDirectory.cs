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
using System.Collections.Generic;

namespace DotnetPark.NLucene.Store
{
	/// <summary>
	/// Straightforward implementation of Directory as a directory of files that resides in the RAM.
	/// </summary>
	internal class RamDirectory : Directory 
	{
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		protected Dictionary<string, RamFile> files = new Dictionary<string, RamFile>();

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		protected class RamLock : Lock
		{
			private Directory parent;
			private Dictionary<string, RamFile> files;
			private string name;

			/// <summary>
			/// [To be supplied.]
			/// </summary>
			/// <param name="parent">[To be supplied.]</param>
			/// <param name="files">[To be supplied.]</param>
			/// <param name="name">[To be supplied.]</param>
			public RamLock(Directory parent, Dictionary<string, RamFile> files, string name)
			{
				this.parent = parent;
				this.files = files;
				this.name = name;
			}

			/// <summary>
			/// [To be supplied.]
			/// </summary>
			/// <returns>[To be supplied.]</returns>
			public override bool Obtain() 
			{
				lock (files) 
				{
					if (!parent.FileExists(name)) 
					{
						parent.CreateFile(name).Close();
						return true;
					}
					return false;
				}
			}
		
			/// <summary>
			/// [To be supplied.]
			/// </summary>
			public override void Release() 
			{
				parent.DeleteFile(name);
			}
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		public RamDirectory()
		{
		}

		///<summary> Returns an array of strings, one for each file in the directory. </summary>
		public override string[] List() 
		{
			string[] result = new string[files.Count];
			int i = 0;
			IEnumerable<string> names = files.Keys;
			foreach(string name in names)
			{
				result[i++] = name;
			}
			return result;
		}
       
		///<summary> Returns true iff the named file exists in this directory. </summary>
		public override bool FileExists(string name) 
		{
			return files.ContainsKey(name);
		}

		///<summary> Returns the time the named file was last modified. </summary>
		public override long FileModified(string name)
		{
			RamFile file = files[name];
			return file.LastModified;
		}

		///<summary> Returns the length in bytes of a file in the directory. </summary>
		public override long FileLength(string name) 
		{
			RamFile file = files[name];
			return file.Length;
		}

		///<summary> Removes an existing file in the directory. </summary>
		public override void DeleteFile(string name) 
		{
			files.Remove(name);
		}

		///<summary> Removes an existing file in the directory. </summary>
		public override void RenameFile(string from, string to) 
		{
			RamFile file = files[from];
			files.Remove(from);
			files[to] = file;
		}

		///<summary> Creates a new, empty file in the directory with the given name.
		///Returns a stream writing this file. </summary>
		public override OutputStream CreateFile(string name) 
		{
			RamFile file = new RamFile();
			files[name] = file;
			return new RamOutputStream(file);
		}

		///<summary> Returns a stream reading an existing file. </summary>
		public override InputStream OpenFile(string name) 
		{
			RamFile file = files[name];
			return new RamInputStream(file);
		}

		/// <summary>
		/// Construct a Lock.
		/// </summary>
		/// <param name="name">The name of the lock file</param>
		/// <returns>A constructed lock.</returns>
		public override Lock MakeLock(string name) 
		{
			return new RamLock(this, files, name);
		}

		///<summary> Closes the store to future operations. </summary>
		public override void Close()
		{
		}
	}
}
