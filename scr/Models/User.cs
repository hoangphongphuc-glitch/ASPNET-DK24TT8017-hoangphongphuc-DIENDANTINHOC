using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models
{
    [Table("DoAn")] // Ánh xạ chính xác bảng DoAn trong SQL Server của bạn
    public class User
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _role = "SinhVien";
        private string? _githubUrl;
        private string? _bio;
        private string? _avatar;

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("Username")]
        public string Username 
        { 
            get => _username.Trim(); 
            set => _username = value ?? string.Empty; 
        }

        [Required]
        [Column("Password")] // Trỏ trực tiếp đến cột lưu mật khẩu (dạng text "123")
        public string Password 
        { 
            get => _password.Trim(); 
            set => _password = value ?? string.Empty; 
        }

        [Required]
        [Column("HoTen")] // Trỏ đến cột lưu họ tên sinh viên/giáo viên
        public string FullName 
        { 
            get => _fullName.Trim(); 
            set => _fullName = value ?? string.Empty; 
        }

        [Required]
        [Column("Email")]
        public string Email 
        { 
            get => _email.Trim(); 
            set => _email = value ?? string.Empty; 
        }

        [Required]
        [Column("ChucVu")] // Trỏ đến cột lưu chức vụ (SinhVien, GiangVien, Admin)
        public string Role 
        { 
            get => _role.Trim(); 
            set => _role = value ?? string.Empty; 
        }

        // Các trường phục vụ chức năng đồ án
        public string? GithubUrl 
        { 
            get => _githubUrl?.Trim(); 
            set => _githubUrl = value; 
        }

        public string? Bio 
        { 
            get => _bio?.Trim(); 
            set => _bio = value; 
        }

        public string? Avatar 
        { 
            get => _avatar?.Trim(); 
            set => _avatar = value; 
        }
    }
}
