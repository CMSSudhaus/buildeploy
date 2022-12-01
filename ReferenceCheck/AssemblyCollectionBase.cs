using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Cms.Buildeploy.ReferenceCheck
{
    abstract class AssemblyCollectionBase<TElement> : Collection<TElement> where TElement : class
    {
        private const string neutralCultureName = "neutral";
        private readonly Dictionary<string, AssemblyRedirect> redirects = new Dictionary<string, AssemblyRedirect>(StringComparer.OrdinalIgnoreCase);
        public TElement Find(AssemblyName name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return FindByFullName(name) ?? FindRedirected(name);
        }

        private TElement FindByFullName(AssemblyName name)
        {
            return this.FirstOrDefault(item => RemoveCulture(GetAssemblyName(item)).FullName == RemoveCulture(name).FullName);
        }

        private AssemblyName RemoveCulture(AssemblyName assemblyName)
        {
            var result = (AssemblyName)assemblyName.Clone();
            result.CultureInfo = null;
            return result;
        }
        private TElement FindRedirected(AssemblyName assemblyName)
        {
            if (redirects.TryGetValue(assemblyName.Name, out var redirect))
            {
                if (redirect.RedirectFromLowRange <= assemblyName.Version && redirect.RedirectFromHighRange >= assemblyName.Version
                    && AreTokensEqual(redirect.RedirectTo.GetPublicKeyToken(), assemblyName.GetPublicKeyToken()))
                {
                    return FindByFullName(redirect.RedirectTo);
                }
            }

            return default;
        }


        private bool AreTokensEqual(byte[] token1, byte[] token2)
        {
            if (token1 == null || token2 == null)
                return token1 == token2;

            return token1.SequenceEqual(token2);
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
        internal void AddRedirect(Version redirectToLowRange, Version redirectToHighRange, AssemblyName redirectTo)
        {
            if (redirectToLowRange is null)
            {
                throw new ArgumentNullException(nameof(redirectToLowRange));
            }

            if (redirectToHighRange is null)
            {
                throw new ArgumentNullException(nameof(redirectToHighRange));
            }

            if (redirectTo is null)
            {
                throw new ArgumentNullException(nameof(redirectTo));
            }

            redirects.Add(redirectTo.Name, new AssemblyRedirect(redirectToLowRange, redirectToHighRange, redirectTo));
        }

        internal void ReadRedirectsFromConfigXmlString(string xml)
        {
            using (var stringReader = XmlReader.Create(new StringReader(xml)))
            {
                var document = XDocument.Load(stringReader);
                ReadRedirectsFromConfig(document);

            }
        }

        private void ReadRedirectsFromConfig(XDocument document)
        {
            var dependendAssemblies = document.Descendants(GetXName("dependentAssembly")).ToList();
            var configRedirects = dependendAssemblies.Select(redirect => CreateRedirect(redirect)).ToList();

            foreach (var redirect in configRedirects)
            {
                this.redirects.Add(redirect.RedirectTo.Name, redirect);
            }
        }

        private static XName GetXName(string localName)
        {
            return XName.Get(localName, "urn:schemas-microsoft-com:asm.v1");
        }

        private static AssemblyRedirect CreateRedirect(XElement dependentAssemblyElement)
        {

            XElement identityElement = dependentAssemblyElement.Element(GetXName("assemblyIdentity"));
            string name = identityElement.Attribute("name").Value;
            string tokenString = identityElement.Attribute("publicKeyToken").Value;
            string cultureString = identityElement.Attribute("culture")?.Value ?? neutralCultureName;
            XElement bindingRedirectElement = dependentAssemblyElement.Element(GetXName("bindingRedirect"));
            string versionRangeString = bindingRedirectElement.Attribute("oldVersion").Value;
            string newVersion = bindingRedirectElement.Attribute("newVersion").Value;
            var versionRange = ParseVersionRange(versionRangeString);

            return new AssemblyRedirect(versionRange.Low, versionRange.High, new AssemblyName($"{name}, Version={newVersion}, culture={cultureString}, PublicKeyToken={tokenString}"));
        }

        private static VersionRange ParseVersionRange(string versionRange)
        {
            var versionParts = versionRange.Split('-');
            if (versionParts.Length != 2)
            {
                throw new ArgumentException("Invalid version range", nameof(versionRange));
            }

            var lowRange = new Version(versionParts[0]);
            var highRange = new Version(versionParts[1]);
            return new VersionRange(lowRange, highRange);
        }
    }
}