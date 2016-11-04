using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Buildeploy.Tasks
{
    public class EasyTest : ToolTask
    {
        protected override string ToolName => "TestExecutor";

        protected override string GenerateFullPathToTool()
        {
            return @"c:\Temp\EasyTests\TestExecutor.v16.1.exe";
        }

        public string ScriptFile { get; set; }

        protected override string GenerateCommandLineCommands()
        {
            return $"\"{ScriptFile}\" -l:EasyTestLog.log";
        }
    }
}
