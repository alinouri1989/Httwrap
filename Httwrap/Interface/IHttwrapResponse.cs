using System.Net;

namespace Httwrap.Interface
{
    public interface IHttwrapResponse<out T> : IHttwrapResponse
    {
        T Data { get; }
    }

    public interface IHttwrapResponse
    {
        HttpStatusCode StatusCode { get; }
        string Body { get; }
        bool Success { get; }
        HttpResponseMessage Raw { get; }
    }
}