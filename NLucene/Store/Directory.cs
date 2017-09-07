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

namespace DotnetPark.NLucene.Store
{
	/// <summary>
	/// A Directory is a flat list of files. Files may be written once,
	/// when they are created. Once a file is created it may only be opened
	/// for read, or deleted. Random access is permitted when reading and writing.
	/// </summary>
	abstract public class Directory
	{
		///<summary> Returns an array of strings, one for each file in the directory. </summary>
		abstract public string[] List();
       
		///<summary> Returns true if a file with the given name exists. </summary>
		abstract public bool FileExists(string name);

		///<summary> Returns the time the named file was last modified. </summary>
		abstract public long FileModified(string name);

		///<summary> Removes an existing file in the directory. </summary>
		abstract public void DeleteFile(string name);

		///<summary>
		///Renames an existing file in the directory.
		///If a file already exists with the new name, then it is replaced.
		///This replacement should be atomic.
		///</summary>
		abstract public void RenameFile(string from, string to);

		///<summary> Returns the length of a file in the directory. </summary>
		abstract public long FileLength(string name);

		///<summary>
		///Creates a new, empty file in the directory with the given name.
		///Returns a stream writing this file.
		///</summary>
		abstract public OutputStream CreateFile(string name);

		///<summary> Returns a stream reading an existing file. </summary>
		abstract public InputStream OpenFile(string name);

		/// <summary>
		/// Construct a Lock.
		/// </summary>
		/// <param name="name">The name of the lock file.</param>
		/// <returns>A constructed lock.</returns>
		abstract public Lock MakeLock(string name);

		///<summary> Closes the store. </summary>
		abstract public void Close();
	}
}
