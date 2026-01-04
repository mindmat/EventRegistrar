# EventRegistrar Project - AI Context

## Overview
EventRegistrar is a comprehensive event management system built with .NET 9 and Entity Framework Core. It handles event registration, participant management, payments, mailing, and volunteer planning with a CQRS architecture using MediatR.

## Architecture & Patterns

### CQRS with MediatR
- **Commands**: Handle write operations (Create, Update, Delete)
- **Queries**: Handle read operations and data retrieval
- **Handlers**: Implement IRequestHandler<TRequest, TResponse> for processing
- Requests are authorized in RightsOfEventRoleProvider

### Entity Framework Patterns
- All entities inherit from `Entity` base class with `Id` and `RowVersion`
- Entity configurations use `EntityMap<T>` base class
- Repository pattern with `IRepository<T>` interface
- Read models for optimized queries

### Domain Events
- Domain events for decoupled communication between bounded contexts
- Event bus for publishing and handling domain events
- Event-to-command translations for cross-cutting concerns

## Project Structure

### Core Domains
1. **Events**: Event management and configuration
2. **Registrations**: Participant registration and management
3. **Payments**: Payment processing and tracking
4. **Mailing**: Email composition, templates, and delivery
5. **VolunteerPlanning**: Shift management and volunteer assignments
6. **Registrables**: Event activities and tracks
7. **Authentication**: User management and authorization

### Infrastructure
- **DataAccess**: EF Core context, repositories, read models
- **Mediator**: Request/response pipeline with decorators
- **DomainEvents**: Event publishing and handling
- **Configuration**: Event-specific configuration management
- **ServiceBus**: Message queue integration

## Code Conventions

### Naming Patterns
- Commands: `{Action}{Entity}Command` (e.g., `CreateShiftCommand`)
- Queries: `{Entity}{Purpose}Query` (e.g., `ShiftsOverviewQuery`)
- Handlers: `{Command/Query}Handler`
- Display Items: `{Entity}DisplayItem` for DTOs
- Entities: Pascal case, singular nouns

### File Organization
```
src/EventRegistrar.Backend/
├── {Domain}/
│   ├── {Entity}.cs (Entity + EntityMap)
│   ├── {Action}{Entity}Command.cs
│   ├── {Entity}{Purpose}Query.cs
```

### Entity Relationships
- Foreign keys: `{Entity}Id` or `{Entity}Id_{Role}` for multiple relations
- Navigation properties: `{Entity}` or `{Entity}_{Role}`
- Collections: `ICollection<T>?` with nullable reference

### API Endpoints
- Pattern: `api/events/{eventAcronym}/{domain}/{id?}`
- Event-scoped operations require `eventAcronym` parameter
- HTTP request are mediated to request handlers. there is no need for ASP.NET Controllers

## Key Interfaces & Base Classes

### Commands & Queries
```csharp
// Event-bound requests
public interface IEventBoundRequest
{
    Guid EventId { get; set; }
}

// Command example
public class CreateEntityCommand : IRequest<Guid>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    // Properties...
}

// Query example  
public class EntityQuery : IRequest<EntityDisplayItem>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid EntityId { get; set; }
}
```

### Entities
```csharp
public class MyEntity : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    // Properties...
}

public class MyEntityMap : EntityMap<MyEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");
        // Configuration...
    }
}
```

## Database

### Technology
- SQL Server with Entity Framework Core
- Code-first migrations
- Multiple DbContexts (main + audit)

### Key Tables
- **Events**: Central event management
- **Registrations**: Participant data and state
- **Registrables**: Event activities/tracks
- **Seats/Spots**: Registration assignments
- **Payments**: Financial transactions
- **Mails**: Email communications
- **Shifts**: Volunteer planning (new feature)

## Dependencies & Tools

### Core Framework
- .NET 9 (C# 13.0)
- ASP.NET Core Web API
- Entity Framework Core
- MediatR for CQRS

### External Services
- Auth0 for authentication
- Twilio for SMS
- Various email providers (SendGrid, Postmark)
- Banking APIs for payment processing

### Development
- SimpleInjector for DI
- Swagger/OpenAPI for documentation
- PowerShell for tooling

## Testing Strategy
- Unit tests for handlers and business logic
- Integration tests for API and database
- Domain event testing for cross-cutting concerns

## Deployment
- Azure Functions for background processing
- Web API for real-time operations
- SQL Database for persistence

## Common Patterns

### Read Model Updates
```csharp
// Trigger read model updates after changes
changeTrigger.QueryChanged<MyEntityQuery>(eventId);
changeTrigger.TriggerUpdate<MyCalculator>(entityId, eventId);
```

### Error Handling
- Use specific exception types
- ExceptionTranslator for user-friendly messages
- Validation in command handlers

### Security
- Event-scoped authorization via `IEventBoundRequest`
- Role-based access control
- User context through IAuthenticatedUserProvider

This context should help AI assistants understand the project structure and maintain consistency with existing patterns when suggesting code changes or new features.