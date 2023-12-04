using Domain.Constants;
using Domain.Model;

namespace WebApi.Services.Interfaces;

public interface IWorkFileService
{
    string? GetWorkFileLocation(WorkSpace workSpace, WorkFileType type);
    Stream GetWorkFileReadStream(WorkSpace workSpace, WorkFileType type);
    WorkSpace CreateWorkSpace(WorkSpaceDirectory directory);
    string GetWorkSpaceDirectory(WorkSpace workSpace);
    string CreateWorkFile(WorkSpace workSpace, WorkFileType type, string extension);
    Task SaveWorkSpace(WorkSpace workSpace);
    Task<WorkSpace> LoadWorkSpace(WorkSpaceDirectory directory, Guid id);
}