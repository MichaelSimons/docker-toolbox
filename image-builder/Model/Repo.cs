using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBuilder.Model
{
    public class Repo
    {
        public string DockerRepo { get; set; }
        public Image[] Images { get; set; }
        public IDictionary<string, string[]> TestCommands {get;set;}

        public override string ToString()
        {
            string images = Images
                .Select(image => image.ToString())
                .Aggregate((working, next) => $"{working}{Environment.NewLine}{next}");
            return $"DockerRepo: {DockerRepo}{Environment.NewLine}{images}";
        }
    }
}