using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Bav
{
    using Microsoft.Build.Utilities;
    using static File;
    using static ProjectBasedBumpVersionService;
    using static String;
    using static XNode;
    using static MessageImportance;
    using static VersionKind;
    using static LoadOptions;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public partial class BumpVersion : Task
    {
        private static bool TryReadingFile(string fullPath, out string lines)
        {
            const FileMode mode = FileMode.Open;
            const FileAccess access = FileAccess.Read;
            const FileShare share = FileShare.Read;

            using (var fs = Open(fullPath, mode, access, share))
            {
                using (var sr = new StreamReader(fs))
                {
                    lines = sr.ReadToEndAsync().Result;
                }
            }

            return !IsNullOrEmpty(lines);
        }

        private static bool TryReadingFileLines(string fullPath, out IEnumerable<string> lines)
        {
            var tried = TryReadingFile(fullPath, out var s);

            const StringSplitOptions none = StringSplitOptions.None;
            lines = s.Replace("\r\n", "\n").Split(new[] {"\n"}, none);

            return tried && lines.Any();
        }

        /// <summary>
        /// Tries to Write the <paramref name="s"/> text to the <paramref name="fullPath"/> file.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>After experimenting, <see cref="FileMode.OpenOrCreate"/> is not what
        /// we wanted here after all. In fact, <see cref="FileMode.Create"/> is the perfect
        /// operation for the job.</remarks>
        /// <see cref="WriteAllText(string,string)"/>
        /// <see cref="!:http://docs.microsoft.com/en-us/dotnet/api/system.io.filemode?view=netframework-4.7.2#fields"/>
        private bool TryWritingFile(string fullPath, string s)
        {
            try
            {
                const FileMode mode = FileMode.Create;
                const FileAccess access = FileAccess.Write;
                const FileShare share = FileShare.Read;

                using (var fs = Open(fullPath, mode, access, share))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.Write(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogMessage(High, $"Error writing '{fullPath}': {ex.Message}");
                return false;
            }

            return !IsNullOrEmpty(s);
        }

        private bool TryWritingFileLines(string fullPath, params string[] lines)
        // TODO: TBD: actually, the separator depends on the platform. Would be appropriate to discern Windows (CrLf) from Linux (Lf), for example.
            => TryWritingFile(fullPath, Join("\r\n", lines));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="filePaths"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal bool TryExecuteAssemblyInfoBumpVersion(IEnumerable<IBumpVersionDescriptor> descriptors
            , IEnumerable<string> filePaths, TaskLoggingHelper log = null)
        {
            /* ProjectFilename should be based on ProjectFullPath,
             * which should be relayed to us as the $(ProjectPath). */
            var projectFilename = ProjectFilename;

            // TODO: TBD: should consider logging the descriptors we do have here...
            // TODO: TBD: or possibly as a function of the targets themselves, if possible...

            // Ensure that we are dealing only with the Descriptors we can.
            descriptors = descriptors.Where(descriptor => descriptor.Kind.ContainedBy(AssemblyVersion
                , AssemblyFileVersion, AssemblyInformationalVersion)).ToArray();

            if (!descriptors.Any())
            {
                return false;
            }

            var services = descriptors.Select(d => d.MakeAssemblyInfoBumpVersionService(log))
                .Where(x => x != null).ToArray();

            // ReSharper disable once CollectionNeverQueried.Local
            var results = new List<bool>();

            void OnUsingStatementAdded(object sender, UsingStatementAddedEventArgs e)
            {
                log?.LogMessage(High, $"'{projectFilename}': Adding statement '{e.UsingStatement}'");
            }

            void OnBumpResultFound(object sender, BumpResultEventArgs e)
            {
                // ReSharper disable once InvertIf
                if (e.Result is IAssemblyInfoBumpResult assyInfoResult)
                {
                    (assyInfoResult.DidBump ? log : null)?.LogMessage(High, GetMessage());

                    string GetMessage()
                        => $"'{projectFilename}': Bumped '{assyInfoResult.AttributeType.FullName}'"
                           + $" from '{e.Result.OldVersionAndSemanticString}'"
                           + $" to '{e.Result.VersionAndSemanticString}'";
                }
            }

            services.ToList().ForEach(service =>
            {
                service.UsingStatementAdded += OnUsingStatementAdded;
                service.BumpResultFound += OnBumpResultFound;
            });

            foreach (var fullPath in filePaths.Where(Exists))
            {
                var bumped = false;

                // ReSharper disable once InvertIf
                if (TryReadingFileLines(fullPath, out var givenLines))
                {

#if TASK_LOGGING_HELPER_DIAGNOSTICS
                    log?.LogWarning($"Lines read from '{fullPath}'.");
#endif

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

            services.ToList().ForEach(service =>
            {
                service.UsingStatementAdded -= OnUsingStatementAdded;
                service.BumpResultFound -= OnBumpResultFound;
            });

            return true;
        }

        /// <see cref="EqualityComparer"/>
        private static XNodeEqualityComparer Comparer => EqualityComparer;

        // ReSharper disable once MemberCanBeMadeStatic.Local
        /// <summary>
        /// Tries to Execute the Project File Bump Version operation given
        /// <paramref name="descriptors"/>. May also <paramref name="log"/> progress
        /// as appropriate to do so.
        /// </summary>
        /// <param name="descriptors"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <remarks>Key to this is trying to read the input project file then trying to parse the
        /// XML, including verification that it was a Project file with Sdk attribute. In previous
        /// versions we keyed too strongly on <c>Sdk=&quot;Microsoft.NET.Sdk&quot;</c>, however,
        /// we have since learned that Windows Forms dotnet migration paths expect it to be
        /// <c>Microsoft.NET.Sdk.WindowsDesktop</c>. So therefore, yes, we should still expect
        /// there to be an <c>Sdk</c> attribute, but we can probably ignore the value itself.
        /// If necessary, we might consider injecting a configuration that informs which
        /// <em>Sdk</em> we do support, but only if necessary; for now will ignore the value.</remarks>
        private bool TryExecuteProjectFileBumpVersion(IEnumerable<IBumpVersionDescriptor> descriptors
            , TaskLoggingHelper log = null)
        {
            /* ProjectFilename should be based on ProjectFullPath,
             * which should be relayed to us as the $(ProjectPath). */
            var projectFilename = ProjectFilename;

#pragma warning disable IDE0002 // Simplify member access
            const VersionKind version = VersionKind.Version;
#pragma warning restore IDE0002 // Simplify member access

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // Ensure that we are dealing only with the Descriptors we can.
            descriptors = descriptors.Where(descriptor => descriptor.Kind.ContainedBy(version
                , AssemblyVersion, FileVersion, InformationalVersion, PackageVersion)).ToArray();

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!descriptors.Any())
            {
                return false;
            }

            void OnBumpResultFound(object sender, BumpResultEventArgs e)
            {
                // ReSharper disable once InvertIf
                if (e.Result is IProjectBumpResult projectResult)
                {
                    (projectResult.DidBump ? log : null)?.LogMessage(High, GetMessage());

                    string GetMessage()
                        => $"'{projectFilename}': Bumped '{projectResult.ProtectedElementName}'"
                           + $" from '{e.Result.OldVersionAndSemanticString}'"
                           + $" to '{e.Result.VersionAndSemanticString}'";
                }
            }

            bool TryParseDocument(string s, out XDocument result, Func<XElement, bool> verifyRoot)
            {
                var parsed = true;
                try
                {
                    result = XDocument.Parse(s, PreserveWhitespace);
                }
                catch (XmlException)
                {
                    parsed = false;
                    result = new XDocument();
                }

                // Now verify the Document and do a little vetting for Project criteria.
                return parsed && verifyRoot(result?.Root);
            }

            bool DidBumpDocumentVersions<TNode>(TNode aDoc, TNode bDoc)
                where TNode : XNode
                => !Comparer.Equals(aDoc, bDoc);

            bool TryFormatNode<TNode>(TNode node, out string s)
                where TNode : XNode
            {
                s = node.ToString(SaveOptions.None);
                return true;
            }

            var bumped = false;

            var services = descriptors.Select(d => d.MakeProjectBasedBumpVersionService(log))
                .Where(x => x != null).ToArray();

            // TODO: TBD: I think there must also be BumpResultCreated, probably also for the AssemblyInfo version...
            services.ToList().ForEach(service => { service.BumpResultFound += OnBumpResultFound; });

            // TODO: TBD: what's nice about this is that minimum the XDocument work is nicely contained in the block
            // TODO: TBD: it may make sense to refactor this block into its own local function TryABC method
            // TODO: TBD: with the goal being to eliminate the need for a Bumped variable altogether
            // TODO: TBD: and then just string together the different TryXYZ local functions as in other places
            // TODO: TBD: i.e. try read from file, try matching, try bumping, etc
            if (TryReadingFile(projectFilename, out var given)
                && TryParseDocument(given, out var givenDoc,
                    root => root?.Name.LocalName == "Project"
                            && root.Attribute("Sdk") is XAttribute))
            {
                /* Process the Given Doc using the Services in the Aggregate.
                 Pull the Tried Doc forward when the Bump did Try. */
                var resultDoc = services.Aggregate(new XDocument(givenDoc)
                    , (currentDoc, service) =>
                    {
                        // TODO: TBD: sort of beggars the question at this point; 
                        // TODO: TBD: if we did set the version property via the custom task
                        // TODO: TBD: then is it really that necessary to re-write the file afterwards?
                        // TODO: TBD: yes, for AssemblyInfo Assembly Attribute based work, of course
                        // TODO: TBD: but for CSPROJ Project based work? perhaps not... TBD, TBD, TBD ...
                        var tried = service.TryBumpDocument(currentDoc, out var triedDoc);

                        // ReSharper disable once InvertIf
                        if (tried)
                        {
                            // ReSharper disable once InconsistentNaming
                            const string PropertyGroup = nameof(PropertyGroup);

                            // TODO: TBD: do we need an actual property get/set pair for the Output?
                            if (TryGetVersionElement(triedDoc, PropertyGroup, $"{service.Descriptor.Kind}"
                                , out var elements))
                            {
                                // TODO: TBD: also take into consideration things like Configuration re: multiple returned elements...
                                // Do not miss this. These are the hooks that inform the Task Output properties.
                                ProjectVersions[service.Descriptor.Kind] = elements.Single().Value;
                            }
                        }

                        return tried ? triedDoc : currentDoc;
                    });

                // We Bumped when the Doc changed and we Formatted to Given.
                bumped = DidBumpDocumentVersions(givenDoc, resultDoc) && TryFormatNode(resultDoc, out given);
            }

            services.ToList().ForEach(service => { service.BumpResultFound -= OnBumpResultFound; });

            /* We do still need to re-write the file. But we also need to connect the
             TryBumpDocument with the actual Custom Task Output(s), in order to realize
             comprehensive integration at the time the Properties are being resolved. */
            return bumped && TryWritingFile(projectFilename, given);
        }

        private static TaskLoggingHelper WithHelper(TaskLoggingHelper log = null) => log;

        private IEnumerable<bool> TriedExecuteResults()
        {
            var log = WithHelper(Log);

            // Tested through and through and working.
            yield return TryExecuteAssemblyInfoBumpVersion(BumpDescriptors
                , Files.Select(file => file.ItemSpec), log);

            // TODO: TBD: next up is to verify this one.
            yield return TryExecuteProjectFileBumpVersion(BumpDescriptors, log);
        }

        // TODO: TBD: if I wanted to trap further errors as exceptions, i.e. empty results, that's a slightly more elaborate set of questions
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool Execute()
            => !Bumps.Any()
               || (TriedExecuteResults().ToArray() is var result
                   && result.Any() && result.Any(x => x));
    }
}
