using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{
    public class GetGitBranchName : Task
    {

        [Output]
        public string BranchName { get; set; }
        public override bool Execute()
        {
            BranchName = Utils.GetGitBranchName();
            return true;
        }
    }
}
