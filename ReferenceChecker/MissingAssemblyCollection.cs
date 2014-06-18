using System.Reflection;

namespace CMS.Library.Addin
{
    class MissingAssemblyCollection : AssemblyCollectionBase<MissingReference>
    {
        protected override AssemblyName GetAssemblyName(MissingReference item)
        {
            return item.MissingAssembly;
        }
    }
}