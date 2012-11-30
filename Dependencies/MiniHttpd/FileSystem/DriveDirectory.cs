using System;
using System.Collections;
using System.IO;

namespace MiniHttpd.FileSystem
{
    /// <summary>
    /// Represents a physical directory on disk.
    /// </summary>
    [Serializable]
    public class DriveDirectory : IDirectory, IPhysicalResource
    {
        private static int _rootPathLen = -1;
        private readonly string _alias;
        private readonly IDirectory _parent;
        private readonly string _path;

        /// <summary>
        /// Creates a <see cref="DriveDirectory"/> with the specified alias, path and parent.
        /// </summary>
        /// <param name="alias">The name of the directory as it should be recognized in the parent directory.</param>
        /// <param name="path">The full path of the directory on disk.</param>
        /// <param name="parent">An <see cref="IDirectory"/> specifying the parent directory. This should be set to <c>null</c> to specify that this directory is root.</param>
        public DriveDirectory(string alias, string path, IDirectory parent) : this(alias, path, parent, true)
        {
        }

        private DriveDirectory(string alias, string path, IDirectory parent, bool checkPath)
        {
            if (checkPath)
            {
                path = System.IO.Path.GetFullPath(path);
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException("Directory " + path + " not found");
                if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    throw new ArgumentException("Path cantains invalid characters.", "path");
            }
            _alias = alias;
            _path = path;
            _parent = parent;
        }

        /// <summary>
        /// Creates a <see cref="DriveDirectory"/> with the specified path and parent. 
        /// </summary>
        /// <param name="path">The full path of the directory on disk.</param>
        /// <param name="parent">An <see cref="IDirectory"/> specifying the parent directory. This should be set to <c>null</c> to specify that this directory is root.</param>
        public DriveDirectory(string path, IDirectory parent) : this(path, parent, true)
        {
        }

        /// <summary>
        /// Creates a root <see cref="DriveDirectory"/> with the specified path.
        /// </summary>
        /// <param name="path">The full path of the directory on disk.</param>
        public DriveDirectory(string path) : this(path, null, true)
        {
        }

        private DriveDirectory(string path, IDirectory parent, bool checkValid)
        {
            if (checkValid)
            {
                path = System.IO.Path.GetFullPath(path);
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException("Directory " + path + " not found");
                if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    throw new ArgumentException("Path cantains invalid characters.", "path");
            }
            _path = path;
            _parent = parent;
            _alias = GetName(path);
        }

        #region IDirectory Members

        /// <summary>
        /// Returns the specified subdirectory.
        /// </summary>
        /// <param name="dir">The name of the subdirectory to retrieve.</param>
        /// <returns>An <see cref="IDirectory"/> representing the specified directory, or <c>null</c> if one doesn't exist.</returns>
        public IDirectory GetDirectory(string dir)
        {
            if (dir != null)
            {
                if (dir.StartsWith("."))
                    return null;
            }
            string retDirPath = System.IO.Path.Combine(_path, dir);
            if (!Directory.Exists(retDirPath))
                return null;
            return new DriveDirectory(retDirPath, this, false);
        }

        /// <summary>
        /// Returns the specified file.
        /// </summary>
        /// <param name="filename">The name of the file to retrieve.</param>
        /// <returns>An <see cref="IFile"/> representing the specified file, or <c>null</c> if one doesn't exist.</returns>
        public IFile GetFile(string filename)
        {
            if (filename != null)
            {
                if (filename.StartsWith("."))
                    return null;
            }

            string ret = System.IO.Path.Combine(_path, filename);
            if (File.Exists(ret))
                return new DriveFile(ret, this, false);
            else
                return null;
        }

        /// <summary>
        /// Returns a collection of subdirectories available in the directory.
        /// </summary>
        /// <returns>An <see cref="ICollection"/> containing <see cref="IDirectory"/> objects available in the directory.</returns>
        public ICollection GetDirectories()
        {
            string[] dirs = Directory.GetDirectories(_path);

            ArrayList ret = new ArrayList();

            for (int i = 0; i < dirs.Length; i++)
            {
                if (System.IO.Path.GetFileName(dirs[i])[0] != '.')
                    ret.Add(new DriveDirectory(dirs[i], this, false));
            }

            return ret;
        }

        /// <summary>
        /// Returns a collection of files available in the directory.
        /// </summary>
        /// <returns>An <see cref="ICollection"/> containing <see cref="IFile"/> objects available in the directory.</returns>
        public ICollection GetFiles()
        {
            string[] files = Directory.GetFiles(_path);

            ArrayList ret = new ArrayList();

            for (int i = 0; i < files.Length; i++)
            {
                if (System.IO.Path.GetFileName(files[i])[0] != '.')
                    ret.Add(new DriveFile(System.IO.Path.Combine(Path, files[i]), this, false));
            }

            return ret;
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        public string Name
        {
            get { return _alias; }
        }

        /// <summary>
        /// Gets the parent directory of the directory.
        /// </summary>
        public IDirectory Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Returns the resource (file or directory) with the specified name.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>An IFile or IDirectory with the given name.</returns>
        public IResource GetResource(string name)
        {
            IFile file = GetFile(name);
            if (file != null)
                return file;

            return GetDirectory(name);
        }

        /// <summary>
        /// DriveDirectory can only contain <see cref="DriveDirectory"/> and <see cref="DriveFile"/> objects, neither of which require disposal logic.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion

        #region IPhysicalResource Members

        /// <summary>
        /// Gets the full path of the directory on disk.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        #endregion

        private string GetName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (_parent == null)
                return null;
            if (path.Length == 2 && path[1] == ':')
                path = path + '\\';
            if (_rootPathLen == -1)
                _rootPathLen = System.IO.Path.GetPathRoot(path).Length;

            if (path.Length == _rootPathLen)
                return path;

            if (path[path.Length - 1] == System.IO.Path.DirectorySeparatorChar ||
                path[path.Length - 1] == System.IO.Path.AltDirectorySeparatorChar)
                return System.IO.Path.GetFileName(path.Substring(0, path.Length - 1));

            return System.IO.Path.GetFileName(path);
        }

        /// <summary>
        /// Returns the name of the directory.
        /// </summary>
        /// <returns>The name of the directory.</returns>
        public override string ToString()
        {
            return _alias;
        }
    }
}