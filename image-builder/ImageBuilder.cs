using ImageBuilder.ViewModel;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace ImageBuilder
{
    public static class ImageBuilder
    {
        private const string manifestYml =
@"image: {repo}:{tag}
manifests:
  -
    image: {repo}:{platformTag}-jessie
    platform:
      architecture: amd64
      os: linux
  -
    image: {repo}:{platformTag}-nanoserver
    platform:
      architecture: amd64
      os: windows";

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

                    Cleanup();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                result = 1;
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
                    id =>  ExecuteHelper.Execute("docker", $"volume rm -f {id}", Options.IsDryRun));
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
                // TODO: manifest as parameter
                // write manifest
                // invoke manifest tool
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
