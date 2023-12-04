using Domain.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Attributes;

public class FileExtensionValidationAttribute : ActionFilterAttribute
{
    private readonly string[] _validExtensions;
    public FileExtensionValidationAttribute(params string[] extensions) : base()
    {
        _validExtensions = extensions;
    }
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        foreach (var file in filterContext.HttpContext.Request.Form.Files)
        {
            if (!_validExtensions.Contains(Path.GetExtension(file.FileName.ToLower())))
            {
                filterContext.Result = new BadRequestObjectResult(new Response()
                {
                    Success = false,
                    Message = $"Invalid file extension for file '{file.FileName}'"
                });
            }
        }
    }
}