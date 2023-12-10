using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace Domain.Attributes;

public class VideoFileValidationAttribute : ValidationAttribute
{
    public long MaxFileSize { get; set; }
    public string[] AcceptedContentTypes { get; set; }
    public VideoFileValidationAttribute(long maxFileSize, params string[] acceptedContentTypes)
    {
        MaxFileSize = maxFileSize;
        AcceptedContentTypes = acceptedContentTypes;
    }
    public override bool IsValid(object? value)
    {
        if (value is not IBrowserFile file) return false;
        if (file.Name.Length > 150) return false;
        if (file.Size > MaxFileSize) return false;
        if (AcceptedContentTypes.All(ct => ct.ToLower() != file.ContentType.ToLower())) return false;
        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return "Invalid file selected!";
    }
}