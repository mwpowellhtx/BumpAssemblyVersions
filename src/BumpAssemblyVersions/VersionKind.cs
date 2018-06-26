using System;
using System.Linq;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public enum VersionKind
    {
        /// <summary>
        /// 
        /// </summary>
        Version,

        /// <summary>
        /// 
        /// </summary>
        PackageVersion,

        /// <summary>
        /// 
        /// </summary>
        AssemblyVersion,

        /// <summary>
        /// 
        /// </summary>
        FileVersion
    }

    /// <summary>
    /// 
    /// </summary>
    public static class VersionKindExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static VersionKind ToVersionKind(this string s)
            => Enum.GetValues(typeof(VersionKind))
                .OfType<VersionKind>().Single(x => $"{x}" == s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static VersionKind? ToNullableVersionKind(this string s)
            => Enum.GetValues(typeof(VersionKind))
                .OfType<VersionKind?>().SingleOrDefault(x => $"{x.Value}" == s);
    }
}
