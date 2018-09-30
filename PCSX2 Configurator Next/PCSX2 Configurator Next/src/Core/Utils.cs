using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator_Next.Core
{
    public class Utils
    {
        public static void SystemRemoveDir(string dir)
        {
            if (!Directory.Exists(dir)) return;
            var removeDirProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                    Arguments = $"/c rmdir /s /q \"{dir}\""
                }
            };

            removeDirProcess.Start();
            removeDirProcess.WaitForExit();
        }

        public static void SevenZipExtract(string archive, string outputDir)
        {
            if (!File.Exists(archive)) return;
            var sevenZipProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = $"{Configurator.Model.LaunchBoxDir}\\7-Zip\\7z.exe",
                    Arguments = $"x \"{archive}\" -o\"{outputDir}\""
                }
            };

            sevenZipProcess.Start();
            sevenZipProcess.WaitForExit();
        }

        public static string SvnCheckout(string remotePath, string workingDir)
        {
            var arguments = $"checkout \"{remotePath}\"";
            var output = SvnRun(arguments, workingDir);
            return output;
        }

        public static bool SvnDirNeedsUpdate(string svnDir)
        {
            var headInfo = SvnRun("info -r HEAD", svnDir);
            var info = SvnRun("info", svnDir);

            bool WithLastChagedRev(string str) => str.StartsWith("Last Changed Rev");
            return SvnLine(headInfo, WithLastChagedRev) != SvnLine(info, WithLastChagedRev);
        }

        public static string SvnFindPathInRemote(string remotePath, Func<string, bool> withCondition)
        {
            var arguments = $"list {remotePath}";
            var output = SvnRun(arguments);

            var path = SvnLine(output, withCondition);
            path = path?.Substring(0, path.Length - 1);

            return path;
        }

        private static string SvnLine(string svnOutput, Func<string, bool> withCondition)
        {
            var arr = svnOutput.Replace("\r\n", "\n").Split('\n');
            var ret = arr.FirstOrDefault(withCondition);
            return ret;
        }

        private static string SvnRun(string arguments, string workingDir = "")
        {
            var svnProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = $"{Configurator.Model.LaunchBoxDir}\\SVN\\bin\\svn.exe",
                    Arguments = arguments,
                    WorkingDirectory = workingDir
                }
            };

            svnProcess.Start();
            var output = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            return output;
        }
    }
}
