using ECommerce.Core.DomainEvents;
using ECommerce.Core.DomainServices;
using ECommerce.Core.Entities;
using ECommerce.Core.Enums;
using ECommerce.Core.Exceptions;
using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.Aggregates;

public sealed class Pedido : AggregateRoot
{
    private readonly List<ItemPedido> _itens = [];

    private Pedido()
    {
    }

    public Guid PedidoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public Money ValorTotal { get; private set; }
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    public Pedido(Guid pedidoId, Guid clienteId, DateTime? dataCriacao = null)
    {
        if (pedidoId == Guid.Empty)
            throw new ArgumentException("PedidoId deve ser informado.", nameof(pedidoId));

        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId deve ser informado.", nameof(clienteId));

        PedidoId = pedidoId;
        ClienteId = clienteId;
        DataCriacao = dataCriacao ?? DateTime.UtcNow;
        Status = StatusPedido.Rascunho;
        ValorTotal = Money.Zero;
    }

    public static Pedido Criar(Guid clienteId)
    {
        return new Pedido(Guid.NewGuid(), clienteId);
    }

    public void AdicionarItem(Produto produto, Quantidade quantidade, CalculadoraPedidoDomainService calculadora)
    {
        ArgumentNullException.ThrowIfNull(produto);
        ArgumentNullException.ThrowIfNull(calculadora);

        ValidarPedidoEmRascunho();

        if (!produto.Ativo)
            throw new DomainException("Produto inativo nao pode ser adicionado ao pedido.");

        var itemExistente = _itens.FirstOrDefault(item => item.ProdutoId == produto.ProdutoId);

        if (itemExistente is null)
            _itens.Add(new ItemPedido(produto.ProdutoId, quantidade, produto.Preco));
        else
            itemExistente.AtualizarQuantidade(itemExistente.Quantidade + quantidade);

        RecalcularTotal(calculadora);
    }

    public void RemoverItem(Guid produtoId, CalculadoraPedidoDomainService calculadora)
    {
        ArgumentNullException.ThrowIfNull(calculadora);

        ValidarPedidoEmRascunho();

        var item = _itens.FirstOrDefault(x => x.ProdutoId == produtoId) ?? throw new DomainException("Item do pedido nao encontrado para remocao.");

        _itens.Remove(item);
        RecalcularTotal(calculadora);
    }

    public void Confirmar(CalculadoraPedidoDomainService calculadora)
    {
        ArgumentNullException.ThrowIfNull(calculadora);

        if (Status == StatusPedido.Confirmado)
            throw new DomainException("Pedido ja foi confirmado.");

        if (_itens.Count == 0)
            throw new DomainException("Pedido nao pode ser confirmado sem itens.");

        RecalcularTotal(calculadora);

        Status = StatusPedido.Confirmado;

        AddDomainEvent(new PedidoConfirmadoDomainEvent(
            PedidoId,
            ClienteId,
            ValorTotal,
            DateTime.UtcNow));
    }

    private void RecalcularTotal(CalculadoraPedidoDomainService calculadora)
    {
        ValorTotal = calculadora.CalcularTotal(_itens);
    }

    private void ValidarPedidoEmRascunho()
    {
        if (Status != StatusPedido.Rascunho)
            throw new DomainException("Pedido confirmado nao pode ser alterado.");
    }
}
