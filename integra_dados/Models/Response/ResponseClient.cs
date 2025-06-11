using System.Net;

namespace integra_dados.Models.Response;

public class ResponseClient
{
    public HttpStatusCode ResponseStatus { get; set; }
    public bool WasSuccessful { get; set; }
    public object Response { get; set; }
    public string ResponseMessage { get; set; }

    public ResponseClient(HttpStatusCode responseStatus, bool wasSuccessful, object response, string responseMessage)
    {
        ResponseStatus = responseStatus;
        WasSuccessful = wasSuccessful;
        Response = response;
        ResponseMessage = responseMessage;
    }
    
    public ResponseClient (String responseMessage) {
        ResponseStatus = HttpStatusCode.OK;
        WasSuccessful = true;
        Response = null;
        ResponseMessage = responseMessage;
    }
}