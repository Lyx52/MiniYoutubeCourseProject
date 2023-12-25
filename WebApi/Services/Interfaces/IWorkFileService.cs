using Domain.Constants;
using Domain.Model;

namespace WebApi.Services.Interfaces;

public interface IWorkFileService
{
    string GetWorkFileLocation(WorkSpace workSpace, WorkFile file);
    List<string> GetWorkFiles(WorkSpace workSpace, WorkFileType type);
    WorkSpace CreateWorkSpace(WorkSpaceDirectory directory);
    string GetWorkSpaceDirectory(WorkSpace workSpace);
    WorkFile CreateWorkFile(WorkSpace workSpace, WorkFileType type, string extension, List<string>? tags = null);
    Task SaveWorkSpaceAsync(WorkSpace workSpace);
    Task<WorkSpace> LoadWorkSpace(WorkSpaceDirectory directory, Guid id);
    Task MoveWorkSpace(WorkSpaceDirectory from, WorkSpaceDirectory to, Guid id);
    Task RemoveWorkSpace(WorkSpaceDirectory directory, Guid workSpaceId);
}