using ImageBuilder.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ImageBuilder.Model;

namespace ImageBuilder
{
    public static class ImageBuilder
    {
        private static Options Options { get; set; }
        private static RepoInfo RepoInfo { get; set; }

        public static int Main(string[] args)
        {
            int result = 0;

            try
            {
                Options = Options.ParseArgs(args);
                if (Options.IsHelpRequest)
                {
                    Console.WriteLine(Options.HelpContent);
                }
                else
                {
                    RepoInfo = RepoInfo.Create(Options.RepoInfo);
                    Cleanup();

                    switch (Options.Command)
                    {
                        case CommandType.Build:
                            ExecuteBuild();
                            break;
                        case CommandType.Manifest:
                            ExecuteManifest();
                            break;
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                result = 1;
            }
            finally
            {
                Cleanup();
            }

            return result;
        }

        private static void BuildImages()
        {
            Console.WriteLine($"{Environment.NewLine}BUILDING IMAGES");

            foreach (ImageInfo imageInfo in RepoInfo.Images.Where(image => image.ActivePlatform != null))
            {
                Console.WriteLine($"-- BUILDING: {imageInfo.ActivePlatform.Model.Dockerfile}");
                if (!Options.IsSkipPullingEnabled && imageInfo.ActivePlatform.IsExternalFromImage)
                {
                    // Ensure latest base image exists locally before building
                    ExecuteHelper.ExecuteWithRetry("docker", $"pull {imageInfo.ActivePlatform.FromImage}", Options.IsDryRun);
                }

                string tagArgs = imageInfo.AllTags
                    .Select(tag => $"-t {tag}")
                    .Aggregate((working, next) => $"{working} {next}");
                ExecuteHelper.Execute("docker", $"build {tagArgs} {imageInfo.ActivePlatform.Model.Dockerfile}", Options.IsDryRun);
            }
        }

        private static void Cleanup()
        {
            if (Options.IsCleanupEnabled)
            {
                // TODO: message about resource getting cleaned up
                CleanupResources(
                    "container ls -a --format \"{{ .ID }} ({{.Names}})\"",
                    id => ExecuteHelper.Execute("docker", $"container rm -f {id}", Options.IsDryRun));
                CleanupResources(
                    "volume ls -q",
                    id => ExecuteHelper.Execute("docker", $"volume rm -f {id}", Options.IsDryRun));
                // TODO:  skip nanoserver image
                CleanupResources(
                    "image ls -a --format \"{{.ID}} ({{.Repository}}:{{.Tag}})\"",
                    id => ExecuteHelper.Execute("docker", $"image rm -f {id}", Options.IsDryRun));
            }
        }

        public static void CleanupResources(string retrieveResourcesCommand, Action<string> deleteResource)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("docker", retrieveResourcesCommand);
            startInfo.RedirectStandardOutput = true;
            Process process = ExecuteHelper.Execute(startInfo, false);
            string[] resouces = process.StandardOutput.ReadToEnd()
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string resource in resouces)
            {
                string[] parts = resource.Split(' ');
                Console.WriteLine($"Deleting {resource})");
                deleteResource(parts[0]);
            }
        }

        private static void EmitSummary()
        {
            Console.WriteLine($"{Environment.NewLine}IMAGES BUILT");
            foreach (string tag in RepoInfo.Images.Where(image => image.ActivePlatform != null).SelectMany(image => image.ActivePlatform.Tags))
            {
                Console.WriteLine(tag);
            }
        }

        private static void ExecuteBuild()
        {
            BuildImages();
            RunTests();
            PushImages();
            EmitSummary();
        }

        private static void ExecuteManifest()
        {
            Console.WriteLine($"Generating manifest");
            // build manifest image
            foreach (ImageInfo imageInfo in RepoInfo.Images)
            {
                foreach (string tag in imageInfo.Model.SharedTags)
                {
                    string manifestYml = $@"image: {tag}
manifests:
";
                    foreach (KeyValuePair<string, Platform> kvp in imageInfo.Model.Platforms)
                    {
                        manifestYml += $@"  -
    image: {RepoInfo.Model.DockerRepo}:{kvp.Value.Tags.First()}
    platform:
      architecture: amd64
      os: {kvp.Key}
";
                    }
                    Console.WriteLine($"Publishing Manifest {Environment.NewLine} {manifestYml}");

                    // TODO: manifest as parameter
                    File.WriteAllText("manifest.yml", manifestYml);
                    ExecuteHelper.Execute(
                        "docker",
                        $"run --rm -v /var/run/docker.sock:/var/run/docker.sock -v {Directory.GetCurrentDirectory()}:/manifests msimons/dotnet-buildtools-prereqs:manifest-tool --username {Options.Username} --password {Options.Password} push from-spec /manifests/manifest.yml",
                        Options.IsDryRun);
                }
            }
        }

        private static void RunTests()
        {
            if (!Options.IsTestRunDisabled)
            {
                foreach (string command in RepoInfo.TestCommands)
                {
                    string[] parts = command.Split(' ');
                    ExecuteHelper.Execute(
                        parts[0],
                        command.Substring(parts[0].Length + 1),
                        Options.IsDryRun,
                        "test error");
                }
            }
        }

        private static void PushImages()
        {
            if (Options.IsPushEnabled)
            {
                if (Options.Username != null)
                {
                    string loginArgsWithoutPassword = $"login -u {Options.Username} -p";
                    ExecuteHelper.Execute(
                        "docker",
                        $"{loginArgsWithoutPassword} {Options.Password}",
                        Options.IsDryRun,
                        executeMessageOverride: $"{loginArgsWithoutPassword} ********");
                }

                foreach (string tag in RepoInfo.Images
                    .Where(image => image.ActivePlatform != null)
                    .SelectMany(image => image.ActivePlatform.Tags))
                {
                    ExecuteHelper.ExecuteWithRetry("docker", $"push {tag}", Options.IsDryRun);
                }

                if (Options.Username != null)
                {
                    ExecuteHelper.Execute("docker", $"logout", Options.IsDryRun);
                }
            }
        }
    }
}
