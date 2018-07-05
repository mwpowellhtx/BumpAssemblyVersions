using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Definition;
using Microsoft.Build.Framework;
using Microsoft.Build.Evaluation;

namespace Bav
{
    using Microsoft.Build.Utilities;
    using static String;
    using static MessageImportance;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public partial class BumpVersion : Task
    {
        private AssemblyInfoBumpVersionService AssemblyInfoBumpVersion { get; } = new AssemblyInfoBumpVersionService();

        public BumpVersion()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool Execute()
        {
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
