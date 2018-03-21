using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public interface IGitTagProvider : IDisposable
    {
        IEnumerable<string> GetTags();
    }

    public interface IGitVersionTask
    {
        string MasterBranchName { get; set; }

        string HotfixBranchPrefix { get; set; }

        string MasterVersionPattern { get; set; }

        string HotfixVersionPattern { get; set; }

        string BuildTagPrefix { get; set; }

        IGitTagProvider CreateTagProvider();

    }
    public class SetVersionFromGitTag : Task, IGitVersionTask
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


    public class GitVersionWorker
    {
        public GitVersionWorker(IGitVersionTask info)
        {
            VersionInfo = info;
        }

        private IGitVersionTask VersionInfo { get; }

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


            ChangeVersionParser parser = new ChangeVersionParser(currentVersion.ToString(), null);
        }

        private bool IsInHotfixBranch()
        {
            throw new NotImplementedException();
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
