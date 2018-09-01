using System;
using System.Collections.Generic;

namespace Bav
{
    /// <summary>
    /// This should work but for an issue, it seems, Reflecting the not on the Generic list of
    /// the <see cref="Type"/>, but rather a sort of Generic Parameter Info descriptor.
    /// </summary>
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = true)]
    internal class ConstrainGenericTypesAttribute : Attribute
    {
        internal IEnumerable<Type> AllowedTypes { get; }

        internal ConstrainGenericTypesAttribute(params Type[] allowedTypes)
        {
            /* Must reflect using typeof(MyClass<>) as contrasted with
             typeof(MyClass<T>) in order to emit the proper results. */

            AllowedTypes = allowedTypes;
        }
    }
}
