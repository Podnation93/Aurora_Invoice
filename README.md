# Aurora Invoice

![Aurora Invoice](https://img.shields.io/badge/version-1.0.0--alpha-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

**Aurora Invoice** is an open-source, desktop-based invoicing and financial tracking application designed for small to medium-sized businesses in Australia. Built with C# and WPF, it provides a beautiful, intuitive user experience with essential features for managing invoices, tracking expenses, calculating GST, and generating financial reports.

## Features

### Current Features (v1.0.0-alpha)

- **Modern UI/UX**: Clean, beautiful interface with dark sidebar navigation and modern design principles
- **Dashboard**: At-a-glance overview of key business metrics
  - Total income and expenses (monthly view)
  - Net GST position (payable/refundable)
  - Pending invoices count
  - Recent invoice activity
- **Database Management**: SQLite-based local database with Entity Framework Core
- **GST Calculation Engine**: Automated GST calculations for Australian businesses
- **Navigation System**: Easy-to-use sidebar navigation between all main sections

### Planned Features

- **Invoice Generation & Management**
  - Create professional invoices with customizable templates
  - Support for ABN, customer details, itemized services
  - Multiple GST rates (10% standard, GST-free items)
  - Track invoice status (Draft, Sent, Paid, Overdue)
  - PDF export and printing

- **Customer Management**
  - Comprehensive customer database
  - Store ABN, contact details, address information
  - Quick customer selection for invoices

- **Expense Tracking**
  - Record business expenses with GST component
  - Pre-defined and customizable expense categories
  - Attach receipts and supporting documents
  - Filter by date, category, or vendor
  - Automatic GST input tax credit calculations

- **Tax Tracking (GST)**
  - Automated GST calculation on invoices and expenses
  - Net GST position reporting
  - BAS (Business Activity Statement) support

- **Reporting**
  - Yearly Financial Report
  - Invoice Activity Report
  - Expense Report by category/vendor
  - GST Summary Report

- **Local Backup & Restore**
  - One-click backup to local folder
  - Easy restore functionality
  - Scheduled automatic backups

- **Customization**
  - Customizable invoice templates
  - Business logo and branding
  - Theme colors
  - Invoice numbering preferences

## Technology Stack

- **Language**: C# (.NET 9.0)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Database**: SQLite with Entity Framework Core
- **MVVM Toolkit**: CommunityToolkit.Mvvm
- **PDF Generation**: QuestPDF
- **Architecture**: MVVM (Model-View-ViewModel)

## Requirements

- Windows 10/11
- .NET 9.0 SDK or Runtime
- 50 MB disk space (minimum)

## Installation

### For Users

1. Download the latest release from the [Releases](https://github.com/yourusername/aurora-invoice/releases) page
2. Extract the ZIP file to your preferred location
3. Run `AuroraInvoice.exe`
4. The application will automatically create a database on first run

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
├── Data/                   # Database context and configurations
├── Models/                 # Data models (Customer, Invoice, Expense, etc.)
├── Views/                  # XAML pages and user interfaces
├── ViewModels/             # View models for MVVM pattern
├── Services/               # Business logic services
├── Helpers/                # Utility classes and helpers
├── Resources/              # Styles, themes, and resources
└── Migrations/             # Entity Framework migrations
```

### Key Components

- **AuroraDbContext**: Entity Framework database context
- **Models**: Customer, Invoice, InvoiceItem, Expense, ExpenseCategory, AppSettings
- **Services**:
  - `DatabaseService`: Database initialization and management
  - `GstCalculationService`: GST calculations
  - `BackupService`: Backup and restore functionality

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

We welcome contributions from the community! Here's how you can help:

### Getting Started

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Commit your changes (`git commit -m 'Add some amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

### Contribution Guidelines

- **Code Style**: Follow C# coding conventions and use meaningful variable names
- **Comments**: Add XML documentation comments for public methods and classes
- **Testing**: Test your changes thoroughly before submitting
- **Commits**: Write clear, concise commit messages
- **Pull Requests**: Provide a clear description of what your PR does

### Areas to Contribute

- Complete the Invoice Management page
- Implement Customer CRUD operations
- Build the Expense Tracking functionality
- Create PDF invoice templates
- Develop the Reporting system
- Improve UI/UX design
- Add unit tests
- Write documentation
- Fix bugs

## Roadmap

### Phase 1 (Current - Alpha)
- [x] Project setup and architecture
- [x] Database models and migrations
- [x] Main window and navigation
- [x] Dashboard with basic metrics
- [ ] Customer management (CRUD)
- [ ] Basic invoice creation

### Phase 2 (Beta)
- [ ] Complete invoice management
- [ ] Expense tracking
- [ ] PDF export functionality
- [ ] Basic reporting

### Phase 3 (v1.0)
- [ ] Advanced reporting (Yearly, GST, BAS)
- [ ] Backup and restore
- [ ] Customizable templates
- [ ] Settings and preferences
- [ ] Polish and bug fixes

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
- UI inspiration from modern design principles
- PDF generation powered by [QuestPDF](https://www.questpdf.com/)
- Icons and styling using WPF Material Design principles

## Support

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/yourusername/aurora-invoice/issues)
- **Discussions**: Join our [GitHub Discussions](https://github.com/yourusername/aurora-invoice/discussions)
- **Documentation**: Check the [Wiki](https://github.com/yourusername/aurora-invoice/wiki) for detailed guides

## Screenshots

*Coming soon - Screenshots will be added as features are completed*

## Disclaimer

Aurora Invoice is designed for small to medium-sized businesses in Australia. While we strive for accuracy in GST calculations and reporting, please consult with a qualified accountant or tax professional for your specific business needs. This software is provided "as is" without warranty of any kind.

---

**Made with care for the Australian small business community**
