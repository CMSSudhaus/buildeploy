// -----------------------------------------------------------------------
// <copyright file="NugetPackage.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Cms.Buildeploy.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System.Globalization;
    using System.Diagnostics;
    using System.Threading;
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NugetPackage : PackageTaskBase
    {

        [Required]
        public string NugetExePath { get; set; }

        [Required]
        public string NuspecFile { get; set; }

        [Required]
        public string OutputDirectory { get; set; }

        protected override IPackageArchive CreatePackageArchive()
        {
            return new NugetArchive(NugetExePath, Path.GetFullPath(NuspecFile), OutputDirectory, Version, Log);
        }

        protected override string ReplaceDirectorySeparators(string entryName)
        {
            return entryName;
        }
    }

    class NugetArchive : IPackageArchive
    {

        private readonly string tempPath;
        private readonly string nugetPath;
        private readonly string nuspecFile;
        private readonly string version;
        private readonly string outputDir;

        internal NugetArchive(string nugetPath, string nuspecFile, string outputDir, string version, TaskLoggingHelper log)
        {
            this.nugetPath = nugetPath;
            this.nuspecFile = nuspecFile;
            this.version = version;
            Log = log;
            tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            this.outputDir = Path.GetFullPath(outputDir);
            Directory.CreateDirectory(tempPath);
        }

        private TaskLoggingHelper Log { get; set; }


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
            string commandLine = string.Format(CultureInfo.InvariantCulture,
                "pack \"{0}\"  -NoPackageAnalysis -BasePath \"{1}\" -OutputDirectory \"{2}\" -Version {3}", nuspecFile, tempPath, outputDir, version);


            Log.LogMessage(MessageImportance.Low, "NuGet.exe path: {0} ", nugetPath);
            Log.LogMessage(MessageImportance.Low, "Running NuGet.exe with command line arguments: {0}" + commandLine);

            var exitCode = SilentProcessRunner.ExecuteCommand(
                nugetPath,
                commandLine,
                tempPath,
                output => Log.LogMessage(MessageImportance.Low, output),
                error => Log.LogError("NUGET: {0}", error));

            if (exitCode != 0)
            {
                Log.LogError("There was an error calling NuGet. Please see the output above for more details. Command line: '{0}' {1}", nugetPath, commandLine);
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch (IOException) { }
            }

        }
    }

    public static class SilentProcessRunner
    {
        public static int ExecuteCommand(string executable, string arguments, string workingDirectory, Action<string> output, Action<string> error)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    using (var outputWaitHandle = new AutoResetEvent(false))
                    using (var errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output(e.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error(e.Data);
                            }
                        };

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();
                        outputWaitHandle.WaitOne();
                        errorWaitHandle.WaitOne();

                        return process.ExitCode;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error when attempting to execute {0}: {1}", executable, ex.Message), ex);
            }
        }
    }
}
