using System;
using System.Collections.Generic;

namespace Cms.Buildeploy.Tasks
{
    public interface IGitTagProvider : IDisposable
    {
        IEnumerable<string> GetTags();

        string CurrentBranchName { get; }
    }
}
