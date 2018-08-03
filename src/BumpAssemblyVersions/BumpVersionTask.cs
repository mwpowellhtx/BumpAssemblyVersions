using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Bav
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Utilities;
    using static File;
    using static String;
    using static MessageImportance;
    using static StringSplitOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public partial class BumpVersion : Task
    {
        //// TODO: TBD: not sure exactly what this was going to be... capture services at this level? may not be necessary, or even desired...
        //private AssemblyInfoBumpVersionService AssemblyInfoBumpVersion { get; } = new AssemblyInfoBumpVersionService();

        private IEnumerable<FileInfo> _filePaths;

        private IEnumerable<FileInfo> FilePaths
            => _filePaths ?? (_filePaths
                   = Files.Select(item => new FileInfo(item.ItemSpec))).ToArray();

        private static XDocument GetDocument(string xmlFullPath)
        {
            using (var fs = Open(xmlFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
            , params IBumpVersionDescriptor[] descriptors)
            where T : Attribute
        {
            return new BumpFileVersionServiceAdapter().TryBumpVersions(sourceFileFullPath
                , descriptors.Select(descriptor => new AssemblyInfoBumpVersionService<T>(
                    descriptor)).ToArray<IAssemblyInfoBumpVersionService>());
        }

        private static bool Exists(ITaskItem item) => File.Exists(item.ItemSpec);

        private static bool TryReadingFileLines(string fullPath, out IEnumerable<string> lines)
        {
            const FileMode mode = FileMode.Open;
            const FileAccess access = FileAccess.Read;
            const FileShare share = FileShare.Read;

            using (var fs = Open(fullPath, mode, access, share))
            {
                using (var sr = new StreamReader(fs))
                {
                    lines = sr.ReadToEndAsync().Result.Replace("\r\n", "\n").Split(new[] { "\n" }, None);
                }
            }

            return lines.Any();
        }

        private static bool TryWritingFileLines(string fullPath, params string[] lines)
        {
            const FileMode mode = FileMode.OpenOrCreate;
            const FileAccess access = FileAccess.Write;
            const FileShare share = FileShare.Read;

            using (var fs = Open(fullPath, mode, access, share))
            {
                using (var sr = new StreamWriter(fs))
                {
                    sr.Write(Join("\r\n", lines));
                }
            }

            return lines.Any();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bumps"></param>
        /// <param name="files"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal bool TryExecuteAssemblyInfoBumpVersion(IEnumerable<ITaskItem> bumps
            , IEnumerable<ITaskItem> files, TaskLoggingHelper log = null)
        {
            /* BuildEngine.ProjectFileOfTaskNode seems prone to reporting the Targets file,
             * when what we want is the CSPROJ itself. */
            //var projectPath = GetFileName(BuildEngine.ProjectFileOfTaskNode);
            var projectFilename = ProjectFilename;
            var descriptors = bumps.Select(bump => bump.ToDescriptor(log)).ToArray();

            var services = descriptors.Select(d => d.MakeGetAssemblyInfoBumpVersionService(log))
                .Where(x => x != null).ToArray();

            // ReSharper disable once CollectionNeverQueried.Local
            var results = new List<bool>();

            void OnUsingStatementAdded(object sender, UsingStatementAddedEventArgs e)
            {
                log?.LogMessage(High, $"'{projectFilename}': Adding statement '{e.UsingStatement}'");
            }

            void OnBumpResultFound(object sender, BumpResultEventArgs e)
            {
                (e.Result.DidBump ? log : null)?.LogMessage(High, GetMessage());

                string GetMessage()
                    => $"'{projectFilename}': Bumped '{e.Result.AttributeType.FullName}'"
                       + $" from '{e.Result.OldVersionAndSemanticString}'"
                       + $" to '{e.Result.VersionAndSemanticString}'";
            }

            foreach (var fileItem in files.Where(Exists))
            {
                var bumped = false;
                var fullPath = fileItem.ItemSpec;

                // ReSharper disable once InvertIf
                if (TryReadingFileLines(fullPath, out var givenLines))
                {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Lines read from '{fullPath}'.");
#endif

                    services.ToList().ForEach(service =>
                    {
                        service.UsingStatementAdded += OnUsingStatementAdded;
                        service.BumpResultFound += OnBumpResultFound;
                    });

                    foreach (var service in services)
                    {
                        service.Log = log;

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                        log?.LogWarning($"Trying to bump lines from '{fullPath}'.");
#endif

                        // ReSharper disable once PossibleMultipleEnumeration
                        if (service.TryBumpVersion(givenLines, out var bumpedLines))
                        {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                            log?.LogWarning($"Lines bumped for '{fullPath}'.");
#endif

                            // ReSharper disable once PossibleMultipleEnumeration
                            bumped = bumped || !bumpedLines
                                         // ReSharper disable once PossibleMultipleEnumeration
                                         .SequenceEqual(givenLines);

                            if (!bumped)
                            {
                                continue;
                            }

                            // ReSharper disable once PossibleMultipleEnumeration
                            givenLines = bumpedLines.ToArray();
                        }
                    }

                    services.ToList().ForEach(service =>
                    {
                        service.UsingStatementAdded -= OnUsingStatementAdded;
                        service.BumpResultFound -= OnBumpResultFound;
                    });

                    if (!bumped)
                    {
                        continue;
                    }

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Writing lines to file '{fullPath}' ...");
#endif

                    // ReSharper disable once PossibleMultipleEnumeration
                    results.Add(TryWritingFileLines(fullPath, givenLines.ToArray()));
                }
                else
                {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Lines not read from '{fullPath}'.");
#endif

                    results.Add(false);
                }
            }

            return true;
        }

        private static TaskLoggingHelper WithHelper(TaskLoggingHelper log = null) => log;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool Execute()
            => TryExecuteAssemblyInfoBumpVersion(Bumps, Files, WithHelper(Log));
    }
}
