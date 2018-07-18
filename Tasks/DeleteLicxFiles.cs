using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cms.Buildeploy.Tasks
{
    public class DeleteLicxFiles : Task
    {
        [Required]
        public ITaskItem[] ProjectFiles { get; set; }
        

        public override bool Execute()
        {
            foreach (var projectFile in ProjectFiles)
            {
                string fileName = projectFile.ItemSpec;
                if (fileName.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase))
                    HandleProjectFile(fileName);
                else if (fileName.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase))
                    HandleSolution(fileName);
                else
                    Log.LogWarning("File extension not supported: {0}", fileName);
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
                if (!slnRelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                yield return Path.Combine(solutionDirectory, slnRelativePath);
            }
        }


        private void HandleProjectFile(string projectFileLocation)
        {
            var project = new Project(projectFileLocation);
            try
            {
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
            finally
            {
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            }
        }

        private void HandleSolution(string solutionLocation)
        {
            foreach (var projectLocation in GetProjectLocations(solutionLocation))
            {
                HandleProjectFile(projectLocation);
            }
        }

    }
}