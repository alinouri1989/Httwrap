﻿using Httwrap.Interception;
using Httwrap.Interface;
using System.Net;

namespace Httwrap
{
    public sealed class HttwrapClient : IHttwrapClient, IDisposable
    {
        private const string UserAgent = "Httwrap";
        private readonly IHttwrapConfiguration _configuration;
        private readonly ICollection<IHttpInterceptor> _interceptors;

        private readonly Action<HttpStatusCode, string> _defaultErrorHandler = (statusCode, body) =>
        {
            if (statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest)
            {
                throw new HttwrapHttpException(statusCode, body);
            }
        };

        private readonly HttpClient _httpClient;

        private readonly IQueryStringBuilder _queryStringBuilder;

        public HttwrapClient(IHttwrapConfiguration configuration)
            : this(configuration, new QueryStringBuilder())
        {
        }

        internal HttwrapClient(IHttwrapConfiguration configuration, IQueryStringBuilder queryStringBuilder)
        {
            _configuration = configuration;
            _queryStringBuilder = queryStringBuilder;
            _httpClient = _configuration.GetHttpClient();
            _interceptors = new List<IHttpInterceptor>();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<IHttwrapResponse> GetAsync(string path, Action<HttpStatusCode, string> errorHandler = null,
            Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }


        public async Task<IHttwrapResponse> GetAsync(string path, object payload,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            path = $"{path}?{_queryStringBuilder.BuildFrom(payload)}";

            return await RequestAsync(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }

        public async Task<IHttwrapResponse<T>> GetAsync<T>(string path,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync<T>(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }

        public async Task<IHttwrapResponse<T>> GetAsync<T>(string path, object payload,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            path = $"{path}?{_queryStringBuilder.BuildFrom(payload)}";
            return await RequestAsync<T>(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }

        public IHttwrapResponse Get(string path, Action<HttpStatusCode, string> errorHandler = null,
           Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return Request(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }
        public IHttwrapResponse Get(string path, object payload, Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            path = $"{path}?{_queryStringBuilder.BuildFrom(payload)}";

            return Request(HttpMethod.Get, path, null, errorHandler, customHeaders, requestTimeout);
        }

        public async Task<IHttwrapResponse> PutAsync<T>(string path, T data,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync(HttpMethod.Put, path, data, errorHandler, customHeaders, requestTimeout);
        }

        public IHttwrapResponse Put<T>(string path, T data, Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return Request(HttpMethod.Put, path, data, errorHandler, customHeaders, requestTimeout);
        }

        public async Task<IHttwrapResponse> PostAsync<T>(string path, T data,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync(HttpMethod.Post, path, data, errorHandler, customHeaders, requestTimeout);
        }

        public IHttwrapResponse Post<T>(string path, T data, Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return Request(HttpMethod.Post, path, data, errorHandler, customHeaders, requestTimeout);
        }

        public async Task<IHttwrapResponse> DeleteAsync(string path, Action<HttpStatusCode, string> errorHandler = null,
            Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync(HttpMethod.Delete, path, null, errorHandler, customHeaders, requestTimeout);
        }

        public IHttwrapResponse Delete(string path, Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return Request(HttpMethod.Delete, path, null, errorHandler, customHeaders, requestTimeout);

        }

        public async Task<IHttwrapResponse> PatchAsync<T>(string path, T data,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return await RequestAsync(new HttpMethod("PATCH"), path, data, errorHandler, customHeaders, requestTimeout);
        }

        public IHttwrapResponse Patch<T>(string path, T data, Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            return Request(new HttpMethod("PATCH"), path, data, errorHandler, customHeaders, requestTimeout);
        }

        public void AddInterceptor(IHttpInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
        }

        private IHttwrapResponse Request(HttpMethod method, string path, object body,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            var response = RequestImpl(requestTimeout, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None, method,
                        path, body, customHeaders);

            var content = response.Content.ReadAsStringAsync().Result;

            HandleIfErrorResponse(response.StatusCode, content, errorHandler);

            return new HttwrapResponse(response.StatusCode, content);
        }

        private IHttwrapResponse Request<T>(HttpMethod method, string path, object body,
          Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            var response = RequestImpl(requestTimeout, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None, method,
                        path, body, customHeaders);

            var content = response.Content.ReadAsStringAsync().Result;

            HandleIfErrorResponse(response.StatusCode, content, errorHandler);

            return new HttwrapResponse(response.StatusCode, content);
        }

        private async Task<IHttwrapResponse> RequestAsync(HttpMethod method, string path, object body,
            Action<HttpStatusCode, string> errorHandler = null, Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            var response =
                await
                    RequestAsyncImpl(requestTimeout, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None, method,
                        path, body, customHeaders);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, content, errorHandler);

            return new HttwrapResponse(response.StatusCode, content);
        }

        private async Task<IHttwrapResponse<T>> RequestAsync<T>(HttpMethod method, string path,
            object body, Action<HttpStatusCode, string> errorHandler = null,
            Dictionary<string, string> customHeaders = null, TimeSpan? requestTimeout = null)
        {
            var response =
                await
                    RequestAsyncImpl(requestTimeout, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None, method,
                        path, body, customHeaders);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, content, errorHandler);

            return new HttwrapResponse<T>(response)
            {
                Data = _configuration.Serializer.Deserialize<T>(content)
            };
        }

        private HttpResponseMessage RequestImpl(TimeSpan? requestTimeout,
            HttpCompletionOption completionOption, CancellationToken cancellationToken, HttpMethod method,
            string path, object body, Dictionary<string, string> customHeaders = null)
        {
            try
            {
                if (requestTimeout.HasValue)
                {
                    _httpClient.Timeout = requestTimeout.Value;
                }

                var request = PrepareRequest(method, body, path, customHeaders);

                foreach (IHttpInterceptor interceptor in _interceptors)
                {
                    interceptor.OnRequest(request);
                }

                HttpResponseMessage response = _httpClient.SendAsync(request, completionOption, cancellationToken).Result;

                foreach (IHttpInterceptor interceptor in _interceptors)
                {
                    interceptor.OnResponse(request, response);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new HttwrapException(
                    $"An error occured while execution request. Path : {path} , HttpMethod : {method}", ex);
            }
        }

        private async Task<HttpResponseMessage> RequestAsyncImpl(TimeSpan? requestTimeout,
            HttpCompletionOption completionOption, CancellationToken cancellationToken, HttpMethod method,
            string path, object body, Dictionary<string, string> customHeaders = null)
        {
            try
            {
                if (requestTimeout.HasValue)
                {
                    _httpClient.Timeout = requestTimeout.Value;
                }

                var request = PrepareRequest(method, body, path, customHeaders);

                foreach (IHttpInterceptor interceptor in _interceptors)
                {
                    interceptor.OnRequest(request);
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request, completionOption, cancellationToken);

                foreach (IHttpInterceptor interceptor in _interceptors)
                {
                    interceptor.OnResponse(request, response);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new HttwrapException(
                    $"An error occured while execution request. Path : {path} , HttpMethod : {method}", ex);
            }
        }

        private HttpRequestMessage PrepareRequest(HttpMethod method, object body, string path,
            Dictionary<string, string> customHeaders = null)
        {
            var url = $"{_configuration.BasePath}{path}";

            var request = new HttpRequestMessage(method, url);

            request.Headers.Add("User-Agent", UserAgent);

            request.Headers.Add("Accept", "application/json");

            if (customHeaders != null)
                foreach (var header in customHeaders) request.Headers.Add(header.Key, header.Value);

            if (body != null)
            {
                var content = new JsonRequestContent(body, _configuration.Serializer);
                var requestContent = content.GetContent();
                request.Content = requestContent;
            }

            return request;
        }

        private void HandleIfErrorResponse(HttpStatusCode statusCode, string content,
            Action<HttpStatusCode, string> errorHandler)
        {
            if (errorHandler != null)
            {
                errorHandler(statusCode, content);
            }
            else
            {
                _defaultErrorHandler(statusCode, content);
            }
        }
    }
}