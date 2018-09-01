using System.Reflection;

namespace Bav
{
    internal class AssemblyFileVersionAttributeTransformation
        : VersionAssemblyAttributeTransformationBase<AssemblyFileVersionAttribute>
    {
        internal AssemblyFileVersionAttributeTransformation(string sourcePath)
            : base(sourcePath)
        {
        }
    }
}
