namespace DeveloperPortfolioShowcase;

public sealed class AppData
{
    public DeveloperProfile Profile { get; set; } = new();
    public List<ProjectItem> Projects { get; set; } = new();
    public List<ReviewEntry> Reviews { get; set; } = new();
    public List<RunEntry> Runs { get; set; } = new();
}

public sealed class DeveloperProfile
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Skills { get; set; } = "";
    public string PortfolioUrl { get; set; } = "";
}

public sealed class ProjectItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Language { get; set; } = "";
    public string Tags { get; set; } = "";
    public string FolderPath { get; set; } = "";
    public string EntryFile { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public override string ToString() => Title;
}

public sealed class ReviewEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProjectTitle { get; set; } = "";
    public string Reviewer { get; set; } = "";
    public int Rating { get; set; } = 5;
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class RunEntry
{
    public string ProjectTitle { get; set; } = "";
    public string Target { get; set; } = "";
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public double DurationSeconds { get; set; }
    public string Result { get; set; } = "";
}
