using BlazorApplicationInsights;
using BlazorShared;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorAdmin.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IApplicationInsights _applicationInsights;
        private readonly IJSRuntime _js;
        private readonly string _apiUrl;


        public HttpService(HttpClient httpClient, BaseUrlConfiguration baseUrlConfiguration, IApplicationInsights applicationInsights
            , IJSRuntime js)
        {
            _httpClient = httpClient;
            _applicationInsights = applicationInsights ?? throw new ArgumentNullException(nameof(applicationInsights));
            _js = js;
            _apiUrl = baseUrlConfiguration.ApiBase;
        }

        public async Task<T> HttpGet<T>(string uri)
            where T : class
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            try
            {
                result = await _httpClient.GetAsync($"{_apiUrl}{uri}");
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                timer.Stop();
                await TrackDependancy(result, uri, "Get", timer.Elapsed);
                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {

            }
        }

        private Task TrackDependancy(HttpResponseMessage result, string uri, string method, TimeSpan duration)
        {
            return _applicationInsights.TrackDependencyData(Guid.NewGuid().ToString()
                , Convert.ToDouble(result.StatusCode)
                , absoluteUrl: _apiUrl
                , success: true
                , commandName: uri
                , duration: duration.TotalSeconds
                , method: method);
        }

        public async Task<T> HttpDelete<T>(string uri, int id)
            where T : class
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            try
            {
                result = await _httpClient.DeleteAsync($"{_apiUrl}{uri}/{id}");
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, uri, "Delete", timer.Elapsed);
            }
        }

        public async Task<T> HttpPost<T>(string uri, object dataToSend)
            where T : class
        {
            var content = ToJson(dataToSend);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            try
            {
                result = await _httpClient.PostAsync($"{_apiUrl}{uri}", content);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, uri, "Post", timer.Elapsed);
            }
        }

        public async Task<T> HttpPut<T>(string uri, object dataToSend)
            where T : class
        {
            var content = ToJson(dataToSend);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            try
            {
                result = await _httpClient.PutAsync($"{_apiUrl}{uri}", content);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, uri, "Put", timer.Elapsed);
            }
        }


        private StringContent ToJson(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }

        private async Task<T> FromHttpResponseMessage<T>(HttpResponseMessage result)
        {
            return JsonSerializer.Deserialize<T>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
