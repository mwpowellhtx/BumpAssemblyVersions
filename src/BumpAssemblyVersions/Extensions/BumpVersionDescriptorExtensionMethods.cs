using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Microsoft.Build.Utilities;
    using static VersionKind;

    /// <summary>
    /// 
    /// </summary>
    public static class BumpVersionDescriptorExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IAssemblyInfoBumpVersionService MakeAssemblyInfoBumpVersionService(
            this IBumpVersionDescriptor descriptor, TaskLoggingHelper log = null)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (descriptor.Kind)
            {
                case AssemblyVersion:

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Yielding '{typeof(AssemblyVersionAttribute).FullName}' bump version service.");
#endif

                    return new AssemblyInfoBumpVersionService<AssemblyVersionAttribute>(descriptor);

                case AssemblyFileVersion:

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Yielding '{typeof(AssemblyFileVersionAttribute).FullName}' bump version service.");
#endif

                    return new AssemblyInfoBumpVersionService<AssemblyFileVersionAttribute>(descriptor);

                case AssemblyInformationalVersion:

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Yielding '{typeof(AssemblyInformationalVersionAttribute).FullName}' bump version service.");
#endif

                    return new AssemblyInfoBumpVersionService<AssemblyInformationalVersionAttribute>(descriptor);
            }

#if TASK_LOGGING_HELPER_DIAGNOSTICS
            log?.LogWarning($"Did not yield bump version service for kind '{descriptor.Kind}'");
#endif

            return null;
        }

        /// <summary>
        /// Returns the <see cref="IProjectBasedBumpVersionService"/> corresponding with the
        /// <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static IProjectBasedBumpVersionService MakeProjectBasedBumpVersionService(
            this IBumpVersionDescriptor descriptor, TaskLoggingHelper log = null)
        {
            IEnumerable<VersionKind> GetKinds()
            {
                yield return VersionKind.Version;
                yield return AssemblyVersion;
                yield return FileVersion;
                yield return InformationalVersion;
                yield return PackageVersion;
            }

            return GetKinds().Contains(descriptor.Kind)
                ? new ProjectBasedBumpVersionService(descriptor) {Log = log}
                : null;
        }
    }
}
