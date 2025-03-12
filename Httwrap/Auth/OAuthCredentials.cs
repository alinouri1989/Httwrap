﻿using System.Net.Http.Headers;

namespace Httwrap.Auth
{
    public class OAuthCredentials : Credentials
    {
        private readonly string _grantType;
        private readonly bool _isTls;
        private readonly string _password;
        private readonly string _requestEndpoint;
        private readonly string _username;
        private string _token;

        public OAuthCredentials(string token, bool isTls = false)
        {
            Check.NotNullOrEmpty(token, "token");
            _token = token;
            _isTls = isTls;
        }

        public OAuthCredentials(string username, string password, string requestEndpoint)
        {
            Check.NotNullOrEmpty(username, "username");
            Check.NotNullOrEmpty(password, "password");
            Check.NotNullOrEmpty(requestEndpoint, "requestEndpoint");
            _username = username;
            _password = password;
            _requestEndpoint = requestEndpoint;
            _grantType = "password";
        }

        public override HttpClient BuildHttpClient(HttpMessageHandler httpHandler = null)
        {
            //TODO: Refresh Token :)
            var httpClient = httpHandler != null ? new HttpClient(httpHandler) : new HttpClient();

            if (string.IsNullOrEmpty(_token))
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"username", _username},
                    {"password", _password},
                    {"grant_type", _grantType}
                });

                var response = httpClient.PostAsync(_requestEndpoint, content).Result;
                var token = new HttwrapResponse(response).ReadAs<Token>();
                _token = token.AccessToken;
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return httpClient;
        }

        public override bool IsTlsCredentials()
        {
            return _isTls;
        }
    }
}