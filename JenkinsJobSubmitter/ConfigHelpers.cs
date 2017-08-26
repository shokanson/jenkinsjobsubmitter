using System.Collections.Generic;
using System.Configuration;

namespace JenkinsJobSubmitter
{
    public static class ConfigHelpers
    {
        private const string JobTokensKey = "JobTokens";
        private const string UserNameKey = "UserName";
        private const string UserTokenKey = "UserToken";

        private static readonly Dictionary<string, string> JobTokens = new Dictionary<string, string>();

        static ConfigHelpers()
        {
            string jobTokens = ConfigurationManager.AppSettings[JobTokensKey];
            if (string.IsNullOrEmpty(jobTokens)) return;

            foreach (var pair in jobTokens.Trim().Split(','))
            {
                string[] parts = pair.Trim().Split(':');
                if (parts.Length != 2) return;

                JobTokens[parts[0].Trim()] = parts[1].Trim();
            }
        }

        public static bool TryGetJobToken(string jobName, out string jobToken)
        {
            return JobTokens.TryGetValue(jobName, out jobToken);
        }

        public static bool TryGetUserName(out string userName)
        {
            userName = ConfigurationManager.AppSettings[UserNameKey];
            return !string.IsNullOrEmpty(userName);
        }

        public static bool TryGetUserToken(out string userToken)
        {
            userToken = ConfigurationManager.AppSettings[UserTokenKey];
            return !string.IsNullOrEmpty(userToken);
        }
    }
}
