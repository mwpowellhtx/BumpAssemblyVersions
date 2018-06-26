using System;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using static Type;
    using static BindingFlags;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class ChangedVersionProviderTests : VersionProviderTestsBase<ChangedVersionProvider>
    {
        /// <summary>
        /// This does not ever effect production code, but it is something we do
        /// in order to vet the test fixtures with a bit of a handshake.
        /// </summary>
        /// <param name="providerType"></param>
        private static void VerifyProviderTypeHasExpectedCtor(Type providerType)
        {
            // It should be the case anyway, but extend that handshake to an obvious smoke test.
            Assert.Equal(ProviderType, providerType);
            var ctor = providerType.GetConstructor(Instance | NonPublic, DefaultBinder, new[] {typeof(bool?)}, null);
            // We do not want to Invoke it, we just need to make sure it is there.
            Assert.NotNull(ctor);
            // Same as saying, IsInternal, in IL speak.
            Assert.True(ctor.IsAssembly);
            Assert.Collection(ctor.GetParameters(), arg =>
            {
                // We are expecting: "bool? changed = null", quite literally.
                Assert.Equal("changed", arg.Name);
                Assert.True(arg.HasDefaultValue);
                Assert.Null(arg.DefaultValue);
            });
        }

        /// <summary>
        /// Returns the Created <see cref="ChangedVersionProvider"/>. Basically in which,
        /// <see cref="VersionProviderBase.Changed"/> may be set accordingly depending upon the
        /// desired outcome.
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        protected override ChangedVersionProvider CreateNew()
        {
            var provider = base.CreateNew();
            VerifyProviderTypeHasExpectedCtor(ProviderType);
            Assert.True(provider.Changed);
            return provider;
        }

#pragma warning disable xUnit1008 // Test data attribute should only be used on a Theory
        /// <summary>
        /// Do a bit of verification that our <see cref="ChangedVersionProvider"/> fixture is setup as we expect it to be.
        /// </summary>
        /// <param name="beforeChange"></param>
        /// <param name="afterChange"></param>
        /// <param name="init"></param>
        /// <param name="expectedTried"></param>
        /// <inheritdoc />
        [InlineData("0", "1", null, true)]
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange, string afterChange
            , InitializeProviderCallback<ChangedVersionProvider> init, bool expectedTried)
        {
            var provider = CreateNew();
            // We do not actually use the Out Parameter in this instance on account it Should Throw.
            Assert.Throws<NotImplementedException>(() => provider.TryChange(beforeChange, out var _));
        }
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory

    }
}
