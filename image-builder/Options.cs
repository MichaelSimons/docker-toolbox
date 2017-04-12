using System;

namespace ImageBuilder
{
    public class Options
    {
        public const string HelpContent = @"Docker Image Builder

Summary:  Builds all Dockerfiles detected in the current folder and sub-folders in the correct order to satisfy cross dependencies.

Usage:  image-builder [repo-info] [options]

Arguments:
    repo-info                           Path to json file which describes the repo

Options:
      --dry-run                         Dry run of what images get built and order they would get built in
  -h, --help                            Show help information
      --password                        Password for the Docker registry the images are pushed to
  -p, --push                            Push built images to Docker registry
      --skip-pulling                    Skip explicitly pulling the base images of the Dockerfiles
      --username                        Username for the Docker registry the images are pushed to
  -v, --verbose                         Enable verbose output
";

        public bool IsDryRun { get; private set; }
        public bool IsHelpRequest { get; private set; }
        public bool IsPushEnabled { get; private set; }
        public bool IsSkipPullingEnabled { get; private set; }
        public bool IsVerboseOutputEnabled { get; private set; }
        public string Password { get; private set; }
        public string RepoInfo { get; private set; }
        public string Username { get; private set; }

        private Options()
        {
        }

        public static Options ParseArgs(string[] args)
        {
            Options options = new Options();

            int i = 0;
            if (!args[i].StartsWith("-"))
            {
                options.RepoInfo = args[i];
                i++;
            }

            for (; i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, "--dry-run", StringComparison.Ordinal))
                {
                    options.IsDryRun = true;
                }
                else if (string.Equals(arg, "-h", StringComparison.Ordinal) || string.Equals(arg, "--help", StringComparison.Ordinal))
                {
                    options.IsHelpRequest = true;
                }
                else if (string.Equals(arg, "-p", StringComparison.Ordinal) || string.Equals(arg, "--push", StringComparison.Ordinal))
                {
                    options.IsPushEnabled = true;
                }
                else if (string.Equals(arg, "--password", StringComparison.Ordinal))
                {
                    options.Password = GetArgValue(args, ref i, "password");
                }
                else if (string.Equals(arg, "--username", StringComparison.Ordinal))
                {
                    options.Username = GetArgValue(args, ref i, "username");
                }
                else if (string.Equals(arg, "-v", StringComparison.Ordinal) || string.Equals(arg, "--verbose", StringComparison.Ordinal))
                {
                    options.IsVerboseOutputEnabled = true;
                }
                else if (string.Equals(arg, "--skip-pulling", StringComparison.Ordinal))
                {
                    options.IsSkipPullingEnabled = true;
                }
                else
                {
                    throw new ArgumentException($"Unknown argument: '{arg}'{Environment.NewLine}{HelpContent}");
                }
            }

            return options;
        }

        private static string GetArgValue(string[] args, ref int i, string argName)
        {
            if (!IsNextArgValue(args, i))
            {
                throw GetValueArgNotFoundException(argName);
            }

            i++;
            return args[i];
        }

        private static bool IsNextArgValue(string[] args, int i)
        {
            return i + 1 < args.Length && !args[i + 1].StartsWith("-");
        }

        private static Exception GetValueArgNotFoundException(string argName)
        {
            return new ArgumentException($"A '{argName}' value was not specified.{Environment.NewLine}{HelpContent}");
        }
    }
}