using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Cms.Buildeploy.ReferenceCheck
{
    abstract class AssemblyCollectionBase<TElement> : Collection<TElement>
    {
        public TElement Find(AssemblyName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this.FirstOrDefault(item => GetAssemblyName(item).FullName == name.FullName);
        }

        public TElement FindLazy(AssemblyName fullAssemblyName)
        {
            if (fullAssemblyName == null) throw new ArgumentNullException(nameof(fullAssemblyName));
            return this.FirstOrDefault(item => LazyMatch(GetAssemblyName(item), fullAssemblyName));
        }

        private static bool LazyMatch(AssemblyName findWhat, AssemblyName assemblyName)
        {
            if (findWhat == null) throw new ArgumentNullException(nameof(findWhat));
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

            return (findWhat.Name == null || assemblyName.Name == findWhat.Name) &&
                   (findWhat.Version == null || assemblyName.Version == findWhat.Version) &&
                   (findWhat.GetPublicKeyToken() == null ||
                    (assemblyName.GetPublicKeyToken() != null &&
                     assemblyName.GetPublicKeyToken().SequenceEqual(findWhat.GetPublicKeyToken())));
        }

        protected abstract AssemblyName GetAssemblyName(TElement item);
    }
}