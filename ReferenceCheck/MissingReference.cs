using System;
using System.Reflection;

namespace Buildeploy.ReferenceCheck
{
    [Serializable]
    class MissingReference
    {
        internal AssemblyName MissingAssembly { get; set; }
        internal AssemblyName ReferencesBy { get; set; }
    }
}