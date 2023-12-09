namespace Domain.Constants;

public static class VideoIdEncoder
{
    public static string ToVideoUrlId(this Guid id)
    {
        try
        {
            byte[] bytes = id.ToByteArray();
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
        catch (Exception _)
        {
            return string.Empty;
        }
    }

    public static Guid ToVideoId(this string? id)
    {
        try
        {
            int padding = (4 - id!.Length % 4) % 4;
            id = id
                .PadRight(id.Length + padding, '=')
                .Replace("-", "+")
                .Replace("_", "/");

            byte[] bytes = Convert.FromBase64String(id);
            return new Guid(bytes);
        }
        catch (Exception _)
        {
            return Guid.Empty;
        }
    }
}