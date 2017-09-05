using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Hokanson.JenkinsClient
{
    public class JobToken
    {
        public string JobName { get; set; }
        public string Token { get; set; }
    }

    public class JenkinsOptions
    {
        public string JenkinsServerUri { get; set; }
        public string UserName { get; set; }
        public string UserToken { get; set; }
        public List<JobToken> JobTokens { get; set; }

        private static JenkinsOptions _current;
        private static readonly object LockObj = new object();

        public static JenkinsOptions Current()
        {
            if (_current == null)
            {
                lock (LockObj)
                {
                    if (_current == null)
                    {
                        _current = Get();
                    }
                }
            }

            return _current;
        }

        private static JenkinsOptions Get()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .Get<JenkinsOptions>();
        }
    }
}
