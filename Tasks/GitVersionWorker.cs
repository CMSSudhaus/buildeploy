using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{


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
                currentVersion = GetMasterVersion();

            if (string.IsNullOrWhiteSpace(versionChangePattern))
                versionChangePattern = Task.MasterVersionPattern;

            if (currentVersion == null) currentVersion = CreateDefaultVersion();

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
        return GitTagProvider.GetTags().Select(t => GetVersionFromTag(t, prefix)).FirstOrDefault(v => v != null);
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
