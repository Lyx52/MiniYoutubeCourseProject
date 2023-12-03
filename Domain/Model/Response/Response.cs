using Microsoft.AspNetCore.Mvc;

namespace Domain.Model.Response;

public class Response
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}