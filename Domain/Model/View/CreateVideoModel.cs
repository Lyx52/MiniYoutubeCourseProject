﻿namespace Domain.Model;

public class CreateVideoModel
{
    public string Title { get; set; }
    public Guid WorkSpaceId { get; set; }
}