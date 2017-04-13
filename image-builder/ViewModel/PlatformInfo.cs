using ImageBuilder.Model;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ImageBuilder.ViewModel
{
    public class PlatformInfo
    {
        private static readonly Regex fromRegex = new Regex(@"FROM\s+(?<fromImage>\S+)");

        private static Regex GetFromRegex()
        {
            return fromRegex;
        }

        public Platform Model { get; private set;}
        public string FromImage { get; private set;}
        public bool IsExternalFromImage { get; private set;}
        public IEnumerable<string> Tags {get; private set;}

        public static PlatformInfo Create(Platform model, Repo repo)
        {
            PlatformInfo platformInfo = new PlatformInfo();
            platformInfo.Model = model;
            platformInfo.InitializeFromImage();
            platformInfo.IsExternalFromImage = platformInfo.FromImage.StartsWith($"{repo.DockerRepo}:");
            platformInfo.Tags = model.Tags
                .Select(tag => $"{repo.DockerRepo}:{tag}")
                .ToArray();

            return platformInfo;
        }

        private void InitializeFromImage()
        {
            Console.WriteLine(Model.Dockerfile);
            Match fromMatch = GetFromRegex().Match(File.ReadAllText(Path.Combine(Model.Dockerfile, "Dockerfile")));
            if (!fromMatch.Success)
            {
                throw new InvalidOperationException($"Unable to find the FROM image in {Model.Dockerfile}.");
            }

            FromImage = fromMatch.Groups["fromImage"].Value;
        }
    }
}