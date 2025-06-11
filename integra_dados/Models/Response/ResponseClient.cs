using System.Net;

namespace integra_dados.Models.Response;

public class ResponseClient
{
    private HttpStatusCode ResponseStatus { get; set; }
    private bool WasSuccessful { get; set; }
    private object Response { get; set; }
    private string ResponseMessage { get; set; }
}