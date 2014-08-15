using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cms.Buildeploy
{
    public class BuildNameParser
    {
        public static string ParseBuildNameToPattern(string buildName, DateTime startDate, string prefix)
        {
            Regex regex = new Regex(@".*_(?<date>\d{8})\.(?<buildnumber>\d*)");
            var match = regex.Match(buildName);
            DateTime date = DateTime.ParseExact(match.Groups["date"].Value, 
                "yyyyMMdd", CultureInfo.InvariantCulture);

            int buildNumber = int.Parse(match.Groups["buildnumber"].Value);
            return string.Format(CultureInfo.InvariantCulture, "{2}.{0}.{1}",
                date.Subtract(startDate).TotalDays, buildNumber * 100, prefix);
        }
    }
}
