using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public class WriteTextFile : Task
    {
        public string FileName { get; set; }

        public string Text { get; set; }

        public override bool Execute()
        {
            File.WriteAllText(FileName, Text);
            return true;
        }
    }
}

