using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài viết không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung bài viết không được để trống")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chủ đề bài viết không được để trống")]
        [Display(Name = "Chủ đề")]
        public string Category { get; set; } = "AI"; // AI, Phần cứng, Phần mềm

        [Display(Name = "Ngày đăng")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
