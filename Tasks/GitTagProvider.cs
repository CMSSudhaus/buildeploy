using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string GetDescriptionsSinceTag(string tagName)
        {
            var tag = repository.Tags.FirstOrDefault(t => t.FriendlyName == tagName);
            if (tag == null)
                throw new ArgumentException($"Tag {tagName} not found.", nameof(tagName));

            StringBuilder sb = new StringBuilder();
            foreach(var commit in repository.Head.Commits)
            {
                if (commit == tag.PeeledTarget) break;
                sb.AppendLine(commit.Message);
            }

            return sb.ToString();
        }

        public IEnumerable<string> GetTags()
        {
            return repository.Head.Commits.SelectMany(c => repository.Tags.Where(t => t.PeeledTarget == c)).Select(t => t.FriendlyName);
        }
    }
}

