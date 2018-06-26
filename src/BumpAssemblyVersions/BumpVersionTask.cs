using Microsoft.Build.Framework;

namespace Bav
{
    using Microsoft.Build.Utilities;
    using static MessageImportance;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public partial class BumpVersion : Task
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool Execute()
        {
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
