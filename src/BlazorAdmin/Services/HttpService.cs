using BlazorApplicationInsights;
using BlazorShared;
using System;
using System.Diagnostics;
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
        private readonly string _apiUrl;


        public HttpService(HttpClient httpClient, BaseUrlConfiguration baseUrlConfiguration
            , IApplicationInsights applicationInsights)
        {
            _httpClient = httpClient;
            _applicationInsights = applicationInsights 
                ?? throw new ArgumentNullException(nameof(applicationInsights));
            _apiUrl = baseUrlConfiguration.ApiBase;

            //Activity activity = new Activity("CallAPI");
            //activity.Start();
            //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("traceparent", activity.Id);
            //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("tracestate", activity.TraceStateString);
        }

        public async Task<T> HttpGet<T>(string uri)
            where T : class
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            string requestUrl = $"{_apiUrl}{uri}";
            try
            {           
                
                result = await _httpClient.GetAsync(requestUrl);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }
                
                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, requestUrl, "Get", timer.Elapsed);
            }
        }

        private Task TrackDependancy(HttpResponseMessage result, string uri, string method, TimeSpan duration)
        {
            return Task.CompletedTask;
            //return _applicationInsights.TrackDependencyData(Guid.NewGuid().ToString()
            //    , Convert.ToDouble(result.StatusCode)
            //    , absoluteUrl: uri
            //    , success: true
            //    , commandName: method
            //    , duration: duration.TotalSeconds
            //    , method: method);
        }

        public async Task<T> HttpDelete<T>(string uri, int id)
            where T : class
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            string requestUrl = $"{_apiUrl}{uri}/{id}";
            try
            {
                result = await _httpClient.DeleteAsync(requestUrl);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, requestUrl, "Delete", timer.Elapsed);
            }
        }

        public async Task<T> HttpPost<T>(string uri, object dataToSend)
            where T : class
        {
            var content = ToJson(dataToSend);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            string requestUri = $"{_apiUrl}{uri}";
            try
            {
                result = await _httpClient.PostAsync(requestUri, content);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, requestUri, "Post", timer.Elapsed);
            }
        }

        public async Task<T> HttpPut<T>(string uri, object dataToSend)
            where T : class
        {
            var content = ToJson(dataToSend);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage result = null;
            string requestUri = $"{_apiUrl}{uri}";
            try
            {
                result = await _httpClient.PutAsync(requestUri, content);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }

                return await FromHttpResponseMessage<T>(result);
            }
            finally
            {
                timer.Stop();
                await TrackDependancy(result, requestUri, "Put", timer.Elapsed);
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
