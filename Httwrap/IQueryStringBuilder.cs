namespace Httwrap
{
    public interface IQueryStringBuilder
    {
        string BuildFrom<T>(T payload, string separator = ",");
    }
}