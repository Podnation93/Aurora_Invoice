# Contributing to Aurora Invoice

First off, thank you for considering contributing to Aurora Invoice! It's people like you that make Aurora Invoice such a great tool for the Australian small business community.

## Code of Conduct

This project and everyone participating in it is governed by common sense and mutual respect. By participating, you are expected to uphold this standard. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

- **Use a clear and descriptive title** for the issue
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and what behavior you expected
- **Include screenshots** if relevant
- **Include your environment details**: Windows version, .NET version, etc.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful** to Aurora Invoice users
- **List any similar features** in other applications if applicable

### Pull Requests

1. **Fork the repo** and create your branch from `main`
2. **Follow the coding standards** outlined below
3. **Test your changes** thoroughly
4. **Update documentation** if you've changed functionality
5. **Write clear commit messages**
6. **Ensure the build succeeds** before submitting

## Development Setup

### Prerequisites

- Windows 10/11
- .NET 9.0 SDK
- Visual Studio 2022 (recommended) or VS Code with C# extension
- Git

### Setting Up Your Development Environment

1. Fork and clone the repository:
   ```bash
   git clone https://github.com/YOUR-USERNAME/aurora-invoice.git
   cd aurora-invoice
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

### Project Structure

```
AuroraInvoice/
├── Data/                   # Database context and configurations
│   └── AuroraDbContext.cs
├── Models/                 # Data models
│   ├── Customer.cs
│   ├── Invoice.cs
│   ├── Expense.cs
│   └── ...
├── Views/                  # XAML UI pages
│   ├── DashboardPage.xaml
│   └── ...
├── ViewModels/             # MVVM view models
├── Services/               # Business logic
│   ├── DatabaseService.cs
│   ├── GstCalculationService.cs
│   └── BackupService.cs
├── Helpers/                # Utility classes
├── Resources/              # Styles and resources
│   └── Styles.xaml
└── Migrations/             # EF Core migrations
```

## Coding Standards

### C# Code Style

- **Naming Conventions**:
  - PascalCase for classes, methods, properties, and public fields
  - camelCase for private fields (prefix with underscore `_fieldName`)
  - Descriptive names over abbreviations

- **Formatting**:
  - Use 4 spaces for indentation (not tabs)
  - Opening braces on new line
  - One statement per line
  - Add spaces around operators

- **Documentation**:
  - Add XML documentation comments for public classes and methods
  - Example:
    ```csharp
    /// <summary>
    /// Calculates GST from a total amount including GST
    /// </summary>
    /// <param name="totalAmount">Total amount including GST</param>
    /// <param name="gstRate">GST rate (e.g., 0.10 for 10%)</param>
    /// <returns>GST component of the total amount</returns>
    public decimal CalculateGst(decimal totalAmount, decimal gstRate)
    {
        // Implementation
    }
    ```

### XAML Style

- Use proper indentation (4 spaces)
- Group related properties together
- Use resource dictionaries for reusable styles
- Follow WPF naming conventions for controls (e.g., `CustomerNameTextBox`)

### Git Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests after the first line
- Examples:
  - `Add customer search functionality`
  - `Fix GST calculation for GST-free items`
  - `Update invoice PDF template design`

## Database Changes

When modifying data models:

1. Make your changes to the model classes
2. Create a migration:
   ```bash
   dotnet ef migrations add YourMigrationName
   ```
3. Review the generated migration file
4. Test the migration:
   ```bash
   dotnet ef database update
   ```
5. Include the migration files in your commit

## Testing

- Test all functionality thoroughly before submitting a PR
- Ensure the application builds without errors or warnings
- Test on a clean database (delete the database file and restart)
- Verify UI responsiveness and appearance
- Test edge cases and error handling

## Areas That Need Help

We especially welcome contributions in these areas:

### High Priority
- [ ] Customer Management CRUD operations
- [ ] Invoice creation and editing UI
- [ ] Expense tracking functionality
- [ ] PDF invoice generation
- [ ] Reports implementation

### Medium Priority
- [ ] Settings page implementation
- [ ] Backup and restore UI
- [ ] Invoice template customization
- [ ] Data validation and error handling
- [ ] Unit tests

### Low Priority
- [ ] UI/UX improvements
- [ ] Performance optimizations
- [ ] Additional reporting features
- [ ] Documentation improvements
- [ ] Code refactoring

## Questions?

Feel free to:
- Open an issue for questions
- Start a discussion in GitHub Discussions
- Contact the maintainers

## Recognition

Contributors will be recognized in:
- The project README
- Release notes
- The application's About dialog (coming soon)

Thank you for contributing to Aurora Invoice!
