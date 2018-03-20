using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public interface IGitVersionInfo
    {
        string MasterBranchName { get; set; }

        string HotfixBranchPrefix { get; set; }

        string MasterVersionPattern { get; set; }

        string HotfixVersionPattern { get; set; }

        string BuildTagPrefix { get; set; }

        IEnumerable<string> TagNames { get; set; }

    }
    public class SetVersionFromGitTag : Task, IGitVersionInfo
    {
        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public string MasterBranchName { get; set; }

        [Required]
        public string HotfixBranchPrefix { get; set; }

        [Required]
        public string MasterVersionPattern { get; set; }

        [Required]
        public string HotfixVersionPattern { get; set; }

        [Output]
        public string NewVersion { get; set; }


        IEnumerable<string> IGitVersionInfo.TagNames => throw new NotImplementedException();
        public override bool Execute()
        {


            return true;
        }
    }


    public class GitVersionWorker
    {
        public GitVersionWorker(IGitVersionInfo info)
        {
            VersionInfo = info;
        }

        private IGitVersionInfo VersionInfo { get; }

        public Version NewVersion { get; }

        public string TagName { get; }

        public void Execute()
        {
            Version currentVersion = null;
            if (IsInHotfixBranch())
            {
                currentVersion = GetHotFixVersion();
            }

            if (currentVersion == null)
                currentVersion = GetMasterVersion();
        }

        private Version GetMasterVersion()
        {
            throw new NotImplementedException();
        }

        private Version GetHotFixVersion()
        {
            throw new NotImplementedException();
        }
    }
}
