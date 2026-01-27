# Copilot Instructions

## General Guidelines
- When creating new `IRequest` implementations (queries or commands), always add the request name to the appropriate role in `RightsOfEventRoleProvider.cs`.
- Read-only queries typically go in the Reader role section, while commands typically go in Writer or Admin roles based on their permissions level.

## Code Style
- Use specific formatting rules.
- Follow naming conventions.