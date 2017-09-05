using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Hokanson.JenkinsClient
{
    public class JenkinsClient : IDisposable
    {
        public JenkinsClient(JenkinsOptions options)
        {
            _options = options;
            _httpClient = new HttpClient { BaseAddress = new Uri(options.JenkinsServerUri) };
            string basicAuthValue = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{options.UserName}:{options.UserToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
        }

        private readonly HttpClient _httpClient;
        private readonly JenkinsOptions _options;

        public Task SubmitParameterizedJob(string jobName, IEnumerable<(string name, string value)> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (!_options.JobTokens.Any(jt => jt.JobName == jobName)) throw new Exception($"no token exists for job '{jobName}'");

            string jobToken = _options.JobTokens.First(jt => jt.JobName == jobName).Token;
            var uriBuilder = new StringBuilder($"job/{jobName}/buildWithParameters?token={jobToken}");
            foreach (var parameter in parameters)
            {
                uriBuilder.Append($"&{parameter.name}={parameter.value}");
            }

            return PostRequestAsync(uriBuilder.ToString());
        }

        public Task SubmitJobAsync(string jobName)
        {
            if (!_options.JobTokens.Any(jt => jt.JobName == jobName)) throw new Exception($"no token exists for job '{jobName}'");

            string jobToken = _options.JobTokens.First(jt => jt.JobName == jobName).Token;
            return PostRequestAsync($"job/{jobName}/build?token={jobToken}");
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
        }

        #endregion

        #region Internal stuff
        private async Task<(string requestField, string crumb)> GetCrumbAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "crumbIssuer/api/json"))
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // anonymous type to provide shape of return data
                var crumbDefinition = new
                {
                    _class = default(string),
                    crumb = default(string),
                    crumbRequestField = default(string)
                };
                var crumbData = JsonConvert.DeserializeAnonymousType(responseBody, crumbDefinition);

                return (crumbData.crumbRequestField, crumbData.crumb);
            }
        }

        private async Task PostRequestAsync(string requestUri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                (string requestField, string crumb) = await GetCrumbAsync();
                request.Headers.Add(requestField, crumb);

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
        #endregion
    }
}
