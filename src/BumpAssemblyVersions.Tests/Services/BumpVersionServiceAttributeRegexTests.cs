using System;
using System.Collections.Generic;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract partial class BumpVersionServiceAttributeRegexTests<T>
        where T : Attribute
    {
        /// <summary>
        /// Provides access to an <see cref="ITestOutputHelper"/>.
        /// </summary>
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected BumpVersionServiceAttributeRegexTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Provides the FixtureAttributeType.
        /// </summary>
        protected static readonly Type FixtureAttributeType = typeof(T);

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Provides the FixtureType.
        /// </summary>
        protected static readonly Type FixtureType = typeof(AssemblyInfoBumpVersionServiceFixture<T>);

        // ReSharper disable once UnusedMember.Local
        /// <summary>
        /// Provides some <see cref="IVersionProvider"/> vetting the tests.
        /// </summary>
        /// <inheritdoc />
        private class VersionProviderSamenessComparer : EqualityComparer<IVersionProvider>
        {
            public override bool Equals(IVersionProvider x, IVersionProvider y)
                => !(x == null || y == null) && ReferenceEquals(x, y);

            public override int GetHashCode(IVersionProvider obj)
                => (obj?.Id ?? Guid.Empty).GetHashCode();
        }

        /// <summary>
        /// Returns a Created Fixture. Should not throw any exceptions, but we do not need to test
        /// for this explicitly. Running successfully through passing is sufficient evidence along
        /// these lines.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="descriptor"></param>
        /// <param name="verify"></param>
        /// <returns></returns>
        internal static IAssemblyInfoBumpVersionService CreateServiceFixture(ServiceMode mode
            , IBumpVersionDescriptor descriptor = null
            , Action<IAssemblyInfoBumpVersionService> verify = null)
        {
            var serviceFixture = new AssemblyInfoBumpVersionServiceFixture<T>(descriptor) {Mode = mode};

            VerifyFixture(serviceFixture, mode);

            verify?.Invoke(serviceFixture);

            return serviceFixture;
        }

        /// <summary>
        /// Verifies the Fixture <paramref name="service"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="service"></param>
        /// <param name="expectedMode"></param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void VerifyFixture<TService>(TService service
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            , ServiceMode expectedMode)
            where TService : class, IBumpVersionService
        {
            Assert.NotNull(service);
            Assert.Equal(expectedMode, service.Mode);
        }

        //private static IVersionProvider NoOp => new NoOpVersionProvider();

        //private static IVersionProvider Unknown => new UnknownVersionProvider();
    }
}
