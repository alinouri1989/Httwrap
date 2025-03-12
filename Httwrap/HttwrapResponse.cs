﻿using Httwrap.Interface;
using System.Net;

namespace Httwrap
{
    public class HttwrapResponse : IHttwrapResponse
    {
        public HttwrapResponse(HttpResponseMessage message)
        {
            StatusCode = message.StatusCode;
            Body = message.Content.ReadAsStringAsync().Result;
            Raw = message;
        }

        public HttwrapResponse(HttpStatusCode statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public HttpStatusCode StatusCode { get; protected set; }
        public string Body { get; protected set; }

        public virtual bool Success
        {
            get { return StatusCode == HttpStatusCode.OK; }
        }

        public HttpResponseMessage Raw { get; protected set; }
    }

    public class HttwrapResponse<T> : HttwrapResponse, IHttwrapResponse<T>
    {
        public HttwrapResponse(HttpStatusCode statusCode, string body)
            : base(statusCode, body)
        {
        }

        public HttwrapResponse(HttpResponseMessage message)
            : base(message)
        {
        }

        public T Data { get; set; }
    }
}