using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Buildeploy.Tasks
{
    public class AssemblyAttributeHelper : MarshalByRefObject
    {
        public string GetProductName(string assemblyPath)
        {
            var asm = Assembly.LoadFrom(assemblyPath);
            var productAttribute = asm.GetCustomAttribute<AssemblyProductAttribute>();
            if (productAttribute != null)
            {
                return productAttribute.Product;

            }

            return null;
        }

        internal static T InvokeWithDomain<T>( Func<AssemblyAttributeHelper, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            AppDomain domain = AppDomain.CreateDomain(nameof(AssemblyAttributeHelper));
            try
            {
                Type myType = typeof(AssemblyAttributeHelper);
                AssemblyAttributeHelper helper = (AssemblyAttributeHelper)domain.CreateInstanceFromAndUnwrap(myType.Assembly.Location, myType.FullName);
                var result = func(helper);
                return result;
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }
}
