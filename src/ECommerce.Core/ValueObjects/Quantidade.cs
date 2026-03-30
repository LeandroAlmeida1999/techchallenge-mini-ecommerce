namespace ECommerce.Core.ValueObjects;

public readonly record struct Quantidade
{
    public int Valor { get; }

    public Quantidade(int valor)
    {
        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "Quantidade deve ser maior que zero.");

        Valor = valor;
    }

    public Quantidade Somar(Quantidade outra) => new(Valor + outra.Valor);

    public static Quantidade operator +(Quantidade esquerda, Quantidade direita) => esquerda.Somar(direita);

    public override string ToString() => Valor.ToString();
}
