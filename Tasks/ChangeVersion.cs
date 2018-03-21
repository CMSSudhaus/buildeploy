using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Cms.Buildeploy.Tasks
{
    public sealed class ChangeVersion : LogWriterTaskBase
    {

        public override bool Execute()
        {
            ChangeVersionParser parser = new ChangeVersionParser(Version, this);
            if (Files == null || Files.Length == 0)
            {
                Log.LogError("No version files specified");
                return false;
            }

            foreach (ITaskItem item in Files)
            {
                NewVersion = parser.ProcessAssemblyInfo(item.ItemSpec);
            }

            return !string.IsNullOrEmpty(NewVersion);
        }

        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public string Version { get; set; }

        [Output]
        public string NewVersion { get; private set; }

    }
}
