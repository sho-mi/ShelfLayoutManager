# Shelf Layout Visualization and Modification Tool

A Blazor WebAssembly application for visualizing and modifying shelf layouts in stores.

## Project Overview

This application allows operations team members to:
- Visualize shelf layouts in stores
- Add new SKU drinks to shelves
- Move existing SKU drinks between positions
- Remove SKU drinks from shelves

## Technical Stack

- .NET 8.0
- Blazor WebAssembly
- System.Reactive for reactive programming
- xUnit for unit testing
- Clean Architecture principles

## Project Structure

```
ShelfLayout/
├── src/
│   ├── ShelfLayout.Core/           # Domain entities and interfaces
│   ├── ShelfLayout.Infrastructure/ # Implementation of interfaces
│   ├── ShelfLayout.Application/    # Business logic and use cases
│   └── ShelfLayout.Web/           # Blazor WebAssembly UI
└── tests/
    ├── ShelfLayout.Core.Tests/
    ├── ShelfLayout.Infrastructure.Tests/
    ├── ShelfLayout.Application.Tests/
    └── ShelfLayout.Web.Tests/
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extensions

### Running the Application

1. Clone the repository
2. Navigate to the `src/ShelfLayout.Web` directory
3. Run the following commands:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
4. Open your browser and navigate to `https://localhost:5001`

### Running Tests

```bash
dotnet test
```

## Development Time Breakdown

- Project Setup and Architecture: -- hours
- Core Domain Implementation: -- hours
- Infrastructure Layer: -- hours
- Application Layer: -- hours
- UI Implementation: -- hours
- Testing: -- hours
- Documentation: -- hour
- Total: ~18 hours

## Future Improvements

### Must Have
1. Implement proper error handling and logging
2. Add user authentication and authorization
3. Implement data persistence with a proper database
4. Add input validation and error messages
5. Implement proper state management

### Nice to Have
1. Add drag-and-drop functionality for SKU placement
2. Implement real-time collaboration features
3. Add export/import functionality for shelf layouts
4. Implement undo/redo functionality
5. Add performance optimizations for large datasets
6. Implement responsive design for mobile devices
7. Add analytics and reporting features

## Contributing

Please read CONTRIBUTING.md for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 
