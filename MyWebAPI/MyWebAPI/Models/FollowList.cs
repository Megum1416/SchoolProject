using System;
using System.Collections.Generic;

namespace MyWebAPI.Models;

public partial class FollowList
{
    public string FollowID { get; set; } = null!;

    public string MemberID { get; set; } = null!;

    public string LodestoneID { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Members Member { get; set; } = null!;

    public virtual ICollection<Post> Post { get; set; } = new List<Post>();
}
