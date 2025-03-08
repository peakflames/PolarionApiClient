
using System;

namespace PolarionApiClient.Core;

public class PolarionClientException : Exception
{
    public PolarionClientException(string message) : base(message)
    {
    }

    public PolarionClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

