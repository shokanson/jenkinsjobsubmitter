using Hokanson.JenkinsClient;
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

            try
            {
                using (var jenkinsClient = new JenkinsClient(new JenkinsConfiguration()))
                {
                    jenkinsClient.SubmitParameterizedJobAsync(jobName, new[] { new Tuple<string, string>(paramName, paramValue) })
                                 .Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}