using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models
{
    public class Milestone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual ProjectTopic? Project { get; set; }

        [Required]
        [Display(Name = "Tên mốc tiến độ")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả yêu cầu")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Hạn nộp")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Link nộp bài (GitHub)")]
        [Url(ErrorMessage = "Vui lòng nhập đúng định dạng URL")]
        public string? GithubSubmitUrl { get; set; }

        [Display(Name = "Trạng thái mốc")]
        public string Status { get; set; } = "Chưa nộp"; // Chưa nộp, Chờ duyệt, Đạt, Không đạt

        [Display(Name = "Nhận xét của giảng viên")]
        public string? Feedback { get; set; }

        [Display(Name = "Thời gian nộp")]
        public DateTime? SubmittedAt { get; set; }
    }
}
