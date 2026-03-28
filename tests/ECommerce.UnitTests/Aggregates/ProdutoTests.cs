using ECommerce.Core.Aggregates;
using ECommerce.Core.ValueObjects;

namespace ECommerce.UnitTests.Aggregates;

public sealed class ProdutoTests
{
    [Fact]
    public void Deve_criar_produto_ativo_por_padrao()
    {
        var produto = Produto.Criar("Teclado", new Money(299.90m));

        Assert.True(produto.Ativo);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_nome_for_invalido()
    {
        Assert.Throws<ArgumentException>(() => Produto.Criar(" ", new Money(10m)));
    }
}
