namespace Polarion;

public class SessionHeaderInspector : IClientMessageInspector
{
    private readonly string _sessionId;

    public SessionHeaderInspector(string sessionId)
    {
        _sessionId = sessionId;
    }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var header = MessageHeader.CreateHeader(
            "sessionID",
            "http://ws.polarion.com/session",
            _sessionId);

        request.Headers.Add(header);
        return null;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState) { }
}