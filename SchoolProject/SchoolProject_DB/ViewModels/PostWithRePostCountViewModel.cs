using SchoolProject_DB.Models;

namespace SchoolProject_DB.ViewModels
{
    public class PostWithRePostCountViewModel
    {
        public Post Post { get; set; }
        public int RePostCount { get; set; }

    }
}
