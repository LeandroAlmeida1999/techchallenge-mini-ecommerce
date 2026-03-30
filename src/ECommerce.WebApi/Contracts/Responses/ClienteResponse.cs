namespace ECommerce.WebApi.Contracts.Responses;

public sealed record ClienteResponse(
    Guid Id,
    string Nome,
    string Email,
    DateTime DataCadastro);
