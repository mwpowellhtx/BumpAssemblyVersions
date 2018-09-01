using System.Reflection;

namespace Bav
{
    // ReSharper disable once UnusedMember.Global
    internal class AssemblyInformationalVersionAttributeTransformation
        : NuGetVersionAssemblyAttributeTransformationBase<AssemblyInformationalVersionAttribute>
    {
        // ReSharper disable once UnusedMember.Global
        internal AssemblyInformationalVersionAttributeTransformation(string sourcePath)
            : base(sourcePath)
        {
        }
    }
}
