# ECommerce technical challenge

Mini e-commerce backend em `.NET 10` com DDD, Clean Architecture, Minimal APIs, EF Core, SQL Server 2022, Outbox Pattern e worker de publicação para Kafka.

## Stack

- .NET 10
- C#
- Minimal APIs
- Swagger
- EF Core
- SQL Server 2022
- Docker Compose
- Kafka via `Confluent.Kafka`
- xUnit

## Estrutura da solução

```text
src/
  ECommerce.Core/
  ECommerce.UseCases/
  ECommerce.Infrastructure/
  ECommerce.WebApi/
  ECommerce.Worker/

tests/
  ECommerce.UnitTests/
```

## Visão de arquitetura

O projeto foi organizado com direção de dependências compatível com Clean Architecture:

- `ECommerce.Core`
  Contém o domínio puro: agregados, entidade, value objects, domain events, domain service, exceções e contratos de repositório.

- `ECommerce.UseCases`
  Contém comandos, queries, handlers e DTOs de aplicação. Essa camada orquestra o fluxo, mas não conhece HTTP nem detalhes de persistência.

- `ECommerce.Infrastructure`
  Contém EF Core, configurações de mapeamento, repositórios, outbox, migration inicial e integração com Kafka.

- `ECommerce.WebApi`
  Expõe os endpoints Minimal API, contratos HTTP, Swagger e tratamento consistente de erros com `ProblemDetails`.

- `ECommerce.Worker`
  Processa mensagens pendentes do outbox e tenta publicá-las no Kafka.

## Decisões adotadas

- Os agregados principais são `Cliente`, `Produto` e `Pedido`, usando linguagem de domínio em português para favorecer clareza.
- O `Pedido` encapsula adição de itens, remoção, recálculo do total e confirmação.
- `ItemPedido` permanece dentro do agregado `Pedido` e não possui repositório próprio.
- `Email`, `Money` e `Quantidade` foram implementados como value objects imutáveis com validações.
- O mapeamento para DTOs foi mantido explícito, sem `AutoMapper`, para deixar o fluxo mais fácil de entender e explicar.
- O Outbox é a fonte de verdade para publicação de eventos de integração após a confirmação do pedido.
- O worker foi mantido enxuto e delega a lógica de processamento para um `OutboxProcessor`, o que melhora separação de responsabilidades.

## Fluxo principal

1. A API recebe a requisição.
2. O handler da aplicação carrega o agregado necessário.
3. O domínio executa a regra de negócio.
4. Ao confirmar o pedido, o domínio gera `PedidoConfirmadoDomainEvent`.
5. A infraestrutura traduz o evento para `PedidoConfirmadoIntegrationEvent` e persiste uma linha na tabela de outbox na mesma transação do pedido.
6. O worker lê mensagens pendentes do outbox.
7. O worker publica no Kafka.
8. A mensagem é marcada como processada ou falha.

## Persistência

- Banco: SQL Server 2022
- ORM: EF Core
- Migrations: incluídas no projeto de infraestrutura
- Aplicação automática das migrations:
  - Web API no startup
  - Worker no startup

Tabelas principais criadas pela migration inicial:

- `Clientes`
- `Produtos`
- `Pedidos`
- `PedidoItens`
- `OutboxMessages`

## Endpoints disponíveis

### Clientes

- `POST /clientes`
- `GET /clientes/{id}`

### Produtos

- `POST /produtos`
- `GET /produtos`

### Pedidos

- `POST /pedidos`
- `POST /pedidos/{id}/itens`
- `POST /pedidos/{id}/confirmar`
- `GET /pedidos/{id}`

Swagger:

- `/swagger`

## Como executar com Docker Compose

O `docker-compose.yml` atual sobe:

- `sqlserver`
- `api`
- `worker`

### Subir tudo

```powershell
docker compose up --build
```

### Subir apenas o banco

```powershell
docker compose up -d sqlserver
```

### Subir banco e API

```powershell
docker compose up -d sqlserver api
```

### Verificar status

```powershell
docker compose ps
```

### Ver logs

```powershell
docker compose logs -f sqlserver
docker compose logs -f api
docker compose logs -f worker
```

### Encerrar

```powershell
docker compose down
```

## Execução local sem Docker

### API

```powershell
dotnet run --project src/ECommerce.WebApi
```

### Worker

```powershell
dotnet run --project src/ECommerce.Worker
```

## Configurações importantes

Connection string padrão:

```text
Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True
```

Configuração Kafka padrão do worker:

```text
BootstrapServers=host.docker.internal:9092
Topic=pedido-confirmado
```

Quando o Kafka estiver rodando fora do compose principal, o `worker` usa `host.docker.internal` para alcançar a máquina host.

## Testes

Executar build:

```powershell
dotnet build ECommerce.sln
```

Executar testes:

```powershell
dotnet test tests/ECommerce.UnitTests/ECommerce.UnitTests.csproj
```

Cobertura atual de domínio:

- total do pedido
- pedido sem itens não pode ser confirmado
- e-mail inválido lança exceção
- produto inativo não pode ser adicionado ao pedido
- remoção de item recalcula total
- pedido já confirmado não pode ser confirmado novamente

## Revisão final

### Consistência de nomes

- Os nomes principais estão consistentes com o domínio e com a linguagem do teste.
- Termos técnicos foram mantidos em inglês quando ligados à infraestrutura, como `Outbox`, `Worker` e `Kafka`.

### Fronteiras arquiteturais

- `Core` permanece sem dependência de outras camadas.
- `UseCases` depende apenas de `Core`.
- `Infrastructure` implementa persistência e integrações técnicas.
- `WebApi` e `Worker` apenas compõem e executam os fluxos.

### Invariantes de domínio

- cliente exige nome e email válido
- produto exige nome e preço válido
- pedido inicia em rascunho
- pedido não aceita produto inativo
- pedido confirmado não aceita novas alterações
- pedido não pode ser confirmado sem itens
`
