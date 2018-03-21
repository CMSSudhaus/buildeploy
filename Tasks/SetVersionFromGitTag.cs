using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{

    public interface IGitTagProvider : IDisposable
    {
        IEnumerable<string> GetTags();

        string CurrentBranchName { get; }
    }

    public interface IGitVersionTask : ILogWriter
    {
        string MasterBranchName { get; set; }

        string HotfixBranchPrefix { get; set; }

        string MasterVersionPattern { get; set; }

        string HotfixVersionPattern { get; set; }

        string BuildTagPrefix { get; set; }

        IGitTagProvider CreateTagProvider();

    }
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


    public class GitVersionWorker : IDisposable
    {
        public GitVersionWorker(IGitVersionTask task)
        {
            Task = task;
            GitTagProvider = task.CreateTagProvider();
        }


        private IGitTagProvider GitTagProvider { get; }
        private IGitVersionTask Task { get; }

        public Version NewVersion { get; private set; }

        public string TagName { get; private set; }

        public void Execute()
        {
            Version currentVersion = null;
            string versionChangePattern = null;
            if (IsInHotfixBranch())
            {
                currentVersion = GetHotFixVersion();
                versionChangePattern = Task.HotfixVersionPattern;
            }

            if (currentVersion == null)
            {
                currentVersion = GetMasterVersion();
                versionChangePattern = Task.MasterVersionPattern;
            }

            ChangeVersionParser parser = new ChangeVersionParser(versionChangePattern, Task);
            NewVersion = new Version(parser.ChangeVersion(currentVersion.ToString()));
            TagName = BuildVersionTag(GitTagProvider.CurrentBranchName, NewVersion.ToString());
        }

        private string BuildVersionTag(string branchName, string version)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}-{2}", Task.BuildTagPrefix, branchName, version);
        }


        private Version GetVersionFromTag(string tag, string prefix)
        {
            if (tag != null && tag.StartsWith(prefix, StringComparison.Ordinal))
            {
                if (Version.TryParse(tag.Substring(prefix.Length), out var version))
                    return version;
            }

            return null;
        }
        private Version FindLastVersion(string branchName)
        {
            var prefix = BuildVersionTag(branchName, string.Empty);
            return GitTagProvider.GetTags().Select(t => GetVersionFromTag(t, prefix)).FirstOrDefault(v => v != null) ?? CreateDefaultVersion();
        }

        private static Version CreateDefaultVersion() => new Version(0, 0, 0, 0);

        private bool IsInHotfixBranch() =>
            GitTagProvider.CurrentBranchName?.StartsWith(Task.HotfixBranchPrefix, StringComparison.Ordinal) ?? false;

        private Version GetMasterVersion() => FindLastVersion(Task.MasterBranchName);

        private Version GetHotFixVersion()
        {
            if (IsInHotfixBranch())
                return FindLastVersion(GitTagProvider.CurrentBranchName);

            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    GitTagProvider.Dispose();
                }


                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
