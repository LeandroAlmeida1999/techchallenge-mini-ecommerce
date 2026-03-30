namespace ECommerce.WebApi.Contracts.Requests;

public sealed record CriarClienteRequest(
    string Nome,
    string Email);
