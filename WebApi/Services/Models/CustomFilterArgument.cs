using FFMpegCore.Arguments;

namespace WebApi.Services.Models;

public class CustomFilterArgument : IVideoFilterArgument
{
    public string Key { get; private set; }

    public string Value { get; private set; }

    public CustomFilterArgument(string key, string value)
    {
        Key = key;
        Value = value;
    }
}