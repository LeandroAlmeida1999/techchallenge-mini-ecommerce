using System.Text.RegularExpressions;

namespace ECommerce.Core.ValueObjects;

public sealed partial record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email deve ser informado.", nameof(valor));

        var normalizado = valor.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizado))
            throw new ArgumentException("Email invalido.", nameof(valor));

        Valor = normalizado;
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
