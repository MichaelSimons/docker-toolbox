using System.Collections.Generic;
using System.Linq;

namespace ImageBuilder
{
    public static class Docker
    {
        public static void Build(IEnumerable<string> tags, string contextPath, Options options)
        {
            string tagArgs = tags
                .Select(tag => $"-t {tag}")
                .Aggregate((working, next) => $"{working} {next}");
            ExecuteHelper.Execute("docker", $"build {tagArgs} {contextPath}", options.IsDryRun);
        }

        public static void Container(string args, Options options)
        {
            ExecuteHelper.Execute("docker", $"container {args}", options.IsDryRun);
        }

        public static void Image(string args, Options options)
        {
            ExecuteHelper.Execute("docker", $"image {args}", options.IsDryRun);
        }

        public static void Login(Options options)
        {
            string loginArgsWithoutPassword = $"login -u {options.Username} -p";
            ExecuteHelper.Execute(
                "docker",
                $"{loginArgsWithoutPassword} {options.Password}",
                options.IsDryRun,
                executeMessageOverride: $"{loginArgsWithoutPassword} ********");
        }

        public static void Logout(Options options)
        {
            ExecuteHelper.Execute("docker", $"logout", options.IsDryRun);
        }

        public static void Pull(string image, Options options)
        {
            ExecuteHelper.ExecuteWithRetry("docker", $"pull {image}", options.IsDryRun);
        }

        public static void Push(string image, Options options)
        {
            ExecuteHelper.ExecuteWithRetry("docker", $"push {image}", options.IsDryRun);
        }

        public static void Volume(string args, Options options)
        {
            ExecuteHelper.Execute("docker", $"volume {args}", options.IsDryRun);
        }
    }
}