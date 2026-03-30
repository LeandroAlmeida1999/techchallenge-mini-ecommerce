using ECommerce.Core.ValueObjects;

namespace ECommerce.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Deve_somar_valores_monetarios()
    {
        var resultado = new Money(10.50m) + new Money(5.25m);

        Assert.Equal(15.75m, resultado.Valor);
    }

    [Fact]
    public void Deve_multiplicar_valor_por_quantidade()
    {
        var resultado = new Money(12.30m).Multiplicar(new Quantidade(3));

        Assert.Equal(36.90m, resultado.Valor);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_valor_for_negativo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-1m));
    }
}
