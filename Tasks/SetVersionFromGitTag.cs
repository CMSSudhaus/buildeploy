using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.Linq;
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

        [Required]
        public string BuildTagPrefix { get; set; }

        [Output]
        public string NewVersion { get; set; }

        public IGitTagProvider CreateTagProvider() => throw new NotImplementedException();

        public override bool Execute()
        {


            return true;
        }
    }
}
