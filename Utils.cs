using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cms.Buildeploy
{
    public static class Utils
    {
        /// <summary>
        /// http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <param name="dontEscape">Boolean indicating whether to add uri safe escapes to the relative path</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException(nameof(fromPath));
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException(nameof(toPath));

            Uri fromUri = new Uri(Path.GetFullPath(fromPath) + "\\");
            Uri toUri = new Uri(Path.GetFullPath(toPath));

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string GetGitBranchName()
        {
            for (var di = new DirectoryInfo(Directory.GetCurrentDirectory()); di != null; di = di.Parent)
            {
                string gitRepositoryDirName = Path.Combine(di.FullName, ".git");
                if (Directory.Exists(gitRepositoryDirName))
                {
                    string head = File.ReadAllText(Path.Combine(gitRepositoryDirName, "HEAD"));
                    var match = Regex.Match(head, "ref: refs/heads/(.*)");
                    if (match != null && match.Success)
                        return match.Groups[1].Value;
                }
            }

            return null;
        }
    }
}
