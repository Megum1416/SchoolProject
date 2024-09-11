using System;
using System.Collections.Generic;

namespace MyWebAPI.Models;

public partial class Post
{
    public string PostID { get; set; } = null!;

    public string MemberID { get; set; } = null!;

    public string PostTitle { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public byte[]? Photos { get; set; }

    public string? ImageType { get; set; }

    public string? FollowID { get; set; }

    public virtual FollowList? Follow { get; set; }

    public virtual Members Member { get; set; } = null!;

    public virtual ICollection<RePost> RePost { get; set; } = new List<RePost>();
}
