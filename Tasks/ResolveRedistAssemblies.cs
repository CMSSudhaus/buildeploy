using Cms.Buildeploy.ReferenceCheck;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cms.Buildeploy.Tasks
{
    public class ResolveRedistAssemblies : Task
    {

        public ITaskItem[] Files { get; set; }

        [Output]
        public ITaskItem[] Result { get; set; }

        [Required]
        public string LookupPath { get; set; }

        public override bool Execute()
        {
            if (!Directory.Exists(LookupPath))
            {
                Log.LogError("LookUpPath '{0}' not found.", LookupPath);
                return false;
            }

            HashSet<string> resultFileNames = new HashSet<string>();
            if (Files != null)
            {
                foreach (var item in Files)
                {
                    string fileName = Path.GetFullPath(Path.Combine(LookupPath, item.GetMetadata("filename") + item.GetMetadata("extension")));
                    if (!CheckFileExists(fileName)) return false;
                    resultFileNames.Add(fileName);
                    if (string.Equals(item.GetMetadata("AddReferences"), "true", StringComparison.OrdinalIgnoreCase))
                        AddReferences(fileName, resultFileNames);
                }
            }

            Result = resultFileNames.Select(fn => new TaskItem(fn)).ToArray();
            return true;
        }

        private Assembly TryLoadAssembly(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    return Assembly.ReflectionOnlyLoadFrom(fileName);
            }
            catch (BadImageFormatException) { }
            catch (FileLoadException) { }

            return null;
        }

        private string GetAssemblyFileName(AssemblyName assemblyName)
        {
            string fileName = Path.GetFullPath(Path.Combine(LookupPath, assemblyName.Name + ".dll"));
            if (File.Exists(fileName))
                return fileName;

            return null;
        }
        private void AddReferences(string fileName, HashSet<string> resultFileNames, HashSet<string> visited)
        {
            if (visited.Contains(fileName))
                return;
            visited.Add(fileName);

            if (!resultFileNames.Contains(fileName))
                resultFileNames.Add(fileName);

            Assembly assembly = TryLoadAssembly(fileName);
            if (assembly != null)
            {
                var referencedAssemblies = assembly.GetReferencedAssemblies();
                foreach (var assemblyName in referencedAssemblies)
                {
                    if (!ReferenceChecker.IsFrameworkAssembly(assemblyName))
                    {
                        string referencedFileName = GetAssemblyFileName(assemblyName);

                        if (!string.IsNullOrEmpty(referencedFileName))
                            AddReferences(referencedFileName, resultFileNames, visited);
                    }
                }
            }
        }

        private void AddReferences(string fileName, HashSet<string> resultFileNames)
        {
            AddReferences(fileName, resultFileNames, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private bool CheckFileExists(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Log.LogError("File '{0}' not found.", fileName);
                return false;
            }

            return true;

        }
    }
}
