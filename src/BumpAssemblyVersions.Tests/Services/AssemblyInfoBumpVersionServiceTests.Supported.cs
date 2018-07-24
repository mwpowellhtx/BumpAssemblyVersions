using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using static BindingFlags;

    public abstract partial class AssemblyInfoBumpVersionServiceTests<T>
    {
        /// <summary>
        /// Verify that the
        /// <see cref="AssemblyInfoBumpVersionService{T}.SupportedAttributeTypes"/> themselves
        /// are accurate and accounted for among the <paramref name="expectedSupportedTypes"/>.
        /// </summary>
        /// <param name="expectedSupportedTypes"></param>
        [Theory, MemberData(nameof(ExpectedSupportedTypes))]
        public void Verify_Supported_Attribute_Type(params Type[] expectedSupportedTypes)
        {
            const string propertyName = nameof(AssemblyInfoBumpVersionServiceFixture<T>.SupportedAttributeTypes);

            // Not technically from the FixtureType, per se.
            var property = typeof(AssemblyInfoBumpVersionService<T>).GetProperty(propertyName, Static | NonPublic | GetProperty);

            Assert.NotNull(property);
            Assert.Equal(typeof(IEnumerable<Type>), property.PropertyType);

            // While technically we could just Get the Value, let's also assert that we did.
            bool TryGetPropertyValue(PropertyInfo pi, out IEnumerable<Type> types)
            {
                types = (IEnumerable<Type>)pi.GetValue(null);
                return types != null && types.Any();
            }

            // In other words, we are not expecting any Exceptions be thrown from the Static Ctor.
            Assert.True(TryGetPropertyValue(property, out var actualSupportedTypes));

            // ReSharper disable once PossibleMultipleEnumeration
            Assert.Equal(expectedSupportedTypes.Length, actualSupportedTypes.Count());

            foreach (var expectedSupportedType in expectedSupportedTypes)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                Assert.Contains(expectedSupportedType, actualSupportedTypes);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static IEnumerable<object[]> _expectedSupportedTypes;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> ExpectedSupportedTypes
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    IEnumerable<object> GetOne(params Type[] types)
                        => types.Select(type => (object)type);

                    yield return GetOne(
                        typeof(AssemblyVersionAttribute)
                        , typeof(AssemblyFileVersionAttribute)
                        , typeof(AssemblyInformationalVersionAttribute)).ToArray();
                }

                return _expectedSupportedTypes ?? (_expectedSupportedTypes = GetAll().ToArray());
            }
        }
    }
}
