using ImageBuilder.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ImageBuilder.ViewModel
{
    public class RepoInfo
    {
        private string DockerOS { get; set; }
        private Repo Model { get; set; }

        public IEnumerable<ImageInfo> Images { get; set; }

        public string[] TestCommands { get; private set;}

        private RepoInfo()
        {
        }

        public static RepoInfo Create(string repoJsonPath)
        {
            RepoInfo repoInfo = new RepoInfo();
            repoInfo.DetectDockerOS();
            string json = File.ReadAllText(repoJsonPath);
            repoInfo.Model = JsonConvert.DeserializeObject<Repo>(json);
            Console.WriteLine(repoInfo.Model);
            repoInfo.Images = repoInfo.Model.Images
                .Select(image => ImageInfo.Create(image, repoInfo.DockerOS, repoInfo.Model))
                .ToArray();
            if (repoInfo.Model.TestCommands.TryGetValue(repoInfo.DockerOS, out string[] activeCommand))
            {
repoInfo.TestCommands= activeCommand;
            }

            return repoInfo;
        }

        private void DetectDockerOS()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("docker", "version -f \"{{ .Server.Os }}\"");
            startInfo.RedirectStandardOutput = true;
            Process process = ExecuteHelper.Execute(
                startInfo,
                false,
                $"Failed to detect Docker Mode");
            DockerOS = process.StandardOutput.ReadToEnd().Trim();

            Console.WriteLine(DockerOS);
        }
    }
}