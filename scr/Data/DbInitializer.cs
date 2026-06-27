using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Models;

namespace ASPNET_DK24TT8017_hoangphongphuc_DIENDANTINHOC.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Đảm bảo CSDL được tạo (nếu chưa có)
            context.Database.OpenConnection();
            try
            {
                // 1. Tạo bảng DoAn nếu chưa tồn tại
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoAn]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[DoAn] (
                            [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Username] NVARCHAR(250) NOT NULL,
                            [Password] NVARCHAR(250) NOT NULL,
                            [HoTen] NVARCHAR(250) NOT NULL,
                            [Email] NVARCHAR(250) NOT NULL,
                            [ChucVu] NVARCHAR(100) NOT NULL,
                            [GithubUrl] NVARCHAR(MAX) NULL,
                            [Bio] NVARCHAR(MAX) NULL,
                            [Avatar] NVARCHAR(MAX) NULL
                        );
                    END
                ");

                // 2. Thêm các cột thiếu vào bảng DoAn (trong trường hợp bảng đã tồn tại nhưng thiếu cột)
                var columnsToAdd = new[]
                {
                    "Username NVARCHAR(250) NOT NULL DEFAULT ''",
                    "Password NVARCHAR(250) NOT NULL DEFAULT ''",
                    "HoTen NVARCHAR(250) NOT NULL DEFAULT ''",
                    "Email NVARCHAR(250) NOT NULL DEFAULT ''",
                    "ChucVu NVARCHAR(100) NOT NULL DEFAULT 'SinhVien'",
                    "GithubUrl NVARCHAR(MAX) NULL",
                    "Bio NVARCHAR(MAX) NULL",
                    "Avatar NVARCHAR(MAX) NULL"
                };

                foreach (var col in columnsToAdd)
                {
                    var colName = col.Split(' ')[0];
                    // Kiểm tra xem cột đã tồn tại chưa
                    var colCheckSql = $@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DoAn]') AND name = '{colName}')
                        BEGIN
                            ALTER TABLE [dbo].[DoAn] ADD {col};
                        END
                    ";
                    context.Database.ExecuteSqlRaw(colCheckSql);
                }

                // 3. Tạo các bảng khác phục vụ đồ án nếu chưa tồn tại
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectTopics]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[ProjectTopics] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Title] NVARCHAR(MAX) NOT NULL,
                            [Description] NVARCHAR(MAX) NOT NULL,
                            [Category] NVARCHAR(MAX) NOT NULL,
                            [CreatedById] INT NOT NULL,
                            [StudentId] INT NULL,
                            [Status] NVARCHAR(MAX) NOT NULL,
                            [Grade] FLOAT NULL,
                            [Feedback] NVARCHAR(MAX) NULL
                        );
                    END
                ");

                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Milestones]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Milestones] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [ProjectId] INT NOT NULL,
                            [Title] NVARCHAR(MAX) NOT NULL,
                            [Description] NVARCHAR(MAX) NOT NULL,
                            [DueDate] DATETIME2 NOT NULL,
                            [GithubSubmitUrl] NVARCHAR(MAX) NULL,
                            [Status] NVARCHAR(MAX) NOT NULL,
                            [Feedback] NVARCHAR(MAX) NULL,
                            [SubmittedAt] DATETIME2 NULL
                        );
                    END
                ");

                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Posts]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Posts] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Title] NVARCHAR(MAX) NOT NULL,
                            [Content] NVARCHAR(MAX) NOT NULL,
                            [Category] NVARCHAR(MAX) NOT NULL,
                            [CreatedAt] DATETIME2 NOT NULL,
                            [UserId] INT NOT NULL
                        );
                    END
                ");

                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comments]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Comments] (
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [PostId] INT NOT NULL,
                            [UserId] INT NOT NULL,
                            [Content] NVARCHAR(MAX) NOT NULL,
                            [CreatedAt] DATETIME2 NOT NULL
                        );
                    END
                ");

                // 4. Tạo các khoá ngoại nếu chưa có
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectTopics_DoAn_CreatedById')
                        ALTER TABLE [dbo].[ProjectTopics] ADD CONSTRAINT [FK_ProjectTopics_DoAn_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[DoAn] ([id]);

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectTopics_DoAn_StudentId')
                        ALTER TABLE [dbo].[ProjectTopics] ADD CONSTRAINT [FK_ProjectTopics_DoAn_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[DoAn] ([id]);

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Milestones_ProjectTopics_ProjectId')
                        ALTER TABLE [dbo].[Milestones] ADD CONSTRAINT [FK_Milestones_ProjectTopics_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[ProjectTopics] ([Id]) ON DELETE CASCADE;

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Posts_DoAn_UserId')
                        ALTER TABLE [dbo].[Posts] ADD CONSTRAINT [FK_Posts_DoAn_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[DoAn] ([id]) ON DELETE CASCADE;

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_Posts_PostId')
                        ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [dbo].[Posts] ([Id]) ON DELETE CASCADE;

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_DoAn_UserId')
                        ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [FK_Comments_DoAn_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[DoAn] ([id]);
                ");

            }
            finally
            {
                context.Database.CloseConnection();
            }

            // 5. Gieo dữ liệu tài khoản nếu chưa có
            var defaultUsers = new User[]
            {
                new User { Username = "admin@school.vn", Password = "123", FullName = "Admin", Email = "admin@school.vn", Role = "Admin" },
                new User { Username = "mien.dp@school.vn", Password = "123", FullName = "Đoàn Phước Miền", Email = "mien.dp@school.vn", Role = "GiaoVien" },
                new User { Username = "phuc.hp@school.vn", Password = "123", FullName = "Hoàng Phong Phúc", Email = "phuc.hp@school.vn", Role = "SinhVien" },
                new User { Username = "vana@school.vn", Password = "123", FullName = "Nguyễn Văn A", Email = "vana@school.vn", Role = "GiangVien" },
                new User { Username = "gv_thanhson", Password = "123", FullName = "Nguyễn Thanh Sơn", Email = "gv_thanhson@school.vn", Role = "GiangVien" },
                new User { Username = "gv_minhtu", Password = "123", FullName = "Thân Minh Tú", Email = "gv_minhtu@school.vn", Role = "GiangVien" },
                new User { Username = "sv_dangkhoa", Password = "123", FullName = "Vũ Nguyễn Đăng Khoa", Email = "sv_dangkhoa@school.vn", Role = "SinhVien" }
            };

            bool changed = false;
            foreach (var u in defaultUsers)
            {
                if (!context.Users.Any(x => x.Username == u.Username))
                {
                    context.Users.Add(u);
                    changed = true;
                }
            }
            if (changed)
            {
                context.SaveChanges();
            }

            // 6. Gieo dữ liệu đề tài mẫu nếu chưa có
            if (!context.ProjectTopics.Any())
            {
                var mien = context.Users.FirstOrDefault(u => u.Username == "mien.dp@school.vn");
                var vana = context.Users.FirstOrDefault(u => u.Username == "vana@school.vn");
                var thanhson = context.Users.FirstOrDefault(u => u.Username == "gv_thanhson");

                if (mien != null && vana != null && thanhson != null)
                {
                    var topics = new ProjectTopic[]
                    {
                        new ProjectTopic
                        {
                            Title = "Phân loại ảnh y khoa sử dụng Deep Learning",
                            Description = "Xây dựng mô hình học sâu (CNN) để phân loại các tổn thương da hoặc X-quang phổi nhằm hỗ trợ chẩn đoán y tế.",
                            Category = "AI",
                            CreatedById = mien.Id,
                            Status = "Chờ đăng ký"
                        },
                        new ProjectTopic
                        {
                            Title = "Hệ thống IoT giám sát và điều khiển nhà kính thông minh",
                            Description = "Thiết kế mạch phần cứng ESP32 kết nối cảm biến nhiệt độ, độ ẩm và relay điều khiển máy bơm, gửi dữ liệu lên Dashboard qua giao thức MQTT.",
                            Category = "Phần cứng",
                            CreatedById = vana.Id,
                            Status = "Chờ đăng ký"
                        },
                        new ProjectTopic
                        {
                            Title = "Xây dựng website bán hàng trực tuyến tích hợp ví điện tử",
                            Description = "Phát triển ứng dụng web thương mại điện tử bằng ASP.NET Core MVC, hỗ trợ giỏ hàng, thanh toán qua cổng VNPay/Momo và trang quản trị.",
                            Category = "Phần mềm",
                            CreatedById = thanhson.Id,
                            Status = "Chờ đăng ký"
                        }
                    };

                    context.ProjectTopics.AddRange(topics);
                    context.SaveChanges();
                }
            }

            // 7. Gieo dữ liệu bài viết diễn đàn mẫu nếu chưa có
            if (!context.Posts.Any())
            {
                var phuc = context.Users.FirstOrDefault(u => u.Username == "phuc.hp@school.vn");
                var mien = context.Users.FirstOrDefault(u => u.Username == "mien.dp@school.vn");

                if (phuc != null && mien != null)
                {
                    var post1 = new Post
                    {
                        Title = "Xu hướng phát triển Trí tuệ nhân tạo (AI) trong năm 2026",
                        Content = "AI đang thay đổi cách chúng ta làm việc. Các mô hình ngôn ngữ lớn (LLM) ngày càng thông minh và tích hợp sâu vào các công cụ lập trình hàng ngày. Theo các bạn, AI sẽ thay thế lập trình viên trong tương lai gần hay chỉ là công cụ hỗ trợ đắc lực?",
                        Category = "AI",
                        CreatedAt = DateTime.Now.AddDays(-2),
                        UserId = phuc.Id
                    };

                    var post2 = new Post
                    {
                        Title = "Kinh nghiệm thiết kế mạch Arduino cho người mới bắt đầu",
                        Content = "Khi làm việc với Arduino, việc thiết kế mạch điện tử cần chú ý đến dòng điện tiêu thụ và chống nhiễu. Mình thường dùng thêm tụ lọc nguồn 10uF và 100nF để mạch chạy ổn định hơn. Ai có mẹo gì hay khi thiết kế phần cứng chia sẻ thêm nhé!",
                        Category = "Phần cứng",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        UserId = mien.Id
                    };

                    context.Posts.Add(post1);
                    context.Posts.Add(post2);
                    context.SaveChanges();

                    // Thêm bình luận mẫu
                    var comment1 = new Comment
                    {
                        PostId = post1.Id,
                        UserId = mien.Id,
                        Content = "Chào Phúc, theo thầy AI sẽ là trợ thủ đắc lực giúp lập trình viên tăng năng suất lao động lên gấp nhiều lần. Người học lập trình cần tập trung vào tư duy logic và kiến trúc hệ thống hơn là chỉ gõ code thuần túy.",
                        CreatedAt = DateTime.Now.AddHours(-12)
                    };

                    var comment2 = new Comment
                    {
                        PostId = post1.Id,
                        UserId = phuc.Id,
                        Content = "Dạ em cảm ơn nhận định của thầy. Em cũng thấy năng suất tăng đáng kể khi biết cách tận dụng các trợ lý AI hỗ trợ viết code và debug.",
                        CreatedAt = DateTime.Now.AddHours(-10)
                    };

                    context.Comments.Add(comment1);
                    context.Comments.Add(comment2);
                    context.SaveChanges();
                }
            }
        }
    }
}
