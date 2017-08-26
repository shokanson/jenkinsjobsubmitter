using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JenkinsJobSubmitter
{
    public class JenkinsConfiguration
    {
        private const string JenkinsServerUriKey = "JenkinsServerUri";
        private const string JobTokensKey = "JobTokens";
        private const string UserNameKey = "UserName";
        private const string UserTokenKey = "UserToken";

        public JenkinsConfiguration()
        {
            string userName = ConfigurationManager.AppSettings[UserNameKey];
            if (string.IsNullOrEmpty(userName)) throw new Exception($"{UserNameKey} not found in config");
            UserName = userName;

            string userToken = ConfigurationManager.AppSettings[UserTokenKey];
            if (string.IsNullOrEmpty(userToken)) throw new Exception($"{UserTokenKey} not found in config");
            UserToken = userToken;

            string jenkinsServerUri = ConfigurationManager.AppSettings[JenkinsServerUriKey];
            if (string.IsNullOrEmpty(jenkinsServerUri)) throw new Exception($"{JenkinsServerUriKey} not found in config");
            if (!jenkinsServerUri.EndsWith("/")) jenkinsServerUri += "/";
            JenkinsServerUri = jenkinsServerUri;
            
            string jobTokens = ConfigurationManager.AppSettings[JobTokensKey];
            if (!string.IsNullOrEmpty(jobTokens))
            {
                foreach (var pair in jobTokens.Trim().Split(','))
                {
                    string[] parts = pair.Trim().Split('=');
                    if (parts.Length != 2) continue;

                    _jobTokens[parts[0].Trim()] = parts[1].Trim();
                }
            }
            if (_jobTokens.Count == 0) throw new Exception("No job tokens found in config");
        }

        private readonly Dictionary<string,string> _jobTokens = new Dictionary<string, string>();

        public string JenkinsServerUri { get; private set; }
        public string UserName { get; private set; }
        public string UserToken { get; private set; }
        public IReadOnlyDictionary<string,string> JobTokens => _jobTokens;
    }
}
