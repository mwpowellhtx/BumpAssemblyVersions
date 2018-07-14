using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    internal class StreamBumpVersionServiceFixture<T> : StreamBumpVersionServiceBase<T>
        where T : Attribute
    {
        internal StreamBumpVersionServiceFixture(params IVersionProvider[] versionProviders)
            : base(versionProviders)
        {
        }
    }
}
