using System.Drawing;
using Domain.Constants;
using Domain.Model.Configuration;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;
using Microsoft.AspNetCore.WebUtilities;
using WebApi.Services.Interfaces;

namespace WebApi.Services;

public class ContentService : IContentService
{
    private readonly ILogger<ContentService> _logger;
    private readonly IContentProcessingService _processingService;
    private readonly IWorkFileService _workFileService;
    public ContentService(ILogger<ContentService> logger, IContentProcessingService processingService, IWorkFileService workFileService)
    {
        _logger = logger;
        _processingService = processingService;
        _workFileService = workFileService;
    }

    public async Task<Guid?> SaveTemporaryFile(MemoryStream readStream, string fileName, CancellationToken cancellationToken = default(CancellationToken))
    {
        readStream.Seek(0, SeekOrigin.Begin);
        if (await _processingService.IsValidVideoFileAsync(readStream, cancellationToken))
        {
            try
            {
                var workSpace = _workFileService.CreateWorkSpace(WorkSpaceDirectory.TempDir);
                var file = _workFileService.CreateWorkFile(workSpace, WorkFileType.Original, Path.GetExtension(fileName));
                await using var fileStream = File.Create(file);
                readStream.Seek(0, SeekOrigin.Begin);
                await readStream.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
                
                fileStream.Close();
                await _processingService.GeneratePosterGifAsync(workSpace, new Size(1920, 1080), cancellationToken);
                await using (var fs = File.OpenRead(file))
                {
                    byte[] data = new byte[fs.Length];
                    var len = await fs.ReadAsync(data, cancellationToken);
                    MemoryStream ms1 = new MemoryStream(data, false);
                    ms1.Seek(0, SeekOrigin.Begin);
                    MemoryStream ms2 = new MemoryStream(data, false);
                    ms2.Seek(0, SeekOrigin.Begin);
                    MemoryStream ms3 = new MemoryStream(data, false);
                    ms3.Seek(0, SeekOrigin.Begin);
                    await Task.WhenAll(
                        _processingService.EncodeVideoAsync(workSpace, ms1, VideoSize.Original,
                            cancellationToken),
                        _processingService.GeneratePosterAsync(workSpace, ms2, new Size(1920, 1080), 
                            cancellationToken),
                        _processingService.GeneratePosterGifAsync(workSpace, ms3, new Size(480, -1), 
                            cancellationToken)
                    );
                }
                // 
                await _workFileService.SaveWorkSpaceAsync(workSpace);
                return workSpace.Id;
            }
            catch (IOException e)
            {
                _logger.LogError("Failed to save temporary file: {ExceptionMessage}", e.Message);
            }
        }

        return null;
    }
}