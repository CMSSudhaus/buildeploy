﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cms.Buildeploy.Tasks
{
    public class EasyTest : ToolTask
    {
        private const string logFileName = "EasyTestLog.log";

        protected override string ToolName => "TestExecutor";

        protected override string GenerateFullPathToTool() => string.IsNullOrWhiteSpace(PSExecPath) ? TestExecutorPath : PSExecPath;

        [Required]
        public string TestExecutorPath { get; set; }

        public string PSExecPath { get; set; }

        public string PSExecParameters { get; set; }

        [Required]
        public string DatabaseName { get; set; }

        public override bool Execute()
        {
            return base.Execute() && AnalyzeLogFile();
        }

        public string ScriptsDirectory { get; set; }

        private bool AnalyzeLogFile()
        {
            var doc = XElement.Load(LogFilePath);
            bool result = true;
            foreach (var failedTest in doc.Elements("Test").Where(e => e.Attribute("Result")?.Value != "Passed"))
            {
                result = false;
                var errorElement = failedTest.Element("Error");
                if (errorElement != null)
                    Log.LogError("Test '{0}' failed: {1}, {2}",
                        failedTest.Attribute("Name").Value,
                        errorElement.Attribute("Type").Value,
                        errorElement.Element("Message").Value);
            }

            return result;
        }

        private string LogFilePath => Path.GetFullPath(Path.Combine(ScriptsDirectory, logFileName));
        protected override string GenerateCommandLineCommands()
        {
            string psexecArgs = string.IsNullOrWhiteSpace(PSExecParameters) ? "-i -s" : PSExecParameters;
            return (string.IsNullOrWhiteSpace(PSExecPath) ? string.Empty : $"{psexecArgs} {TestExecutorPath} ") +
                $"\"{Path.GetFullPath(ScriptsDirectory)}\" -l:{LogFilePath} -o:DatabaseName={DatabaseName}";
        }
    }
}
