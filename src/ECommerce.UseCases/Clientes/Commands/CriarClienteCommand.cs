namespace ECommerce.UseCases.Clientes.Commands;

public sealed record CriarClienteCommand(
    string Nome,
    string Email);
