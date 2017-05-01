using System;

namespace ImageBuilder
{
    public class Options
    {
        public const string Usage = @"Docker Image Builder

Summary:  Builds all Dockerfiles detected in the current folder and sub-folders in the correct order to satisfy cross dependencies.

Usage:  image-builder [options]

Options:
      --command                         Build command to execeute (build/manifest)
      --dry-run                         Dry run of what images get built and order they would get built in
  -h, --help                            Show help information
      --password                        Password for the Docker registry the images are pushed to
      --push                            Push built images to Docker registry
      --repo-info                       path to json file which describes the repo
      --skip-pulling                    Skip explicitly pulling the base images of the Dockerfiles
      --skip-test                       Skip running the tests
      --username                        Username for the Docker registry the images are pushed to
";

        public CommandType Command { get; private set; }
        public bool IsDryRun { get; private set; }
        public bool IsHelpRequest { get; private set; }
        public bool IsPushEnabled { get; private set; }
        public bool IsSkipPullingEnabled { get; private set; }
        public bool IsTestRunDisabled { get; private set; }
        public string Password { get; private set; }
        public string RepoInfo { get; private set; } = "repo-info.json";
        public string Username { get; private set; }

        private Options()
        {
        }

        public static Options ParseArgs(string[] args)
        {
            Options options = new Options();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, "--command", StringComparison.Ordinal))
                {
                    string commandType = GetArgValue(args, ref i, "command");
                    options.Command = (CommandType)Enum.Parse(typeof(CommandType), commandType, true);
                }
                else if (string.Equals(arg, "--dry-run", StringComparison.Ordinal))
                {
                    options.IsDryRun = true;
                }
                else if (string.Equals(arg, "-h", StringComparison.Ordinal) || string.Equals(arg, "--help", StringComparison.Ordinal))
                {
                    options.IsHelpRequest = true;
                }
                else if (string.Equals(arg, "--push", StringComparison.Ordinal))
                {
                    options.IsPushEnabled = true;
                }
                else if (string.Equals(arg, "--password", StringComparison.Ordinal))
                {
                    options.Password = GetArgValue(args, ref i, "password");
                }
                else if (string.Equals(arg, "--repo-info", StringComparison.Ordinal))
                {
                    options.RepoInfo = GetArgValue(args, ref i, "repo-info");
                }
                else if (string.Equals(arg, "--username", StringComparison.Ordinal))
                {
                    options.Username = GetArgValue(args, ref i, "username");
                }
                else if (string.Equals(arg, "--skip-pulling", StringComparison.Ordinal))
                {
                    options.IsSkipPullingEnabled = true;
                }
                else if (string.Equals(arg, "--skip-test", StringComparison.Ordinal))
                {
                    options.IsTestRunDisabled = true;
                }
                else
                {
                    throw new ArgumentException($"Unknown argument: '{arg}'{Environment.NewLine}{Usage}");
                }
            }

            return options;
        }

        private static string GetArgValue(string[] args, ref int i, string argName)
        {
            if (!IsNextArgValue(args, i))
            {
                throw new ArgumentException($"A '{argName}' value was not specified.{Environment.NewLine}{Usage}");
            }

            i++;
            return args[i];
        }

        private static bool IsNextArgValue(string[] args, int i)
        {
            return i + 1 < args.Length && !args[i + 1].StartsWith("-");
        }
    }
}