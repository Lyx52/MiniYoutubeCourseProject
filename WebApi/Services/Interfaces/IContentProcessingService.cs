using System.Drawing;
using Domain.Model;
using FFMpegCore;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApi.Services.Interfaces;

public interface IContentProcessingService
{
    Task<bool> IsValidVideoFileAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    Task<string> GeneratePosterAsync(WorkSpace space, Size posterSize, CancellationToken cancellationToken = default(CancellationToken));
}