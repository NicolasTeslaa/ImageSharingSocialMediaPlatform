# Image Sharing Social Media Platform

Plataforma de compartilhamento de imagens com arquitetura de microservicos em .NET.

## Servicos

- `UsersService`
- `PostsService`
- `TimelineService`
- `SearchService`
- `APIGateway`
- `frontend` (React + Vite)

## Arquitetura

Cada servico segue a separacao:

- `Domain`: entidades e contratos centrais.
- `Application`: casos de uso e DTOs.
- `Infrastructure`: persistencia e integracoes externas.
- `API`: endpoints HTTP.

Direcao de dependencias:

- `API -> Application`
- `API -> Infrastructure`
- `Infrastructure -> Application`
- `Application -> Domain`

## Pre-requisitos

- Docker Desktop
- .NET SDK 10
- Node.js 20+ e npm
- PowerShell (Windows)

## Infra local (Docker)

A infraestrutura local fica em `infra/posts-cluster` e sobe:

- MySQL cluster (3 nos) + MySQL Router
- Kafka + Kafka UI
- Elasticsearch
- MinIO

Passos:

```powershell
cd C:\repo\ImageSharingSocialMediaPlatform\infra\posts-cluster
docker compose down -v
docker compose up -d
```

Credenciais padrao usadas localmente:

- MySQL root: `root` / `root123!`
- App DB user: `posts_app` / `posts_app_123`
- MinIO: `minioadmin` / `minioadmin`

Portas importantes:

- MySQL node1: `33061`
- Router RW: `6446`
- Router RO: `6447`
- Kafka: `9092`
- Kafka UI: `http://localhost:8080`
- Elasticsearch: `http://localhost:9200`
- MinIO API: `http://localhost:9000`
- MinIO Console: `http://localhost:9001`

## Subindo as APIs

Na raiz do projeto, rode cada servico em um terminal:

```powershell
cd C:\repo\ImageSharingSocialMediaPlatform
dotnet run --project .\src\UsersService\UsersService.API\
dotnet run --project .\src\SearchService\SearchService.API\
dotnet run --project .\src\TimelineService\TimelineService.API\
dotnet run --project .\src\PostsService\PostsService.API\
dotnet run --project .\src\APIGateway\APIGateway.API\
```

Portas locais:

- UsersService: `http://localhost:5166`
- SearchService: `http://localhost:5239`
- TimelineService: `http://localhost:5174`
- PostsService: `http://localhost:5237`
- APIGateway: `http://localhost:5071`

## Gateway

O `APIGateway` faz proxy para os microservicos, incluindo:

- `/auth/*`, `/users/*`, `/internal/*` -> `UsersService`
- `/posts/*` -> `PostsService`
- `/timeline/*` -> `TimelineService`
- `/search/*` -> `SearchService`

Endpoint util de diagnostico:

- `GET /gateway/routes`

## Frontend (React)

O frontend foi feito para testar o fluxo fim a fim usando apenas o gateway.

```powershell
cd C:\repo\ImageSharingSocialMediaPlatform\frontend
npm install
npm run dev
```

Acesse:

- `http://localhost:5173`

Variavel opcional:

- `VITE_API_BASE` (default: `http://localhost:5071`)

## Fluxo rapido de teste

1. Suba a infra Docker.
2. Suba `UsersService`, `PostsService`, `TimelineService`, `SearchService` e `APIGateway`.
3. Suba o frontend.
4. Na UI:
- crie usuario
- faca login
- publique imagem
- carregue feed (`GET /posts`)
- carregue timeline
- teste busca de usuarios

## Referencias

- Infra local detalhada: [infra/posts-cluster/README.md](/C:/repo/ImageSharingSocialMediaPlatform/infra/posts-cluster/README.md)
