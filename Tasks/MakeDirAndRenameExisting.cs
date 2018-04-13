using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public class MakeDirAndRenameExisting : Task
    {
        public override bool Execute()
        {
            int index = 0;
            string name = DirectoryName;
            while (Directory.Exists(name) || File.Exists(name))
                name = BuildIndexedName(++index);

            if (name != DirectoryName)
            {
                Log.LogMessage(MessageImportance.Normal, "Renaming directory {0} to {1}", DirectoryName, name);
                Directory.Move(DirectoryName, name);
            }

            Log.LogMessage(MessageImportance.Normal, "Creating directory {0}", DirectoryName);

            Directory.CreateDirectory(DirectoryName);

            return true;
        }

        private string BuildIndexedName(int index) => string.Format(CultureInfo.InvariantCulture, "{0}-{1}-({2})", DirectoryName, Prefix, index);

        [Required]
        public string DirectoryName { get; set; }

        [Required]
        public string Prefix { get; set; }
    }
}

