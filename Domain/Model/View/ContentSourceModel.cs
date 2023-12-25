using Domain.Constants;
using Domain.Entity;

namespace Domain.Model.View;

public class ContentSourceModel(ContentSource source)
{
    public string Id { get; init; } = source.Id;
    public string ContentType { get; init; } = source.ContentType;
    public string Resolution { get; init; } = source.Resolution;
    public ContentSourceType Type { get; init; } = source.Type;
}