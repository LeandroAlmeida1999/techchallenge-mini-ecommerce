namespace ECommerce.Core.ValueObjects;

public readonly record struct Money
{
    public static Money Zero => new(0m);

    public decimal Valor { get; }

    public Money(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "Valor monetario nao pode ser negativo.");

        Valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    public Money Somar(Money outro) => new(Valor + outro.Valor);

    public Money Multiplicar(Quantidade quantidade) => new(Valor * quantidade.Valor);

    public static Money operator +(Money esquerdo, Money direito) => esquerdo.Somar(direito);

    public override string ToString() => Valor.ToString("0.00");
}
