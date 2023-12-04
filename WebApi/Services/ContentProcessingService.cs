using System.Drawing;
using Domain.Constants;
using Domain.Model;
using FFMpegCore;
using WebApi.Services.Interfaces;
using FFMpegImage = FFMpegCore.Extensions.System.Drawing.Common.FFMpegImage;
namespace WebApi.Services;

public class ContentProcessingService : IContentProcessingService
{
    private readonly ILogger<ContentProcessingService> _logger;
    private readonly FFOptions _ffOptions;
    private readonly IWorkFileService _workFileService;
    public ContentProcessingService(ILogger<ContentProcessingService> logger, IWorkFileService workFileService)
    {
        _logger = logger;
        _workFileService = workFileService;
        // TODO: Move this to configuration
        _ffOptions = new FFOptions()
        {
            BinaryFolder = "D:\\ffmpeg\\bin",
            UseCache = true,
        };
    }
    public async Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
    {
        var analysis = await FFProbe.AnalyseAsync(stream, _ffOptions, cancellationToken);
        if (analysis.ErrorData.Count > 0) return false;
        
        // Minimum video length
        if (analysis.Duration.TotalSeconds < 10) return false;
        
        // Score that determines if mediaformat corresponds to extension
        if (analysis.Format.ProbeScore <= 80) return false;
        
        // Contains primary videostream
        if (analysis.PrimaryVideoStream is null || analysis.VideoStreams.Count != 1) return false;

        // Minimum resolution 640x360
        if (analysis.PrimaryVideoStream.Width < 640 && analysis.PrimaryVideoStream.Height < 360) return false;
        
        // Maximum resolution 1920x1080
        if (analysis.PrimaryVideoStream.Width > 1920 && analysis.PrimaryVideoStream.Height > 1080) return false;

        if (analysis.PrimaryVideoStream.AvgFrameRate is < 15 or > 60) return false;
        
        return true;
    }

    public async Task<string> GeneratePosterAsync(WorkSpace workSpace, Size posterSize, CancellationToken cancellationToken = default(CancellationToken))
    {
        var originalLocation = _workFileService.GetWorkFileLocation(workSpace, WorkFileType.Original);
        if (originalLocation is null)
            throw new ApplicationException($"WorkSpace {workSpace.Id} does not contain 'Original' WorkFile");
        var posterLocation = _workFileService.CreateWorkFile(workSpace, WorkFileType.Poster, ".png");
        var success = await FFMpeg.SnapshotAsync(originalLocation, posterLocation, posterSize, TimeSpan.FromSeconds(1));
        if (success) return posterLocation;
        throw new ApplicationException($"Failed to create Poster for {workSpace.Id} workspace");
    }
}