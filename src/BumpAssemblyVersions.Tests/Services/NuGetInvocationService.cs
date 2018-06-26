using System.Diagnostics;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public class NuGetInvocationService
    {
        /// <summary>
        /// 
        /// </summary>
        private const string NuGetExe = "nuget.exe";

        /// <summary>
        /// 
        /// </summary>
        internal static class Commands
        {
            /// <summary>
            /// 
            /// </summary>
            public const string Restore = "restore";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        private static void Run(params string[] arguments)
        {
            using (var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = NuGetExe,
                    Arguments = string.Join(" ", arguments),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                // TODO: TBD: may connect with output, etc
                p.Start();
                p.WaitForExit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Restore(string path)
        {
            Run(Commands.Restore, $"\"{path}\"");
        }
    }
}
