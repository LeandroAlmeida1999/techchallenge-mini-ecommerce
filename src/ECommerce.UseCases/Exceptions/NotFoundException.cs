namespace ECommerce.UseCases.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
}
