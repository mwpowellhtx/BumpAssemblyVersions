using System.Reflection;

namespace Bav
{
    internal class AssemblyInformationalVersionAttributeTransformation
        : NuGetVersionAssemblyAttributeTransformationBase<AssemblyInformationalVersionAttribute>
    {
        internal AssemblyInformationalVersionAttributeTransformation(string sourcePath)
            : base(sourcePath)
        {
        }
    }
}
