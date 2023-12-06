using System.Drawing;
using Domain.Constants;
using Domain.Model;
using Domain.Model.Configuration;
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
    private readonly ApiConfiguration _configuration;
    public ContentProcessingService(
        ILogger<ContentProcessingService> logger, 
        IWorkFileService workFileService,
        IVideoRepository videoRepository,
        ApiConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
        _workFileService = workFileService;
        _videoRepository = videoRepository;
        //_videoRepository = videoRepository;
        // TODO: Move this to configuration
        _ffOptions = new FFOptions
        {
            BinaryFolder = _configuration.Processing.FFMpegLocation,
            UseCache = true,
            WorkingDirectory = _configuration.Processing.FFMpegLocation
        };
        GlobalFFOptions.Configure(_ffOptions);
    }

    private bool IsValidVideoFile(IMediaAnalysis analysis)
    {
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
    public async Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
    {
        var analysis = await FFProbe.AnalyseAsync(stream, _ffOptions, cancellationToken);
        return IsValidVideoFile(analysis);
    }
    public async Task<WorkFile> GeneratePosterAsync(WorkSpace workSpace, Stream stream, Size posterSize, CancellationToken cancellationToken = default(CancellationToken))
    {
        var workFile = _workFileService.CreateWorkFile(workSpace, WorkFileType.Poster, ".jpg", new ()
        {
            $"Resolution={posterSize.Width}x{posterSize.Height}", 
            "ContentType=image/jpg"
        });
        var posterLocation = _workFileService.GetWorkFileLocation(workSpace, workFile);
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
        if (File.Exists(posterLocation)) return workFile;
        throw new ApplicationException($"Failed to create poster image for WorkSpace {workSpace.Id}");
    }

    public async Task<WorkFile> GeneratePosterGifAsync(WorkSpace workSpace, Stream stream, Size gifSize,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var workFile = _workFileService.CreateWorkFile(workSpace, WorkFileType.PosterGif, ".gif", new ()
        {
            $"Resolution={gifSize.Width}x{gifSize.Height}", 
            "ContentType=image/gif"
        });
        var gifLocation = _workFileService.GetWorkFileLocation(workSpace, workFile);
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
        if (success) return workFile;
        throw new ApplicationException($"Failed to create PosterGif for {workSpace.Id} workspace");
    }
    public async Task<WorkFile> EncodeVideoAsync(WorkSpace workSpace, Stream stream, VideoSize size, CancellationToken cancellationToken = default(CancellationToken))
    {
        var workFile = _workFileService.CreateWorkFile(workSpace, WorkFileType.EncodedSource, ".mp4", new ()
        {
            $"Resolution={(int)size}",
            "ContentType=video/mp4"
        });
        var fileLocation = _workFileService.GetWorkFileLocation(workSpace, workFile);
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
        if (success) return workFile;
        throw new ApplicationException($"Failed to create EncodeSource for {workSpace.Id} workspace");
    }
    private async Task<WorkFile> CreateVideoAttachment(byte[] data, WorkSpace workSpace, WorkFileType type, Size? size = null, VideoSize? videoSize = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        WorkFile? workFile = null;
        await using (var stream = new MemoryStream(data, false))
        {
            stream.Seek(0, SeekOrigin.Begin);
            switch (type)
            {
                case WorkFileType.Poster:
                {
                    workFile = await GeneratePosterAsync(workSpace, stream, size!.Value, cancellationToken);
                } break;
                case WorkFileType.PosterGif:
                {
                    workFile = await GeneratePosterGifAsync(workSpace, stream, size!.Value, cancellationToken);
                } break;
                case WorkFileType.EncodedSource:
                {
                    workFile = await EncodeVideoAsync(workSpace, stream, videoSize!.Value, cancellationToken);
                } break;
            }   
        }

        if (workFile is null) throw new ApplicationException($"Failed to create video attachment {type} for workspace {workSpace.Id}");
        return workFile;
    }

    private List<Task<WorkFile>> GenerateProcessingTasks(byte[] data, WorkSpace workSpace, IMediaAnalysis originalAnalysis, CancellationToken cancellationToken = default(CancellationToken))
    {
        var tasks = new List<Task<WorkFile>>();
        int originalWidth = originalAnalysis.PrimaryVideoStream!.Width;
        foreach (var vs in Enum.GetValues<VideoSize>())
        {
            if ((int)vs <= originalWidth && (int)vs > 0)
            {
                tasks.Add(CreateVideoAttachment(data, workSpace, WorkFileType.EncodedSource, null, vs, cancellationToken));
            }
        }
        tasks.Add(CreateVideoAttachment(data, workSpace, WorkFileType.Poster, new Size(480, -1), null, cancellationToken));
        tasks.Add(CreateVideoAttachment(data, workSpace, WorkFileType.PosterGif, new Size(480, -1), null, cancellationToken));
        return tasks;
    }

    public async Task PublishVideo(VideoTask payload, CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var workSpace = await _workFileService.LoadWorkSpace(WorkSpaceDirectory.WorkDir, payload.WorkSpaceId);
            if (workSpace.Files.Count <= 0)
                throw new ApplicationException($"Trying to publish an empty workspace {workSpace.Id}");
            await _videoRepository.EnrichWithSources(payload.VideoId, workSpace.Files, cancellationToken);
            
            await _workFileService.MoveWorkSpace(WorkSpaceDirectory.WorkDir, WorkSpaceDirectory.RepoDir, payload.WorkSpaceId);
            await _videoRepository.UpdateVideoStatus(payload.VideoId, VideoProcessingStatus.Published, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception while processing task {Exception}, {StackTrace}", e.Message, e.StackTrace);
            await _videoRepository.UpdateVideoStatus(payload.VideoId, VideoProcessingStatus.ProcessingFailed, cancellationToken);
        }
    }
    
    public async Task ProcessVideo(VideoTask payload, CancellationToken cancellationToken = default(CancellationToken))
    {
         try
         {
             await _videoRepository.UpdateVideoStatus(payload.VideoId, VideoProcessingStatus.Processing, cancellationToken);
             await _workFileService.MoveWorkSpace(WorkSpaceDirectory.TempDir, WorkSpaceDirectory.WorkDir, payload.WorkSpaceId); 
             var workSpace = await _workFileService.LoadWorkSpace(WorkSpaceDirectory.WorkDir, payload.WorkSpaceId);
             var files = _workFileService.GetWorkFiles(workSpace, WorkFileType.Original);
             if (files.Count <= 0) throw new IOException($"WorkSpace {payload.WorkSpaceId} does not contain any 'Original' files");
             byte[] data;
             await using (var fs = File.OpenRead(files[0]))
             {
                 data = new byte[fs.Length];
                 var len = await fs.ReadAsync(data, cancellationToken);
                 if (len != data.Length) throw new IOException("Unexpected amount of bytes while trying to read the Original video!");
             }

             IMediaAnalysis originalAnalysis;
             await using (var ms = new MemoryStream(data, false))
             {
                 ms.Seek(0, SeekOrigin.Begin);
                 originalAnalysis = await FFProbe.AnalyseAsync(ms, _ffOptions, cancellationToken);
             }
            
             if (!IsValidVideoFile(originalAnalysis))
                 throw new ApplicationException("Trying to process invalid Original video file!");
            
             var workFiles = await Task.WhenAll(GenerateProcessingTasks(data, workSpace, originalAnalysis));
             workSpace.Files.AddRange(workFiles);
             await _workFileService.SaveWorkSpaceAsync(workSpace);
         }
         catch (Exception e)
         {
             _logger.LogError("Exception while processing task {Exception}, {StackTrace}", e.Message, e.StackTrace);
             await _videoRepository.UpdateVideoStatus(payload.VideoId, VideoProcessingStatus.ProcessingFailed, cancellationToken);
         }
    }
}