using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bav
{
    /// <inheritdoc />
    public class BumpFileVersionServiceAdapter : IBumpFileVersionServiceAdapter
    {
        private static bool TryBumpVersionsFromFile(string fullPath, out IEnumerable<string> bumpedLines
            , params IAssemblyInfoBumpVersionService[] bumpVersionServices)
        {
            const FileMode mode = FileMode.Open;
            const FileAccess access = FileAccess.Read;
            const FileShare share = FileShare.Read;

            var bumped = new List<bool>();

            using (var fs = File.Open(fullPath, mode, access, share))
            {
                using (var sr = new StreamReader(fs))
                {
                    // Replace the CR+LF first in order to avoid unnecessary empty lines after Split.
                    bumpedLines = sr.ReadToEnd().Replace(@"\r\n", @"\n").Split('\n');

                    // The cross-cutting concern are the Services across the Lines.
                    foreach (var service in bumpVersionServices)
                    {
                        if (service.TryBumpVersion(bumpedLines, out var servicedLines).AddTo(bumped))
                        {
                            bumpedLines = servicedLines.ToArray();
                        }
                    }
                }
            }

            return bumped.Any(x => x);
        }

        private static bool TryWriteBumpedLinesToFile(string fullPath, IEnumerable<string> bumpedLines)
        {
            const FileMode mode = FileMode.CreateNew;
            const FileAccess access = FileAccess.Write;
            const FileShare share = FileShare.Read;

            using (var fs = File.Open(fullPath, mode, access, share))
            {
                using (var sw = new StreamWriter(fs))
                {
                    bumpedLines.ToList().ForEach(sw.WriteLine);
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool TryBumpVersions(string fullPath, params IAssemblyInfoBumpVersionService[] bumpVersionServices)
            => TryBumpVersionsFromFile(fullPath, out var bumpedLines, bumpVersionServices)
               && TryWriteBumpedLinesToFile(fullPath, bumpedLines);
    }
}
