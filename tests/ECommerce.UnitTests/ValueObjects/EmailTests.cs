using ECommerce.Core.ValueObjects;

namespace ECommerce.UnitTests.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Deve_normalizar_email_valido()
    {
        var email = new Email("  Maria@Email.com  ");

        Assert.Equal("maria@email.com", email.Valor);
    }

    [Fact]
    public void Deve_lancar_excecao_quando_email_for_invalido()
    {
        Assert.Throws<ArgumentException>(() => new Email("email-invalido"));
    }
}
