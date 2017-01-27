﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static System.Diagnostics.Debugger;

namespace Cms.Buildeploy.Tasks
{
    public class DeleteLicxFiles : Task
    {
        [Required]
        public ITaskItem[] ProjectFiles { get; set; }

        public ITaskItem SolutionLocation { get; set; }

        public override bool Execute()
        {
            if (!string.IsNullOrEmpty(SolutionLocation.ItemSpec))
            {
                foreach (var projectLocation in GetProjectLocations(SolutionLocation.ItemSpec))
                {
                    HandleProjectFile(projectLocation);   
                }
                return true;
            }
            
            foreach (var projectFile in ProjectFiles)
            {
                HandleProjectFile(projectFile.ItemSpec);
            }
            return true;
        }

        private IEnumerable<string> GetProjectLocations(string solutionLocation)
        {
            var solutionDirectory = Path.GetDirectoryName(solutionLocation) ?? string.Empty;

            foreach (var line in File.ReadLines(solutionLocation))
            {
                if (line.IndexOf("Project", StringComparison.InvariantCulture) != 0)
                    continue;

                if (line.IndexOf("Global",StringComparison.InvariantCulture) == 0)
                    break;

                var slnRelativePath = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)[1].Trim().Replace("\"", string.Empty);
                if (!slnRelativePath.EndsWith(".csproj"))
                    continue;
                
                yield return Path.Combine(solutionDirectory, slnRelativePath);
            }
        }


        private void HandleProjectFile(string projectFileLocation)
        {
            var project = new Project(projectFileLocation);
            var license = project.GetItemsByEvaluatedInclude(@"Properties\licenses.licx").SingleOrDefault();
            if (license != null)
            {
                project.RemoveItem(license);
                project.Save();

                foreach (var licx in Directory.EnumerateFiles(Path.GetDirectoryName(projectFileLocation) ?? string.Empty, "*.licx", SearchOption.AllDirectories))
                {
                    File.Delete(licx);
                }
            }

        }
    }
}