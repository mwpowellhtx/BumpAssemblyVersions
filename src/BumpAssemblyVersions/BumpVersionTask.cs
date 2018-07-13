using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Bav
{
    using Microsoft.Build.Definition;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Utilities;
    using static String;
    using static MessageImportance;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public partial class BumpVersion : Task
    {
        //// TODO: TBD: not sure exactly what this was going to be... capture services at this level? may not be necessary, or even desired...
        //private AssemblyInfoBumpVersionService AssemblyInfoBumpVersion { get; } = new AssemblyInfoBumpVersionService();

        /// <summary>
        /// Default Public Constructor.
        /// </summary>
        /// <inheritdoc />
        public BumpVersion()
        {
            Bumps = null;
        }

        private static XDocument GetDocument(string xmlFullPath)
        {
            using (var fs = File.Open(xmlFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    return XDocument.Parse(sr.ReadToEnd());
                }
            }
        }

        private static bool TryBumpingProjectXmlVersions(string projectFileFullPath
            , params BumpVersionDescriptor[] descriptors)
        {
            bool TryVettingProjectXml(string fullPath, out XDocument vettedDoc)
            {
                vettedDoc = GetDocument(fullPath);

                if (vettedDoc.Root == null
                    || vettedDoc.Root.Name != nameof(Project)
                    || !vettedDoc.Root.HasAttributes)
                {
                    return false;
                }

                var sdkAttrib = vettedDoc.Root.Attribute("Sdk");

                return sdkAttrib != null && sdkAttrib.Value == "Microsoft.NET.Sdk";
            }

            // TODO: TBD: add bits concerning Version Xml properties...

            bool TryBumpingVettedDocument(XDocument vettedDoc, BumpVersionDescriptor descriptor)
            {
                return false;
            }

            return descriptors.Any()
                   && TryVettingProjectXml(projectFileFullPath, out var projectDoc)
                   && descriptors.Any(d => TryBumpingVettedDocument(projectDoc, d));
        }

        /// <summary>
        /// Versions of this form are typically found in an AssemblyInfo file, but they do not
        /// necessarily reside there exclusively.
        /// </summary>
        /// <param name="sourceFileFullPath"></param>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        private static bool TryBumpingAssemblyInfoVersion<T>(string sourceFileFullPath
            , params BumpVersionDescriptor[] descriptors)
            where T : Attribute
        {
            using (var service = new AssemblyInfoBumpVersionService<T>())
            {
                return descriptors.Any(descriptor =>
                {
                    service.VersionProviders = descriptor.GetVersionProviders();
                    return service.TryBumpVersion(sourceFileFullPath);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool Execute()
        {
            var descriptors = Bumps.Select(item => item.ToDescriptor()).ToArray();

            // TODO: TBD: ... ostensibly, this is how we get at the project and CSharp files contained within...
            var project = Project.FromFile(BuildEngine.ProjectFileOfTaskNode, new ProjectOptions { });

            var csharpFiles = project.GetItems("Compile")
                .Select(item => Path.Combine(project.DirectoryPath, item.EvaluatedInclude))
                .Where(fullPath => fullPath.EndsWith(".cs")).ToArray();

            //throw new InvalidOperationException("unknown error occurred...");

            Log.LogMessage(High, $"Running {nameof(Execute)}");

            Log.LogMessage(High, $"Running {nameof(Execute)}");

            Log.LogWarning($"There are {Bumps.Length} Bumps ...");

            foreach (var bump in Bumps)
            {
                foreach (string name in bump.MetadataNames)
                {
                    // TODO: TBD: tweaking which metadata properties are available...
                    Log.LogWarning($"Bumping: {{ \"{name}\": \"{bump.GetMetadata(name)}\" }}");
                    //Log.LogWarning($"Bumping: {bump.GetMetadata("name").ToVersionKind()}");
                }
            }

            return true;
        }
    }
}
