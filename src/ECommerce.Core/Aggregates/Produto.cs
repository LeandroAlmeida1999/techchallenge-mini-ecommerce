using ECommerce.Core.ValueObjects;

namespace ECommerce.Core.Aggregates;

public sealed class Produto : AggregateRoot
{
    public Guid ProdutoId { get; private set; }
    public string Nome { get; private set; }
    public Money Preco { get; private set; }
    public bool Ativo { get; private set; }

    public Produto(Guid produtoId, string nome, Money preco, bool ativo = true)
    {
        if (produtoId == Guid.Empty)
            throw new ArgumentException("ProdutoId deve ser informado.", nameof(produtoId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do produto deve ser informado.", nameof(nome));

        ProdutoId = produtoId;
        Nome = nome.Trim();
        Preco = preco;
        Ativo = ativo;
    }

    public static Produto Criar(string nome, Money preco, bool ativo = true)
    {
        return new Produto(Guid.NewGuid(), nome, preco, ativo);
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }
}
