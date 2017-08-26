using System;

namespace JenkinsJobSubmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 3) return;

            string jobName = args[0];
            string paramName = args[1];
            string paramValue = args[2];

            var config = new JenkinsConfiguration();

            if (config.JobTokens.TryGetValue(jobName, out string jobToken))
            {
                using (var jenkinsClient = new JenkinsClient(config))
                {
                    jenkinsClient.SubmitParameterizedJobAsync(jobName, jobToken, new[] { new Tuple<string, string>(paramName, paramValue) })
                                 .Wait();
                }
            }
        }
    }
}
