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
            ExecuteHelper.Execute(
                "docker",
                $"build {tagArgs} {contextPath}",
                $"Failed to build {contextPath}",
                options.IsDryRun);
        }

        public static void Image(string args, Options options)
        {
            ExecuteHelper.Execute(
                "docker",
                $"image {args}",
                $"Failed to get images",
                options.IsDryRun);
        }

        public static void Login(Options options)
        {
            string loginArgsWithoutPassword = $"login -u {options.Username} -p";
            ExecuteHelper.Execute(
                "docker",
                $"{loginArgsWithoutPassword} {options.Password}",
                $"Failed to login to Docker registry",
                options.IsDryRun,
                $"{loginArgsWithoutPassword} ********");
        }

        public static void Logout(Options options)
        {
            ExecuteHelper.Execute(
                "docker",
                $"logout",
                $"Failed to logout of Docker registry",
                options.IsDryRun);
        }

        public static void Pull(string image, Options options)
        {
            ExecuteHelper.ExecuteWithRetry(
                "docker",
                $"pull {image}",
                $"Failed to pull {image}",
                options.IsDryRun);
        }

        public static void Push(string image, Options options)
        {
            ExecuteHelper.ExecuteWithRetry(
                "docker",
                $"push {image}",
                $"Failed to push {image}",
                options.IsDryRun);
        }

        public static void Container(string args, Options options)
        {
            ExecuteHelper.Execute(
                "docker",
                $"container {args}",
                $"Failed to get volumes",
                options.IsDryRun);
        }

        public static void Volume(string args, Options options)
        {
            ExecuteHelper.Execute(
                "docker",
                $"volume {args}",
                $"Failed to get volumes",
                options.IsDryRun);
        }
    }
}