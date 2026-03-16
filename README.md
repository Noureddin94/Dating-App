# Dating-App

# LoveMatch

## Project Structure
```
.
├── Areas
│   └── Identity
│       └── Pages
├── Components
│   ├── App.razor
│   ├── Layout
│   │   └── MainLayout.razor
│   ├── Routes.razor
│   └── _Imports.razor
├── Controllers
│   ├── API
│   │   ├── AccountController.cs
│   │   └── ValuesController.cs
│   └── Web
│       └── HomeController.cs
├── Domain
│   ├── Entities
│   │   ├── BaseEntity.cs
│   │   ├── Block.cs
│   │   ├── Conversation.cs
│   │   ├── DailyActionCount.cs
│   │   ├── GameInvite.cs
│   │   ├── GameSession.cs
│   │   ├── Like.cs
│   │   ├── Match.cs
│   │   ├── Message.cs
│   │   ├── ProfileImage.cs
│   │   ├── Report.cs
│   │   └── UserProfile.cs
│   ├── Enums
│   │   ├── AccountStatus.cs
│   │   ├── ActionType.cs
│   │   ├── InviteStatus.cs
│   │   ├── ReportStatus.cs
│   │   └── SessionStatus.cs
│   └── Interfaces
│       └── IRepository.cs
├── Infrastructure
│   ├── Data
│   │   └── AppDbContext.cs
│   └── Migrations
│       ├── 20260308190241_FirstMigration.Designer.cs
│       ├── 20260308190241_FirstMigration.cs
│       ├── 20260316040044_InitialDomainModel.Designer.cs
│       ├── 20260316040044_InitialDomainModel.cs
│       └── AppDbContextModelSnapshot.cs
├── Presentation
│   └── ViewModels
│       └── ErrorViewModel.cs
├── Program.cs
├── Properties
│   └── launchSettings.json
├── ScaffoldingReadMe.txt
├── Services
│   └── CustomAuthStateProvider.cs
├── Views
│   ├── Home
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Shared
│   │   ├── Error.cshtml
│   │   ├── _Layout.cshtml
│   │   ├── _Layout.cshtml.css
│   │   ├── _LoginPartial.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── WebApp.csproj
├── WebApp.csproj.user
├── appsettings.Development.json
├── appsettings.json
├── bin
│   └── Debug
│       └── net9.0
├── obj
│   ├── Debug
│   │   └── net9.0
│   ├── WebApp.csproj.EntityFrameworkCore.targets
│   ├── WebApp.csproj.nuget.dgspec.json
│   ├── WebApp.csproj.nuget.g.props
│   ├── WebApp.csproj.nuget.g.targets
│   ├── project.assets.json
│   └── project.nuget.cache
└── wwwroot
    ├── css
    │   └── site.css
    ├── favicon.ico
    ├── js
    │   └── site.js
    └── lib
        ├── bootstrap
        ├── jquery
        ├── jquery-validation
        └── jquery-validation-unobtrusive
```