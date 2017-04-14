using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBuilder.Model
{
    public class Image
    {
        public string[] SharedTags { get; set; }
        public IDictionary<string, Platform> Platforms { get; set; }

        public override string ToString()
        {
            string platforms = Platforms
                .Select(platform => platform.ToString())
                .Aggregate((working, next) => $"{working}{Environment.NewLine}{next}");
            return $"SharedTags: {String.Join(", ", SharedTags)}{Environment.NewLine}{platforms}";
        }
    }
}