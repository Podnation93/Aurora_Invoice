# Aurora Invoice

![Aurora Invoice](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

**Aurora Invoice** is an open-source, desktop-based invoicing and financial tracking application designed for small to medium-sized businesses in Australia. Built with C# and WPF, it provides a beautiful, intuitive user experience with essential features for managing invoices, tracking expenses, calculating GST, and generating financial reports.

## Features

- **Modern UI/UX**: Clean, beautiful interface with dark sidebar navigation and modern design principles.
- **Dashboard**: At-a-glance overview of key business metrics, including monthly income, expenses, and net GST position.
- **Invoice Management**: Create, edit, and manage professional invoices with customizable templates. Track invoice status (Draft, Sent, Paid, Overdue) and export to PDF.
- **Customer Management**: Maintain a comprehensive customer database with contact details, ABN, and address information.
- **Expense Tracking**: Record and categorize business expenses with GST calculations for accurate input tax credit tracking.
- **Financial Reporting**: Generate key financial reports:
  - **GST Summary Report**: View GST collected and paid for a selected period.
  - **Yearly Financial Report**: A complete financial summary for any given year.
  - **Invoice Activity Report**: An overview of all invoice activity within a date range.
  - **Expense Report**: A detailed breakdown of expenses by category.
- **Local Backup & Restore**: One-click backup and restore functionality to keep your data safe.
- **Robust Architecture**: Built with a modern MVVM architecture, dependency injection, and a service-oriented approach for maintainability and testability.

## Technology Stack

- **Language**: C# (.NET 9.0)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Database**: SQLite with Entity Framework Core
- **MVVM Toolkit**: CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **PDF Generation**: QuestPDF
- **Architecture**: MVVM (Model-View-ViewModel)

## Requirements

- Windows 10/11
- .NET 9.0 SDK or Runtime
- 50 MB disk space (minimum)

## Installation

### For Users

1. Download the latest release from the [Releases](https://github.com/yourusername/aurora-invoice/releases) page.
2. Extract the ZIP file to your preferred location.
3. Run `AuroraInvoice.exe`.
4. The application will automatically create a database on first run.

### For Developers

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/aurora-invoice.git
   cd aurora-invoice
   ```

2. Ensure you have .NET 9.0 SDK installed:
   ```bash
   dotnet --version
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Build the application:
   ```bash
   dotnet build
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

## Development

### Project Structure

```
AuroraInvoice/
├── Converters/             # Value converters for XAML data binding
├── Data/                   # Database context and configurations
├── Models/                 # Data models (Customer, Invoice, Expense, etc.)
├── Services/               # Business logic services
├── ViewModels/             # View models for MVVM pattern
├── Views/                  # XAML pages and user interfaces
├── Resources/              # Styles, themes, and resources
└── Migrations/             # Entity Framework migrations
```

### Database

The SQLite database is stored in:
```
%LocalAppData%\AuroraInvoice\aurora_invoice.db
```

### Creating Migrations

When you modify the data models:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

## Contributing

We welcome contributions from the community! Please follow the standard fork, branch, and pull request workflow.

## Roadmap

### Version 1.0 (Current)
- [x] Project setup and architecture
- [x] Database models and migrations
- [x] Main window and navigation
- [x] Dashboard with key metrics
- [x] Customer management (CRUD)
- [x] Invoice management (CRUD)
- [x] Expense tracking (CRUD)
- [x] PDF export for invoices
- [x] All reporting features (GST, Yearly, Invoice, Expense)
- [x] Backup and restore functionality
- [x] Settings and preferences
- [x] MVVM and DI refactoring

### Future Enhancements
- Multi-currency support
- Cloud sync (optional)
- Email integration for sending invoices
- Recurring invoices
- Payment tracking
- Multi-user support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/)
- PDF generation powered by [QuestPDF](https://www.questpdf.com/)

## Screenshots

*Coming soon - Screenshots will be added as features are completed*

## Disclaimer

Aurora Invoice is designed for small to medium-sized businesses in Australia. While we strive for accuracy in GST calculations and reporting, please consult with a qualified accountant or tax professional for your specific business needs. This software is provided "as is" without warranty of any kind.

---

**Made with care for the Australian small business community**