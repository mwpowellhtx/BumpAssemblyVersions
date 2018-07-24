using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    internal class AssemblyInfoBumpVersionServiceFixture<T> : AssemblyInfoBumpVersionService<T>
        where T : Attribute
    {
        internal AssemblyInfoBumpVersionServiceFixture(IBumpVersionDescriptor descriptor)
            : base(descriptor)
        {
        }
    }
}
