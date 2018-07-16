using System;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// We do not need a whole slew of test cases for this one. We only need to demonstrate that
    /// an <see cref="Attribute"/> is Unsupported exactly Once. We cannot negatively affirm nor
    /// deny whether an <see cref="Attribute"/> is Supported from this perspective on account of
    /// the fact that any access to the Fixture would result in the static exception being thrown.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UnsupportedAttributeBumpVersionServiceTests<T>
        where T : Attribute
    {
        private ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected UnsupportedAttributeBumpVersionServiceTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Verify_Attribute_Unsupported()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Action create = () => new StreamBumpVersionServiceFixture<T>();

            // We do not need many examples of this really.
            create.Throws<TypeInitializationException>(tex =>
            {
                tex.InnerException.VerifyThrownExeption(OutputHelper
                    , (InvalidOperationException ioex, ITestOutputHelper oh) =>
                    {
                        oh.WriteLine($"Message: {ioex.Message}");
                        oh.WriteLine($"Stack Trace: {ioex.StackTrace}");
                        var message = ioex.Message;
                        Assert.NotNull(message);
                        Assert.NotEmpty(message);
                        Assert.Contains(nameof(UnsupportedAttribute), message);
                        Assert.Contains(nameof(AssemblyVersionAttribute), message);
                        Assert.Contains(nameof(AssemblyFileVersionAttribute), message);
                        Assert.Contains(nameof(AssemblyInformationalVersionAttribute), message);
                    });
            });
        }
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// Concreted implementation utilizing <see cref="UnsupportedAttribute"/>.
    /// </summary>
    /// <inheritdoc />
    public class UnsupportedAttributeBumpVersionServiceTests
        : UnsupportedAttributeBumpVersionServiceTests<UnsupportedAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Public Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <inheritdoc />
        public UnsupportedAttributeBumpVersionServiceTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            // Nothing further to do here apart from passing along the helper.
        }
    }
}
