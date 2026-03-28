using ECommerce.Core.ValueObjects;

namespace ECommerce.UnitTests.ValueObjects;

public sealed class QuantidadeTests
{
    [Fact]
    public void Deve_criar_quantidade_positiva()
    {
        var quantidade = new Quantidade(2);

        Assert.Equal(2, quantidade.Valor);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_quantidade_for_zero_ou_negativa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quantidade(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quantidade(-1));
    }
}
