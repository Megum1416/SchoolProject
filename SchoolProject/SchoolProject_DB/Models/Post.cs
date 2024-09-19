using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject_DB.Models;

public partial class Post
{
    [Key]
    [Required]
    [Display(Name = "文章 ID")]
    public string PostID { get; set; } = null!;


    [Required]
    [Display(Name = "會員 ID")]
    public string MemberID { get; set; } = null!; // 確保非空並與 Members 表關聯


    [Required(ErrorMessage = "必填！")] //在表單上會自動加上驗證器
    [StringLength(30, ErrorMessage = "最多只可輸入30字")]
    [Display(Name = "標題")]
    public string PostTitle { get; set; } = null!;

    [Required(ErrorMessage = "必填！")]
    [StringLength(250, ErrorMessage = "最多只可輸入250字")]
    [DataType(DataType.MultilineText)] //多行文字方塊，讓內容可顯示多行內容(預設單行)
    [Display(Name = "內文")]
    public string Description { get; set; } = null!;

    [HiddenInput]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm}")]
    [Display(Name = "建立日期")]
    public DateTime CreatedAt { get; set; }

    [HiddenInput]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm}")]
    [Display(Name = "更新日期")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "上傳圖片")]
    public byte[]? Photos { get; set; }

    public string? ImageType { get; set; }

    public virtual Members? Member { get; set; } = null!;

    public virtual ICollection<RePost>? RePost { get; set; } = new List<RePost>();
}
