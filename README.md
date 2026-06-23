HƯỚNG DẪN CÀI ĐẶT VÀ TRIỂN KHAI HỆ THỐNG
PHẦN MỀM QUẢN LÝ BÁN HÀNG MINISHOP

Kính gửi hội đồng chấm thi và thầy/cô, 
Dưới đây là tài liệu hướng dẫn chi tiết từng bước để cài đặt, cấu hình và triển khai hệ thống phần mềm quản lý bán hàng MiniShop. Hệ thống hoạt động theo mô hình Client-Server với Backend (.NET 8 Web API) và Frontend (Vanilla JS, Bootstrap 5). 
1. YÊU CẦU MÔI TRƯỜNG VÀ CÔNG CỤ
Để đảm bảo phần mềm chạy ổn định và đầy đủ chức năng, máy tính cần cài đặt các công cụ sau: 
Công cụ / Phần mềm	Chi tiết yêu cầu
Visual Studio 2022	Cần cài đặt workload ASP.NET and web development và hỗ trợ .NET 8.0 SDK. 
SQL Server & SSMS	Dùng SQL Server Management Studio (SSMS) để khởi chạy và quản trị cơ sở dữ liệu. 
Visual Studio Code	Trình soạn thảo nhẹ, khuyến nghị dùng để chạy mã nguồn Frontend. 
Trình duyệt web	Google Chrome, Microsoft Edge hoặc Cốc Cốc (ưu tiên các phiên bản mới nhất). 
2. CÀI ĐẶT CƠ SỞ DỮ LIỆU (DATABASE)
Hệ thống sử dụng SQL Server. Bạn cần chạy script SQL đã được đính kèm để tự động tạo cấu trúc bảng và chèn dữ liệu mẫu. 
1.	Khởi động: Mở phần mềm SQL Server Management Studio (SSMS). 
2.	Kết nối máy chủ: Tại cửa sổ Connect to Server, chọn Server type là Database Engine; nhập Server name là . hoặc localhost hoặc .\SQLEXPRESS; chọn Authentication là Windows Authentication và nhấn Connect. 
3.	Mở Script: Trên thanh menu, chọn File -> Open -> File... (hoặc nhấn Ctrl + O) và tìm đến vị trí chứa file DataBase.sql trong thư mục của dự án (bạn cũng có thể kéo thả trực tiếp file vào SSMS). 
4.	Thực thi: Nhấn nút Execute trên thanh công cụ (hoặc nhấn phím tắt F5) để chạy file. 
5.	Hoàn tất: Đợi vài giây cho đến khi có thông báo "Commands completed successfully" ở góc dưới. Tại bảng Object Explorer bên trái, click chuột phải vào thư mục Databases -> chọn Refresh để thấy database mới xuất hiện có tên là MiniShopDb. 
LƯU Ý QUAN TRỌNG: Nếu máy tính của thầy/cô có cấu hình Tên Server khác với mặc định, vui lòng vào thư mục Backend, mở file appsettings.json và cập nhật lại biến Server=... trong chuỗi kết nối DefaultConnection cho trùng khớp. 
3. KHỞI CHẠY BACKEND (API SERVER)
Backend chứa toàn bộ logic xử lý, kiểm tra bảo mật và cung cấp API. Nó cần được chạy lên trước khi bật Frontend. 
1.	Mở dự án: Truy cập vào thư mục mã nguồn Backend, nhấp đúp chuột vào file Solution có đuôi .sln để mở toàn bộ dự án bằng Visual Studio 2022. 
2.	Chọn Startup Project: Đảm bảo đã click chuột phải vào project Web API chính (thường là project chứa folder Controllers) và chọn Set as Startup Project. 
3.	Build Solution: Tại cửa sổ Solution Explorer, click chuột phải vào tên Solution trên cùng, chọn Build Solution (hoặc Ctrl + Shift + B). Visual Studio sẽ tự động tải về (Restore) các thư viện NuGet phụ thuộc (Entity Framework Core, BCrypt.Net, JWTBearer...). Chờ đến khi dòng chữ "Build succeeded" xuất hiện ở góc dưới. 
4.	Chạy API: Nhấn nút Run / Start (biểu tượng Play màu xanh lá) hoặc nhấn phím tắt F5. 
5.	Kiểm tra: Visual Studio sẽ mở cửa sổ console ngầm và tự động bật trình duyệt lên trang Swagger UI (giao diện liệt kê và test API). 
 QUAN TRỌNG: Hãy thu nhỏ cửa sổ Visual Studio lại và giữ cho API luôn chạy ngầm (không bấm Stop) trong suốt quá trình test giao diện web. 
4. KHỞI CHẠY FRONTEND (GIAO DIỆN NGƯỜI DÙNG)
Giao diện (UI) được code tĩnh bằng HTML/CSS và dùng JS để lấy dữ liệu từ API. 
1.	Mở thư mục: Mở Visual Studio Code (VS Code), chọn File -> Open Folder... và trỏ tới thư mục chứa mã nguồn Frontend. 
2.	Khởi chạy (Cách 1 - Khuyến nghị): Dùng extension Live Server. Tại bảng Explorer, click chuột phải vào file login.html (hoặc index.html) và chọn Open with Live Server (hoặc nhấn nút Go Live ở viền dưới màn hình). Web sẽ tự động mở trên trình duyệt với địa chỉ dạng [http://127.0.0.1:5500/login.html] (Lưu ý: Nếu chưa có extension này, hãy vào tab Extensions, tìm "Live Server" của Ritwick Dey và nhấn Install). 
3.	Khởi chạy (Cách 2 - Cơ bản): Nếu không có sẵn VS Code, chỉ cần mở thư mục Frontend trực tiếp trên Windows, tìm file login.html và click đúp chuột để mở thẳng lên bằng trình duyệt web. 
5. THÔNG TIN TÀI KHOẢN TEST PHÂN QUYỀN
MiniShop sử dụng hệ thống bảo mật JWT Token để quản lý đăng nhập và phân chia quyền hạn chặt chẽ. Dưới đây là thông tin các tài khoản test đã được tạo sẵn trong Database kèm theo dữ liệu đầy đủ: 
Vai trò	Quyền hạn	Tên đăng nhập (Email)	Mật khẩu
Quản trị viên (Admin)	Toàn quyền hệ thống. Truy cập Dashboard thống kê, quản lý nhân viên, danh mục, sản phẩm và xem báo cáo doanh thu...	admin@minishop.com	ToilaADMIN123
Thu ngân (Cashier)	Tài khoản đặc thù dành cho điểm bán hàng (POS). Có chức năng tìm sản phẩm, quét mã vạch, tính toán tiền và in hóa đơn...	CN01@minishop.com	ToilaADMIN123
Toàn bộ dữ liệu về sản phẩm, danh mục, hóa đơn mẫu đều đã được gieo (seed data) trong file DataBase.sql để đảm bảo có sẵn dữ liệu test đẹp mắt ngay lần đầu cài đặt. 
Nhóm 13 xin chân thành cảm ơn thầy/cô trong hội đồng đã dành thời gian xem xét và đánh giá đồ án!
