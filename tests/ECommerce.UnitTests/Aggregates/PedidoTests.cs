using ECommerce.Core.Aggregates;
using ECommerce.Core.DomainEvents;
using ECommerce.Core.DomainServices;
using ECommerce.Core.Enums;
using ECommerce.Core.Exceptions;
using ECommerce.Core.ValueObjects;

namespace ECommerce.UnitTests.Aggregates;

public sealed class PedidoTests
{
    private static readonly CalculadoraPedidoDomainService Calculadora = new();
    private static readonly ValidadorConfirmacaoPedidoDomainService ValidadorConfirmacao = new();

    [Fact]
    public void Deve_atualizar_total_ao_adicionar_item()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", new Money(50m));

        pedido.AdicionarItem(produto, new Quantidade(2), Calculadora);

        Assert.Equal(100m, pedido.ValorTotal.Valor);
        Assert.Single(pedido.Itens);
    }

    [Fact]
    public void Deve_atualizar_total_ao_remover_item()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", new Money(50m));

        pedido.AdicionarItem(produto, new Quantidade(2), Calculadora);
        pedido.RemoverItem(produto.ProdutoId, Calculadora);

        Assert.Empty(pedido.Itens);
        Assert.Equal(0m, pedido.ValorTotal.Valor);
    }

    [Fact]
    public void Nao_deve_confirmar_pedido_sem_itens()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());

        var exception = Assert.Throws<DomainException>(() => pedido.Confirmar(Calculadora, ValidadorConfirmacao));

        Assert.Equal("Pedido nao pode ser confirmado sem itens.", exception.Message);
    }

    [Fact]
    public void Nao_deve_confirmar_pedido_ja_confirmado()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", new Money(50m));

        pedido.AdicionarItem(produto, new Quantidade(1), Calculadora);
        pedido.Confirmar(Calculadora, ValidadorConfirmacao);

        var exception = Assert.Throws<DomainException>(() => pedido.Confirmar(Calculadora, ValidadorConfirmacao));

        Assert.Equal("Pedido ja foi confirmado.", exception.Message);
    }

    [Fact]
    public void Nao_deve_adicionar_produto_inativo()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", new Money(50m), ativo: false);

        var exception = Assert.Throws<DomainException>(() =>
            pedido.AdicionarItem(produto, new Quantidade(1), Calculadora));

        Assert.Equal("Produto inativo nao pode ser adicionado ao pedido.", exception.Message);
    }

    [Fact]
    public void Deve_confirmar_pedido_e_gerar_evento_de_dominio()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", new Money(50m));

        pedido.AdicionarItem(produto, new Quantidade(2), Calculadora);
        pedido.Confirmar(Calculadora, ValidadorConfirmacao);

        Assert.Equal(StatusPedido.Confirmado, pedido.Status);
        Assert.Single(pedido.DomainEvents);
        Assert.IsType<PedidoConfirmadoDomainEvent>(pedido.DomainEvents.Single());
    }

    [Fact]
    public void Nao_deve_confirmar_pedido_com_total_zerado()
    {
        var pedido = Pedido.Criar(Guid.NewGuid());
        var produto = Produto.Criar("Mouse", Money.Zero);

        pedido.AdicionarItem(produto, new Quantidade(1), Calculadora);

        var exception = Assert.Throws<DomainException>(() => pedido.Confirmar(Calculadora, ValidadorConfirmacao));

        Assert.Equal("Pedido precisa ter valor total maior que zero para ser confirmado.", exception.Message);
    }
}
