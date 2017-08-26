using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Hokanson.JenkinsClient
{
    public class JenkinsClient : IDisposable
    {
        public JenkinsClient(JenkinsConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient { BaseAddress = new Uri(config.JenkinsServerUri) };
            string basicAuthValue = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{config.UserName}:{config.UserToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
        }

        private readonly HttpClient _httpClient;
        private readonly JenkinsConfiguration _config;

        public async Task SubmitParameterizedJobAsync(string jobName, Tuple<string,string>[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (!_config.JobTokens.TryGetValue(jobName, out string jobToken))
                throw new Exception($"no token exists for job '{jobName}'");

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"job/{jobName}/buildWithParameters?token={jobToken}"))
            {
                (string requestField, string crumb) = await GetCrumbAsync();
                request.Headers.Add(requestField, crumb);

                object obj = new { parameter = parameters.Select(p => new { name = p.Item1, value = p.Item2 }) };
                string serializedObj = JsonConvert.SerializeObject(obj);
                request.Content = new StringContent(
                    serializedObj,
                    Encoding.UTF8,
                    "application/json");

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task SubmitJobAsync(string jobName)
        {
            if (!_config.JobTokens.TryGetValue(jobName, out string jobToken))
                throw new Exception($"no token exists for job '{jobName}'");

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"job/{jobName}/build?token={jobToken}"))
            {
                (string requestField, string crumb) = await GetCrumbAsync();
                request.Headers.Add(requestField, crumb);

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
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
                var crumbDefinition = new {
                    _class = default(string),
                    crumb = default(string),
                    crumbRequestField = default(string)
                };
                var crumbData = JsonConvert.DeserializeAnonymousType(responseBody, crumbDefinition);

                return (crumbData.crumbRequestField, crumbData.crumb);
            }
        }
#endregion
    }
}