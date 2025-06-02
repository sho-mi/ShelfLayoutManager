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
The server will be available at `https://localhost:5237` and `http://localhost:5236`

### Running the Web Client
```bash
cd src/ShelfLayout.Web
dotnet run
```
The web client will be available at `http://localhost:5120`

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

### Application Flow
- Once the user accesses the application, Homepage shows the Cabinet Management Tool, using which users can add new cabinets, remove rows, add and remove various SKU's. Frontend of the Cabinet Management Tool looks like - 
![image](https://github.com/user-attachments/assets/af12d9ef-67e1-4bf8-b65e-7eef21e089f6)

- Once the user removes a SKU, he/she gets an option to add another SKU to the same lane and once he/she clicks on Add SKU button, visible for a particular lane, Add SKU to Lane form is opened just below the cabinet.
<img width="1242" alt="Screenshot 2568-06-02 at 12 12 25" src="https://github.com/user-attachments/assets/d51155ce-7a8f-4426-b47c-4fad01775be6" />

- We also have a separate SKU Management Tool, using which we can add new SKU's and manage existing SKU's. Images depicting the functionalities, as they appear on the frontend -

Add New SKU form -
<img width="1257" alt="Screenshot 2568-06-02 at 12 12 51" src="https://github.com/user-attachments/assets/803232ac-f6e0-488d-84b8-ab75071120b1" />

SKU Management Section -
<img width="1265" alt="Screenshot 2568-06-02 at 12 13 00" src="https://github.com/user-attachments/assets/35fe57ac-dee5-4217-b70d-d9f75b4d859c" />



## Development Time BreakdownAdd commentMore actions

- Project Setup and Architecture: 2 hours
- Core Domain Implementation: 3 hours
- Infrastructure Layer: 2 hours
- Application Layer: 3 hours
- UI Implementation: 4 hours
- Testing: 3 hours
- Documentation: 1 hour
- Total: ~18 hours

## Future Improvements
-
- 

## License

This project is licensed under the MIT License - see the LICENSE file for details. 
