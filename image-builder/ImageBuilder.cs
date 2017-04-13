using ImageBuilder.ViewModel;
using System;
using System.Linq;

namespace ImageBuilder
{
    public static class ImageBuilder
    {
        private static Options Options { get; set;}
        private static RepoInfo RepoInfo { get; set;}

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
                    BuildImages();
                    PushImages();
                    EmitSummary();
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

        private static void EmitSummary()
        {
            Console.WriteLine($"{Environment.NewLine}IMAGES BUILT");
            foreach (string tag in RepoInfo.Images.Where(image => image.ActivePlatform != null).SelectMany(image => image.ActivePlatform.Tags))
            {
                Console.WriteLine(tag);
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
