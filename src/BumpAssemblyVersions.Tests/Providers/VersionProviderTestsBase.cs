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
    /// <param name="provider"></param>
    public delegate void InitializeProviderCallback<in T>(T provider)
        where T : class, IVersionProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Type must be a class of implementing <see cref="IVersionProvider"/>.
    /// We cannot use the New constraint here because we expect the Default Constructor to be
    /// Internal, not Public.</typeparam>
    public abstract class VersionProviderTestsBase<T>
        where T : class, IVersionProvider
    {
        /// <summary>
        /// 
        /// </summary>
        protected static Type ProviderType { get; } = typeof(T);

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void VerifyProviderType(Type providerType)
        {
            Assert.NotNull(providerType);
            Assert.True(typeof(IVersionProvider).IsAssignableFrom(providerType));
            Assert.True(providerType.IsClass);
            Assert.False(providerType.IsAbstract);
            Assert.False(providerType.IsInterface);
            Assert.True(providerType.IsPublic);
        }

        /// <summary>
        /// Returns a Created Instance of the <typeparamref name="T"/> Provider.
        /// </summary>
        /// <returns></returns>
        protected virtual T CreateNew()
        {
            VerifyProviderType(ProviderType);

            /* The Default Constructor should (SHOULD) be Internal, Non-Public at any rate. From
             a Reflection perspective, this requires some interpolation since the same verbiage
             may or may not have parallels in the IL. */
            var ctor = ProviderType.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null);

            Assert.NotNull(ctor);
            Assert.False(ctor.IsPrivate);

            /* To identify an internal method using reflection, use the IsAssembly property:
             * http://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection
             * http://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isassembly */
            Assert.True(ctor.IsAssembly);
            Assert.False(ctor.IsFamily);

            var instance = ctor.Invoke(new object[] { });
            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IVersionProvider>(instance);
            return (T) instance;
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Verify_Provider_Type() => VerifyProviderType(ProviderType);

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Verify_Default_Create_New_Instance() => CreateNew();

#pragma warning disable xUnit1003 // Theory methods must have test data
        /// <summary>
        /// Override in order to implement your <see cref="IVersionProvider"/> specific
        /// verification.
        /// </summary>
        /// <param name="beforeChange"></param>
        /// <param name="afterChange"></param>
        /// <param name="init"></param>
        /// <param name="expectedTried"></param>
        [Theory]
        public abstract void Verify_Version_element_After_Trying_to_Change(string beforeChange
            , string afterChange, InitializeProviderCallback<T> init, bool expectedTried);
#pragma warning restore xUnit1003 // Theory methods must have test data

    }
}
