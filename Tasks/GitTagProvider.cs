using LibGit2Sharp;
using System.Collections.Generic;
using System.Linq;

namespace Cms.Buildeploy.Tasks
{
    public sealed class GitTagProvider : IGitTagProvider
    {
        private readonly Repository repository;
        public GitTagProvider(string path)
        {
            repository = new Repository(Repository.Discover(path));
        }

        public string CurrentBranchName => repository.Head.FriendlyName;

        public void Dispose() => repository.Dispose();

        public IEnumerable<string> GetTags()
        {
            return repository.Head.Commits.SelectMany(c => repository.Tags.Where(t => t.PeeledTarget == c)).Select(t => t.FriendlyName);
        }
    }
}

