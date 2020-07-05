using System;

namespace Bav
{
    // ReSharper disable once PartialTypeWithSinglePart
    /// <summary>
    /// Internally we will provide <see cref="IVersionProvider"/> Templates for test purposes.
    /// </summary>
    internal partial class BumpVersionDescriptorFixture : BumpVersionDescriptor
    {
        /// <summary>
        /// Sets the <see cref="BumpVersionDescriptor.DescriptorTimestamp"/> to the Value for
        /// Internal test purposes.
        /// </summary>
        internal DateTime InternalDescriptorTimestamp
        {
            set => DescriptorTimestamp = value;
        }

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="majorProviderTemplate"></param>
        /// <param name="minorProviderTemplate"></param>
        /// <param name="patchProviderTemplate"></param>
        /// <param name="buildProviderTemplate"></param>
        /// <param name="releaseProviderTemplate"></param>
        internal BumpVersionDescriptorFixture(
            IVersionProvider majorProviderTemplate = null
            , IVersionProvider minorProviderTemplate = null
            , IVersionProvider patchProviderTemplate = null
            , IVersionProvider buildProviderTemplate = null
            , IVersionProvider releaseProviderTemplate = null
        )
            : base(
                majorProviderTemplate
                , minorProviderTemplate
                , patchProviderTemplate
                , buildProviderTemplate
                , releaseProviderTemplate
            )
        {
        }

        /// <summary>
        /// See also <see cref="BumpVersionDescriptor.Id"/>.
        /// </summary>
        /// <returns></returns>
        /// <see cref="BumpVersionDescriptor.Id"/>
        public override string ToString() => $"{Id:D}";
    }
}
