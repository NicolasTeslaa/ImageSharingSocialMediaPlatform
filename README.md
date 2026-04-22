# Image Sharing Social Media Platform

Base inicial em .NET para uma plataforma de compartilhamento de imagens com arquitetura orientada a servicos.

## Estrutura

Cada contexto foi separado em quatro camadas:

- `Domain`: entidades e contratos centrais do dominio.
- `Application`: casos de uso, DTOs e orquestracao.
- `Infrastructure`: implementacoes externas e persistencia.
- `API`: exposicao HTTP do servico.

## Servicos

- `UsersService`
- `SearchService`
- `TimelineService`
- `PostsService`
- `APIGateway`

## Resiliencia do PostsService

O `PostsService` foi preparado para usar separacao de leitura e escrita via MySQL Router:

- `PostsWriteDatabase`: endpoint read-write do Router
- `PostsReadDatabase`: endpoint read-only do Router

Tambem foi adicionada uma infraestrutura local em [infra/posts-cluster/README.md](/C:/repo/ImageSharingSocialMediaPlatform/infra/posts-cluster/README.md) para subir um cluster com 3 nos MySQL e 1 Router.

## Direcao de dependencias

- `API -> Application`
- `API -> Infrastructure`
- `Infrastructure -> Application`
- `Application -> Domain`

Isso mantem o dominio isolado e facilita evoluir cada servico seguindo DDD e principios SOLID.
