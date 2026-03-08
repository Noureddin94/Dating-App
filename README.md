# Dating-App

# LoveMatch

## Project Structure
```
.
├── README.md
├── WebApp
│   ├── Application
│   │   └── Services
│   ├── Controllers
│   │   ├── API
│   │   ├── HomeController.cs
│   │   └── Web
│   ├── Domain
│   │   ├── Entities
│   │   │   ├── BaseEntity.cs
│   │   │   └── User.cs
│   │   └── Interfaces
│   │       └── IRepository.cs
│   ├── Infrastructure
│   │   ├── Data
│   │   │   └── AppDbContext.cs
│   │   └── Repositories
│   ├── Presentation
│   │   └── ViewModels
│   │       └── ErrorViewModel.cs
│   ├── Program.cs
│   ├── Properties
│   │   └── launchSettings.json
│   ├── Views
│   │   ├── Home
│   │   │   ├── Index.cshtml
│   │   │   └── Privacy.cshtml
│   │   ├── Shared
│   │   │   ├── Error.cshtml
│   │   │   ├── _Layout.cshtml
│   │   │   ├── _Layout.cshtml.css
│   │   │   └── _ValidationScriptsPartial.cshtml
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   ├── WebApp.csproj
│   ├── WebApp.csproj.user
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── bin
│   │   └── Debug
│   │       └── net9.0
│   ├── obj
│   │   ├── Debug
│   │   │   └── net9.0
│   │   ├── WebApp.csproj.nuget.dgspec.json
│   │   ├── WebApp.csproj.nuget.g.props
│   │   ├── WebApp.csproj.nuget.g.targets
│   │   ├── project.assets.json
│   │   └── project.nuget.cache
│   └── wwwroot
│       ├── css
│       │   └── site.css
│       ├── favicon.ico
│       ├── js
│       │   └── site.js
│       └── lib
│           ├── bootstrap
│           ├── jquery
│           ├── jquery-validation
│           └── jquery-validation-unobtrusive
├── WebApplication1.sln
└── WebApplication1.slnLaunch.user
```