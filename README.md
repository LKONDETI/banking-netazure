# Modernized Banking API using DOTNET

## Project Overview
This project aims to modernize the banking application, transitioning it from its original .NET 5.0 implementation to a modern .NET (targeting .NET 8 LTS) architecture. This is a multi-tenant platform providing internet banking services, including account management, bill pay, eStatements, and more.

## Objective
Re-implement the core API endpoints and business logic using modern .NET best practices while maintaining compatibility with the existing domain requirements and Clean Architecture principles.

## Proposed Architecture
We will follow a **Clean Architecture** approach, similar to the original project, but with modern enhancements:

- **Presentation Layer**: ASP.NET Core Web API (using Controllers or Minimal APIs).
- **Application Layer**: Use Case Interactors, MediatR handlers, and FluentValidation.
- **Core (Domain) Layer**: Domain entities, Value Objects, and Domain Logic.
- **Infrastructure Layer**: Implementation of external concerns (Caching, Identity, Messaging).
- **Data Access Layer**: Raw ADO.NET (or EF Core) providers for IBM DB2.

## Key Features to Port
1. **Multi-Tenancy**: Dynamic tenant resolution based on request headers/cookies/route.
2. **Identity & Security**: Cookie-based authentication, custom TFA (Two-Factor Authentication), and CSRF protection.
3. **Account Management**: Comprehensive account listing and filtering.
4. **Banking Features**: Funds transfers, Bill Pay, E-Services (eMessages, eStatements).
5. **Modernization**: 
    - Upgrade to .NET 8.
    - Improve Dependency Injection patterns.
    - Modernize Logging and Middleware.
    - (Optional) Transition from raw ADO.NET to EF Core where feasible.

## Modernization Challenges & Priorities
- **Target Framework**: Upgrade to .NET 8 LTS for long-term support and performance.
- **Dependency Modernization**: Update MediatR, FluentValidation, and other core libraries to their latest versions, handling any breaking changes.
- **SPA Integration**: Transition from the deprecated `SpaServices.Extensions` to a modern standalone React build process.
- **WCF & COM Interop**: 
    - Port legacy WCF service bindings to modern `System.ServiceModel` equivalents.
    - Evaluate the LIGS COM interop dependencies which currently restrict the platform to Windows.
- **Security & Configuration**:
    - Remove plaintext credentials from `appsettings.json`.
    - Implement Azure Key Vault or environment-based secret management.
    - Standardize JSON serialization (System.Text.Json vs Newtonsoft.Json).
- **Data Access**: Review raw ADO.NET usage and consider introducing Entity Framework Core for better maintainability while retaining performance.

## Getting Started
1. **Prerequisites**: .NET 8 SDK installed.
2. **Configuration**: Set up connection strings and tenant configurations in `appsettings.json`.
3. **Run**: `dotnet run --project banking-netazure` (or similar).

