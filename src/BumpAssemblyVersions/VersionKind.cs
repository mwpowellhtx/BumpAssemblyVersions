using System;
using System.Reflection;

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
        /// The CSPROJ equivalent of <see cref="AssemblyFileVersionAttribute"/>.
        /// </summary>
        /// <see cref="AssemblyFileVersionAttribute"/>
        /// <see cref="AssemblyFileVersion"/>
        FileVersion,

        /// <summary>
        /// The CSPROJ equivalent of <see cref="AssemblyInformationalVersionAttribute"/>.
        /// </summary>
        /// <see cref="AssemblyInformationalVersionAttribute"/>
        /// <see cref="AssemblyInformationalVersion"/>
        InformationalVersion,

        /// <summary>
        /// 
        /// </summary>
        PackageVersion,

        /// <summary>
        /// Be careful with this one as it serves a dual purposes, for either the
        /// CSPROJ equivalent of <see cref="AssemblyVersionAttribute"/>, or for
        /// <see cref="AssemblyVersionAttribute"/> in the AssemblyInfo.
        /// </summary>
        /// <see cref="AssemblyVersionAttribute"/>
        AssemblyVersion,

        /// <summary>
        /// For use with AssemblyInfo <see cref="AssemblyFileVersionAttribute"/>.
        /// </summary>
        /// <see cref="AssemblyFileVersionAttribute"/>
        /// <see cref="FileVersion"/>
        AssemblyFileVersion,

        /// <summary>
        /// For use with AssemblyInfo <see cref="AssemblyInformationalVersionAttribute"/>.
        /// </summary>
        /// <see cref="InformationalVersion"/>
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
