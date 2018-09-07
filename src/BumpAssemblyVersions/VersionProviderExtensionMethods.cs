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
    using static VersionProviderTemplateRegistry;

    internal static class VersionProviderExtensionMethods
    {
        private static IVersionProvider Unknown => (IVersionProvider) ((IVersionProvider) Registry.Unknown).Clone();
        //private static IVersionProvider Unknown => new UnknownVersionProvider();

        private static IVersionProvider LookupVersionProvider(this string providerRequest)
        {
            var providerType = typeof(IVersionProvider);

            Type requestedType;

            try
            {
                requestedType = providerType.Assembly.GetTypes()
                    .Where(type => providerType.IsAssignableFrom(type)
                                   && type.IsClass
                                   && !(type.IsInterface || type.IsAbstract))
                    .SingleOrDefault(type => type.Name.StartsWith(providerRequest));
            }
            catch (InvalidOperationException ioex)
            {
                throw new InvalidOperationException(
                    $"Unable to determine '{typeof(IVersionProvider).FullName}'"
                    + $" given '{providerRequest}'.", ioex);
            }

            if (requestedType == null)
            {
                return Unknown;
            }

            var ctor = requestedType.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null);

            var provider = ctor?.Invoke(new object[] { }) as IVersionProvider;

            if (provider?.ForInternalUseOnly == true)
            {
                throw new InvalidOperationException(
                    $"Invalid version provider '{requestedType.FullName}' requested."
                    + " For internal use only.");
            }

            return provider ?? Unknown;
        }

        private static IVersionProvider NoOp => (IVersionProvider) ((IVersionProvider) Registry.NoOp).Clone();

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
                    : NoOp;

            var provider = Lookup();

            int ParseInteger(string s) => int.Parse(s);
            bool ParseBoolean(string s) => bool.Parse(s);

            // ReSharper disable once ImplicitlyCapturedClosure
            void SetPropertyFromMetadata<T>(string metadataName, Func<string, T> convert
                , Action<IVersionProvider, T> setProviderAttribute)
            {
                if (!item.HasMetadataName(metadataName))
                {
                    return;
                }

                setProviderAttribute(provider, convert(item.GetMetadata(metadataName)));
            }

            SetPropertyFromMetadata($"{preq}{nameof(IVersionProvider.MayReset)}"
                , ParseBoolean, (versionProvider, x) => versionProvider.MayReset = x);

            // May specify whether to UseUtc across the entire request, or for specific Version Elements.
            SetPropertyFromMetadata($"{preq}{nameof(IVersionProvider.UseUtc)}"
                , ParseBoolean, (versionProvider, x) => versionProvider.SetTimestamp(timestamp, x));

            /* Most of the Providers are "vanilla" with base options. However, in a
             couple of instances there are specialized options that we can expect. */

            // ReSharper disable once InvertIf
            if (provider is PreReleaseIncrementVersionProvider preReleaseProvider)
            {
                string FilterLabel(string s)
                {
                    s = s.Trim();

                    /* TODO: TBD: may want this to be a subset involving just the alpha characters, and maybe
                     * the hypen as well, but not the digits on account that is the Value of this Provider. */

                    const string acceptablePattern = @"^[0-9A-Za-z\-]+$";

                    if (IsNullOrEmpty(s))
                    {
                        // This is acceptable.
                    }
                    else if (!IsMatch(s, acceptablePattern))
                    {
                        /* This according to Semantic Versioning 2.0.0: http://semver.org/#spec-item-9.
                         * Not matching now, however, is not acceptable. */
                        throw new InvalidOperationException(
                            $"Label '{s}' contains invalid Semantic Version characters '{acceptablePattern}'."
                        );
                    }

                    return s;
                }

                SetPropertyFromMetadata($"{preq}{nameof(preReleaseProvider.Label)}"
                    , FilterLabel, (_, x) => preReleaseProvider.Label = x);

                SetPropertyFromMetadata($"{preq}{nameof(preReleaseProvider.ValueWidth)}"
                    , ParseInteger, (_, x) => preReleaseProvider.ValueWidth = x);

                SetPropertyFromMetadata($"{preq}{nameof(preReleaseProvider.ShouldDiscard)}"
                    , ParseBoolean, (_, x) => preReleaseProvider.ShouldDiscard = x);
            }

            return provider;
        }
    }
}
