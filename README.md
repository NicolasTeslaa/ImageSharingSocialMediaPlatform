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

## Direcao de dependencias

- `API -> Application`
- `API -> Infrastructure`
- `Infrastructure -> Application`
- `Application -> Domain`

Isso mantem o dominio isolado e facilita evoluir cada servico seguindo DDD e principios SOLID.
