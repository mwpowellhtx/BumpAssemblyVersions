using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using static String;
    using static Type;
    using static BindingFlags;

    internal static class VersionProviderTemplateRegistry
    {
        internal static IEnumerable<Type> GetProviderTypes()
        {
            var providerType = typeof(IVersionProvider);

            foreach (var type in providerType.Assembly.GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && providerType.IsAssignableFrom(x)))
            {
                yield return type;
            }
        }


        internal static IEnumerable<IVersionProvider> GetProviders()
        {
            var types = GetProviderTypes().ToArray();

            // ReSharper disable once IdentifierTypo
            var ctors = types.Select(x => x.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null)).ToArray();

            return ctors.Select(ctor => ctor?.Invoke(new object[] { }) as IVersionProvider)
                .Where(provider => provider != null);
        }

        private static IDictionary<string, IVersionProvider> _providers;

        // TODO: TBD: getting them here as a Dictionary; may consider whether an ExpandoObject would make sense...
        // TODO: TBD: which may also be treated like a kind of string-keyed Dictionary
        internal static IDictionary<string, IVersionProvider> Providers
            => _providers
               ?? (_providers = GetProviders().ToDictionary(provider => provider.Name, provider => provider));

        private static dynamic _registry;

        /// <summary>
        /// Gets the Dynamic Registry. Dynamic Field Names are basically based upon
        /// the <see cref="IVersionProvider"/> concrete implementations, sans the
        /// <see cref="IVersionProvider"/> suffix. Additionally, also accounts for the
        /// <see cref="IMultipartVersionProvider"/> suffix.
        /// </summary>
        internal static dynamic Registry
        {
            get
            {
                dynamic GetRegistry()
                {
                    string GetSuffix<T>(object provider)
                        where T : IVersionProvider
                    {
                        var typeName = typeof(T).Name;
                        return provider is T ? typeName.Substring(1, typeName.Length - 1) : null;
                    }

                    // TODO: TBD: may want to rethink why we are using an EO here...
                    // TODO: TBD: and how it is keyed... positionally? Major, Minor, Patch, Build, etc ... ?
                    var result = new ExpandoObject();
                    var dictionary = (IDictionary<string, object>) result;
                    foreach (var provider in Providers.Values)
                    {
                        // Do not use the Providers Key which is the Name but rather the Provider Types themselves.
                        var suffix = GetSuffix<IMultipartVersionProvider>(provider)
                                     ?? GetSuffix<IVersionProvider>(provider);
                        dictionary[provider.GetType().Name.Replace(suffix, Empty)] = provider;
                    }

                    return result;
                }

                return _registry ?? (_registry = GetRegistry());
            }
        }
    }
}
