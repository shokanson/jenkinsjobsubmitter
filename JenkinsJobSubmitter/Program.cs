using System;
using System.Configuration;

namespace JenkinsJobSubmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 3) return;

            string baseUri = ConfigurationManager.AppSettings["JenkinsServerUri"];
            string userName = ConfigurationManager.AppSettings["UserName"];
            string userToken = ConfigurationManager.AppSettings["UserToken"];

            string jobName = args[0];
            string paramName = args[1];
            string paramValue = args[2];

            if (ConfigHelpers.TryGetJobToken(jobName, out string jobToken))
            {
                using (var jenkinkClient = new JenkinsClient(baseUri, userName, userToken))
                {
                    jenkinkClient.SubmitParameterizedJobAsync(jobName, jobToken, new[] { new Tuple<string, string>(paramName, paramValue) })
                                 .Wait();
                }
            }
        }
    }
}
