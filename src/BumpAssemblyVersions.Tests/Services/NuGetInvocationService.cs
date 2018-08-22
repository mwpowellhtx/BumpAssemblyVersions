using System;
using System.Diagnostics;

namespace Bav
{
    using Xunit;
    using static String;

    /// <summary>
    /// 
    /// </summary>
    public class NuGetInvocationService
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <remarks>This one may be used by the test framework
        /// to provide DI generated instances.</remarks>
        public NuGetInvocationService()
        {
        }

        /// <summary>
        /// Internal Constructor useful when the <paramref name="expectedExitCode"/>
        /// is known across the full life cycle of the Service.
        /// </summary>
        /// <param name="expectedExitCode"></param>
        internal NuGetInvocationService(int expectedExitCode)
        {
            ExpectedExitCode = expectedExitCode;
        }

        // TODO: TBD: may want to connect this with logging...
        /// <summary>
        /// 
        /// </summary>
        private const string NuGetExe = "nuget.exe";

        /// <summary>
        /// 0
        /// </summary>
        private const int DefaultExitCode = 0;

        // TODO: TBD: Please provide expected exit codes for the commands / https://github.com/NuGet/docs.microsoft.com-nuget/issues/1025
        private int? ExpectedExitCode { get; set; }

        /// <summary>
        /// Runs the NuGet command given the <paramref name="arguments"/>. Verifies the
        /// <see cref="Process.ExitCode"/> against the <see cref="ExpectedExitCode"/>,
        /// or bypass the verification when it was Null.
        /// </summary>
        /// <param name="arguments"></param>
        private void Run(params string[] arguments)
        {
            using (var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = NuGetExe,
                    Arguments = Join(" ", arguments),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                // TODO: TBD: may connect with output, etc
                p.Start();
                p.WaitForExit();

                // TODO: TBD: it would be helpful to determine what the error code was...
                // Please provide expected exit codes for the commands / http://github.com/NuGet/docs.microsoft.com-nuget/issues/1025
                // Verify the ExitCode only when an Expected one was specified.
                if (ExpectedExitCode.HasValue)
                {
                    Assert.Equal(ExpectedExitCode.Value, p.ExitCode);
               }
            }
        }

        /// <summary>
        /// Performs the NuGet Restore command.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="expectedExitCode">Provide the <see cref="ExpectedExitCode"/>.
        /// When it is Null we do not Verify the <see cref="Process.ExitCode"/>.</param>
        /// <see cref="!:http://docs.microsoft.com/en-us/nuget/tools/cli-ref-restore"/>
        public void Restore(string path, int? expectedExitCode = null)
        {
            ExpectedExitCode = expectedExitCode;
            Run(nameof(Restore).ToLower(), $"\"{path}\"");
        }
    }
}
