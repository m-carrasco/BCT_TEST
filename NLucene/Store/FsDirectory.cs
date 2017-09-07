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
	/// Straightforward implementation of Directory as a directory of files.
	/// </summary>
	public class FsDirectory : Directory
	{
		/// <summary>
		/// [To be supplied.]
		/// </summary>
		protected class FsLock : Lock
		{
			/// <summary>
			/// [To be supplied.]
			/// </summary>
			public FileInfo lockFileInfo;
			/// <summary>
			/// [To be supplied.]
			/// </summary>
			/// <returns>[To be supplied.]</returns>
			public override bool Obtain() 
			{
				try
				{
					FileStream fs = lockFileInfo.Create();
					fs.Close(); 
				}
				catch
				{
					return false;
				}
				return true;
			}

			/// <summary>
			/// [To be supplied.]
			/// </summary>
			public override void Release() 
			{
				lockFileInfo.Delete();
			}

			/// <summary>
			/// [To be supplied.]
			/// </summary>
			/// <returns>[To be supplied.]</returns>
			public override string ToString() 
			{
				return "Lock@" + lockFileInfo;
			}
		}

		///<summary>
		/// This cache of directories ensures that there is a unique Directory
		/// instance per path, so that synchronization on the Directory can be used to
		/// synchronize access between readers and writers.
		/// </summary>
		private static Dictionary<DirectoryInfo, FsDirectory> DIRECTORIES = new Dictionary<DirectoryInfo, FsDirectory>();

		/// <summary>
		/// Returns the directory instance for the named location.
		/// 
		/// <p>Directories are cached, so that, for a given canonical path, the same
		/// FsDirectory instance will always be returned.  This permits
		/// synchronization on directories.</p>
		/// </summary>
		/// <param name="path">The path to the directory.</param>
		/// <param name="create">If true, create, or erase any existing contents.</param>
		/// <returns>The FsDirectory for the named file.</returns>
		public static FsDirectory GetDirectory(string path, bool create)
		{
			return GetDirectory(new DirectoryInfo(path), create);
		}

		/// <summary>
		/// Returns the directory instance for the named location.
		/// 
		/// <p>Directories are cached, so that, for a given canonical path, the same
		/// FsDirectory instance will always be returned.  This permits
		/// synchronization on directories.</p>
		/// </summary>
		/// <param name="file">The path to the directory.</param>
		/// <param name="create">If true, create, or erase any existing contents.</param>
		/// <returns>The FsDirectory for the named file.</returns>
		public static FsDirectory GetDirectory(DirectoryInfo file, bool create)
		{
			FsDirectory dir = null;
			lock (DIRECTORIES)
			{
				if(DIRECTORIES.ContainsKey(file))
					dir = DIRECTORIES[file];
				
				if (dir == null) 
				{
					dir = new FsDirectory(file, create);
					DIRECTORIES[file] = dir;
				} 
				else if (create) 
				{
					dir.Create();
				}
			}
			lock (dir) 
			{
				dir.refCount++;
			}
			return dir;
		}

		private DirectoryInfo directory = null;
		private int refCount;

		private FsDirectory(DirectoryInfo path, bool create) 
		{
			directory = path;

			if (create)
				Create();
		}

		private void Create() 
		{
			if (!directory.Exists)
				try
				{
					directory.Create();
				}
				catch(Exception e)
				{
					throw new IOException("Cannot create directory: " + directory + e);
				}
	    

			FileInfo[] files = directory.GetFiles();            // clear old files
			for (int i = 0; i < files.Length; i++) 
			{
				try
				{
					files[i].Delete();
				}
				catch(Exception e)
				{
					throw new IOException("couldn't delete " + files[i] + e);
				}
			}
		}

		///<summary> Returns an array of strings, one for each file in the directory. </summary>
		public override string[] List() 
		{
			FileInfo[] files = directory.GetFiles();
			string[] str = new string[files.Length];
			for(int i=0; i<str.Length; i++)
			{
				str[i] = files[i].FullName;
			}
			return str;
		}
       
		///<summary> Returns true iff a file with the given name exists. </summary>
		public override bool FileExists(string name)
		{
			return GetFileInfo(name).Exists;
		}
       
		///<summary> Returns the time the named file was last modified. </summary>
		public override long FileModified(string name)
		{
			return GetFileInfo(name).LastWriteTime.ToFileTime();
		}

		/// <summary>
		/// [To be supplied.]
		/// </summary>
		/// <param name="name">[To be supplied.]</param>
		/// <returns>[To be supplied.]</returns>
		public static long GetFileModified(string name)
		{
			return new FileInfo(name).LastWriteTime.ToFileTime();
		}

		///<summary> Returns the length in bytes of a file in the directory. </summary>
		public override long FileLength(string name) 
		{
			return GetFileInfo(name).Length;
		}

		private FileInfo GetFileInfo(string file) 
		{
			return new FileInfo(GetFileFullName(file));
		}

		private string GetFileFullName(string file)
		{
			return directory.FullName + "/" + file;
		}

		///<summary> Removes an existing file in the directory. </summary>
		public override void DeleteFile(string name) 
		{
			try
			{
				GetFileInfo(name).Delete();
			}
			catch(Exception e)
			{
				throw new IOException("couldn't delete " + name + e);
			}
		}

		///<summary> Renames an existing file in the directory. </summary>
		public override void RenameFile(string from, string to)
		{
   
			FileInfo nu = GetFileInfo(to);

			/* This is not atomic.  If the program crashes between the call to
				   delete() and the call to renameTo() then we're screwed, but I've
				   been unable to figure out how else to do this... */

			if (nu.Exists)
			{
				try 
				{
					nu.Delete();
				}
				catch (IOException) 
				{
					throw new IOException("couldn't delete " + to);
				}
			}

			try
			{
				GetFileInfo(from).MoveTo(GetFileFullName(to));
			}
			catch(Exception e)
			{
				throw new IOException("couldn't rename " + from + " to " + to + e);
			}
		}

		///<summary> Creates a new, empty file in the directory with the given name.
		///Returns a stream writing this file. </summary>
		public override OutputStream CreateFile(string name) 
		{
			return new FsOutputStream(GetFileInfo(name));
		}

		///<summary> Returns a stream reading an existing file. </summary>
		public override InputStream OpenFile(string name) 
		{
			return new FsInputStream(GetFileInfo(name));
		}

		/// <summary>
		/// Construct a Lock.
		/// </summary>
		/// <param name="name">The name of the lock file.</param>
		/// <returns>A constructed lock.</returns>
		public override Lock MakeLock(string name) 
		{
			FsLock lck = new FsLock();
			lck.lockFileInfo = GetFileInfo(name);
			return lck;
		}

		///<summary> Closes the store to future operations. </summary>
		public override void Close() 
		{
			if (--refCount <= 0) 
			{
				lock (DIRECTORIES) 
				{
					DIRECTORIES.Remove(directory);
				}
			}
		}

		///<summary> For debug output. </summary>
		public override string ToString() 
		{
			return "FsDirectory@" + directory;
		}
	}

}
