using System.Drawing;
using Domain.Model;
using FFMpegCore.Enums;

namespace WebApi.Services.Interfaces;

public interface IContentProcessingService
{
    Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    Task<string> GeneratePosterAsync(WorkSpace space, Stream stream, Size posterSize, CancellationToken cancellationToken = default(CancellationToken));

    Task<string> GeneratePosterGifAsync(WorkSpace workSpace, Size gifSize, CancellationToken cancellationToken = default(CancellationToken));
    Task<string> GeneratePosterGifAsync(WorkSpace workSpace, Stream stream, Size gifSize, CancellationToken cancellationToken = default(CancellationToken));

    Task<string> EncodeVideoAsync(WorkSpace workSpace, Stream stream, VideoSize size, CancellationToken cancellationToken = default(CancellationToken));
}