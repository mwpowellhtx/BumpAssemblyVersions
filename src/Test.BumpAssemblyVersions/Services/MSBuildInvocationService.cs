﻿using System;
using System.Linq;
using Microsoft.Build.Execution;

namespace Bav
{
    using Microsoft.Build.Evaluation;
    using Xunit;
    using static BuildManager;
    using static BuildResultCode;

    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class MSBuildInvocationService
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ToolsetRequiredEventArgs> ToolsetRequired;

        private void OnToolsetRequired(out ToolsetRequiredEventArgs e)
        {
            e = new ToolsetRequiredEventArgs();
            ToolsetRequired?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ConfigureEnvironmentVariablesEventArgs> ConfigureEnvironmentVariables;

        private void OnConfigureEnvironmentVariables(Toolset toolset, string installDirectoryName)
            => ConfigureEnvironmentVariables?.Invoke(this
                , new ConfigureEnvironmentVariablesEventArgs(toolset, installDirectoryName)
            );

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ConfigureBuildEventArgs> ConfigureBuild;

        private void OnConfigureBuild(out ConfigureBuildEventArgs e)
        {
            e = new ConfigureBuildEventArgs();
            ConfigureBuild?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<BuildResultEventArgs> AfterBuild;

        private void OnAfterBuild(BuildResult result)
            => AfterBuild?.Invoke(this, new BuildResultEventArgs(result));

        /// <summary>
        /// I do not know that we are expecting the Build itself to actually throw anything,
        /// never mind <see cref="InvalidOperationException"/>, but rather simply to respond
        /// via <see cref="BuildResultCode"/>, with either <see cref="Success"/> or
        /// <see cref="Failure"/>, for instance.
        /// </summary>
        public event EventHandler<BuildResultEventArgs> BuildExceptionOccurred;

        /// <summary>
        /// Tries to Handle the Build <paramref name="exception"/>.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool TryOnBuildExceptionOccurred<TException>(TException exception)
            where TException : Exception
        {
            bool TryHandleInvoke(BuildResultEventArgs e)
            {
                BuildExceptionOccurred?.Invoke(this, e);
                return e.IsHandled;
            }

            return TryHandleInvoke(new BuildResultEventArgs(null, exception));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            // ReSharper disable once SuggestBaseTypeForParameter
            Toolset GetToolset(ProjectCollection pc, out string installDirectoryName)
            {
                // Aligning the Toolset with the Build Request is kind of a separate step.
                OnToolsetRequired(out var e);
                var ts = pc.Toolsets.SingleOrDefault(e.Predicate);
                Assert.NotNull(ts);
                installDirectoryName = e.GetInstallDirectoryName(ts);
                return ts;
            }

            using (var pc = new ProjectCollection())
            {
                var ts = GetToolset(pc, out var installDir);

                OnConfigureEnvironmentVariables(ts, installDir);

                OnConfigureBuild(out var e);

                var parameters = new BuildParameters(pc) {Loggers = e.Loggers.ToArray()};

                var requestData = new BuildRequestData(e.ProjectOrSolutionFullPath, e.GlobalProperties
                    , ts.ToolsVersion, e.TargetsToBuild.ToArray(), null);

                try
                {
                    var actualResult = DefaultBuildManager.Build(parameters, requestData);
                    OnAfterBuild(actualResult);
                }
                catch (InvalidOperationException ioex)
                {
                    if (!TryOnBuildExceptionOccurred(ioex))
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (!TryOnBuildExceptionOccurred(ex))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
