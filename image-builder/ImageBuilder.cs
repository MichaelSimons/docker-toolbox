using ImageBuilder.ViewModel;
using System;
using System.Linq;
using System.Diagnostics;

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
                    Docker.Pull(imageInfo.ActivePlatform.FromImage, Options);
                }

                Docker.Build(imageInfo.AllTags, imageInfo.ActivePlatform.Model.Dockerfile, Options);
            }
        }

        private static void Cleanup()
        {
            if (Options.IsCleanupEnabled)
            {
                // TODO: message about resource getting cleaned up
                CleanupResources(
                    "container ls -a --format \"{{ .ID }} ({{.Names}})\"",
                    id => Docker.Container($"rm -f {id}", Options)
                );
                CleanupResources(
                    "volume ls -q",
                    id => Docker.Volume($"rm -f {id}", Options)
                );
                // TODO:  skip nanoserver image
                CleanupResources(
                    "image ls -a --format \"{{.ID}} ({{.Repository}}:{{.Tag}})\"",
                    id => Docker.Image($"rm -f {id}", Options)
                );
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
                    Docker.Login(Options);
                }

                foreach (string tag in RepoInfo.Images
                    .Where(image => image.ActivePlatform != null)
                    .SelectMany(image => image.ActivePlatform.Tags))
                {
                    Docker.Push(tag, Options);
                }

                if (Options.Username != null)
                {
                    Docker.Logout(Options);
                }
            }
        }
    }
}
