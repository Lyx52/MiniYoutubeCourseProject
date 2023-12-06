using System.Drawing;
using Domain.Model;
using FFMpegCore.Enums;
using WebApi.Services.Models;

namespace WebApi.Services.Interfaces;

public interface IContentProcessingService
{
    Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    Task ProcessVideo(VideoTask payload, CancellationToken cancellationToken = default(CancellationToken));
    Task PublishVideo(VideoTask payload, CancellationToken cancellationToken = default(CancellationToken));
}