# OmniDesk Architecture

OmniDesk is a multi-tenant customer engagement and agent workspace platform.

This document describes the target system architecture and the main architectural decisions that guide implementation.

---

## 1. Architecture Goals

- Keep the system as a modular monolith with clear internal boundaries.
- Separate business logic from infrastructure and transport concerns.
- Enforce multi-tenant data isolation.
- Support realtime conversation updates.
- Keep AI capabilities isolated from core business logic.
- Support containerized deployment to Azure.
- Avoid unnecessary distributed-system complexity.

---

## 2. System Overview

```mermaid
flowchart LR

    User["Agent / User"]
    Web["React + TypeScript<br/>Agent Workspace"]
    Backend["ASP.NET Core Backend<br/>REST API + SignalR"]
    Database[("Azure SQL")]
    AI["Azure OpenAI"]
    Monitoring["Application Insights"]

    User --> Web
    Web -->|"REST API"| Backend
    Web <-->|"SignalR"| Backend

    Backend --> Database
    Backend --> AI
    Backend --> Monitoring
```

The frontend communicates with the ASP.NET Core backend through REST APIs and SignalR.  
The backend owns the application logic and integrates with Azure SQL, Azure OpenAI, and Application Insights.

---

## 3. Backend Project Architecture

The backend follows a simplified Clean Architecture structure.

```mermaid
flowchart TD

    API["OmniDesk.Api<br/>HTTP / SignalR / Composition Root"]
    APP["OmniDesk.Application<br/>Use Cases / Abstractions"]
    INFRA["OmniDesk.Infrastructure<br/>Technical Implementations"]
    DOMAIN["OmniDesk.Domain<br/>Business Model / Rules"]

    API --> APP
    API --> INFRA

    INFRA --> APP
    INFRA --> DOMAIN

    APP --> DOMAIN
```

> Arrows represent compile-time project dependencies.

| Project | Responsibility |
|---|---|
| `OmniDesk.Domain` | Core business entities and business rules |
| `OmniDesk.Application` | Use cases, application logic, and abstractions |
| `OmniDesk.Infrastructure` | Persistence, external services, and other technical implementations |
| `OmniDesk.Api` | HTTP, SignalR, host configuration, and dependency composition |

Business features such as Identity, Customers, Conversations, and AI are organized as folders inside these projects rather than as separately deployed services.

---

## 4. Deployment Architecture

The target deployment uses containers and Azure-managed services.

```mermaid
flowchart LR

    Developer["Developer"]
    GitHub["GitHub Repository"]
    CICD["GitHub Actions<br/>CI / CD"]
    Registry["Azure Container Registry"]
    App["Azure Container Apps<br/>ASP.NET Core Backend"]

    SQL[("Azure SQL")]
    OpenAI["Azure OpenAI"]
    KeyVault["Azure Key Vault"]
    Insights["Application Insights"]

    Developer --> GitHub
    GitHub --> CICD

    CICD -->|"Build / Test / Package"| Registry
    Registry --> App

    App --> SQL
    App --> OpenAI
    App --> KeyVault
    App --> Insights
```

---

## 5. Key Architecture Decisions

### Modular Monolith

OmniDesk starts as a modular monolith because the current scale and team size do not justify the operational complexity of microservices.

Clear internal boundaries are maintained so modules can be extracted later if independent deployment or scaling becomes necessary.

### Relational Database

SQL Server is used because the core domain is relational and requires transactional consistency.

Azure SQL is the target managed production database.

### Realtime Communication

SignalR is used for realtime conversation updates between the backend and React clients.

Business logic remains independent of the realtime transport mechanism.

### AI Integration

AI is treated as an external capability rather than part of the core domain.

AI-generated output is advisory and does not own authoritative business state.

### Multi-Tenancy

Tenant boundaries are treated as a security boundary across business data access.

All tenant-owned data must remain isolated between tenants.
