using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject_DB.Models;

public partial class Members
{
    [Key]
    public string MemberID { get; set; } = null!;


    [Required(ErrorMessage = "必填！")]
    [Display(Name = "電子郵件/帳號")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    public string Email { get; set; } = null!;

    [Display(Name = "密碼")]
    [Required(ErrorMessage = "請輸入正確密碼")]
    [StringLength(12, MinimumLength = 8, ErrorMessage = "密碼長度必須在8到12碼之間")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "必填！")]
    [StringLength(30, ErrorMessage = "最多只可輸入30字")]
    [Display(Name = "暱稱")]
    public string UserName { get; set; } = null!;

    [Display(Name = "Lodestone")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Lodestone ID 必須是 8 位數字")]
    public string LodestoneID { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public string DataCenter { get; set; } = null!;

    public string ServerName { get; set; } = null!;

    [HiddenInput]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
    [Display(Name = "加入時間")]
    public DateTime CreatedAt { get; set; }

    [HiddenInput]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm}")]
    [Display(Name = "最後更新時間")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "上傳圖片")]
    public byte[]? Photos { get; set; }

    [HiddenInput]
    public string? ImageType { get; set; }

    // 新增的 IsAdmin 布林值欄位
    [Display(Name = "是否為管理員")]
    public bool IsAdmin { get; set; } = false; // 默認值為 false

    public virtual ICollection<FollowList> FollowList { get; set; } = new List<FollowList>();

    public virtual ICollection<Post> Post { get; set; } = new List<Post>();
}
