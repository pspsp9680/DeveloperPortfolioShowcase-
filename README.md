# Developer Portfolio & Local Project Showcase Utility (C#)

A Windows desktop application built with C# and Windows Forms for Visual Studio Community.

## Requirements
- Windows 10 or Windows 11
- Visual Studio 2022 Community (or newer) with the **.NET desktop development** workload
- .NET 8 SDK / targeting pack (Visual Studio Installer can install it)

## Open and run
1. Extract `DeveloperPortfolioShowcase.zip`.
2. Open `DeveloperPortfolioShowcase.sln` in Visual Studio.
3. If prompted, install/restore the .NET 8 SDK/targeting pack.
4. In Solution Explorer, right-click the project and choose **Set as Startup Project**.
5. Press **F5** to run, or **Ctrl+F5** to run without debugging.

No NuGet packages are required.

## Features
- Developer profile editor
- Project catalog with add/edit/delete
- Search by project title, description, language, and tags
- Language filter
- Open project folders
- Run supported local entry files
- File count for a project folder
- Reviewer feedback and 1–5 ratings
- Dashboard statistics and run history
- Export portfolio summary as text
- Import/export all app data as JSON
- Automatic local JSON persistence

## Where data is saved
`%LOCALAPPDATA%\\DeveloperPortfolioShowcase\\portfolio-data.json`

Use Dashboard → **Open data folder** to view it.

## Running a project
When adding/editing a project, select its project folder and set an entry file. Supported launch types:
- `.exe`
- `.bat` / `.cmd`
- `.py` (requires Python installed and available as `python`)
- `.js` (requires Node.js installed and available as `node`)
- `.ps1` (PowerShell)
- `.vbs`

For C#, C++, Java, or other compiled projects, build them with the relevant toolchain first and select the resulting executable. This app launches the process; it does not compile arbitrary source code itself. Only run code you trust.

## Notes
- Project folders and entry paths are stored as local paths. If you move a project folder, edit the project entry to update its path.
- File counting may take time for large folders.
- Importing JSON replaces the currently loaded profile, projects, reviews, and run history. Export a backup first if needed.
- This is a local portfolio organizer; it does not create online accounts or upload data.
