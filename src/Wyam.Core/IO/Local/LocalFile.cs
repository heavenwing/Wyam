﻿using System;
using System.IO;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Local
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalFile : IFile
    {
        private readonly FileInfo _file;
        private readonly FilePath _path;

        public FilePath Path => _path;

        NormalizedPath IFileSystemEntry.Path => _path;

        public IDirectory Directory => new LocalDirectory(_path.Directory);

        public bool Exists => _file.Exists;

        public long Length => _file.Length;

        public LocalFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            _path = path;
            _file = new FileInfo(path.Collapse().FullPath);
        }

        public void CopyTo(IFile destination, bool overwrite = true, bool createDirectory = true)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // Create the directory
            if (createDirectory)
            {
                destination.Directory.Create();
            }
            
            // Use the file system APIs if destination is also in the file system
            if (destination is LocalFile)
            {
                LocalFileProvider.Retry(() => _file.CopyTo(destination.Path.FullPath, overwrite));
            }
            else
            {
                // Otherwise use streams to perform the copy
                using (Stream sourceStream = OpenRead())
                {
                    using (Stream destinationStream = destination.OpenWrite())
                    {
                        sourceStream.CopyTo(destinationStream);
                    }
                }
            }
        }

        public void MoveTo(IFile destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // Use the file system APIs if destination is also in the file system
            if (destination is LocalFile)
            {
                LocalFileProvider.Retry(() => _file.MoveTo(destination.Path.FullPath));
            }
            else
            {
                // Otherwise use streams to perform the move
                using (Stream sourceStream = OpenRead())
                {
                    using (Stream destinationStream = destination.OpenWrite())
                    {
                        sourceStream.CopyTo(destinationStream);
                    }
                }
                Delete();
            }
        }

        public void Delete() => LocalFileProvider.Retry(() => _file.Delete());

        public string ReadAllText() =>
            LocalFileProvider.Retry(() => File.ReadAllText(_file.FullName));

        public Stream OpenRead() =>
            LocalFileProvider.Retry(() => _file.OpenRead());

        public Stream OpenWrite(bool createDirectory = true)
        {
            if (createDirectory)
            {
                Directory.Create();
            }
            return LocalFileProvider.Retry(() => _file.OpenWrite());
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            if (createDirectory)
            {
                Directory.Create();
            }
            return LocalFileProvider.Retry(() => _file.Open(FileMode.Append, FileAccess.Write));
        }
    }
}
