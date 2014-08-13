using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cms.Buildeploy.Tasks
{
    public class GenerateVersionPattern : Task
    {
        [Required]
        public string BuildNumber { get; set; }

        [Output]
        public string Pattern { get; set; }

        public string StartDate { get; set; }

        public override bool Execute()
        {
            DateTime date = new DateTime(2014, 1,1);

            if (!string.IsNullOrWhiteSpace(StartDate))
            {
                date = DateTime.Parse(StartDate, CultureInfo.InvariantCulture);
            }

            Pattern = BuildNameParser.ParseBuildNameToPattern(BuildNumber, date);
            return true;
        }

    }
}
