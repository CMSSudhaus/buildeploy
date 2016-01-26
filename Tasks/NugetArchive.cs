using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Globalization;

namespace Cms.Buildeploy.Tasks
{
    class NugetArchive : IPackageArchive
    {
        private readonly string tempPath;
        private readonly string nugetPath;
        private readonly string nuspecFile;
        private readonly string version;
        private readonly string outputDir;
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        internal NugetArchive(string nugetPath, string nuspecFile, string outputDir, string version, TaskLoggingHelper log)
        {
            this.nugetPath = nugetPath;
            this.nuspecFile = nuspecFile;
            this.version = version;
            Log = log;
            tempPath = CreateTempDirectory();
            this.outputDir = Path.GetFullPath(outputDir);
        }

        private static string CreateTempDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(directory);
            return directory;
        }

        internal string PushLocation
        {
            get;
            set;
        }

        internal string ApiKey
        {
            get;
            set;
        }

        internal void AddProperty(string name, string value)
        {
            properties.Add(name, value);
        }

        private string BuildPropertiesString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in properties)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", entry.Key, entry.Value);
            }

            return sb.ToString();
        }

        private TaskLoggingHelper Log
        {
            get;
            set;
        }

        public void AddEntry(string entryName, DateTime dateTime, System.IO.FileStream stream)
        {
            string fileName = Path.Combine(tempPath, entryName);
            string directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            using (var destStream = File.Create(fileName))
            {
                stream.CopyTo(destStream);
            }

            File.SetLastWriteTime(fileName, dateTime);
        }

        public bool Finish()
        {
            StringBuilder commandLine = new StringBuilder();
            string nupkgTempDirectory = CreateTempDirectory();
            try
            {
                commandLine.AppendFormat(CultureInfo.InvariantCulture, "pack \"{0}\"  -NoPackageAnalysis -BasePath \"{1}\" -OutputDirectory \"{2}\" -Version {3}", nuspecFile, tempPath, nupkgTempDirectory, version);
                foreach (var fileName in Directory.GetFiles(nupkgTempDirectory))
                    File.Move(fileName, Path.Combine(outputDir, Path.GetFileName(fileName)));
                string propertiesString = BuildPropertiesString();
                if (!string.IsNullOrWhiteSpace(propertiesString))
                    commandLine.AppendFormat(CultureInfo.InvariantCulture, " -Properties {0}", propertiesString);
                Log.LogMessage(MessageImportance.Low, "NuGet.exe path: {0} ", nugetPath);
                if (!RunNuget(commandLine.ToString()))
                    return false;
                string packageFileName = Directory.GetFiles(nupkgTempDirectory, "*.nupkg").Single();
                string pushCommandLine = CreatePushCommandLine(packageFileName);
                if (!string.IsNullOrWhiteSpace(pushCommandLine))
                {
                    Log.LogMessage(MessageImportance.Normal, "Pushing package {0} to {1}", Path.GetFileName(packageFileName), PushLocation);
                    if (!RunNuget(pushCommandLine))
                        return false;
                    Log.LogMessage(MessageImportance.Normal, "Push complete.");
                }

                string destFileName = Path.Combine(outputDir, Path.GetFileName(packageFileName));
                if (File.Exists(destFileName))
                {
                    Log.LogError("Cannot copy package to '{0}', file already exists.", destFileName);
                    return false;
                }

                File.Copy(packageFileName, destFileName);
                Log.LogMessage(MessageImportance.Normal, "Created package '{0}'", destFileName);
                return true;
            }
            finally
            {
                TryRemoveTempDirectory(nupkgTempDirectory);
            }
        }

        private string CreatePushCommandLine(string packageFileName)
        {
            if (!string.IsNullOrWhiteSpace(PushLocation))
            {
                StringBuilder sb = new StringBuilder("push ");
                sb.Append(packageFileName);
                if (!string.IsNullOrWhiteSpace(ApiKey))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " -ApiKey {0}", ApiKey);
                }

                sb.AppendFormat(CultureInfo.InvariantCulture, " -Source \"{0}\"", PushLocation);
                return sb.ToString();
            }
            else
                return null;
        }

        private bool RunNuget(string commandLine)
        {
            Log.LogMessage(MessageImportance.Low, "Running NuGet.exe with command line arguments: {0}", commandLine);
            var exitCode = SilentProcessRunner.ExecuteCommand(nugetPath, commandLine, tempPath, output => Log.LogMessage(MessageImportance.Low, output), error => Log.LogError("NUGET: {0}", error));
            if (exitCode != 0)
            {
                Log.LogError("There was an error calling NuGet. Please see the output above for more details. Command line: '{0}' {1}", nugetPath, commandLine);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            TryRemoveTempDirectory(tempPath);
        }

        private static void TryRemoveTempDirectory(string tempDirectory)
        {
            if (Directory.Exists(tempDirectory))
            {
                try
                {
                    Directory.Delete(tempDirectory, true);
                }
                catch (IOException)
                {
                }
            }
        }
    }
}