namespace Polarion;

public class MessageInspector : IClientMessageInspector
{
    public XDocument? LastResponseEnvelope { get; private set; }

    public object? BeforeSendRequest(ref Message request, IClientChannel channel) => null;

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var buffer = reply.CreateBufferedCopy(int.MaxValue);
        reply = buffer.CreateMessage();
        var messageCopy = buffer.CreateMessage();

        LastResponseEnvelope = XDocument.Parse(messageCopy.ToString());
    }
}
