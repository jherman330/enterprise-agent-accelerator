# Enterprise Agent Accelerator

Enterprise Agent Accelerator is a reference application for a local developer workbench with an ASP.NET Core API backend and a React frontend that will support chat interactions through Azure OpenAI or Azure AI Foundry.

## Local Setup

> Full setup instructions will be expanded as the project builds out.

**Prerequisites:** Docker Desktop, .NET 8 SDK, Node 18+

**Quick start (Docker):**

```bash
cp .env.example .env
# Edit .env with your Azure OpenAI / AI Foundry credentials
docker compose up
```

**Quick start (direct):**

```bash
# Terminal 1 - backend
cd src/api && dotnet run

# Terminal 2 - frontend
cd src/web && npm install && npm run dev
```
