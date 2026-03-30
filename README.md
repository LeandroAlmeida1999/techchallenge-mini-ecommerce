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
- Strimzi
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

deploy/
  strimzi/
```

## Visão de arquitetura

O projeto foi organizado com direção de dependências compativel com Clean Architecture:

- `ECommerce.Core`
  Contém o dominio puro: agregados, entidade, value objects, domain events, domain service, exceções e contratos de repositorio.

- `ECommerce.UseCases`
  Contém comandos, queries, handlers e DTOs de aplicação. Essa camada orquestra o fluxo, mas não conhece HTTP nem detalhes de persistência.

- `ECommerce.Infrastructure`
  Contém EF Core, configurações de mapeamento, repositórios, outbox, migrations e integração com Kafka.

- `ECommerce.WebApi`
  Expõe os endpoints Minimal API, contratos HTTP, Swagger e tratamento consistente de erros com `ProblemDetails`.

- `ECommerce.Worker`
  Processa mensagens pendentes do outbox e publica eventos no Kafka.

## Decisões adotadas

- Os agregados principais são `Cliente`, `Produto` e `Pedido`.
- O `Pedido` encapsula adição de itens, remoção, recalculo do total e confirmação.
- `Email`, `Money` e `Quantidade` foram implementados como value objects imutaveis com validações.
- O mapeamento para DTOs foi mantido explícito, sem `AutoMapper`.
- O Outbox garante persistência atômica da mudança de estado do pedido e do evento de integração.
- O worker pública com chave Kafka baseada no `ClienteId`.

## Fluxo principal

1. A API recebe a requisição.
2. O handler da aplicação carrega o agregado necessário.
3. O domínio executa a regra de negócio.
4. ão confirmar o pedido, o domínio gera `PedidoConfirmadoDomainEvent`.
5. A infraestrutura traduz o evento para `PedidoConfirmadoIntegrationEvent` e persiste uma linha na tabela de outbox na mesma transação do pedido.
6. O worker le mensagens pendentes do outbox.
7. O worker pública no tópico Kafka `pedido`.
8. A mensagem e marcada como processada ou falha.

## Persistencia

- Banco: SQL Server 2022
- ORM: EF Core
- Migrations: incluidas no projeto de infraestrutura
- Aplicação automática das migrations:
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

## Como executar

### Fluxo principal para avaliação

O fluxo esperado e este:

1. ja possui um cluster Kafka configurado com Strimzi
2. altere apenas as informações do arquivo [docker-compose.kafka.yml]
3. suba a aplicação com os dois arquivos de compose

### 1. Quando o cluster Strimzi ja existe

No arquivo [docker-compose.kafka.yml], ajustar os valores do Kafka para o ambiente fornecido.

Exemplo:

```text
KafkaSettings__SectionName=Kafka
Kafka__BootstrapServers=host.docker.internal:32092
Kafka__Topic=pedido
Kafka__ClientId=ecommerce-worker
Kafka__SecurityProtocol=Plaintext
```

Se o ambiente exigir autenticação ou outro protocolo, preencher tambem:

```text
Kafka__SaslMechanism=...
Kafka__SaslUsername=...
Kafka__SaslPassword=...
Kafka__EnableSslCertificateVerification=true
```

Depois disso, subir a aplicação:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

Confirmar que os servicos ficaram de pe:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml ps
```

Esperado:

- `sqlserver`
- `api`
- `worker`

Acessos:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- SQL Server: `localhost,1433`

Ver logs:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml logs -f
```

### 2. Caso não tenha um cluster Strimzi 

Este repositorio tambem inclui os manifests para subir um ambiente local com Strimzi.

#### Pre-requisitos

- Docker Desktop instalado e em execução
- Kubernetes habilitado no Docker Desktop
- `kubectl` disponivel no terminal

#### Validar o Kubernetes local

```powershell
kubectl config current-context
kubectl get nodes
```

O contexto esperado neste ambiente e `docker-desktop`.

#### Criar o namespace do Kafka

```powershell
kubectl create namespace kafka
```

Se o namespace ja existir, o comando pode falhar sem problema.

#### Instalar o Strimzi Operator

```powershell
kubectl create -f "https://strimzi.io/install/latest?namespace=kafka" -n kafka
```

#### Subir o cluster Kafka do projeto

```powershell
kubectl apply -f deploy/strimzi/kafka-my-cluster.yaml
kubectl wait kafka/my-cluster --for=condition=Ready --timeout=600s -n kafka
```

#### Criar o topico usado pela aplicação

```powershell
kubectl apply -f deploy/strimzi/kafkatopic-pedido.yaml
kubectl get kafkatopic -n kafka
```

#### Expor o bootstrap do Strimzi para a aplicação em Docker

Abrir um terminal e executar:

```powershell
kubectl port-forward svc/my-cluster-kafka-external-bootstrap 32092:9094 -n kafka
```

Abrir outro terminal e executar:

```powershell
kubectl port-forward svc/my-cluster-dual-role-0 32090:9094 -n kafka
```

Esses dois forwards precisam permanecer abertos enquanto a aplicação estiver rodando.

#### Ajustar o `docker-compose.kafka.yml`

Para esse ambiente local, manter:

```text
KafkaSettings__SectionName=Kafka
Kafka__BootstrapServers=host.docker.internal:32092
Kafka__Topic=pedido
Kafka__ClientId=ecommerce-worker
Kafka__SecurityProtocol=Plaintext
```

#### Subir a aplicação

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
```

#### Validar a mensagem no tópico `pedido`

```powershell
kubectl delete pod kafka-consumer-check -n kafka --ignore-not-found
kubectl run kafka-consumer-check --image=quay.io/strimzi/kafka:0.51.0-kafka-4.2.0 -n kafka --rm -i --restart=Never --command -- bin/kafka-console-consumer.sh --bootstrap-server my-cluster-kafka-bootstrap:9092 --topic pedido --from-beginning --property print.key=true --property key.separator=" | " --max-messages 1
```

## Configuração do Kafka usada pela aplicação

O arquivo [docker-compose.kafka.yml] centraliza as configurações do Kafka para `api` e `worker`.

Os campos principais que podem ser alterados por ambiente são:

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

## Publicação no Kafka

O `worker` publica eventos no Kafka usando o client `.NET` `Confluent.Kafka`.

Isso significa que:

- o client da aplicação é o vendor Confluent
- o broker pode ser Apache Kafka puro, Strimzi ou outro ambiente compatível com o protocolo Kafka
- a principal diferenca entre ambientes fica concentrada em configuração, como `BootstrapServers`, protocolo de seguranca e credenciais

## Testes automatizados

Executar build:

```powershell
dotnet build ECommerce.sln
```

Executar testes:

```powershell
dotnet test tests/ECommerce.UnitTests/ECommerce.UnitTests.csproj
```

## Encerramento

Parar a aplicação:

```powershell
docker compose -f docker-compose.yml -f docker-compose.kafka.yml down
```

Parar os `port-forward`:

- fechar os dois terminais em que eles estão rodando
- ou interromper com `Ctrl+C`

Se quiser remover o topico:

```powershell
kubectl delete -f deploy/strimzi/kafkatopic-pedido.yaml
```

Se quiser remover o cluster Kafka:

```powershell
kubectl delete -f deploy/strimzi/kafka-my-cluster.yaml
```

## Revisão final

### Fronteiras arquiteturais

- `Core` permanece sem dependência de outras camadas.
- `UseCases` depende apenas de `Core`.
- `Infrastructure` implementa persistência e integrações técnicas.
- `WebApi` e `Worker` apenas compoem e executam os fluxos.

### Invariantes de dominio

- cliente exige nome e email válido
- produto exige nome e preço válido
- pedido inicia em rascunho
- pedido não aceita produto inativo
- pedido confirmado não aceita novas alterações
- pedido não pode ser confirmado sem itens
