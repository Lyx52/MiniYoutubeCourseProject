﻿using Domain.Model.Query;
using Domain.Model.View;

namespace Domain.Model.Request;

public class VideoPlaylistRequest
{
    public Guid? PlaylistId { get; set; }
    public Guid? CreatorId { get; set; }
    public int From { get; set; }
    public int Count { get; set; }
    public bool OrderByNewest { get; set; }
    public bool OrderByPopularity { get; set; }
}