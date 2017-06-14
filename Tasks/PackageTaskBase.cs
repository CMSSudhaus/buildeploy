// -----------------------------------------------------------------------
// <copyright file="PackageTaskBase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Cms.Buildeploy.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Build.Framework;
    using System.Globalization;
    using System.Security;
    using System.IO;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System.Runtime.Versioning;
    using System.Reflection;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class PackageTaskBase : Task
    {
        private readonly HashSet<string> addedDirs = new HashSet<string>();
        private readonly HashSet<string> fileEntries = new HashSet<string>();

        public string ConfigName { get; set; }

        public string LocalProduct { get; set; }

        public bool Install { get; set; }

        public bool AutoUpdate { get; set; }

        public string Product { get; set; }

        public bool UseAssemblyProductName { get; set; }

        public string Publisher { get; set; }

        public bool UseConfigName { get; set; }

        public string WebsiteBasePath { get; set; }

        public string BaseUrl { get; set; }

        public ITaskItem ConfigFile { get; set; }


        public string CertificatePassword { get; set; }

        public ITaskItem Certificate { get; set; }

        public ITaskItem[] EntryPoints { get; set; }
        public ITaskItem[] EntryPointsUrls { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        public bool UrlParameters { get; set; }

        public string TargetFramework { get; set; }

        public string MinimumRequiredVersion { get; set; }

        [Required]
        public string Version { get; set; }

        public bool RequireLatestVersion { get; set; }

        public bool CreateDesktopShortcut { get; set; }

        public string IconFile { get; set; }

        public ITaskItem[] WebsiteFiles { get; set; }

        public ITaskItem[] PackageFilterRefs { get; set; }
        public string PackageFilter { get; set; }

        private string[] packageFilterParts;
        private string[] PackageFilterParts
        {
            get
            {
                if (packageFilterParts == null)
                {
                    if (!string.IsNullOrWhiteSpace(PackageFilter))
                        packageFilterParts = PackageFilter.Split(';');
                    else
                        packageFilterParts = new string[0];
                }

                return packageFilterParts;
            }
        }
        public bool CombineWithWebsite { get; set; }

        public bool LinkAssembliesWithManifestAsFile { get; set; }

        private SecureString GetCertPassword()
        {
            if (string.IsNullOrEmpty(CertificatePassword)) return null;

            SecureString secString = new SecureString();

            foreach (char c in CertificatePassword)
                secString.AppendChar(c);

            return secString;
        }

        #region Compression Methods

        private string AddDeploySuffix(string entryName)
        {
            if (!entryName.EndsWith(".application", StringComparison.OrdinalIgnoreCase) &&
                    !entryName.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase))
                return entryName + ".deploy";
            else
                return entryName;
        }


        private ITaskItem GetPackageFilterRef(ITaskItem item)
        {
            const string fileNameMetadata = "Filename";
            if (PackageFilterRefs == null || item == null) return null;
            return PackageFilterRefs.FirstOrDefault(r => r.GetMetadata(fileNameMetadata) == item.GetMetadata(fileNameMetadata));
        }
        private bool MatchFilter(ITaskItem fileItem)
        {
            fileItem = GetPackageFilterRef(fileItem);
            if (fileItem == null) return true;
            string fileFilter = fileItem.GetMetadata(nameof(PackageFilter));
            if (string.IsNullOrWhiteSpace(fileFilter))
                return true;

            string[] filterParts = fileFilter.Split(';');

            return filterParts.Intersect(PackageFilterParts).Any();
        }


        private bool CompressFiles(IList<PackageFileInfo> additionalFiles)
        {
            using (IPackageArchive package = CreatePackageArchive())
            {

                Log.LogMessage("Packaging {0} files", Files.Length);


                string targetDir = null;
                if (WebsiteFiles != null && WebsiteFiles.Length > 0 && CombineWithWebsite)
                {
                    targetDir = "ClickOnce";
                    CompressFileSet(package, WebsiteBasePath, null, WebsiteFiles
                        .Where(MatchFilter)
                        .Select(f => f.ItemSpec), false);
                }
                CompressFileSet(package, BasePath, targetDir, Files
                    .Where(MatchFilter)
                    .Select(f => f.ItemSpec), CombineWithWebsite);

                if (additionalFiles != null)
                {
                    foreach (PackageFileInfo info in additionalFiles)
                    {
                        CompressFile(package, info.SourcePath,
                            (string.IsNullOrEmpty(targetDir) ? string.Empty : targetDir + "/") + info.DestPath);
                    }
                }
                return package.Finish();
            }
        }

        protected abstract IPackageArchive CreatePackageArchive();


        [Required]
        public string BasePath { get; set; }




        private bool CompressFileSet(IPackageArchive zOutstream, string basePath, string targetDir, IEnumerable<string> fileNames, bool addDeploy)
        {
            addedDirs.Clear();
            fileEntries.Clear();

            basePath = Path.GetFullPath(basePath);
            // add files to zip
            foreach (string fn in fileNames)
            {
                string file = Path.GetFullPath(fn);

                if (!File.Exists(file))
                {
                    Log.LogError("File '{0}' not found.", file);
                    return false;
                }

                // the name of the zip entry
                string entryName;

                // determine name of the zip entry
                if (file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                {
                    entryName = file.Substring(basePath.Length);
                    if (entryName.Length > 0 && entryName[0] == Path.DirectorySeparatorChar)
                    {
                        entryName = entryName.Substring(1);
                    }

                    if (!string.IsNullOrEmpty(targetDir))
                        entryName = Path.Combine(targetDir, entryName);

                    // remember that directory was added to zip file, so
                    // that we won't add it again later
                    string dir = Path.GetDirectoryName(file);
                    if (!addedDirs.Contains(dir))
                    {
                        addedDirs.Add(dir);
                    }
                }
                else
                {
                    entryName = Path.GetFileName(file);
                }

                if (addDeploy) entryName = AddDeploySuffix(entryName);

                entryName = ReplaceDirectorySeparators(entryName);

                if (fileEntries.Contains(entryName))
                {
                    Log.LogError("Dublicate file was found: {0}", entryName);
                    return false;

                }

                CompressFile(zOutstream, file, entryName);
            }

            return true;
        }

        protected abstract string ReplaceDirectorySeparators(string entryName);

        private void CompressFile(IPackageArchive zOutstream, string file, string entryName)
        {

            fileEntries.Add(entryName);

            Log.LogMessage(MessageImportance.Low, "Adding {0}", entryName);

            using (FileStream fs = File.OpenRead(file)) //TODO: Exchange with CopyTo
            {
                zOutstream.AddEntry(entryName, File.GetLastWriteTime(file), fs);
            }
        }
        #endregion

        private ApplicationManifest CreateApplicationManifest(string entryPoint, out string configFileName, out string entryPointFilePath)
        {

            entryPointFilePath = null;
            string frameworkVersion;
            if (string.IsNullOrEmpty(TargetFramework))
                frameworkVersion = "3.5";
            else
            {
                FrameworkName fn = new FrameworkName(TargetFramework);
                frameworkVersion = fn.Version.ToString();
            }
            ApplicationManifest manifest = new ApplicationManifest(frameworkVersion);

            manifest.IsClickOnceManifest = true;
            manifest.IconFile = IconFile;
            configFileName = null;

            Dictionary<string, AssemblyIdentity> addedIdentities = new Dictionary<string, AssemblyIdentity>();
            string basePath = Path.GetFullPath(BasePath);
            foreach (var taskItem in Files.Where(MatchFilter))
            {
                string filePath = taskItem.GetMetadata("FullPath");
                string targetPath = null;
                string dir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                BaseReference reference = null;
                if (!dir.Equals(basePath, StringComparison.InvariantCultureIgnoreCase) &&
                    dir.StartsWith(basePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    int index = basePath.Length;
                    if (dir[index] == Path.DirectorySeparatorChar) index++;

                    targetPath = Path.Combine(dir.Substring(index), fileName);
                }


                AssemblyIdentity identity = null;
                try
                {
                    identity = AssemblyIdentity.FromFile(filePath);
                    if (LinkAssembliesWithManifestAsFile && HasEmbeddedManifest(filePath))
                        identity = null;
                }
                catch (BadImageFormatException)
                {
                }

                if (identity != null)
                {
                    string identityFullName = identity.GetFullName(AssemblyIdentity.FullNameFlags.All);
                    if (addedIdentities.ContainsKey(identityFullName))
                    {
                        throw new DuplicateAssemblyReferenceException(identityFullName);
                    }
                    else
                        addedIdentities.Add(identityFullName, identity);

                    AssemblyReference asmRef = new AssemblyReference(fileName);
                    reference = asmRef;
                    asmRef.AssemblyIdentity = identity;
                    manifest.AssemblyReferences.Add(asmRef);
                    if (manifest.EntryPoint == null &&
                        (string.IsNullOrEmpty(entryPoint) || string.Equals(entryPoint, fileName, StringComparison.InvariantCultureIgnoreCase)) &&
                        Path.GetExtension(fileName).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        configFileName = SetEntryPointAndConfig(manifest, filePath, asmRef);
                        entryPointFilePath = filePath;
                    }
                }
                else
                {
                    FileReference fileRef = new FileReference(fileName);
                    reference = fileRef;
                    manifest.FileReferences.Add(fileRef);
                }

                Log.LogMessage(MessageImportance.Low, "TargetPath for {0}: {1}", fileName, targetPath);
                reference.TargetPath = targetPath;
            }

            List<string> searchPaths = new List<string>();
            searchPaths.Add(BasePath);
            if (ConfigFile != null)
                searchPaths.Add(Path.GetDirectoryName(ConfigFile.ItemSpec));
            manifest.ResolveFiles(searchPaths.ToArray());
            manifest.UpdateFileInfo();

            TrustInfo trust = new TrustInfo();
            trust.IsFullTrust = true;
            manifest.TrustInfo = trust;
            if (manifest.EntryPoint == null)
                Log.LogError("Cannot determine EntryPoint. EntryPoint property = '{0}'", entryPoint ?? string.Empty);

            return manifest;
        }

        private string SetEntryPointAndConfig(ApplicationManifest manifest, string fileName, AssemblyReference asmRef)
        {
            string entryPointFileName = Path.GetFileName(fileName);
            manifest.EntryPoint = asmRef;
            manifest.AssemblyIdentity = AssemblyIdentity.FromFile(fileName);
            if (UseConfigName)
                manifest.AssemblyIdentity.Name = entryPointFileName + "_" + this.ConfigName;
            else
                manifest.AssemblyIdentity.Name = entryPointFileName;

            if (ConfigFile != null)
            {
                string configFilePath = Path.GetFullPath(ConfigFile.ItemSpec);
                FileReference configRef = new FileReference(configFilePath);
                string configFileName = entryPointFileName + ".config";
                configRef.TargetPath = configFileName;
                manifest.FileReferences.Add(configRef);

                return configFileName;
            }
            else
                return null;
        }

        private static bool HasEmbeddedManifest(string fileName)
        {

            Type manifestType = typeof(System.Deployment.Application.ApplicationDeployment).Assembly.GetType("System.Deployment.Application.Manifest.AssemblyManifest");
            try
            {
                object manifest = Activator.CreateInstance(manifestType, fileName);
                var property = manifestType.GetProperty("Id1ManifestPresent");
                if (property == null)
                    throw new InvalidOperationException("Property 'Id1ManifestPresent' not found.");

                return (bool)property.GetValue(manifest, null);

            }
            catch (TargetInvocationException)
            {
                return false;
            }
        }

        private DeployManifest CreateDeployManifest(ApplicationManifest applicationManifest, string applicationManifestPath,
            string applicationManifestName, string url, string entryPointFilePath)
        {

            DeployManifest manifest = new DeployManifest(TargetFramework);

            if (!string.IsNullOrWhiteSpace(url))
                manifest.DeploymentUrl = url;

            manifest.MapFileExtensions = true;
            manifest.Publisher = Publisher;
            manifest.Product = GetProductName(entryPointFilePath);
            manifest.TrustUrlParameters = UrlParameters;
            manifest.Install = Install;
            manifest.MinimumRequiredVersion = MinimumRequiredVersion;
            manifest.CreateDesktopShortcut = CreateDesktopShortcut;

            if (RequireLatestVersion)
                manifest.MinimumRequiredVersion = Version;
            if (AutoUpdate && Install)
            {
                manifest.UpdateEnabled = true;
                manifest.UpdateMode = UpdateMode.Foreground;

            }


            AssemblyReference assembly = new AssemblyReference(applicationManifestPath);
            AssemblyIdentity identity = new AssemblyIdentity(applicationManifest.AssemblyIdentity);
            if (UseConfigName)
                identity.Name = string.Format(CultureInfo.InvariantCulture, "{0}_{1}.app", Path.GetFileNameWithoutExtension(identity.Name), this.ConfigName);
            else
                identity.Name = string.Format(CultureInfo.InvariantCulture, "{0}.app", Path.GetFileNameWithoutExtension(identity.Name));

            identity.Type = string.Empty;

            manifest.AssemblyIdentity = identity;
            manifest.AssemblyReferences.Add(assembly);
            manifest.EntryPoint = assembly;
            manifest.ResolveFiles();
            manifest.UpdateFileInfo();
            assembly.TargetPath = applicationManifestName;

            manifest.ResolveFiles();
            manifest.UpdateFileInfo();
            return manifest;
        }

        private string GetProductName(string entryPointFilePath)
        {
            string productName = !string.IsNullOrEmpty(LocalProduct) ? LocalProduct : Product;

            if (UseAssemblyProductName)
            {
                if (string.IsNullOrWhiteSpace(entryPointFilePath))
                    throw new ArgumentException("Entrypoint not specified.", nameof(entryPointFilePath));


                string assemblyProductName = AssemblyAttributeHelper.InvokeWithDomain(h => h.GetProductName(entryPointFilePath));
                if (!string.IsNullOrWhiteSpace(assemblyProductName))
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} {1}", assemblyProductName, productName).Trim();
                }
            }

            return productName;
        }

        public bool SkipClickOnce { get; set; }

        public string EntryPoint { get; set; }

        public override bool Execute()
        {
            if (Certificate != null && !File.Exists(Certificate.ItemSpec))
            {
                Log.LogError("Cannot find certificate file '{0}'", Certificate.ItemSpec);
                return false;
            }
            //If no ClickOnce files, compressing the website only.
            if (Files == null || Files.Length == 0 || SkipClickOnce)
            {
                CompressFiles(null);
                return true;
            }

            List<PackageFileInfo> additionalFiles = new List<PackageFileInfo>();
            List<string> filesToDelete = new List<string>();
            if (EntryPoints != null && EntryPoints.Length > 0)
            {
                for (int i = 0; i < EntryPoints.Length; i++)
                {

                    if (!CreateManifests(EntryPoints[i].ItemSpec, GetEntryPointUrl(i), additionalFiles, filesToDelete))
                        return false;
                }

            }
            else
            {
                if (!CreateManifests(EntryPoint, null, additionalFiles, filesToDelete))
                    return false;
            }

            try
            {
                CompressFiles(additionalFiles);
            }
            finally
            {
                foreach (var fn in filesToDelete)
                    File.Delete(fn);
            }

            return true;
        }

        private string GetEntryPointUrl(int index)
        {

            var url = EntryPoints[index].GetMetadata(nameof(BaseUrl));
            if (!string.IsNullOrWhiteSpace(url)) return url;

            if (EntryPointsUrls != null && EntryPointsUrls.Length > index)
                return EntryPointsUrls[index].ItemSpec;

            return null;
        }

        private bool CreateManifests(string entryPoint, string baseUrl, List<PackageFileInfo> additionalFiles, List<string> filesToDelete)
        {
            string configFileName;
            string entryPointFilePath;
            ApplicationManifest appManifest;
            try
            {
                appManifest = CreateApplicationManifest(entryPoint, out configFileName, out entryPointFilePath);
            }
            catch (DuplicateAssemblyReferenceException ex)
            {
                Log.LogError("ClickOnce: {0}", ex.Message);
                return false;
            }

            string appManifestFileName = Path.GetFileName(appManifest.EntryPoint.SourcePath) + ".manifest";
            string deployManifestFileName = Path.GetFileName(appManifest.EntryPoint.SourcePath) + ".application";
            string deployManifestTempFileName = Path.GetTempFileName();
            string appManifestTempFileName = Path.GetTempFileName();

            ManifestWriter.WriteManifest(appManifest, appManifestTempFileName);
            SecurityUtilities.SignFile(Certificate.ItemSpec, GetCertPassword(), null, appManifestTempFileName);
            string deploymentUrl = BuildDeploymentUrl(deployManifestFileName, baseUrl);
            DeployManifest deployManifest = CreateDeployManifest(
                appManifest,
                appManifestTempFileName,
                appManifestFileName,
                deploymentUrl,
                entryPointFilePath);


            ManifestWriter.WriteManifest(deployManifest, deployManifestTempFileName);
            SecurityUtilities.SignFile(Certificate.ItemSpec, GetCertPassword(), null, deployManifestTempFileName);

            additionalFiles.Add(new PackageFileInfo(appManifestTempFileName, appManifestFileName));
            additionalFiles.Add(new PackageFileInfo(deployManifestTempFileName, deployManifestFileName));

            filesToDelete.Add(appManifestTempFileName);
            filesToDelete.Add(deployManifestTempFileName);
            if (ConfigFile != null && !string.IsNullOrEmpty(configFileName))
            {
                additionalFiles.Add(new PackageFileInfo(ConfigFile.ItemSpec, CombineWithWebsite ? AddDeploySuffix(configFileName) : configFileName));
            }

            return true;
        }

        private string BuildDeploymentUrl(string manifestFileName, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = BaseUrl;

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
                    baseUrl += "/";

                baseUrl += manifestFileName;
            }

            return baseUrl;
        }
    }

    public interface IPackageFileEntry
    {
        string SourcePath { get; }
        string DestPath { get; }
        DateTime DateTime { get; set; }
    }

    public interface IPackageArchive : IDisposable
    {
        void AddEntry(string entryName, DateTime dateTime, FileStream stream);
        bool Finish();
    }

    class PackageFileInfo
    {
        private readonly string sourcePath;
        private readonly string destPath;

        public PackageFileInfo(string sourcePath, string destPath)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destPath == null) throw new ArgumentNullException(nameof(destPath));

            this.sourcePath = sourcePath;
            this.destPath = destPath;
        }

        public string SourcePath => sourcePath;
        public string DestPath => destPath;
    }

}
