using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JenkinsJobSubmitter
{
    class JenkinsClient : IDisposable
    {
        public JenkinsClient(string baseUri, string userName, string userToken)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
            string basicAuthValue = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{userName}:{userToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
        }

        private readonly HttpClient _httpClient;

        public async Task SubmitParameterizedJobAsync(string jobName, string jobToken, Tuple<string,string>[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            string crumb = await GetCrumbAsync();

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"job/{jobName}/buildWithParameters?token={jobToken}"))
            {
                request.Headers.Add("Jenkins-Crumb", crumb);
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
        private async Task<string> GetCrumbAsync()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "crumbIssuer/api/json"))
            using (var response = await _httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(responseBody);
                return obj.crumb;
            }
        }
#endregion
    }
}