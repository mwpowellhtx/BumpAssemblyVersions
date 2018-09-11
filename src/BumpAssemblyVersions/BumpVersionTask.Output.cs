using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static String;
    using static MessageImportance;
    using static VersionKind;

    // ReSharper disable once UnusedMember.Global
    public partial class BumpVersion
    {
        private IDictionary<VersionKind, string> ProjectVersions { get; }
            = new Dictionary<VersionKind, string>();

        private string GetProjectVersion(VersionKind key)
        {
            var result = ProjectVersions.TryGetValue(key, out var value) ? value : Empty;
            Log.LogMessage(Low, $"Getting Project Version for '{key}': '{result}' ");
            return result;
        }

        /// <summary>
        /// Gets the New <see cref="Version"/>.
        /// </summary>
        [Output]
        public string NewVersion => GetProjectVersion(VersionKind.Version);

        /// <summary>
        /// Gets the New <see cref="AssemblyVersion"/>.
        /// </summary>
        [Output]
        public string NewAssemblyVersion => GetProjectVersion(AssemblyVersion);

        /// <summary>
        /// Gets the New <see cref="FileVersion"/>.
        /// </summary>
        [Output]
        public string NewFileVersion => GetProjectVersion(FileVersion);

        /// <summary>
        /// Gets the New <see cref="InformationalVersion"/>.
        /// </summary>
        [Output]
        public string NewInformationalVersion => GetProjectVersion(InformationalVersion);

        /// <summary>
        /// Gets the New <see cref="PackageVersion"/>.
        /// </summary>
        [Output]
        public string NewPackageVersion => GetProjectVersion(PackageVersion);
    }
}
