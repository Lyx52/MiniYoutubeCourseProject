using Domain.Constants;
using Domain.Model;
using Domain.Model.Configuration;
using WebApi.Services.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebApi.Services;

public class WorkFileService : IWorkFileService
{
    public readonly ILogger<WorkFileService> _logger;
    private readonly ApiConfiguration _configuration;
    public WorkFileService(ILogger<WorkFileService> logger, ApiConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        PreCreateDirectories();
    }

    private void PreCreateDirectories()
    {
        foreach (var value in Enum.GetValues<WorkSpaceDirectory>())
        {
            var directory = GetWorkDirectory(value);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    public WorkFile CreateWorkFile(WorkSpace workSpace, WorkFileType type, string extension, List<string>? tags = null)
    {
        var id = Guid.NewGuid();
        var directory = GetWorkSpaceDirectory(workSpace);
        return new WorkFile()
        {
            Type = type,
            Id = id,
            FileName = $"{id}{extension}",
            Tags = tags ?? new List<string>()
        };
    }
    
    public async Task SaveWorkSpaceAsync(WorkSpace workSpace)
    {
        var directory = GetWorkSpaceDirectory(workSpace);
        var metadataFile = Path.Join(directory, "metadata.json");
        await using var fs = File.Open(metadataFile, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(fs, workSpace);
        await fs.FlushAsync();
    }

    public async Task<WorkSpace> LoadWorkSpace(WorkSpaceDirectory directory, Guid id)
    {
        var workSpaceDirectory = GetWorkDirectory(directory);
        var metadataFile = Path.Join(workSpaceDirectory, id.ToString(), "metadata.json");
        await using var fs = File.Open(metadataFile, FileMode.Open);
        var workSpace = await JsonSerializer.DeserializeAsync<WorkSpace>(fs);
        if (workSpace is not null) return workSpace;
        throw new ApplicationException($"Cannot find workspace at {directory} with id {id}");
    }

    public async Task MoveWorkSpace(WorkSpaceDirectory from, WorkSpaceDirectory to, Guid id)
    {
        var fromDirectory = GetWorkDirectory(from);
        var toDirectory = GetWorkDirectory(to);
        var fromWorkspace = Path.Join(fromDirectory, id.ToString());
        var toWorkspace = Path.Join(toDirectory, id.ToString());
        if (!Directory.Exists(fromDirectory))
            throw new ApplicationException($"Cannot find workspace at {from} with id {id}");
        
        Directory.Move(fromWorkspace, toWorkspace);
        var workSpace = await LoadWorkSpace(to, id);
        workSpace.Directory = to;
        await SaveWorkSpaceAsync(workSpace);
    }

    public async Task RemoveWorkSpace(WorkSpaceDirectory directory, Guid workSpaceId)
    {
        var workSpace = await LoadWorkSpace(directory, workSpaceId);
        var workSpaceDirectory = GetWorkSpaceDirectory(workSpace);
        Directory.Delete(workSpaceDirectory, true);
    }

    public WorkSpace CreateWorkSpace(WorkSpaceDirectory directory)
    {
        var id = Guid.NewGuid();
        var workspaceDirectory = GetWorkDirectory(directory);
        var workspace = Path.Join(workspaceDirectory, id.ToString());
        if (!Directory.Exists(workspace))
        {
            Directory.CreateDirectory(workspace);
        }

        return new WorkSpace()
        {
            Directory = directory,
            Files = new List<WorkFile>(),
            Id = id
        };
    }

    public string GetWorkSpaceDirectory(WorkSpace workSpace)
    {
        var workspaceDirectory = GetWorkDirectory(workSpace.Directory);
        return Path.Join(workspaceDirectory, workSpace.Id.ToString());
    }
    public string GetWorkFileLocation(WorkSpace workSpace, WorkFile file)
    {
        var directory = GetWorkSpaceDirectory(workSpace);
        return Path.Join(directory, file.FileName);
    }
    public List<string> GetWorkFiles(WorkSpace workSpace, WorkFileType type)
    {
        var directory = GetWorkSpaceDirectory(workSpace);
        return workSpace.Files
            .Where((f) => f.Type == type)
            .Select((f) => Path.Join(directory, f.FileName))
            .ToList();
    }

    private string GetWorkDirectory(WorkSpaceDirectory directory)
    {
        switch (directory)
        {
            case WorkSpaceDirectory.WorkDir: return Path.Join(Directory.GetCurrentDirectory(), _configuration.Processing.WorkDirectory);
            case WorkSpaceDirectory.TempDir: return Path.Join(Directory.GetCurrentDirectory(), _configuration.Processing.TempDirectory);
            case WorkSpaceDirectory.RepoDir: return Path.Join(Directory.GetCurrentDirectory(), _configuration.Processing.RepoDirectory);
        }

        throw new ArgumentException(nameof(directory));
    }

}