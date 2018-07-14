using System;

namespace Bav
{
    internal static class AttributeUsageExtensionMethods
    {
        private static readonly Type AttributeType = typeof(Attribute);

        internal static string ToLongName(this Type type) => type.Name;

        internal static string ToShortName(this Type type)
            => !type.Name.EndsWith(AttributeType.Name)
                ? type.Name
                : type.Name.Substring(0, type.Name.Length - AttributeType.Name.Length);
    }
}
