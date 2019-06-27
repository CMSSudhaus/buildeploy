using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Cms.Buildeploy
{

    public class ChangeVersionParser
    {

        private VersionChangerBase[] versionChangers;
        private string constantVersion = string.Empty;
        private ILogWriter logWriter;

        public ChangeVersionParser(string versionChangePattern, ILogWriter logWriter)
        {
            if (versionChangePattern == null) throw new ArgumentNullException(nameof(versionChangePattern));
            if (logWriter == null) throw new ArgumentNullException(nameof(logWriter));

            this.logWriter = logWriter;

            this.VersionChangePattern = versionChangePattern;
            if (!ParseVersion(versionChangePattern))
                throw new ArgumentException($"Invalid version pattern: {versionChangePattern}", nameof(versionChangePattern));
        }

        public bool ShouldCreateBackup { get; set; }
        public string ConstantVersion { get { return constantVersion; } }


        public VersionChangerBase[] VersionChangers
        {
            get { return versionChangers; }
        }


        public bool AllowOlderVersions { get; set; }

        public string VersionChangePattern { get; }

        private VersionChangerBase CreateVersionChanger(string newVersion)
        {
            if (string.IsNullOrEmpty(newVersion)) return null;

            if (newVersion == "*") return new DummyVersionChanger();

            int intVersion;
            if (!int.TryParse(newVersion, out intVersion))
                return null;

            switch (newVersion[0])
            {
                case '+':
                case '-':
                    return new IncrementVersionChanger(intVersion);
                default:
                    return new ConstantVersionChanger(intVersion);
            }

        }

        protected bool ParseVersion(string version)
        {
            string[] parts = version.Split('.');
            if (parts.Length > 4) return false;

            VersionChangerBase[] changers = new VersionChangerBase[parts.Length];

            bool constant = true;

            for (int i = 0; i < parts.Length; i++)
            {
                VersionChangerBase changer = CreateVersionChanger(parts[i]);
                constant &= changer is ConstantVersionChanger;

                if (changer == null) return false;
                changers[i] = changer;
            }

            if (constant)
            {
                foreach (ConstantVersionChanger changer in changers)
                    constantVersion += changer.NewVersion.ToString() + ".";
                constantVersion = constantVersion.Substring(0, constantVersion.Length - 1);
            }
            else
                constantVersion = string.Empty;
            versionChangers = changers;
            return true;
        }


        protected void WriteLine(string format, params object[] args)
        {
            logWriter.WriteLine(format, args);
        }

        public Version ProcessAssemblyInfo(string fileName)
        {
            WriteLine("Processing File {0}", fileName);
            string newFile = Path.Combine(Path.GetDirectoryName(fileName), "AssemblyInfo.new");

            Version newVersion = null;

            using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
            {

                using (StreamWriter writer = new StreamWriter(newFile, false, Encoding.Default))
                {
                    newVersion = ProcessAssemblyInfo(reader, writer);
                }
            }

            if (newVersion == null)
            {
                File.Delete(newFile);
                throw new InvalidOperationException("Assembly Version Attribute not found.");
            }

            if (ShouldCreateBackup)
            {
                try
                {
                    string backupFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)) + ".bak";
                    if (File.Exists(backupFile)) File.Delete(backupFile);
                    File.Move(fileName, backupFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Cannot create backup file", ex);
                }
            }

            try
            {
                File.Delete(fileName);
                File.Move(newFile, fileName);
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(newFile);
                }
                catch (IOException) { }

                throw new InvalidOperationException("Cannot update file", ex);
            }

            if (newVersion != null)
            {
                WriteLine("Version Changed to {0}", newVersion);
                return newVersion;
            }

            return null;
        }

        public Version ProcessAssemblyInfo(TextReader reader, TextWriter writer)
        {

            Regex regex = new Regex("(\\[assembly:.*AssemblyVersion\\(\")(.*)(\"\\)\\])");
            string allText = reader.ReadToEnd();
            var match = regex.Match(allText);
            if (match != null && match.Success)
            {
                var currentVersion = Version.Parse(match.Groups[2].Value);
                var changedVersion = new Version(ChangeVersion(currentVersion.ToString()));
                if (AllowOlderVersions || changedVersion > currentVersion)
                {
                    writer.Write(regex.Replace(allText, "${1}" + changedVersion + "${3}"));
                    return changedVersion;

                }
                writer.Write(allText);
                return currentVersion;
            }

            writer.Write(allText);
            return null;
        }

        public string ChangeVersion(string version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            string newVersion;
            string[] parts = version.Split('.');


            if (string.IsNullOrWhiteSpace(ConstantVersion))
            {
                for (int i = 0; i < parts.Length && i < VersionChangers.Length; i++)
                    parts[i] = VersionChangers[i].Change(parts[i]);

                newVersion = string.Join(".", parts);
            }
            else
                newVersion = ConstantVersion;

            return newVersion;
        }
    }

}
