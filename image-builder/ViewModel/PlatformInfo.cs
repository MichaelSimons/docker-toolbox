using ImageBuilder.Model;
using System;
using System.Text.RegularExpressions;
using System.IO;

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

        public static PlatformInfo Create(Platform model, Repo repo)
        {
            PlatformInfo platformInfo = new PlatformInfo();
            platformInfo.Model = model;
            platformInfo.InitializeFromImage();
            platformInfo.IsExternalFromImage = platformInfo.FromImage.StartsWith($"{repo.DockerRepo}:");

            return platformInfo;
        }

        private void InitializeFromImage()
        {
            // TODO Encapsulate Dockerfile path better and use separator const.
            Match fromMatch = GetFromRegex().Match(File.ReadAllText($"{Model.Dockerfile}\\Dockerfile"));
            if (!fromMatch.Success)
            {
                throw new InvalidOperationException($"Unable to find the FROM image in {Model.Dockerfile}.");
            }

            FromImage = fromMatch.Groups["fromImage"].Value;
        }
    }
}