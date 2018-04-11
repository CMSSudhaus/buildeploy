namespace Cms.Buildeploy.Tasks
{
    public interface IGitVersionTask : ILogWriter
    {
        string MasterBranchName { get; set; }

        string HotfixBranchPrefix { get; set; }

        string MasterVersionPattern { get; set; }

        string HotfixVersionPattern { get; set; }

        string BuildTagPrefix { get; set; }

        IGitTagProvider CreateTagProvider();

string ReleaseNotes { get; set; }

    }
}
