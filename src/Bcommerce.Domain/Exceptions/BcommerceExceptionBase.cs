using System.Net;

namespace Bcommerce.Domain.Exceptions;

public abstract class BcommerceExceptionBase : SystemException
{
    protected BcommerceExceptionBase(string message) : base(message)
    {
    }

    protected BcommerceExceptionBase()
    {
        throw new NotImplementedException();
    }

    public abstract IList<string> GetErrorMessages();
    public abstract HttpStatusCode GetStatusCode();
}