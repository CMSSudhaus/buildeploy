using System;
using System.Reflection;

namespace CMS.Library.Addin
{
    [Serializable]
    class MissingReference
    {
        internal AssemblyName MissingAssembly { get; set; }
        internal AssemblyName ReferencesBy { get; set; }
    }
}