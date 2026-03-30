namespace ECommerce.Core.Exceptions;

public sealed class DomainException(string message) : Exception(message)
{
}
