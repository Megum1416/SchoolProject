using System;
using System.Collections.Generic;

namespace MyWebAPI.Models;

public partial class RePost
{
    public string RePostID { get; set; } = null!;

    public string PostID { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
