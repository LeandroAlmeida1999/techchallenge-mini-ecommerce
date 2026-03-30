# ECommerce technical challenge

Mini e-commerce backend em `.NET 10` com DDD, Clean Architecture, Minimal APIs, EF Core, SQL Server 2022, Outbox Pattern e worker de publicacao para Kafka.

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

## Estrutura da solucao

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

## Visao de arquitetura

O projeto foi organizado com direcao de dependencias compativel com Clean Architecture:

- `ECommerce.Core`
  Contem o dominio puro: agregados, entidade, value objects, domain events, domain service, excecoes e contratos de repositorio.

- `ECommerce.UseCases`
  Contem comandos, queries, handlers e DTOs de aplicacao. Essa camada orquestra o fluxo, mas nao conhece HTTP nem detalhes de persistencia.

- `ECommerce.Infrastructure`
  Contem EF Core, configuracoes de mapeamento, repositorios, outbox, migration inicial e integracao com Kafka.

- `ECommerce.WebApi`
  Expoe os endpoints Minimal API, contratos HTTP, Swagger e tratamento consistente de erros com `ProblemDetails`.

- `ECommerce.Worker`
  Processa mensagens pendentes do outbox e tenta publica-las no Kafka.

## Decisoes adotadas

- Os agregados principais sao `Cliente`, `Produto` e `Pedido`, usando linguagem de dominio em portugues para favorecer clareza.
- O `Pedido` encapsula adicao de itens, remocao, recalculo do total e confirmacao.
- `ItemPedido` permanece dentro do agregado `Pedido` e nao possui repositorio proprio.
- `Email`, `Money` e `Quantidade` foram implementados como value objects imutaveis com validacoes.
- O mapeamento para DTOs foi mantido explicito, sem `AutoMapper`, para deixar o fluxo mais facil de entender e explicar.
- O Outbox e a fonte de verdade para publicacao de eventos de integracao apos a confirmacao do pedido.
- O worker foi mantido enxuto e delega a logica de processamento para um `OutboxProcessor`, o que melhora separacao de responsabilidades.

## Fluxo principal

1. A API recebe a requisicao.
2. O handler da aplicacao carrega o agregado necessario.
3. O dominio executa a regra de negocio.
4. Ao confirmar o pedido, o dominio gera `PedidoConfirmadoDomainEvent`.
5. A infraestrutura traduz o evento para `PedidoConfirmadoIntegrationEvent` e persiste uma linha na tabela de outbox na mesma transacao do pedido.
6. O worker le mensagens pendentes do outbox.
7. O worker publica no Kafka.
8. A mensagem e marcada como processada ou falha.

## Persistencia

- Banco: SQL Server 2022
- ORM: EF Core
- Migrations: incluidas no projeto de infraestrutura
- Aplicacao automatica das migrations:
  - Web API no startup
  - Worker no startup

Tabelas principais criadas pela migration inicial:

- `Clientes`
- `Produtos`
- `Pedidos`
- `PedidoItens`
- `OutboxMessages`

## Endpoints disponiveis

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

Este repositorio possui dois arquivos de compose:

- `docker-compose.yml`
  Sobe `sqlserver`, `api` e `worker`

- `docker-compose.kafka.yml`
  Centraliza a configuracao do Kafka usada pelo `worker`

### Fluxo unico de execucao

O projeto sempre deve ser executado com os dois arquivos de compose:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

### Como usar com Kafka local

No arquivo `docker-compose.kafka.yml`, mantenha:

```text
Kafka__BootstrapServers=kafka:9092
```

Nesse modo, o broker definido no proprio `docker-compose.kafka.yml` sera usado pelo `worker`.

### Como usar com Kafka fornecido externamente

No mesmo arquivo `docker-compose.kafka.yml`, altere apenas:

```text
Kafka__BootstrapServers=host.docker.internal:9092
```

Ou substitua pelo endereco fornecido para avaliacao.

Se o ambiente externo exigir autenticacao ou outro protocolo, ajuste tambem no mesmo arquivo:

```text
Kafka__SecurityProtocol=...
Kafka__SaslMechanism=...
Kafka__SaslUsername=...
Kafka__SaslPassword=...
Kafka__EnableSslCertificateVerification=true
```

### Etapas de execucao

1. Ajustar o `docker-compose.kafka.yml` para o broker desejado.

2. Subir o ambiente:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

3. Confirmar que os servicos ficaram de pe:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml ps
```

4. Acessar a aplicacao:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- SQL Server: `localhost,1433`
- Kafka local para ferramentas externas, quando usado: `localhost:9094`

5. Se precisar acompanhar os logs:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f
```

6. Se quiser logs por servico:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f sqlserver
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f api
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f worker
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f kafka
```

7. Para encerrar o ambiente:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml down
```

### Comandos uteis durante o desenvolvimento

Subir apenas o banco:

```powershell
docker compose up -d sqlserver
```

Subir banco e API:

```powershell
docker compose up -d sqlserver api
```

Verificar status:

```powershell
docker compose ps
```

Ver logs:

```powershell
docker compose logs -f sqlserver
docker compose logs -f api
docker compose logs -f worker
```

Encerrar:

```powershell
docker compose down
```

## Configuracoes importantes

Connection string padrao:

```text
Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True
```

Configuracao Kafka padrao do worker:

```text
BootstrapServers=host.docker.internal:9092
Topic=pedido-confirmado
```

Quando o Kafka estiver rodando fora do compose principal, o `worker` usa `host.docker.internal` para alcancar a maquina host.

Quando o `docker-compose.kafka.yml` for usado junto com o compose principal, o `worker` passa a usar automaticamente `kafka:9092`.

## Publicacao no Kafka

O `worker` publica eventos no Kafka usando o client `.NET` `Confluent.Kafka`.

Isso significa que:

- o client da aplicacao e o vendor Confluent
- o broker pode ser Apache Kafka puro, Strimzi ou outro ambiente compativel com o protocolo Kafka
- a principal diferenca entre ambientes fica concentrada em configuracao, como `BootstrapServers`, protocolo de seguranca e credenciais

No ambiente local com `docker-compose.kafka.yml`, o `worker` sobe configurado com:

```text
Kafka__BootstrapServers=kafka:9092
Kafka__Topic=pedido-confirmado
Kafka__ClientId=ecommerce-worker
Kafka__SecurityProtocol=Plaintext
```

## Testes

Executar build:

```powershell
dotnet build ECommerce.sln
```

Executar testes:

```powershell
dotnet test tests/ECommerce.UnitTests/ECommerce.UnitTests.csproj
```

Cobertura atual de dominio:

- total do pedido
- pedido sem itens nao pode ser confirmado
- e-mail invalido lanca excecao
- produto inativo nao pode ser adicionado ao pedido
- remocao de item recalcula total
- pedido ja confirmado nao pode ser confirmado novamente

## Revisao final

### Consistencia de nomes

- Os nomes principais estao consistentes com o dominio e com a linguagem do teste.
- Termos tecnicos foram mantidos em ingles quando ligados a infraestrutura, como `Outbox`, `Worker` e `Kafka`.

### Fronteiras arquiteturais

- `Core` permanece sem dependencia de outras camadas.
- `UseCases` depende apenas de `Core`.
- `Infrastructure` implementa persistencia e integracoes tecnicas.
- `WebApi` e `Worker` apenas compoem e executam os fluxos.

### Invariantes de dominio

- cliente exige nome e email valido
- produto exige nome e preco valido
- pedido inicia em rascunho
- pedido nao aceita produto inativo
- pedido confirmado nao aceita novas alteracoes
- pedido nao pode ser confirmado sem itens
