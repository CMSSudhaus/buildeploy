// -----------------------------------------------------------------------
// <copyright file="MSTest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
using System.Resources;

namespace Buildeploy.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Utilities;
    using Microsoft.Build.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MSTest : ToolTask
    {
        public MSTest()
        {
            
        }

        public string MSTestExe { get; set; }
        protected override string GenerateFullPathToTool()
        {
            if (!string.IsNullOrEmpty(MSTestExe))
                return MSTestExe;

            return System.IO.Path.Combine(Environment.ExpandEnvironmentVariables(@"%VS100COMNTOOLS%..\IDE"), ToolName);
        }



        protected override string ToolName
        {
            get { return "MSTest.exe"; }
        }



        protected override string GetWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(WorkingDirectory))
                return WorkingDirectory;
            else
                return base.GetWorkingDirectory();
        }

        public string WorkingDirectory { get; set; }

        [Required]
        public ITaskItem[] Assemblies { get; set; }

        public string Category { get; set; }

        protected override bool ValidateParameters()
        {
            if (Assemblies != null || Assemblies.Length > 0)
                return base.ValidateParameters();
            else
            {
                Log.LogError("No assemblies specfied.");
                return false;
            }
        }
        protected override string GenerateCommandLineCommands()
        {

            StringBuilder sb = new StringBuilder("/detail:errormessage /detail:errorstacktrace");
            foreach (var item in Assemblies)
                sb.AppendFormat(" /testcontainer:{0}", item.ItemSpec);

            if (!string.IsNullOrEmpty(Category))
            {
                sb.AppendFormat(" /category:\"{0}\"", Category);
            }
            return sb.ToString();
        }
    }
}
