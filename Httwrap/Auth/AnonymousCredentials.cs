﻿namespace Httwrap.Auth
{
    public class AnonymousCredentials : Credentials
    {
        public override HttpClient BuildHttpClient(HttpMessageHandler httpHandler = null)
        {
            return httpHandler != null ? new HttpClient(httpHandler) : new HttpClient();
        }

        public override bool IsTlsCredentials()
        {
            return false;
        }
    }
}