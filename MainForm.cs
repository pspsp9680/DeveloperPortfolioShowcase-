using System.Diagnostics;
using System.Text;

namespace DeveloperPortfolioShowcase;

public sealed class MainForm : Form
{
    private readonly StorageService _storage = new();
    private AppData _data;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _projectsGrid = CreateGrid();
    private readonly DataGridView _reviewsGrid = CreateGrid();
    private readonly DataGridView _runsGrid = CreateGrid();
    private readonly TextBox _search = new() { Width = 220, PlaceholderText = "Search title, language, tags..." };
    private readonly ComboBox _languageFilter = new() { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _name = new() { Width = 360 };
    private readonly TextBox _email = new() { Width = 360 };
    private readonly TextBox _bio = new() { Width = 500, Multiline = true, Height = 80, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _skills = new() { Width = 500, Multiline = true, Height = 55 };
    private readonly TextBox _portfolioUrl = new() { Width = 360 };
    private readonly Label _dashboardText = new() { AutoSize = true, Font = new Font("Segoe UI", 11), Padding = new Padding(10) };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };

    public MainForm()
    {
        _data = _storage.Load();
        Text = "Developer Portfolio & Local Project Showcase Utility";
        Width = 1180;
        Height = 760;
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);
        BuildUi();
        LoadProfileToForm();
        RefreshAll();
        FormClosing += (_, _) => SaveQuietly();
        SetStatus("Ready. Data is stored locally at: " + _storage.DataFolder);
    }

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.FromArgb(28, 42, 64) };
        var title = new Label
        {
            Text = "DEVELOPER PORTFOLIO",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(18, 8)
        };
        var subtitle = new Label
        {
            Text = "Local project catalog • script runner • reviews • metrics",
            ForeColor = Color.Gainsboro,
            AutoSize = true,
            Location = new Point(20, 39)
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        Controls.Add(_tabs);
        Controls.Add(_status);
        Controls.Add(header);
        

        _tabs.TabPages.Add(BuildDashboardTab());
        _tabs.TabPages.Add(BuildProfileTab());
        _tabs.TabPages.Add(BuildProjectsTab());
        _tabs.TabPages.Add(BuildReviewsTab());
        _tabs.TabPages.Add(BuildRunsTab());
        _tabs.SelectedIndexChanged += (_, _) => RefreshAll();
    }

    private TabPage BuildDashboardTab()
    {
        var page = new TabPage("Dashboard");
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(20) };
        panel.Controls.Add(new Label { Text = "Portfolio overview", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Margin = new Padding(4, 4, 4, 16) });
        _dashboardText.MaximumSize = new Size(950, 0);
        panel.Controls.Add(_dashboardText);
        var actions = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(4, 18, 4, 4) };
        actions.Controls.Add(MakeButton("Export portfolio summary", (_, _) => ExportSummary()));
        actions.Controls.Add(MakeButton("Export data (JSON)", (_, _) => ExportData()));
        actions.Controls.Add(MakeButton("Import data (JSON)", (_, _) => ImportData()));
        actions.Controls.Add(MakeButton("Open data folder", (_, _) => OpenDataFolder()));
        panel.Controls.Add(actions);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildProfileTab()
    {
        var page = new TabPage("Developer Profile");
        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(22) };
        int y = 20;
        AddField(panel, "Full name", _name, ref y);
        AddField(panel, "Email", _email, ref y);
        AddField(panel, "Portfolio URL", _portfolioUrl, ref y);
        AddField(panel, "Short bio", _bio, ref y);
        AddField(panel, "Technical skills (comma-separated)", _skills, ref y);
        var save = MakeButton("Save profile", (_, _) => SaveProfile());
        save.Location = new Point(170, y + 8);
        panel.Controls.Add(save);
        var note = new Label { Text = "Profile and project information are stored only on this computer by default.", AutoSize = true, ForeColor = Color.DimGray, Location = new Point(170, y + 52) };
        panel.Controls.Add(note);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildProjectsTab()
    {
        var page = new TabPage("Projects");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), WrapContents = false };
        top.Controls.Add(new Label { Text = "Search:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
        top.Controls.Add(_search);
        top.Controls.Add(new Label { Text = "Language:", AutoSize = true, Margin = new Padding(8, 8, 0, 0) });
        _languageFilter.Items.Add("All languages");
        _languageFilter.SelectedIndex = 0;
        top.Controls.Add(_languageFilter);
        top.Controls.Add(MakeButton("Apply filters", (_, _) => RefreshProjects()));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8), WrapContents = false };
        buttons.Controls.Add(MakeButton("Add project", (_, _) => AddProject()));
        buttons.Controls.Add(MakeButton("Edit selected", (_, _) => EditSelectedProject()));
        buttons.Controls.Add(MakeButton("Delete selected", (_, _) => DeleteSelectedProject()));
        buttons.Controls.Add(MakeButton("Run selected", (_, _) => RunSelectedProject()));
        buttons.Controls.Add(MakeButton("Open project folder", (_, _) => OpenSelectedFolder()));
        buttons.Controls.Add(MakeButton("Count files", (_, _) => CountSelectedFiles()));
        _projectsGrid.Dock = DockStyle.Fill;
        page.Controls.Add(_projectsGrid);
        page.Controls.Add(top);
        page.Controls.Add(buttons);
        return page;
    }

    private TabPage BuildReviewsTab()
    {
        var page = new TabPage("Reviews & Feedback");
        _reviewsGrid.Dock = DockStyle.Fill;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
        buttons.Controls.Add(MakeButton("Add review", (_, _) => AddReview()));
        buttons.Controls.Add(MakeButton("Delete selected review", (_, _) => DeleteSelectedReview()));
        page.Controls.Add(_reviewsGrid);
        page.Controls.Add(buttons);
        return page;
    }

    private TabPage BuildRunsTab()
    {
        var page = new TabPage("Run History");
        _runsGrid.Dock = DockStyle.Fill;
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
        buttons.Controls.Add(MakeButton("Clear run history", (_, _) => ClearRunHistory()));
        page.Controls.Add(_runsGrid);
        page.Controls.Add(buttons);
        return page;
    }

    private static DataGridView CreateGrid() => new()
    {
        ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false
    };

    private static Button MakeButton(string text, EventHandler click)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 32, Padding = new Padding(8, 2, 8, 2), Margin = new Padding(4) };
        b.Click += click;
        return b;
    }

    private static void AddField(Control parent, string label, Control input, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 5) });
        input.Location = new Point(170, y);
        parent.Controls.Add(input);
        y += input.Height + 16;
    }

    private void LoadProfileToForm()
    {
        _name.Text = _data.Profile.Name;
        _email.Text = _data.Profile.Email;
        _bio.Text = _data.Profile.Bio;
        _skills.Text = _data.Profile.Skills;
        _portfolioUrl.Text = _data.Profile.PortfolioUrl;
    }

    private void SaveProfile()
    {
        _data.Profile.Name = _name.Text.Trim();
        _data.Profile.Email = _email.Text.Trim();
        _data.Profile.Bio = _bio.Text.Trim();
        _data.Profile.Skills = _skills.Text.Trim();
        _data.Profile.PortfolioUrl = _portfolioUrl.Text.Trim();
        SaveAndRefresh();
        SetStatus("Developer profile saved.");
    }

    private void AddProject()
    {
        using var dialog = new ProjectDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _data.Projects.Add(dialog.Project);
        SaveAndRefresh();
        SetStatus("Project added.");
    }

    private ProjectItem? SelectedProject()
    {
        if (_projectsGrid.CurrentRow?.DataBoundItem is ProjectItem p) return p;
        var title = _projectsGrid.CurrentRow?.Cells["Title"]?.Value?.ToString();
        return _data.Projects.FirstOrDefault(x => x.Title == title);
    }

    private void EditSelectedProject()
    {
        var p = SelectedProject();
        if (p == null) { MessageBox.Show(this, "Select a project first."); return; }
        using var dialog = new ProjectDialog(p);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            p.Title = dialog.Project.Title; p.Description = dialog.Project.Description;
            p.Language = dialog.Project.Language; p.Tags = dialog.Project.Tags;
            p.FolderPath = dialog.Project.FolderPath; p.EntryFile = dialog.Project.EntryFile;
            SaveAndRefresh();
            SetStatus("Project updated.");
        }
    }

    private void DeleteSelectedProject()
    {
        var p = SelectedProject();
        if (p == null) { MessageBox.Show(this, "Select a project first."); return; }
        if (MessageBox.Show(this, $"Delete project '{p.Title}'? Review and run history records will remain.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _data.Projects.Remove(p);
        SaveAndRefresh();
    }

    private void RunSelectedProject()
    {
        var p = SelectedProject();
        if (p == null) { MessageBox.Show(this, "Select a project first."); return; }
        string target = p.EntryFile;
        if (string.IsNullOrWhiteSpace(target) && !string.IsNullOrWhiteSpace(p.FolderPath) && Directory.Exists(p.FolderPath))
        {
            using var ofd = new OpenFileDialog { Title = "Select the file to run", InitialDirectory = p.FolderPath, Filter = "Runnable files|*.exe;*.bat;*.cmd;*.py;*.js;*.ps1;*.vbs|All files|*.*" };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            target = ofd.FileName;
        }
        if (string.IsNullOrWhiteSpace(target)) { MessageBox.Show(this, "Edit this project and provide an entry file, or choose a file when prompted."); return; }
        if (!Path.IsPathRooted(target) && !string.IsNullOrWhiteSpace(p.FolderPath)) target = Path.Combine(p.FolderPath, target);
        if (!File.Exists(target)) { MessageBox.Show(this, "Entry file not found:\n" + target); return; }

        var started = DateTime.Now;
        var sw = Stopwatch.StartNew();
        string result = "Started";
        try
        {
            var ext = Path.GetExtension(target).ToLowerInvariant();
            var psi = ext switch
            {
                ".py" => new ProcessStartInfo("python", Quote(target)) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                ".js" => new ProcessStartInfo("node", Quote(target)) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                ".ps1" => new ProcessStartInfo("powershell", "-NoProfile -ExecutionPolicy Bypass -File " + Quote(target)) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                ".bat" or ".cmd" => new ProcessStartInfo("cmd.exe", "/c " + Quote(target)) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                ".vbs" => new ProcessStartInfo("wscript.exe", Quote(target)) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                ".exe" => new ProcessStartInfo(target) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(target)! },
                _ => throw new NotSupportedException("Supported entry files: .exe, .bat, .cmd, .py, .js, .ps1, .vbs. For other languages, build the project and select its executable.")
            };
            Process.Start(psi);
            result = "Launched";
            SetStatus("Launched " + Path.GetFileName(target) + ". The process may continue independently.");
        }
        catch (Exception ex)
        {
            result = "Failed: " + ex.Message;
            MessageBox.Show(this, ex.Message, "Could not run project", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            sw.Stop();
            _data.Runs.Add(new RunEntry { ProjectTitle = p.Title, Target = target, StartedAt = started, DurationSeconds = sw.Elapsed.TotalSeconds, Result = result });
            SaveAndRefresh();
        }
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";

    private void OpenSelectedFolder()
    {
        var p = SelectedProject();
        if (p == null) { MessageBox.Show(this, "Select a project first."); return; }
        if (!Directory.Exists(p.FolderPath)) { MessageBox.Show(this, "The saved project folder does not exist."); return; }
        Process.Start(new ProcessStartInfo("explorer.exe", Quote(p.FolderPath)) { UseShellExecute = true });
    }

    private void CountSelectedFiles()
    {
        var p = SelectedProject();
        if (p == null) { MessageBox.Show(this, "Select a project first."); return; }
        try
        {
            if (!Directory.Exists(p.FolderPath)) throw new DirectoryNotFoundException("Project folder does not exist.");
            int count = Directory.EnumerateFiles(p.FolderPath, "*", SearchOption.AllDirectories).Count();
            MessageBox.Show(this, $"Project: {p.Title}\nFiles found: {count}", "File count");
            SetStatus($"Counted {count} files in {p.Title}.");
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "File count error"); }
    }

    private void AddReview()
    {
        var projectNames = _data.Projects.Select(p => p.Title).Distinct().ToArray();
        if (projectNames.Length == 0) { MessageBox.Show(this, "Add a project before adding a review."); return; }
        using var d = new ReviewDialog(projectNames);
        if (d.ShowDialog(this) != DialogResult.OK) return;
        _data.Reviews.Add(d.Review);
        SaveAndRefresh();
    }

    private void DeleteSelectedReview()
    {
        var id = _reviewsGrid.CurrentRow?.Cells["Id"]?.Value;
        var review = _data.Reviews.FirstOrDefault(r => r.Id.ToString() == id?.ToString());
        if (review == null) { MessageBox.Show(this, "Select a review first."); return; }
        _data.Reviews.Remove(review);
        SaveAndRefresh();
    }

    private void ClearRunHistory()
    {
        if (MessageBox.Show(this, "Clear all run history?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        _data.Runs.Clear();
        SaveAndRefresh();
    }

    private void RefreshAll()
    {
        RefreshProjects();
        _reviewsGrid.DataSource = null;
        _reviewsGrid.DataSource = _data.Reviews.OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Id, r.ProjectTitle, r.Reviewer, r.Rating, r.Notes, Date = r.CreatedAt.ToString("yyyy-MM-dd HH:mm") }).ToList();
        if (_reviewsGrid.Columns["Id"] != null) _reviewsGrid.Columns["Id"].Visible = false;
        _runsGrid.DataSource = null;
        _runsGrid.DataSource = _data.Runs.OrderByDescending(r => r.StartedAt)
            .Select(r => new { r.ProjectTitle, r.Target, Date = r.StartedAt.ToString("yyyy-MM-dd HH:mm"), DurationSeconds = Math.Round(r.DurationSeconds, 3), r.Result }).ToList();
        RefreshDashboard();
    }

    private void RefreshProjects()
    {
        var selectedLanguage = _languageFilter.SelectedItem?.ToString() ?? "All languages";
        var search = _search.Text.Trim();
        var filtered = _data.Projects.Where(p =>
            (selectedLanguage == "All languages" || p.Language.Equals(selectedLanguage, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(search) || $"{p.Title} {p.Description} {p.Language} {p.Tags}".Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(p => p.Title).ToList();
        _projectsGrid.DataSource = null;
        _projectsGrid.DataSource = filtered.Select(p => new
        {
            p.Id, p.Title, p.Language, p.Tags, p.Description, p.FolderPath, p.EntryFile,
            Created = p.CreatedAt.ToString("yyyy-MM-dd")
        }).ToList();
        if (_projectsGrid.Columns["Id"] != null) _projectsGrid.Columns["Id"].Visible = false;
        if (_projectsGrid.Columns["FolderPath"] != null) _projectsGrid.Columns["FolderPath"].HeaderText = "Project folder";
        if (_projectsGrid.Columns["EntryFile"] != null) _projectsGrid.Columns["EntryFile"].HeaderText = "Entry file";
        var current = _languageFilter.SelectedItem?.ToString();
        _languageFilter.Items.Clear();
        _languageFilter.Items.Add("All languages");
        foreach (var lang in _data.Projects.Select(p => p.Language).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s))
            _languageFilter.Items.Add(lang);
        _languageFilter.SelectedItem = _languageFilter.Items.Cast<object>().FirstOrDefault(x => x.ToString() == current) ?? _languageFilter.Items[0];
    }

    private void RefreshDashboard()
    {
        int files = 0;
        foreach (var p in _data.Projects.Where(p => Directory.Exists(p.FolderPath)))
        {
            try { files += Directory.EnumerateFiles(p.FolderPath, "*", SearchOption.AllDirectories).Count(); } catch { }
        }
        double avg = _data.Runs.Count == 0 ? 0 : _data.Runs.Average(r => r.DurationSeconds);
        double rating = _data.Reviews.Count == 0 ? 0 : _data.Reviews.Average(r => r.Rating);
        _dashboardText.Text =
            $"Developer: {(_data.Profile.Name.Length == 0 ? "(profile not set)" : _data.Profile.Name)}\n\n" +
            $"Projects saved: {_data.Projects.Count}\n" +
            $"Languages used: {_data.Projects.Select(p => p.Language).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).Count()}\n" +
            $"Reviews recorded: {_data.Reviews.Count}\n" +
            $"Average review rating: {(rating == 0 ? "N/A" : rating.ToString("0.0") + " / 5")}\n" +
            $"Recorded launches: {_data.Runs.Count}\n" +
            $"Successful launches: {_data.Runs.Count(r => r.Result == "Launched")}\n" +
            $"Average launch setup time: {avg:0.000} seconds\n" +
            $"Files in accessible project folders: {files}\n\n" +
            $"Data location:\n{_storage.DataFolder}\n\n" +
            "Tip: Add a project folder and entry file in the Projects tab. Python and Node.js scripts require their runtime to be installed and available on PATH.";
    }

    private void SaveAndRefresh()
    {
        try { _storage.Save(_data); RefreshAll(); }
        catch (Exception ex) { MessageBox.Show(this, "Could not save data:\n" + ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void SaveQuietly() { try { _storage.Save(_data); } catch { } }
    private void SetStatus(string message) => _status.Text = message;

    private void ExportData()
    {
        using var sfd = new SaveFileDialog { Filter = "JSON data (*.json)|*.json", FileName = "developer-portfolio-data.json" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        try { _storage.Export(_data, sfd.FileName); SetStatus("Data exported."); MessageBox.Show(this, "Data exported successfully."); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export error"); }
    }

    private void ImportData()
    {
        using var ofd = new OpenFileDialog { Filter = "JSON data (*.json)|*.json", Title = "Import portfolio data" };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show(this, "Import replaces the currently saved profile, projects, reviews, and run history. Continue?", "Confirm import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { _data = _storage.Import(ofd.FileName); SaveAndRefresh(); LoadProfileToForm(); SetStatus("Data imported."); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ExportSummary()
    {
        using var sfd = new SaveFileDialog { Filter = "Text file (*.txt)|*.txt", FileName = "portfolio-summary.txt" };
        if (sfd.ShowDialog(this) != DialogResult.OK) return;
        var sb = new StringBuilder();
        sb.AppendLine("DEVELOPER PORTFOLIO SUMMARY");
        sb.AppendLine("===========================");
        sb.AppendLine($"Name: {_data.Profile.Name}");
        sb.AppendLine($"Email: {_data.Profile.Email}");
        sb.AppendLine($"Portfolio: {_data.Profile.PortfolioUrl}");
        sb.AppendLine($"Skills: {_data.Profile.Skills}");
        sb.AppendLine();
        sb.AppendLine("BIO");
        sb.AppendLine(_data.Profile.Bio);
        sb.AppendLine();
        sb.AppendLine("PROJECTS");
        foreach (var p in _data.Projects)
        {
            sb.AppendLine($"- {p.Title} [{p.Language}]");
            sb.AppendLine($"  Description: {p.Description}");
            sb.AppendLine($"  Tags: {p.Tags}");
            sb.AppendLine($"  Folder: {p.FolderPath}");
            sb.AppendLine($"  Entry file: {p.EntryFile}");
        }
        sb.AppendLine();
        sb.AppendLine("REVIEWS");
        foreach (var r in _data.Reviews) sb.AppendLine($"- {r.ProjectTitle}: {r.Rating}/5 by {r.Reviewer} — {r.Notes}");
        File.WriteAllText(sfd.FileName, sb.ToString());
        SetStatus("Portfolio summary exported.");
        MessageBox.Show(this, "Portfolio summary exported successfully.");
    }

    private void OpenDataFolder()
    {
        Process.Start(new ProcessStartInfo("explorer.exe", Quote(_storage.DataFolder)) { UseShellExecute = true });
    }
}

public sealed class ProjectDialog : Form
{
    private readonly TextBox _title = new() { Width = 430 };
    private readonly TextBox _description = new() { Width = 430, Height = 70, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _language = new() { Width = 430 };
    private readonly TextBox _tags = new() { Width = 430 };
    private readonly TextBox _folder = new() { Width = 350 };
    private readonly TextBox _entry = new() { Width = 350 };
    public ProjectItem Project { get; private set; }

    public ProjectDialog(ProjectItem? existing = null)
    {
        Project = existing == null ? new ProjectItem() : new ProjectItem
        {
            Id = existing.Id, Title = existing.Title, Description = existing.Description,
            Language = existing.Language, Tags = existing.Tags, FolderPath = existing.FolderPath,
            EntryFile = existing.EntryFile, CreatedAt = existing.CreatedAt
        };
        Text = existing == null ? "Add project" : "Edit project";
        Width = 650; Height = 480; FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent; MaximizeBox = false; MinimizeBox = false;
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        Controls.Add(panel);
        int y = 12;
        Add(panel, "Project title *", _title, ref y);
        Add(panel, "Description / notes", _description, ref y);
        Add(panel, "Programming language", _language, ref y);
        Add(panel, "Tags (comma-separated)", _tags, ref y);
        Add(panel, "Project folder", _folder, ref y);
        Add(panel, "Entry file (full path or relative path)", _entry, ref y);
        var browse = new Button { Text = "Browse...", Location = new Point(510, 330), Width = 90 };
        browse.Click += (_, _) =>
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog(this) == DialogResult.OK) _folder.Text = fbd.SelectedPath;
        };
        panel.Controls.Add(browse);
        var entryBrowse = new Button { Text = "Browse file...", Location = new Point(510, 365), Width = 90 };
        entryBrowse.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog { Filter = "Runnable files|*.exe;*.bat;*.cmd;*.py;*.js;*.ps1;*.vbs|All files|*.*" };
            if (ofd.ShowDialog(this) == DialogResult.OK) { _entry.Text = ofd.FileName; if (string.IsNullOrWhiteSpace(_folder.Text)) _folder.Text = Path.GetDirectoryName(ofd.FileName) ?? ""; }
        };
        panel.Controls.Add(entryBrowse);
        var ok = new Button { Text = "Save", DialogResult = DialogResult.OK, Location = new Point(390, 405), Width = 90 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(490, 405), Width = 90 };
        ok.Click += (_, e) =>
        {
            if (string.IsNullOrWhiteSpace(_title.Text))
            {
                MessageBox.Show(this, "Project title is required.");
                DialogResult = DialogResult.None;
                return;
            }
            Project.Title = _title.Text.Trim(); Project.Description = _description.Text.Trim();
            Project.Language = _language.Text.Trim(); Project.Tags = _tags.Text.Trim();
            Project.FolderPath = _folder.Text.Trim(); Project.EntryFile = _entry.Text.Trim();
        };
        panel.Controls.Add(ok); panel.Controls.Add(cancel);
        AcceptButton = ok; CancelButton = cancel;
        _title.Text = Project.Title; _description.Text = Project.Description; _language.Text = Project.Language;
        _tags.Text = Project.Tags; _folder.Text = Project.FolderPath; _entry.Text = Project.EntryFile;
    }

    private static void Add(Control parent, string label, Control input, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 4) });
        input.Location = new Point(200, y);
        parent.Controls.Add(input);
        y += input.Height + 12;
    }
}

public sealed class ReviewDialog : Form
{
    private readonly ComboBox _project = new() { Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _reviewer = new() { Width = 350 };
    private readonly NumericUpDown _rating = new() { Minimum = 1, Maximum = 5, Value = 5, Width = 100 };
    private readonly TextBox _notes = new() { Width = 350, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };
    public ReviewEntry Review { get; private set; } = new();

    public ReviewDialog(string[] projectNames)
    {
        Text = "Add review"; Width = 540; Height = 380; FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent; MaximizeBox = false; MinimizeBox = false;
        _project.Items.AddRange(projectNames);
        _project.SelectedIndex = 0;
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14) };
        Controls.Add(panel);
        int y = 12;
        Add(panel, "Project", _project, ref y);
        Add(panel, "Reviewer name", _reviewer, ref y);
        Add(panel, "Rating (1-5)", _rating, ref y);
        Add(panel, "Feedback notes", _notes, ref y);
        var ok = new Button { Text = "Save review", Location = new Point(300, 300), Width = 100, DialogResult = DialogResult.OK };
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_reviewer.Text) || string.IsNullOrWhiteSpace(_notes.Text))
            {
                MessageBox.Show(this, "Please enter the reviewer name and feedback notes.");
                DialogResult = DialogResult.None; return;
            }
            Review = new ReviewEntry { ProjectTitle = _project.Text, Reviewer = _reviewer.Text.Trim(), Rating = (int)_rating.Value, Notes = _notes.Text.Trim() };
        };
        var cancel = new Button { Text = "Cancel", Location = new Point(410, 300), Width = 80, DialogResult = DialogResult.Cancel };
        panel.Controls.Add(ok); panel.Controls.Add(cancel);
        AcceptButton = ok; CancelButton = cancel;
    }

    private static void Add(Control parent, string label, Control input, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, y + 4) });
        input.Location = new Point(150, y);
        parent.Controls.Add(input);
        y += input.Height + 12;
    }
}
