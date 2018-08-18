using System.Xml.Linq;

namespace Bav
{
    /// <inheritdoc cref="BumpResult"/>
    /// <inheritdoc cref="IProjectBumpResult"/>
    public class ProjectBumpResult : BumpResult, IProjectBumpResult
    {
        /// <inheritdoc />
        public string ProtectedElementName { get; set; }

        // TODO: TBD: and would need to figure on a way to replace the old element?
        /// <summary>
        /// Returns the <see cref="XElement"/> corresponding with the
        /// <paramref name="versionAndSemantic"/>.
        /// </summary>
        /// <param name="versionAndSemantic"></param>
        /// <returns></returns>
        protected virtual XElement GetResultPattern(string versionAndSemantic)
            => new XElement(ProtectedElementName, versionAndSemantic);

        /// <summary>
        /// Internal Constructor
        /// </summary>
        /// <param name="descriptor"></param>
        /// <inheritdoc />
        internal ProjectBumpResult(IBumpVersionDescriptor descriptor)
            : base(descriptor)
        {
        }
    }
}
