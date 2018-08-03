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

        // ReSharper disable UnusedMember.Global
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
        AssemblyInformationalVersion
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
            => s.ToNullableVersionKind() ?? VersionKind.None;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static VersionKind? ToNullableVersionKind(this string s)
        {
            foreach (VersionKind x in Enum.GetValues(typeof(VersionKind)))
            {
                if ($"{x}" == s)
                {
                    return x;
                }
            }

            return null;
        }
    }
}
