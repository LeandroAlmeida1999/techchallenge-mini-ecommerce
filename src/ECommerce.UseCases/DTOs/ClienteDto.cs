namespace ECommerce.UseCases.DTOs;

public sealed record ClienteDto(
    Guid Id,
    string Nome,
    string Email,
    DateTime DataCadastro);
