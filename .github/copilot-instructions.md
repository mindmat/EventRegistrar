# Copilot Instructions for EventRegistrar

## Project Overview

Admin tool for **dance event registration** with an Angular 15 dashboard (Fuse template) and .NET 9 backend API. Manages registrations, participants, payments, mailing, and volunteer planning.

## Domain Model

### Dance Event Concepts
- **Roles**: Dance events have two roles - **Leaders** and **Followers** - that must be balanced in classes
- **Registration types**: Participants register either **alone** (specifying their role) or as a **couple**
- **Tracks/Classes**: Fill spots with balanced leader/follower ratios

### Core Entities
- **`Registration`**: Created from a Google Forms response. One form submission = one Registration
- **`Spot`**: A Registration has 1-n Spots (places in registrables)
- **`Registrable`**: Generic concept for anything participants can sign up for:
  - Dance classes/tracks
  - Merchandise
  - Reductions/discounts
  - Volunteering shifts
  - Food options
  - Hosting (accommodation)

### Data Flow
```
Google Forms → Azure Functions (JSON) → SQL Server → Backend processing
```

### Communication
- Email via **Postmark**, **SendGrid**, or **IMAP**
- Auto-mail templates and bulk mailing features

### Accounting
- Import bank statements (**ISO 20022** format)
- Assign payments to registrations
- Track payment differences and due payments

---

## Monorepo Structure

```
(root)/src/
├── eventregistrar-admin-angular/   # Angular frontend
└── EventRegistrar.Backend/         # .NET backend API
```

---

## Backend (.NET)

### Authorization & Permissions
- When creating new `IRequest` implementations (queries or commands), always add the request name to the appropriate role in `RightsOfEventRoleProvider.cs`
- Read-only queries → **Reader** role
- Write operations → **Writer** or **Admin** roles based on permissions level

### CQRS Pattern
- Queries: `*Query` classes for read operations
- Commands: `*Command` classes for write operations
- SignalR broadcasts query invalidation to connected clients

---

## Frontend (Angular)

### API Layer (`src/app/api/api.ts`)
- **Auto-generated** via NSwag from the backend - **never edit manually**
- Regenerate when backend contracts change (run NSwag against `EventRegistrar.Backend`)
- CQRS-style naming: `api.*_Query()`, `api.*_Command()`
- Base URL via `API_BASE_URL` injection token

### Real-time Updates (SignalR)
- `NotificationService` manages WebSocket connections
- Services subscribe: `notificationService.subscribe('QueryName')`
- Auto-refresh via `FetchService` base class

### Service Pattern
```typescript
export class MyService extends FetchService<MyData> {
    constructor(api: Api, notificationService: NotificationService) {
        super('MyQueryName', notificationService);
    }
    fetchData(): Observable<MyData> {
        return this.fetchItems(this.api.myData_Query({...}), rowId, eventId);
    }
}
```

### Component Data Loading
- Use **Resolvers** to prefetch data before route activation
- Pattern: `*Resolver` classes in same folder as components

### Event Context
- `EventService` tracks selected event via URL acronym (`/:eventAcronym/...`)
- All API calls require `eventId` from `eventService.selectedId`
- Navigation via `NavigatorService` for event-scoped routes

### File Organization
```
modules/admin/[feature]/
├── feature.component.ts
├── feature.component.html
├── feature.service.ts      # Extends FetchService
├── feature.resolver.ts     # Route resolver
└── [sub-feature]/
```

### Styling & i18n
- **TailwindCSS** + **Angular Material** components
- German locale (de-CH), date format: `DD.MM.YYYY`
- Translations via `@ngx-translate/core` loaded from backend

### Authentication
- **Auth0** (`@auth0/auth0-angular`), config in `auth_config.json`

---

## Development Commands

```bash
# Frontend (requires SSL for Auth0)
cd src/eventregistrar-admin-angular
ng serve --ssl true --ssl-key ./.certs/localhost.key --ssl-cert ./.certs/localhost.cer

# Backend
cd src/EventRegistrar.Backend
dotnet run
# Runs on https://localhost:5001
```

---

## Hosting & CI/CD

- **GitHub Actions** workflows in `.github/workflows/`
- Frontend: `azure-static-web-apps-*.yml` → Azure Static Web App (triggers on `cqsapi` branch, path `src/eventregistrar-admin-angular/**`)
- Backend: `azure-webapps-dotnet-core.yml` → Azure Web App (.NET 9)
- Build output: `dist/fuse`