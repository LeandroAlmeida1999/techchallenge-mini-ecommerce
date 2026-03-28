using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.Aggregates;

public sealed class Cliente : AggregateRoot
{
    private Cliente()
    {
        Nome = string.Empty;
        Email = new Email("placeholder@local.test");
    }

    public Guid ClienteId { get; private set; }
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public Cliente(Guid clienteId, string nome, Email email, DateTime? dataCadastro = null)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId deve ser informado.", nameof(clienteId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do cliente deve ser informado.", nameof(nome));

        ClienteId = clienteId;
        Nome = nome.Trim();
        Email = email;
        DataCadastro = dataCadastro ?? DateTime.UtcNow;
    }

    public static Cliente Criar(string nome, Email email)
    {
        return new Cliente(Guid.NewGuid(), nome, email);
    }
}
