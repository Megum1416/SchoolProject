using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject_DB.Models;

public partial class Post
{
    [Display(Name = "文章 ID")]
    public string PostID { get; set; } = null!;

    [Display(Name = "會員 ID")]
    public string MemberID { get; set; } = null!;

    [Display(Name = "文章標題")]
    public string PostTitle { get; set; } = null!;

    [Display(Name = "內文")]
    public string Description { get; set; } = null!;

    [Display(Name = "建立日期")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "照片")]
    public byte[]? Photos { get; set; }

    public string? ImageType { get; set; }

    public string? FollowID { get; set; }

    public virtual FollowList? Follow { get; set; }

    public virtual Members Member { get; set; } = null!;

    public virtual ICollection<RePost> RePost { get; set; } = new List<RePost>();
}
