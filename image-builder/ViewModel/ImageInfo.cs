using ImageBuilder.Model;
using System.Linq;
using System.Collections.Generic;

namespace ImageBuilder.ViewModel
{
    public class ImageInfo
    {
        private Image Model { get; set;}
        public PlatformInfo ActivePlatform { get; private set;}
        public IEnumerable<string> AllTags{ get; private set;}

        public static ImageInfo Create(Image model, string dockerOS, Repo repo)
        {
            ImageInfo imageInfo = new ImageInfo();
            imageInfo.Model = model;
            imageInfo.ActivePlatform = PlatformInfo.Create(model.Platforms[dockerOS], repo);
            imageInfo.AllTags = model.SharedTags
                .Concat(imageInfo.ActivePlatform.Model.Tags)
                .Select(tag => $"{repo.DockerRepo}:{tag}")
                .ToArray();

            return imageInfo;
        }
    }
}