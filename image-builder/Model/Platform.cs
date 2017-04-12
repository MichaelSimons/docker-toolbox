using System;

namespace ImageBuilder.Model
{
    public class Platform
    {
        public string Dockerfile { get; set; }
        public string[] Tags { get; set; }

        public override string ToString()
        {
            return $"Dockerfile: {Dockerfile}{Environment.NewLine}Tags: {String.Join(", ", Tags)}";
        }
    }
}