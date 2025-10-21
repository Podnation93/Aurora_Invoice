# Aurora Invoice - Quick Start Guide

Welcome to Aurora Invoice! This guide will help you get started quickly.

## First Time Setup

### Running the Application

1. **Build the application**:
   ```bash
   dotnet build
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. The application will:
   - Automatically create a database in `%LocalAppData%\AuroraInvoice\`
   - Apply all necessary database migrations
   - Open the main window with the Dashboard

## Understanding the Interface

### Navigation Sidebar (Left Side)

The dark sidebar on the left contains navigation buttons for all main sections:

- **Dashboard**: Overview of your business finances
- **Invoices**: Create and manage invoices (coming soon)
- **Customers**: Manage customer database (coming soon)
- **Expenses**: Track business expenses (coming soon)
- **Reports**: Generate financial reports (coming soon)
- **Settings**: Configure application preferences (coming soon)
- **Backup & Restore**: Manage data backups (coming soon)

### Dashboard (Home Screen)

The Dashboard shows four key metric cards:

1. **Total Income**: Income from paid invoices this month
2. **Total Expenses**: Total expenses recorded this month
3. **Net GST**: Net GST position (collected minus paid)
4. **Pending Invoices**: Count of invoices awaiting payment

Below the metrics:
- **Recent Invoices**: Table showing the 10 most recent invoices
- **Quick Actions**: Buttons to quickly create new records or access features

## Current Functionality (v1.0.0-alpha)

### What Works Now

✅ **Application Launch**: Database initialization and setup
✅ **Navigation**: Switch between different sections
✅ **Dashboard**: View business metrics (will show real data once invoices/expenses are added)
✅ **Modern UI**: Beautiful, clean interface with professional design

### What's Coming Next

The following features are in development and will be implemented in future updates:

#### Phase 1 (Next Release)
- Customer Management (Add, edit, view, delete customers)
- Basic Invoice Creation
- Invoice List and Search

#### Phase 2
- Complete Invoice Management
- Expense Tracking
- PDF Invoice Export

#### Phase 3
- Comprehensive Reporting
- Backup and Restore
- Settings and Customization

## Database Location

Your data is stored locally in:
```
%LocalAppData%\AuroraInvoice\aurora_invoice.db
```

To find this on your computer:
1. Press `Win + R`
2. Type `%LocalAppData%\AuroraInvoice`
3. Press Enter

## Development Mode

### Hot Reload (Recommended)

For development with hot reload:
```bash
dotnet watch run
```

This will automatically rebuild and restart when you make changes to the code.

### Database Management

#### View Current Database
The database file is a SQLite database that can be opened with tools like:
- [DB Browser for SQLite](https://sqlitebrowser.org/)
- [SQLite Studio](https://sqlitestudio.pl/)
- Visual Studio SQL Server Object Explorer

#### Reset Database
To start fresh:
1. Close the application
2. Delete the `aurora_invoice.db` file from `%LocalAppData%\AuroraInvoice\`
3. Restart the application (it will recreate the database)

#### Create New Migration
After modifying models:
```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

## Customization

### Changing Theme Colors

Edit [Resources/Styles.xaml](Resources/Styles.xaml):

```xml
<SolidColorBrush x:Key="PrimaryBrush" Color="#2563eb"/>  <!-- Main blue -->
<SolidColorBrush x:Key="AccentBrush" Color="#7c3aed"/>   <!-- Purple accent -->
```

### Modifying the Sidebar

Edit [MainWindow.xaml](MainWindow.xaml) to:
- Add/remove navigation items
- Change sidebar width (currently 250px)
- Modify colors or styling

## Troubleshooting

### Application won't start
- Ensure .NET 9.0 SDK is installed: `dotnet --version`
- Check for error messages in the console
- Try deleting the database and restarting

### Build errors
- Run `dotnet restore` to restore packages
- Run `dotnet clean` then `dotnet build`
- Check that all NuGet packages are restored

### Database errors
- Close all instances of the application
- Delete the database file
- Restart the application

## Next Steps

### For Users
- Wait for the next release which will include customer and invoice management
- Follow the project on GitHub for updates
- Report any bugs or suggestions

### For Developers
- Read [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines
- Check the GitHub Issues for tasks that need help
- Start with "good first issue" labels

## Getting Help

- **Documentation**: See [README.md](README.md)
- **Issues**: [GitHub Issues](https://github.com/yourusername/aurora-invoice/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/aurora-invoice/discussions)

## What You Can Do Right Now

Even though many features are still in development, you can:

1. **Explore the UI**: Navigate through all the sections to see the layout
2. **Check the Database**: Look at the pre-populated expense categories
3. **Examine the Code**: Review the architecture and data models
4. **Contribute**: Pick a feature to implement from the roadmap
5. **Provide Feedback**: Suggest improvements to the UI or features

---

**Thank you for trying Aurora Invoice!**

We're building this for the Australian small business community, and your feedback and contributions are invaluable.
