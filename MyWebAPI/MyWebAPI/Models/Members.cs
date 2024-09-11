using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyWebAPI.Models;

public partial class Members
{
    public string MemberID { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string LodestoneID { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public string DataCenter { get; set; } = null!;

    public string ServerName { get; set; } = null!;

    
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? Photos { get; set; }

    public string? ImageType { get; set; }

    public bool? IsAdmin { get; set; }

    public virtual ICollection<FollowList> FollowList { get; set; } = new List<FollowList>();

    public virtual ICollection<Post> Post { get; set; } = new List<Post>();
}
