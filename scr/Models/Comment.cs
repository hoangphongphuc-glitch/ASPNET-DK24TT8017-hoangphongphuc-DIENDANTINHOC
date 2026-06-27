using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Bình luận không được để trống")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
