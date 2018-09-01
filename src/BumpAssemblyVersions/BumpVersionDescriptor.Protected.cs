namespace Bav
{
    using static VersionProviderTemplateRegistry;

    internal partial class BumpVersionDescriptor
    {
        protected BumpVersionDescriptor(
            IVersionProvider majorProviderTemplate = null
            , IVersionProvider minorProviderTemplate = null
            , IVersionProvider patchProviderTemplate = null
            , IVersionProvider buildProviderTemplate = null
            , IVersionProvider releaseProviderTemplate = null
        )
        {
            /* At this level do not Clone anything, which at this level Providers are simply
             * fulfilling the Provider Template role. Cloning will happen when we deliver the
             * set of Internal Providers just prior to the Bump Request. */

            IVersionProvider ProviderOrNoOp(IVersionProvider provider)
                => provider ?? (IVersionProvider) Registry.NoOp;

            // Get the Provider Template or NoOp from the Caller.
            MajorProviderTemplate = ProviderOrNoOp(majorProviderTemplate);
            MinorProviderTemplate = ProviderOrNoOp(minorProviderTemplate);
            PatchProviderTemplate = ProviderOrNoOp(patchProviderTemplate);
            BuildProviderTemplate = ProviderOrNoOp(buildProviderTemplate);
            ReleaseProviderTemplate = ProviderOrNoOp(releaseProviderTemplate);
        }
    }
}
