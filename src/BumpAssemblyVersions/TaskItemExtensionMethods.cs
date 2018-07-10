using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;

    internal static class TaskItemExtensionMethods
    {
        private static IEnumerable<string> GetMetadataNames(this ITaskItem item)
            => item.MetadataNames.Cast<string>();

        /// <summary>
        /// Returns whether <paramref name="item"/> Has the <paramref name="metadataName"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="metadataName"></param>
        /// <returns></returns>
        internal static bool HasMetadataName(this ITaskItem item, string metadataName)
            => item.GetMetadataNames().Any(x => x == metadataName);

        private static T GetMetadataValue<T>(this ITaskItem item, string metadataName, Func<string, T> convert)
            => convert(item.GetMetadata(metadataName));

        internal static bool GetBooleanMetadata(this ITaskItem item, string metadataName)
            => GetMetadataValue(item, metadataName, bool.Parse);
    }
}
