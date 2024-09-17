using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject_DB.Models;

public partial class FollowList
{
    [Display(Name = "追蹤編號")]
    public string FollowID { get; set; } = null!;

    [Display(Name = "會員編號")]
    public string MemberID { get; set; } = null!;

    [Display(Name = "追蹤的LodestoneID")]
    public string LodestoneID { get; set; } = null!;

    [Display(Name = "追蹤時間")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "取消追蹤時間")]
    public DateTime? UpdatedAt { get; set; }

    public virtual Members Member { get; set; } = null!;
    
}
