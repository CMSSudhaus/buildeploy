using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using Microsoft.Build.Framework;

namespace Cms.Buildeploy.Tasks
{
    public class ClickOnce : PackageTaskBase
    {

        protected override IPackageArchive CreatePackageArchive()
        {
            return new ZipArchive(Path.GetFullPath(ZipFile));
        }

        protected override string ReplaceDirectorySeparators(string entryName)
        {
            if (entryName == null)
                throw new ArgumentNullException("entryName");

            if (Path.PathSeparator != '/')
                return entryName.Replace(Path.PathSeparator, '/');
            else
                return entryName;
        }

        [Required]
        public string ZipFile { get; set; }
    }


    [Serializable]
    public class DuplicateAssemblyReferenceException : Exception
    {
        public DuplicateAssemblyReferenceException() { }
        public DuplicateAssemblyReferenceException(string message) : base(message) { }
        public DuplicateAssemblyReferenceException(string message, Exception inner) : base(message, inner) { }
        protected DuplicateAssemblyReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    class ZipArchive : IPackageArchive
    {
        private readonly FileStream fileStream;
        private readonly ZipOutputStream zOutstream;

        internal ZipArchive(string zipFile)
        {
            fileStream = new FileStream(zipFile, FileMode.Create);
            zOutstream = new ZipOutputStream(fileStream);
            zOutstream.SetLevel(6);
            ArchivePath = zipFile;
        }
        public string ArchivePath
       {
            get;
            private set;
        }

        public void AddEntry(string entryName, DateTime dateTime, FileStream stream)
        {
            ZipEntry entry = new ZipEntry(entryName);
            entry.DateTime = dateTime;
            zOutstream.PutNextEntry(entry);
            stream.CopyTo(zOutstream);
        }

        public bool Finish()
        {
            return true;
        }

        public void Dispose()
        {
            zOutstream.Dispose();
            fileStream.Dispose();
        }


    }

}
