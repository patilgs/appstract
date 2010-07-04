﻿#region Copyright (C) 2009-2010 Simon Allaeys

/*
    Copyright (C) 2009-2010 Simon Allaeys
 
    This file is part of AppStract

    AppStract is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AppStract is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AppStract.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.IO;
using AppStract.Core.Virtualization.Engine.FileSystem;

namespace AppStract.Server.FileSystem
{
  /// <summary>
  /// Provides information about, and means to manipulate,
  /// the current virtual environment and platform.
  /// </summary>
  public class VirtualEnvironment
  {

    #region Variables

    /// <summary>
    /// The root of the virtual file system.
    /// </summary>
    private readonly string _root;
    /// <summary>
    /// Provides the virtual counterparts of paths used by the host file system.
    /// </summary>
    private readonly FileSystemRedirector _redirector;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the root folder of the virtual environment.
    /// </summary>
    public string FileSystemRoot
    {
      get { return _root; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new <see cref="VirtualEnvironment"/> which can be used to redirect <see cref="FileRequest"/>s to.
    /// </summary>
    /// <param name="rootDirectory"></param>
    public VirtualEnvironment(string rootDirectory)
    {
      if (rootDirectory == null)
        throw new ArgumentNullException("rootDirectory");
      rootDirectory = !Path.IsPathRooted(rootDirectory)
                        ? Path.GetFullPath(rootDirectory)
                        : rootDirectory;
      _root = rootDirectory.ToLowerInvariant();
      _redirector = new FileSystemRedirector();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Tries to create all system-folders, as defined in <see cref="VirtualFolder"/>.
    /// </summary>
    /// <returns>True if all folders are created; False if the creation of one or more folders failed.</returns>
    public bool CreateSystemFolders()
    {
      GuestCore.Log.Message("Creating system folders for a virtual environment with root \"{0}\"", FileSystemRoot);
      bool succeeded = true;
      foreach (VirtualFolder virtualFolder in Enum.GetValues(typeof(VirtualFolder)))
        if (!TryCreateDirectory(Path.Combine(FileSystemRoot, virtualFolder.ToPath())))
        {
          GuestCore.Log.Critical("Failed to create virtual system folder: " + virtualFolder);
          succeeded = false;
        }
      return succeeded;
    }

    /// <summary>
    /// Returns the absolute path as used in the virtual environment for the specified path string.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetFullPath(string path)
    {
      return Path.Combine(FileSystemRoot, path);
    }

    /// <summary>
    /// Returns whether or nor the specified <paramref name="path"/> is virtualizable;
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool IsVirtualizable(string path)
    {
      if (path.StartsWith(@"\\.\"))
      {
        // Physical Disks and Volumes or Changer Device or Tape Drive or Communications Resource or Named Pipe
        // EXCEPT FOR paths like: @"\\.\C:\" -> opens the file system of the C: volume.
        if (!(path.Length >= 7 && path[5] == ':' && path[6] == '\\'))
          return false;
      }
      if (path.StartsWith(@"\\\\.\\"))
        // Changer Device or Tape Drive from C or C++
        return false;
      if (path.Equals("CONIN$", StringComparison.InvariantCultureIgnoreCase)
          || path.Equals("CONOUT$", StringComparison.InvariantCultureIgnoreCase))
        // Console In or Console Out
        return false;
      // The path already points to the virtual environment.
      if (path.ToLowerInvariant().StartsWith(_root))
        return false;
      // None of the above, save to virtualize the specified path
      return true;
    }

    /// <summary>
    /// Redirects the given <see cref="FileRequest"/> to the virtual environment.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public string RedirectRequest(FileRequest request)
    {
      // Redirect the path to the virtual environment.
      var redirectedPath = _redirector.Redirect(request.Path);
      // Make path absolute, as member of the virtual environment.
      redirectedPath = GetFullPath(redirectedPath);
      // Verify the result.
      if (!File.Exists(redirectedPath))
      {
        // Path doesn't exist, determine whether it should be used anyway.
        if (request.ResourceType == ResourceType.Library)
          return request.Path; // Target is a library unknown to the virtual environment.
        if (request.CreationDisposition == FileCreationDisposition.OpenExisting
            || request.CreationDisposition == FileCreationDisposition.Unspecified)  // BUG: is this safe?
          return request.Path; // The target won't be created, save to return original path.
      }
      return redirectedPath;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Tries to create the directory, specified by <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Directory to create.</param>
    /// <returns>True if the directory is created; False, otherwise.</returns>
    private static bool TryCreateDirectory(string path)
    {
      try
      {
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
        return true;
      }
      catch (IOException)
      {
        return false;
      }
    }

    #endregion

  }
}