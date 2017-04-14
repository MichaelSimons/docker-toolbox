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
                // TODO:  Introduce cmd concept
                // Build
                // Manifest
                else
                {
                    RepoInfo = RepoInfo.Create(Options.RepoInfo);
                    Cleanup();
                    BuildImages();
                    RunTests();
                    PushImages();
                    EmitSummary();
                    Cleanup();

                    BuildManifest();
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
                // TODO:  Pass build options
                Docker.Build(imageInfo.AllTags, imageInfo.ActivePlatform.Model.Dockerfile, Options);
            }
        }

        private static void BuildManifest()
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

        private static void Cleanup()
        {
            if (Options.IsCleanupEnabled)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("docker", "container ls -a --format \"{{ .ID }} {{.Names}}\"");
                startInfo.RedirectStandardOutput = true;
                Process process = ExecuteHelper.Execute(
                    startInfo,
                    $"Failed to detect Docker Mode",
                    false);
                string containers = process.StandardOutput.ReadToEnd().Trim();
                foreach (string container in containers.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = container.Split(' ');
                    Console.WriteLine($"Deleting container {parts[1]} ({parts[0]})");
                    Docker.Container($"rm -f {parts[0]}", Options);
                }

                startInfo = new ProcessStartInfo("docker", "volume ls -q");
                startInfo.RedirectStandardOutput = true;
                process = ExecuteHelper.Execute(
                    startInfo,
                    $"Failed to detect Docker Mode",
                    false);
                string volumes = process.StandardOutput.ReadToEnd().Trim();
                foreach (string volume in volumes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Console.WriteLine($"Deleting volume {volume}");
                    Docker.Volume($"rm -f {volume}", Options);
                }

                startInfo = new ProcessStartInfo("docker", "image ls -a --format \"{{.ID}} {{.Repository}}:{{.Tag}}\"");
                startInfo.RedirectStandardOutput = true;
                process = ExecuteHelper.Execute(
                    startInfo,
                    $"Failed to detect Docker Mode",
                    false);
                string images = process.StandardOutput.ReadToEnd().Trim();
                foreach (string image in images.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    // TODO:  skip nanoserver image
                    string[] parts = image.Split(' ');
                    Console.WriteLine($"Deleting image {parts[1]} ({parts[0]})");
                    Docker.Image($"rm -f {parts[0]}", Options);
                }
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
                    "test error",
                    Options.IsDryRun
                );
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
