using System.Xml.Linq;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public interface IProjectBasedBumpVersionService : IBumpVersionService
    {
        /// <summary>
        /// Tries to Bump the <paramref name="given"/> <see cref="XDocument"/>. Yields the
        /// <paramref name="result"/> when something chances. <paramref name="given"/> should
        /// have been vetted already for basic Project qualities, such as whether the top
        /// level Project node has an Sdk attribute, and so on.
        /// </summary>
        /// <param name="given"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryBumpDocument(XDocument given, out XDocument result);
    }
}
