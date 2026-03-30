using ECommerce.UseCases.DTOs;
using ECommerce.WebApi.Contracts.Responses;

namespace ECommerce.WebApi.Contracts;

public static class ResponseMappings
{
    public static ClienteResponse ToResponse(this ClienteDto dto)
    {
        return new ClienteResponse(dto.Id, dto.Nome, dto.Email, dto.DataCadastro);
    }

    public static ProdutoResponse ToResponse(this ProdutoDto dto)
    {
        return new ProdutoResponse(dto.Id, dto.Nome, dto.Preco, dto.Ativo);
    }

    public static PedidoResponse ToResponse(this PedidoDto dto)
    {
        return new PedidoResponse(
            dto.Id,
            dto.ClienteId,
            dto.Status,
            dto.DataCriacao,
            dto.ValorTotal,
            dto.Itens.Select(ToResponse).ToArray());
    }

    private static ItemPedidoResponse ToResponse(ItemPedidoDto dto)
    {
        return new ItemPedidoResponse(dto.ProdutoId, dto.Quantidade, dto.PrecoUnitario, dto.Subtotal);
    }
}
