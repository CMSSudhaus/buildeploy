using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Evaluation;

namespace Cms.Buildeploy.Tasks
{
    public class DeleteLicxFiles : Task
    {
        public override bool Execute()
        {
            try
            {
                foreach (ITaskItem projectFile in ProjectFiles)
                {
                    foreach (var licx in Directory.EnumerateFiles(Path.GetDirectoryName(projectFile.ItemSpec) ?? string.Empty, "*.licx", SearchOption.AllDirectories))
                    {
                        File.Delete(licx);
                    }

                   var project = new Project(File.ReadAllText(projectFile.ItemSpec));
                   var license = project.GetItems("EmbeddedResource").SingleOrDefault(x => x.EvaluatedInclude.EndsWith("licenses.licx")); 
                   if (license != null)
                        project.RemoveItem(license);
                }
                return true;
            }
            catch (Exception)
            {
                return false; 
            }
        }

        [Required]
        public ITaskItem[] ProjectFiles { get; set; }

    }
}
