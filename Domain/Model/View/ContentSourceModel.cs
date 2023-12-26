using System.Text.Json.Serialization;
using Domain.Constants;
using Domain.Entity;

namespace Domain.Model.View;

public class ContentSourceModel
{
    public string Id { get; set; }
    public string ContentType { get; set; }
    public string Resolution { get; set; }
    public ContentSourceType Type { get; set; }

    [JsonConstructor]
    public ContentSourceModel(string id, string contentType, string resolution)
    {
        Id = id;
        ContentType = contentType;
        Resolution = resolution;
    }
    public ContentSourceModel(ContentSource source)
    {
        Id = source.Id;
        ContentType = source.ContentType;
        Resolution = source.Resolution;
        Type = source.Type;
    }
}