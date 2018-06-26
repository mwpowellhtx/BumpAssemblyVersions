﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using static Type;
    using static BindingFlags;

    internal static class VersionProviderRegistry
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

            var ctors = types.Select(x => x.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null)).ToArray();

            return ctors.Select(ctor => ctor?.Invoke(new object[] { }) as IVersionProvider)
                .Where(provider => provider != null);
        }

        private static IDictionary<string, IVersionProvider> _providers;

        internal static IDictionary<string, IVersionProvider> Providers
            => _providers
               ?? (_providers = GetProviders().ToDictionary(provider => provider.Name, provider => provider));
    }
}
