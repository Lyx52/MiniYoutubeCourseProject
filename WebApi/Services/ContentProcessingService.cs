using System.Drawing;
using Domain.Constants;
using Domain.Model;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using WebApi.Services.Interfaces;
using WebApi.Services.Models;

namespace WebApi.Services;

public class ContentProcessingService : IContentProcessingService
{
    private readonly ILogger<ContentProcessingService> _logger;
    private readonly FFOptions _ffOptions;
    private readonly IWorkFileService _workFileService;
    private readonly IVideoRepository _videoRepository;
    public ContentProcessingService(ILogger<ContentProcessingService> logger, IWorkFileService workFileService, IVideoRepository videoRepository)
    {
        _logger = logger;
        _workFileService = workFileService;
        _videoRepository = videoRepository;
        // TODO: Move this to configuration
        _ffOptions = new FFOptions
        {
            BinaryFolder = "C:\\ProgramData\\chocolatey\\bin",
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
    public async Task<string> GeneratePosterAsync(WorkSpace workSpace, Stream stream, Size posterSize, CancellationToken cancellationToken = default(CancellationToken))
    {
        var posterLocation = _workFileService.CreateWorkFile(workSpace, WorkFileType.Poster, ".jpg");
        try
        {
            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(stream))
                .OutputToFile(posterLocation, false, options =>
                {
                    options
                        .Seek(TimeSpan.FromSeconds(3))
                        .WithCustomArgument("-qscale:v 2")
                        .WithFrameOutputCount(1)
                        .WithFastStart();
                })
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously(true, _ffOptions);
        }
        catch (IOException _)
        {
            // TODO: For some reason when generating jpgs from pipes it processes the pipe but still fails afterwards
        }
        if (File.Exists(posterLocation)) return posterLocation;
        throw new ApplicationException($"Failed to create poster image for WorkSpace {workSpace.Id}");
    }

    public async Task<string> GeneratePosterGifAsync(WorkSpace workSpace, Stream stream, Size gifSize,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var gifLocation = _workFileService.CreateWorkFile(workSpace, WorkFileType.PosterGif, ".gif");
        var success = await FFMpegArguments
            .FromPipeInput(new StreamPipeSource(stream))
            .OutputToFile(gifLocation, false, options =>
            {
                options
                    .Seek(TimeSpan.FromSeconds(0))
                    .WithDuration(TimeSpan.FromSeconds(10))
                    .WithGifPaletteArgument(0, gifSize)
                    .UsingMultithreading(true);
            })
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(true, _ffOptions);
        if (success) return gifLocation;
        throw new ApplicationException($"Failed to create PosterGif for {workSpace.Id} workspace");
    }
    public async Task<string> EncodeVideoAsync(WorkSpace workSpace, Stream stream, VideoSize size, CancellationToken cancellationToken = default(CancellationToken))
    {
        var fileLocation = _workFileService.CreateWorkFile(workSpace, WorkFileType.EncodedSource, ".mp4");
        
        var success = await FFMpegArguments
            .FromPipeInput(new StreamPipeSource(stream))
            .OutputToFile(fileLocation, false, options =>
            {
                options
                    .Seek(TimeSpan.Zero)
                    .UsingMultithreading(true)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(24)
                    .ForceFormat("mp4")
                    .WithVideoFilters(filters =>
                    {
                        filters.Scale(size);
                        filters.Arguments.Add(new CustomFilterArgument("pad", "ceil(iw/2)*2:ceil(ih/2)*2"));
                    })
                    .WithFastStart();
            })
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously(true, _ffOptions);
        if (success) return fileLocation;
        throw new ApplicationException($"Failed to create EncodeSource for {workSpace.Id} workspace");
    }
}