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
                   var project = new Project(projectFile.ItemSpec);
                   var license = project.GetItemsByEvaluatedInclude(@"Properties\licenses.licx").SingleOrDefault();
                   if (license != null)
                   {
                        project.RemoveItem(license);
                        project.Save();

                        foreach (var licx in Directory.EnumerateFiles(Path.GetDirectoryName(projectFile.ItemSpec) ?? string.Empty, "*.licx", SearchOption.AllDirectories))
                        {
                            File.Delete(licx);
                        }
                    }
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
