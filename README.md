# ECommerce Technical Challenge

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
- Strimzi
- xUnit

## Estrutura da Solucao

```text
src/
  ECommerce.Core/
  ECommerce.UseCases/
  ECommerce.Infrastructure/
  ECommerce.WebApi/
  ECommerce.Worker/

tests/
  ECommerce.UnitTests/

deploy/
  strimzi/

scripts/
  start-local.ps1
  start-app-only.ps1
  smoke-test.ps1
  stop-local.ps1
```

## Visao de Arquitetura

- `ECommerce.Core`
  Contem o dominio puro: agregados, entidade, value objects, domain events, domain services, excecoes e contratos de repositorio.

- `ECommerce.UseCases`
  Contem comandos, queries, handlers e DTOs de aplicacao. Essa camada orquestra o fluxo, mas nao conhece HTTP nem detalhes de persistencia.

- `ECommerce.Infrastructure`
  Contem EF Core, configuracoes de mapeamento, repositorios, outbox, migrations e integracao com Kafka.

- `ECommerce.WebApi`
  Expoe os endpoints Minimal API, contratos HTTP, Swagger e tratamento consistente de erros com `ProblemDetails`.

- `ECommerce.Worker`
  Processa mensagens pendentes do outbox e publica eventos no Kafka.

## Decisoes Adotadas

- Os agregados principais sao `Cliente`, `Produto` e `Pedido`.
- O `Pedido` encapsula adicao de itens, remocao, recalculo do total e confirmacao.
- `Email`, `Money` e `Quantidade` foram implementados como value objects imutaveis com validacoes.
- O mapeamento para DTOs foi mantido explicito, sem `AutoMapper`.
- O Outbox garante persistencia atomica da mudanca de estado do pedido e do evento de integracao.
- O worker publica com chave Kafka baseada no `ClienteId`.

## Fluxo Principal

1. A API recebe a requisicao.
2. O handler da aplicacao carrega o agregado necessario.
3. O dominio executa a regra de negocio.
4. Ao confirmar o pedido, o dominio gera `PedidoConfirmadoDomainEvent`.
5. A infraestrutura traduz o evento para `PedidoConfirmadoIntegrationEvent` e persiste uma linha na tabela de outbox na mesma transacao do pedido.
6. O worker le mensagens pendentes do outbox.
7. O worker publica no topico Kafka `pedido`.
8. A mensagem e marcada como processada ou falha.

## Persistencia

- Banco: SQL Server 2022
- ORM: EF Core
- Migrations: incluidas no projeto de infraestrutura
- Aplicacao automatica das migrations:
  - Web API no startup
  - Worker no startup

Tabelas principais:

- `Clientes`
- `Produtos`
- `Pedidos`
- `PedidoItens`
- `OutboxMessages`

## Endpoints Disponiveis

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

## Como Executar

O projeto pode ser executado de quatro formas, dependendo de onde o Kafka/Strimzi ja esta rodando.

### 1. Fluxo Manual com Strimzi Dedicado (Configuração de Ambiente 1)

Use este fluxo quando o cluster Kafka ja existir fora deste repositorio.

1. Ajuste o [docker-compose.kafka.yml] com os dados do ambiente.

Exemplo:

```text
KafkaSettings__SectionName=Kafka
Kafka__BootstrapServers=host.docker.internal:32092
Kafka__Topic=pedido
Kafka__ClientId=ecommerce-worker
Kafka__SecurityProtocol=Plaintext
```

Se o ambiente exigir autenticacao ou outro protocolo, preencha tambem:

```text
Kafka__SaslMechanism=...
Kafka__SaslUsername=...
Kafka__SaslPassword=...
Kafka__EnableSslCertificateVerification=true
```

2. Suba a aplicacao:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

3. Valide os containers:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml ps
```

4. Execute o smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1
```

5. Se quiser validar tambem a mensagem no Kafka:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1 -ConsumeKafka
```

### 2. Fluxo Manual com Strimzi Cluster Local (Configuração de Ambiente 2)

Use este fluxo quando quiser subir o Kafka localmente com Kubernetes + Strimzi.

Pre-requisitos:

- Docker Desktop em execucao
- Kubernetes habilitado no Docker Desktop
- `kubectl` disponivel no terminal

1. Valide o Kubernetes local:

```powershell
kubectl config current-context
kubectl get nodes
```

O contexto esperado e `docker-desktop`.

2. Crie o namespace:

```powershell
kubectl create namespace kafka
```

3. Instale o Strimzi Operator:

```powershell
kubectl apply -f "https://strimzi.io/install/latest?namespace=kafka" -n kafka
kubectl rollout status deployment/strimzi-cluster-operator -n kafka --timeout=300s
```

4. Suba o cluster Kafka:

```powershell
kubectl apply -f deploy/strimzi/kafkanodepool-my-cluster.yaml
kubectl apply -f deploy/strimzi/kafka-my-cluster.yaml
kubectl wait kafka/my-cluster --for=condition=Ready --timeout=600s -n kafka
```

5. Crie o topico:

```powershell
kubectl apply -f deploy/strimzi/kafkatopic-pedido.yaml
kubectl get kafkatopic -n kafka
```

6. Exponha o broker para a aplicacao:

Em um terminal:

```powershell
kubectl port-forward svc/my-cluster-kafka-external-bootstrap 32092:9094 -n kafka
```

Em outro terminal:

```powershell
kubectl port-forward svc/my-cluster-dual-role-0 32090:9094 -n kafka
```

7. Confirme que o [docker-compose.kafka.yml] esta apontando para:

```text
KafkaSettings__SectionName=Kafka
Kafka__BootstrapServers=host.docker.internal:32092
Kafka__Topic=pedido
Kafka__ClientId=ecommerce-worker
Kafka__SecurityProtocol=Plaintext
```

8. Suba a aplicacao:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

9. Execute o smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1 -ConsumeKafka
```

### 3. Fluxo Scriptado com Strimzi Dedicado (Configuração de Ambiente 3)

Use este fluxo quando o cluster Kafka ja existir e voce quiser subir so a aplicacao com menos comandos.

1. Ajuste o [docker-compose.kafka.yml] com os dados do ambiente.

2. Suba a aplicacao:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-app-only.ps1
```

3. Execute o smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1
```

4. Se quiser validar tambem a mensagem no Kafka:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1 -ConsumeKafka
```

5. Para encerrar:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\stop-local.ps1
```

### 4. Fluxo Scriptado com Strimzi Cluster Local (Configuração de Ambiente 4)

Use este fluxo quando quiser preparar Kafka local + aplicacao com o minimo de comandos.

1. Suba tudo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-local.ps1
```

2. Execute o smoke test:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1
```

3. Se quiser validar tambem a mensagem no Kafka:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1 -ConsumeKafka
```

4. Para encerrar:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\stop-local.ps1
```

## Configuracao do Kafka Usada pela Aplicacao

O arquivo [docker-compose.kafka.yml] centraliza as configuracoes do Kafka para `api` e `worker`.

Campos principais que podem ser alterados por ambiente:

```text
KafkaSettings__SectionName
Kafka__BootstrapServers
Kafka__Topic
Kafka__ClientId
Kafka__SecurityProtocol
Kafka__SaslMechanism
Kafka__SaslUsername
Kafka__SaslPassword
Kafka__EnableSslCertificateVerification
```

## Publicacao no Kafka

O `worker` publica eventos no Kafka usando o client `.NET` `Confluent.Kafka`.

Isso significa que:

- o client da aplicacao e o vendor Confluent
- o broker pode ser Apache Kafka puro, Strimzi ou outro ambiente compativel com o protocolo Kafka
- a principal diferenca entre ambientes fica concentrada em configuracao, como `BootstrapServers`, protocolo de seguranca e credenciais

## Teste de Carga (Moderado)

- perfil: `30` requisicoes por endpoint com concorrencia `10`
- resultado HTTP: `100%` de sucesso em todos os endpoints
- faixa de latencia media: `111ms` a `127ms`
- maior `p95`: `164ms` em `POST /pedidos/{id}/confirmar`
- outbox apos o teste: `Processed = 137`, sem pendencias

## Testes Automatizados

Executar build:

```powershell
dotnet build ECommerce.sln
```

Executar testes:

```powershell
dotnet test tests/ECommerce.UnitTests/ECommerce.UnitTests.csproj
```

## Encerramento

Parar a aplicacao:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml down
```

Parar os `port-forward`:

- fechar os dois terminais em que eles estao rodando
- ou interromper com `Ctrl+C`

Se quiser remover o topico:

```powershell
kubectl delete -f deploy/strimzi/kafkatopic-pedido.yaml
```

Se quiser remover o cluster Kafka:

```powershell
kubectl delete -f deploy/strimzi/kafkanodepool-my-cluster.yaml
kubectl delete -f deploy/strimzi/kafka-my-cluster.yaml
```

## Revisao Final

### Fronteiras Arquiteturais

- `Core` permanece sem dependencia de outras camadas.
- `UseCases` depende apenas de `Core`.
- `Infrastructure` implementa persistencia e integracoes tecnicas.
- `WebApi` e `Worker` apenas compoem e executam os fluxos.

### Invariantes de Dominio

- cliente exige nome e email valido
- produto exige nome e preco valido
- pedido inicia em rascunho
- pedido nao aceita produto inativo
- pedido confirmado nao aceita novas alteracoes
- pedido nao pode ser confirmado sem itens
