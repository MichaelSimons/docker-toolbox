using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace DockerHubImageMover
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string jsonContent = File.ReadAllText("Tags.json");
            JObject json = JObject.Parse(jsonContent);

            foreach (JToken image in json["data"])
            {
                string tag = (string)image["name"];
                if (!tag.Contains("image-builder"))
                {
                    string dockerHubimage = $"docker.io/microsoft/dotnet-buildtools-prereqs:{tag}";
                    string acrimage = $"public/dotnet-buildtools/prereqs:{tag}";

                    // Execute($"pull {dockerHubimage}", autoRetry: true);
                    // Execute($"push dotnetdocker.azurecr.io/{acrimage}", autoRetry: true);

                    Execute($"az acr import -n dotnetdocker --source {dockerHubimage} -t {acrimage} --subscription fb511a11-c3d4-48f1-ba06-3ba13f270022", autoRetry: true);
                }
            }
        }

        private static string Execute(
            string args, bool ignoreErrors = false, bool autoRetry = false)
        {
            (Process Process, string StdOut, string StdErr) result;
            if (autoRetry)
            {
                result = ExecuteWithRetry(args, ExecuteProcess);
            }
            else
            {
                result = ExecuteProcess(args);
            }

            if (!ignoreErrors && result.Process.ExitCode != 0)
            {
                ProcessStartInfo startInfo = result.Process.StartInfo;
                string msg = $"Failed to execute {startInfo.FileName} {startInfo.Arguments}{Environment.NewLine}{result.StdErr}";
                throw new InvalidOperationException(msg);
            }

            return result.StdOut;
        }

        private static (Process Process, string StdOut, string StdErr) ExecuteProcess(
            string args)
        {
            Process process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    FileName = "powershell",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            StringBuilder stdOutput = new StringBuilder();
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => stdOutput.AppendLine(e.Data));

            StringBuilder stdError = new StringBuilder();
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => stdError.AppendLine(e.Data));

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            string output = stdOutput.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            string error = stdError.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine(error);
            }

            return (process, output, error);
        }

        private static (Process Process, string StdOut, string StdErr) ExecuteWithRetry(
            string args,
            Func<string, (Process Process, string StdOut, string StdErr)> executor)
        {
            const int maxRetries = 5;
            const int waitFactor = 5;

            int retryCount = 0;

            (Process Process, string StdOut, string StdErr) result = executor(args);
            while (result.Process.ExitCode != 0)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    break;
                }

                int waitTime = Convert.ToInt32(Math.Pow(waitFactor, retryCount - 1));
                Console.WriteLine($"Retry {retryCount}/{maxRetries}, retrying in {waitTime} seconds...");

                Thread.Sleep(waitTime * 1000);
                result = executor(args);
            }

            return result;
        }
    }
}
