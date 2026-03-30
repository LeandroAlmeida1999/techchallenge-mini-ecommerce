using ECommerce.Core.Aggregates;
using ECommerce.Core.Entities;

namespace ECommerce.UseCases.DTOs;

public static class DtoMappings
{
    public static ClienteDto ToDto(this Cliente cliente)
    {
        return new ClienteDto(
            cliente.ClienteId,
            cliente.Nome,
            cliente.Email.Valor,
            cliente.DataCadastro);
    }

    public static ProdutoDto ToDto(this Produto produto)
    {
        return new ProdutoDto(
            produto.ProdutoId,
            produto.Nome,
            produto.Preco.Valor,
            produto.Ativo);
    }

    public static PedidoDto ToDto(this Pedido pedido)
    {
        return new PedidoDto(
            pedido.PedidoId,
            pedido.ClienteId,
            pedido.Status.ToString(),
            pedido.DataCriacao,
            pedido.ValorTotal.Valor,
            pedido.Itens.Select(ToDto).ToArray());
    }

    public static ItemPedidoDto ToDto(this ItemPedido item)
    {
        return new ItemPedidoDto(
            item.ProdutoId,
            item.Quantidade.Valor,
            item.PrecoUnitario.Valor,
            item.Subtotal.Valor);
    }
}
