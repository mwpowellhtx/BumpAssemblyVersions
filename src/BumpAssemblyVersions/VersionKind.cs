using System;
using System.Linq;

namespace Bav
{
    // TODO: TBD: thus far working through the AssemblyInfo bumps... will need to work on the Xml CSPROJ based bumps next...
    /// <summary>
    /// 
    /// </summary>
    public enum VersionKind
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        Version,

        /// <summary>
        /// 
        /// </summary>
        FileVersion,

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
        AssemblyFileVersion,

        /// <summary>
        /// 
        /// </summary>
        AssemblyInformationVersion
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
