# Shelf Layout Management System

A comprehensive shelf layout management system built with .NET 8, featuring a modern web interface and real-time updates.

## Project Architecture

The solution is structured into several projects:

### Core Projects
- **ShelfLayout.Core**: Contains the core domain entities, interfaces, and business logic
  - Entities: `Cabinet`, `Row`, `Lane`, `Sku`
  - Interfaces: `IShelfRepository`, `ISkuRepository`
  - Models: `ShelfData`, `CabinetManagementData`

### Infrastructure
- **ShelfLayout.Infrastructure**: Implements data persistence and external service integrations
  - `ShelfRepository`: Manages shelf layout data persistence
  - `SkuRepository`: Handles SKU data management

### Server
- **ShelfLayout.Server**: ASP.NET Core Web API project
  - RESTful endpoints for shelf layout management
  - SignalR hub for real-time updates
  - Controllers: `ShelfLayoutController`, `SkuController`

### Web Client
- **ShelfLayout.Web**: Blazor WebAssembly client
  - Modern UI with real-time updates
  - Interactive shelf layout management
  - SKU management interface

### Tests
- **ShelfLayout.Core.Tests**: Unit tests for core domain logic
- **ShelfLayout.Infrastructure.Tests**: Tests for data persistence
- **ShelfLayout.Server.Tests**: API and SignalR hub tests
- **ShelfLayout.Web.Tests**: Client-side tests

## Prerequisites

- .NET 8 SDK
- Node.js (for web client development)
- Visual Studio 2022 or VS Code with C# extensions

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/sho-mi/ShelfLayoutManager.git
cd ShelfLayout
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

## Running the Application

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/ShelfLayout.Core.Tests
dotnet test tests/ShelfLayout.Infrastructure.Tests
dotnet test tests/ShelfLayout.Server.Tests
dotnet test tests/ShelfLayout.Web.Tests
```

### Running the Server
```bash
cd src/ShelfLayout.Server
dotnet run
```
The server will be available at `https://localhost:5001` and `http://localhost:5000`

### Running the Web Client
```bash
cd src/ShelfLayout.Web
dotnet run
```
The web client will be available at `https://localhost:5002` and `http://localhost:5003`

## Development

### Project Structure
```
ShelfLayout/
├── src/
│   ├── ShelfLayout.Core/
│   ├── ShelfLayout.Infrastructure/
│   ├── ShelfLayout.Server/
│   └── ShelfLayout.Web/
├── tests/
│   ├── ShelfLayout.Core.Tests/
│   ├── ShelfLayout.Infrastructure.Tests/
│   ├── ShelfLayout.Server.Tests/
│   └── ShelfLayout.Web.Tests/
└── README.md
```

### Key Features
- Real-time shelf layout updates using SignalR
- RESTful API for shelf management
- Modern Blazor WebAssembly UI
- Comprehensive test coverage
- Clean architecture with separation of concerns


## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 
