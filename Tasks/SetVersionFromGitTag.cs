using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Cms.Buildeploy.Tasks
{
    public class SetVersionFromGitTag : LogWriterTaskBase, IGitVersionTask
    {
        [Required]
        public ITaskItem[] Files { get; set; }

        public string MasterBranchName { get; set; } = "master";

        public string HotfixBranchPrefix { get; set; } = "hotfix-";

        [Required]
        public string MasterVersionPattern { get; set; }

        [Required]
        public string HotfixVersionPattern { get; set; }

        public string BuildTagPrefix { get; set; } = "build-";

        [Output]
        public string NewVersion { get; set; }

        [Output]
        public string BuildTag { get; set; }

        [Output]
        public string ReleaseNotes { get; set; }

        public IGitTagProvider CreateTagProvider()
        {
            if (CanCreateTagProvider())
                return new GitTagProvider(Path.GetDirectoryName(GetBuildProjectPath()));
            else
                return null;
        }

        private string GetBuildProjectPath()
        {
            return BuildEngine.ProjectFileOfTaskNode;
        }

        private bool CanCreateTagProvider() => File.Exists(GetBuildProjectPath());
        public override bool Execute()
        {
            if (CanCreateTagProvider())
            {
                using (GitVersionWorker worker = new GitVersionWorker(this))
                {
                    worker.Execute();
                    NewVersion = worker.NewVersion.ToString();
                    BuildTag = worker.TagName;
                    PatchVersion();
                    return true;
                }
            }
            else
            {
                NewVersion = "*.*.*.*";
                PatchVersion();
                return true;
            }
        }

        private void PatchVersion()
        {
            ChangeVersionParser parser = new ChangeVersionParser(NewVersion, this);
            foreach (var fileItem in Files)
            {
                if (!File.Exists(fileItem.ItemSpec))
                    File.WriteAllText(fileItem.ItemSpec, "[assembly: System.Reflection.AssemblyVersion(\"0.0.0.0\")]\r\n");
                parser.ProcessAssemblyInfo(fileItem.ItemSpec);
            }
        }
    }
}

