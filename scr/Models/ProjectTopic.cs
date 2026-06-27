using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models
{
    public class ProjectTopic
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đề tài")]
        [Display(Name = "Tên đề tài")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả đề tài")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chủ đề")]
        [Display(Name = "Chủ đề")]
        public string Category { get; set; } = "AI"; // AI, Phần cứng, Phần mềm

        [Display(Name = "Giảng viên hướng dẫn")]
        public int CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        [Display(Name = "Sinh viên thực hiện")]
        public int? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chờ đăng ký"; // Chờ đăng ký, Chờ duyệt, Đang làm, Hoàn thành

        [Display(Name = "Điểm số")]
        [Range(0, 10, ErrorMessage = "Điểm số từ 0 đến 10")]
        public double? Grade { get; set; }

        [Display(Name = "Nhận xét tổng kết")]
        public string? Feedback { get; set; }

        public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }
}
