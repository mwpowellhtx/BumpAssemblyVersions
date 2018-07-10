using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static BindingFlags;
    using static Regex;
    using static String;
    using static Type;
    using static VersionProviderExtensionMethods.MetadataNames;

    internal static class VersionProviderExtensionMethods
    {
        internal static class MetadataNames
        {
            private const string Major = nameof(Major);
            private const string Minor = nameof(Minor);
            private const string Patch = nameof(Patch);
            private const string Build = nameof(Build);
            private const string Release = nameof(Release);
            private const string Provider = nameof(Provider);
            private const string MajorProvider = Major + Provider;
            private const string MinorProvider = Minor + Provider;
            private const string PatchProvider = Patch + Provider;
            private const string BuildProvider = Build + Provider;
            private const string ReleaseProvider = Release + Provider;
        }

        private static IVersionProvider Unknown() => new UnknownVersionProvider();

        private static IVersionProvider LookupVersionProvider(this string providerRequest)
        {
            var providerType = typeof(IVersionProvider);

            var requestedType = providerType.Assembly.GetTypes()
                .Where(type => providerType.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
                .SingleOrDefault(type => type.Name.StartsWith(providerRequest));

            if (requestedType == null)
            {
                return Unknown();
            }

            var ctor = requestedType.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null);

            var provider = ctor?.Invoke(new object[] { }) as IVersionProvider;

            if (provider?.ForInternalUseOnly == true)
            {
                throw new InvalidOperationException(
                    $"Invalid version provider '{requestedType.FullName}' requested."
                    + " For internal use only.");
            }

            return provider ?? Unknown();
        }

        private static IVersionProvider NoOp() => new NoOpVersionProvider();

        /// <summary>
        /// Returns a <see cref="IVersionProvider"/> corresponding to the
        /// <paramref name="item"/>, <paramref name="providerRequest"/>, and
        /// <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="providerRequest"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        /// <see cref="!:http://semver.org/#spec-item-9"/>
        internal static IVersionProvider ToVersionProvider(this ITaskItem item
            , string providerRequest, DateTime timestamp)
        {
            var preq = providerRequest;

            IVersionProvider Lookup()
                => item.HasMetadataName(preq)
                    ? item.GetMetadata(preq).LookupVersionProvider()
                    : NoOp();

            var provider = Lookup();

            // ReSharper disable once ImplicitlyCapturedClosure
            void SetPropertyFromMetadata<T>(string metadataName, Func<string, T> convert
                , Action<IVersionProvider, T> setter)
            {
                if (!item.HasMetadataName(metadataName))
                {
                    return;
                }

                setter(provider, convert(item.GetMetadata(metadataName)));
            }

            SetPropertyFromMetadata($"{preq}{nameof(IVersionProvider.MayReset)}"
                , bool.Parse, (versionProvider, x) => versionProvider.MayReset = x);

            // May specify whether to UseUtc across the entire request, or for specific Version Elements.
            SetPropertyFromMetadata($"{nameof(IVersionProvider.UseUtc)}"
                , bool.Parse, (versionProvider, x) => versionProvider.SetTimestamp(timestamp, x));

            SetPropertyFromMetadata($"{preq}{nameof(IVersionProvider.UseUtc)}"
                , bool.Parse, (versionProvider, x) => versionProvider.SetTimestamp(timestamp, x));

            /* Most of the Providers are "vanilla" with base options. However, in a
             couple of instances there are specialized options that we can expect. */

            // ReSharper disable once InvertIf
            if (provider is PreReleaseIncrementVersionProvider preReleaseProvider)
            {
                string FilterLabel(string s)
                {
                    if (IsNullOrEmpty(s))
                    {
                        // This is acceptable.
                    }
                    else
                    {
                        // This according to Semantic Versioning 2.0.0: http://semver.org/#spec-item-9.
                        var isMatch = IsMatch(s, @"^[0-9A-Za-z\-]+$");

                        // This, however, is not acceptable.
                        if (!isMatch)
                        {
                            throw new InvalidOperationException(
                                $"Label '{s}' contains invalid Semantic Version characters (0-9A-Za-z-)."
                            );
                        }
                    }

                    return s;
                }

                int ParseValueWidth(string s) => int.Parse(s);
                bool ParseShouldDiscard(string s) => bool.Parse(s);

                SetPropertyFromMetadata($"{preq}{nameof(PreReleaseIncrementVersionProvider.ValueWidth)}"
                    , FilterLabel, (_, x) => preReleaseProvider.Label = x);

                SetPropertyFromMetadata($"{preq}{nameof(PreReleaseIncrementVersionProvider.ValueWidth)}"
                    , ParseValueWidth, (_, x) => preReleaseProvider.ValueWidth = x);

                SetPropertyFromMetadata($"{preq}{nameof(PreReleaseIncrementVersionProvider.ShouldDiscard)}"
                    , ParseShouldDiscard, (_, x) => preReleaseProvider.ShouldDiscard = x);
            }

            return provider;
        }
    }
}
