using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public interface IAssemblyInfoBumpResult : IBumpResult
    {
        /// <summary>
        /// Gets the Original.
        /// </summary>
        string Original { get; }

        /// <summary>
        /// Gets the Result.
        /// </summary>
        string Result { get; }

        /// <summary>
        /// Gets or sets the Line in which either the <see cref="Original"/> or the
        /// <see cref="Result"/> were obtained.
        /// </summary>
        string Line { get; set; }

        /// <summary>
        /// Gets or sets the AttributeType.
        /// </summary>
        Type AttributeType { get; set; }

        /// <summary>
        /// Sets the AttributeName itself. May or may not be directly aligned with
        /// <see cref="AttributeType"/> in terms of being short or long form.
        /// </summary>
        string AttributeName { set; }
    }
}
