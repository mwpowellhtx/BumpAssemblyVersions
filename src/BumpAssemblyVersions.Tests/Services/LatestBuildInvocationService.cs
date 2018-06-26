using System;
using System.IO;
using Microsoft.Build.Framework;

namespace Bav
{
    using Xunit.Abstractions;
    using static LoggerVerbosity;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class LatestBuildInvocationService : MSBuildInvocationService
    {
        private const string LatestToolsVersion = "15.0";

        private class TestLogger : ILogger
        {
            public string Parameters { get; set; }

            /// <summary>
            /// This is really the Threshold against which to mask the level of detail in which
            /// to <see cref="ReportMessage"/>. <see cref="Minimal"/> reporting signals whether
            /// a Build Started or Finished and any Errors that are raised. <see cref="Normal"/>
            /// reporting picks up any Solutions or Projects as well as Warnings that are raised.
            /// Finally, <see cref="Diagnostic"/> reporting includes diagnostic messages that
            /// occur during that build.
            /// </summary>
            /// <inheritdoc />
            /// <see cref="ReportMessage"/>
            /// <see cref="Minimal"/>
            /// <see cref="Normal"/>
            /// <see cref="Diagnostic"/>
            public LoggerVerbosity Verbosity { get; set; }

            private ITestOutputHelper OutputHelper { get; }

            internal TestLogger(ITestOutputHelper outputHelper, LoggerVerbosity verbosity, params string[] parameters)
            {
                OutputHelper = outputHelper;
                Verbosity = verbosity;
                Parameters = string.Join(" ", parameters);
            }

            /// <summary>
            /// Reports the <paramref name="message"/> given the
            /// <paramref name="thresholdVerbosity"/>, which is compared against
            /// <see cref="Verbosity"/> in order to determine whether to report said Message.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="thresholdVerbosity"></param>
            /// <see cref="Minimal"/>
            /// <see cref="Normal"/>
            /// <see cref="Diagnostic"/>
            private void ReportMessage(string message, LoggerVerbosity? thresholdVerbosity = null)
            {
                if (thresholdVerbosity.HasValue && Verbosity < thresholdVerbosity)
                {
                    return;
                }

                OutputHelper.WriteLine(message);
            }

            // TODO: TBD: consider what other events I may need/want to respond  to, and with what level of tracking, reporting, etc
            private void EventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
                => ReportMessage($"Project started: {e.Timestamp:O}: {e.Message} {e.TargetNames}", Normal);

            private void EventSource_BuildStarted(object sender, BuildStartedEventArgs e)
                => ReportMessage($"Build started: {e.Timestamp:O}: {e.Message}", Minimal);

            private void EventSource_BuildFinished(object sender, BuildFinishedEventArgs e)
                => ReportMessage($"Build finished: {e.Timestamp:O}: {e.Message}", Minimal);

            private void EventSource_MessageRaised(object sender, BuildMessageEventArgs e)
                => ReportMessage($"Message: {e.Timestamp:O} {e.Message}", Diagnostic);

            private void EventSource_WarningRaised(object sender, BuildWarningEventArgs e)
                => ReportMessage($"Warning: {e.Timestamp:O} {e.Message}", Normal);

            private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
                => ReportMessage($"Error: {e.Timestamp:O} {e.Message}", Minimal);

            private IEventSource _eventSource;

            public void Initialize(IEventSource eventSource)
            {
                _eventSource = eventSource;
                // TODO: TBD: Targets Started/Finished, Tasks Started/Finished, Status, "Custom", "Any" Event Raised (?)
                _eventSource.ProjectStarted += EventSource_ProjectStarted;
                _eventSource.BuildStarted += EventSource_BuildStarted;
                _eventSource.BuildFinished += EventSource_BuildFinished;
                _eventSource.MessageRaised += EventSource_MessageRaised;
                _eventSource.WarningRaised += EventSource_WarningRaised;
                _eventSource.ErrorRaised += EventSource_ErrorRaised;
            }

            public void Shutdown()
            {
                _eventSource.ErrorRaised -= EventSource_ErrorRaised;
                _eventSource.WarningRaised -= EventSource_WarningRaised;
                _eventSource.MessageRaised -= EventSource_MessageRaised;
                _eventSource.BuildFinished -= EventSource_BuildFinished;
                _eventSource.BuildStarted -= EventSource_BuildStarted;
                _eventSource.ProjectStarted -= EventSource_ProjectStarted;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LatestBuildInvocationService(ITestOutputHelper outputHelper, LoggerVerbosity loggerVerbosity)
        {
            ToolsetRequired += (sender, e) =>
            {
                e.GetInstallDirectoryName = ts => new FileInfo(ts.ToolsPath).Directory?.Parent?.Parent?.FullName;
                e.Predicate = ts => ts.ToolsVersion == LatestToolsVersion;
            };

            ConfigureEnvironmentVariables += (sender, e) =>
            {
                Environment.SetEnvironmentVariable("VSINSTALLDIR", e.InstallDirectoryName);
                Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", e.Toolset.ToolsPath);
                Environment.SetEnvironmentVariable("VisualStudioVersion", e.Toolset.ToolsVersion);
            };

            ConfigureBuild += (sender, e) =>
            {
                e.Loggers = new ILogger[] {new TestLogger(outputHelper, loggerVerbosity)};
            };
        }
    }
}
