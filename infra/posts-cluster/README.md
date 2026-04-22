# PostsService MySQL Cluster

Esta pasta sobe um cluster local com 3 instancias MySQL, 1 MySQL Router, 1 broker Kafka, 1 Kafka UI e 1 MinIO para o `PostsService`.

## Topologia

- `posts-db-1`, `posts-db-2`, `posts-db-3`: nos do InnoDB Cluster
- `posts-db-router`: endpoint estavel para leitura e escrita
- `posts-kafka`: broker Kafka para publicacao de eventos de post criado
- `posts-kafka-ui`: interface web para visualizar topicos, brokers e mensagens
- `posts-minio`: object store compativel com S3 para armazenar imagens

## Portas

- `33061`, `33062`, `33063`: acesso direto aos nos MySQL
- `6446`: endpoint read-write do MySQL Router
- `6447`: endpoint read-only do MySQL Router
- `9092`: broker Kafka
- `8080`: Kafka UI
- `9000`: API S3 do MinIO
- `9001`: console do MinIO

## Como subir

1. Instale Docker e MySQL Shell (`mysqlsh`).
2. Execute `powershell -ExecutionPolicy Bypass -File .\infra\posts-cluster\bootstrap-cluster.ps1`.

## Conexao no PostsService

O `PostsService` foi configurado para usar:

- escrita em `localhost:6446`
- leitura em `localhost:6447`
- Kafka em `localhost:9092`
- Kafka UI em `http://localhost:8080`
- MinIO em `http://localhost:9000`
- Console MinIO em `http://localhost:9001`

## Upload de posts

As rotas `POST /posts` e `PUT /posts/{id}` agora esperam `multipart/form-data` com:

- `file`: arquivo da imagem
- `postType`: opcional, default `IMAGE`

## Observacoes

- O script `setupCluster.js` usa AdminAPI do MySQL Shell para configurar o cluster.
- O Router faz bootstrap depois que o cluster ja existe.
- O `PostsService` usa `outbox_messages` para persistir eventos antes da publicacao no Kafka.
- O bucket `posts` do MinIO e configurado automaticamente com leitura anonima para desenvolvimento local.
- O `PostsService` salva no MySQL tanto `object_key` quanto `post_url`.
- Depois do commit da transacao, um `BackgroundService` tenta publicar imediatamente e tambem faz polling curto como fallback.
- Se quiser reiniciar do zero, remova `infra/posts-cluster/data`, `infra/posts-cluster/router-data` e `infra/posts-cluster/minio-data`.
