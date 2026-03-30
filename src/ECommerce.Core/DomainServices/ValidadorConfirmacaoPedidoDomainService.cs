using ECommerce.Core.Enums;
using ECommerce.Core.Exceptions;

namespace ECommerce.Core.DomainServices;

public sealed class ValidadorConfirmacaoPedidoDomainService
{
    public void Validar(StatusPedido status, int quantidadeItens, decimal valorTotal)
    {
        if (status == StatusPedido.Confirmado)
            throw new DomainException("Pedido ja foi confirmado.");

        if (quantidadeItens == 0)
            throw new DomainException("Pedido nao pode ser confirmado sem itens.");

        if (valorTotal <= 0)
            throw new DomainException("Pedido precisa ter valor total maior que zero para ser confirmado.");
    }
}
