using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace CMS.Library.Addin
{
    abstract class AssemblyCollectionBase<TElement> : Collection<TElement>
    {
        public TElement Find(AssemblyName name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return this.FirstOrDefault(item => GetAssemblyName(item).FullName == name.FullName);
        }

        public TElement FindLazy(AssemblyName fullAssemblyName)
        {
            if (fullAssemblyName == null) throw new ArgumentNullException("fullAssemblyName");
            return this.FirstOrDefault(item => LazyMatch(GetAssemblyName(item), fullAssemblyName));
        }

        private static bool LazyMatch(AssemblyName findWhat, AssemblyName assemblyName)
        {
            if (findWhat == null) throw new ArgumentNullException("findWhat");
            if (assemblyName == null) throw new ArgumentNullException("assemblyName");

            return (findWhat.Name == null || assemblyName.Name == findWhat.Name) &&
                   (findWhat.Version == null || assemblyName.Version == findWhat.Version) &&
                   (findWhat.GetPublicKeyToken() == null ||
                    (assemblyName.GetPublicKeyToken() != null &&
                     assemblyName.GetPublicKeyToken().SequenceEqual(findWhat.GetPublicKeyToken())));
        }

        protected abstract AssemblyName GetAssemblyName(TElement item);
    }
}