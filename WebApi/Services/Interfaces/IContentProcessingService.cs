using System.Drawing;
using Domain.Model;
using FFMpegCore.Enums;
using WebApi.Services.Models;

namespace WebApi.Services.Interfaces;

public interface IContentProcessingService
{
    Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    Task<WorkFile> GeneratePosterAsync(WorkSpace space, Stream stream, Size posterSize, CancellationToken cancellationToken = default(CancellationToken));
    Task<WorkFile> GeneratePosterGifAsync(WorkSpace workSpace, Stream stream, Size gifSize, CancellationToken cancellationToken = default(CancellationToken));
    Task<WorkFile> EncodeVideoAsync(WorkSpace workSpace, Stream stream, VideoSize size, CancellationToken cancellationToken = default(CancellationToken));
    Task ProcessVideo(ProcessVideoTask payload, CancellationToken cancellationToken = default(CancellationToken));
}