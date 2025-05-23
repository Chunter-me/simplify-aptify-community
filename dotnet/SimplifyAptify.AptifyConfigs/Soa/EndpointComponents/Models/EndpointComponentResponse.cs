// Ignore Spelling: Ebiz

namespace SimplifyAptify.AptifyConfigs.Soa.EndpointComponents.Models
{
    public class EndpointComponentResponse<T> where T : class
    {
        public bool IsSuccess { get; set; } = true;
        public T Data { get; set; } = null;
        public string Message { get; set; } = string.Empty;
    }

    public class EndpointComponentResponse
    {
        public bool IsSuccess { get; set; } = true;
        public object Data { get; set; } = null;
        public string Message { get; set; } = string.Empty;
    }
}