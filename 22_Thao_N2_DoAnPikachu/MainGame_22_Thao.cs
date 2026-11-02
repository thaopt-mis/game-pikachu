using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
using System.Runtime.InteropServices;


namespace _22_Thao_N2_DoAnPikachu
{
    public partial class MainGame_22_Thao : Form
    {
        // 1. MẢNG LƯU TRỮ GIAO DIỆN VÀ LOGIC
        // Mảng 2 chiều chứa các ô hình Pokemon
        private PictureBox[,] dsOAnh_22_Thao;
        // Mảng 2 chiều chứa các con số (ID của hình). Ví dụ maTranHinh[0,0] = 5 nghĩa là ô đó chứa hình số 5.
        // để thuật toán tìm đường (BFS) biết 2 ô có giống nhau không mà nối.
        private int[,] maTranHinh_22_Thao;
        // Mảng 2 chiều chứa các Panel. Panel này nằm bọc bên ngoài PictureBox để làm cái viền màu đỏ khi mày click vào ô đó.
        private Panel[,] dsKhungAnh_22_Thao;

        // 2. KÍCH THƯỚC VÀ CẤU TRÚC BÀN CỜ
        // Bàn cờ có 9 dòng (từ trên xuống dưới)
        private int soDong_22_Thao = 9;
        // Bàn cờ có 16 cột (từ trái qua phải) -> Tổng cộng 9 x 16 = 144 ô
        private int soCot_22_Thao = 16;
        // Kích thước mỗi ô vuông là 55x55 pixel
        private int kichThuocO_22_Thao = 55;
        // Khoảng cách hở ra giữa 2 ô liền kề là 1 pixel (để nhìn ko bị dính chùm vào nhau)
        private int khoangCachO_22_Thao = 1;
        // Mặc định dùng đủ 36 loại Pokemon
        private int soLoaiHinh_22_Thao = 36;   

        // 3. CÔNG CỤ HỖ TRỢ
        // Dùng để random (quay xổ số) lúc xáo trộn hình trên bàn cờ.
        private Random random_22_Thao = new Random();

        // 4. BIẾN LƯU TRẠNG THÁI CLICK CHUỘT
        // Khi click vô hình lần 1, nó sẽ lưu cái hình đó vào biến này. Mặc định mới vô game là chưa chọn gì (null).
        private PictureBox oDangChon1_22_Thao = null;
        // Khi click hình lần 2, nó lưu vào biến này. Nếu oDangChon1 và oDangChon2 giống hình nhau thì sẽ ăn điểm.
        private PictureBox oDangChon2_22_Thao = null;
        // Lưu lại "Tọa độ" (gồm dòng mấy, cột mấy) của cái ô thứ 1 vừa click.
        private Point viTriO1_22_Thao;
        // Lưu lại "Tọa độ" (dòng, cột) của cái ô thứ 2. Có tọa độ 2 ô này rồi mới quăng cho thuật toán đi tìm đường nối.
        private Point viTriO2_22_Thao;

        // 5. BIẾN QUẢN LÝ ĐIỂM SỐ VÀ THỜI GIAN
        // Điểm số của người chơi, mặc định mới vô là 0. Nối đúng 1 cặp thì cộng thêm.
        private int diem_22_Thao = 0;
        // Tổng thời gian cho một ván (Ví dụ 600 giây = 10 phút)
        private int tongThoiGian_22_Thao = 600;
        // Thời gian đang chạy lùi. Mỗi giây trôi qua (Timer_Tick) thì cái này trừ đi 1. Về 0 là Thua.
        private int thoiGianConLai_22_Thao = 600;

        // Khai báo bộ phát nhạc WMP 
        WMPLib.WindowsMediaPlayer phatNhac_22_Thao = new WMPLib.WindowsMediaPlayer();
        bool dangBatNhac_22_Thao = true;

        // Khai báo hàm SendMessage để "ép" Windows đổi màu ProgressBar
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // Mã lệnh để đổi màu (PBM_SETBARCOLOR)
        private const uint PBM_SETBARCOLOR = 0x0409;
        // Hàm này để tắt cái giao diện mặc định của Windows trên 1 control cụ thể
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        //Lớp lưu trạng thái tìm đường trong thuật toán Pikachu
        private class TrangThaiTimDuong_22_Thao
        {
            // 1. Tọa độ hiện tại(dòng)
            public int Dong_22_Thao;

            // 2. Tọa độ hiện tại(cột)
            public int Cot_22_Thao;

            // 3. Hướng đi hiện tại đang cắm đầu đi tới: 
            // (Thường quy định: 0=Lên, 1=Xuống, 2=Trái, 3=Phải. Số -1 nghĩa là mới xuất phát chưa biết đi hướng nào)
            public int Huong_22_Thao;

            // Game chỉ cho phép rẽ tối đa 2 lần. Nếu cái biến này nhảy lên số 3 là sai 
            public int SoLanRe_22_Thao;

            // HÀM KHỞI TẠO (Constructor)
            public TrangThaiTimDuong_22_Thao(int dong_22_Thao, int cot_22_Thao, int huong_22_Thao, int soLanRe_22_Thao)
            {
                // Lấy dữ liệu truyền vào gán vô cho mấy cái biến của Class ở trên
                Dong_22_Thao = dong_22_Thao;
                Cot_22_Thao = cot_22_Thao;
                Huong_22_Thao = huong_22_Thao;
                SoLanRe_22_Thao = soLanRe_22_Thao;
            }
        }
        // HÀM KHỞI TẠO (CONSTRUCTOR) CỦA FORM GAME CHÍNH
        // Hàm này tự động chạy đầu tiên ngay khi Form được cấp phát bộ nhớ (trước cả khi hiển thị lên màn hình)
        public MainGame_22_Thao()
        {
            // 1. Hàm mặc định của WinForms: Dùng để nạp toàn bộ các Control (Nút, Nhãn, Panel...) 
            // đã thiết kế bên giao diện Designer vào bộ nhớ.
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            // 2. ĐĂNG KÝ SỰ KIỆN (Event Subscription / Delegate)
            // Cú pháp "+=" dùng để gắn một Hàm xử lý (Event Handler) vào một Sự kiện (Event).

            // Gắn sự kiện Load: Khi Form tải xong giao diện, sẽ gọi hàm MainGame_22_Thao_Load để sinh bàn cờ và chạy Timer.
            this.Load += MainGame_22_Thao_Load;

            // Gắn sự kiện Tick cho Timer: Cứ mỗi nhịp Interval (ví dụ 1000ms = 1 giây) trôi qua thì gọi hàm timer_Tick để trừ thời gian.
            timer_22_Thao.Tick += timer_22_Thao_Tick;
            // Gắn sự kiện: Khi cái Form này bị đóng hẳn (bằng dấu X hoặc Close)
            this.FormClosed += MainGame_22_Thao_FormClosed;
        }
        private void KhoiTaoMaTranHinh_22_Thao()
        {
            // BƯỚC 3: TẠO DANH SÁCH CÁC CẶP HÌNH TRÙNG NHAU
            // Tạo một cái danh sách trống để gom số
            List<int> dsHinh_22_Thao = new List<int>();
            // Tính tổng số ô trên bàn cờ (Ví dụ 9x16 = 144 ô)
            int tongSoO_22_Thao = soDong_22_Thao * soCot_22_Thao;

            // Vòng lặp này chỉ chạy bằng MỘT NỬA tổng số ô thôi (144 / 2 = 72 lần)
            for (int i_22_Thao = 0; i_22_Thao < tongSoO_22_Thao / 2; i_22_Thao++)
            {
                // Có 36 hình Pikachu tất cả. Phép chia lấy dư (% 36) giúp số hình cứ xoay vòng từ 1 đến 36.
                // Chỉnh sửa: Sử dụng soLoaiHinh_22_Thao để khớp với Form Tùy Chỉnh
                int soHinh_22_Thao = (i_22_Thao % soLoaiHinh_22_Thao) + 1;

                // Thêm vào danh sách 2 lần GIỐNG NHAU để tạo thành 1 CẶP (để nối được)
                // Thêm con thứ nhất
                dsHinh_22_Thao.Add(soHinh_22_Thao);
                // Thêm con thứ hai
                dsHinh_22_Thao.Add(soHinh_22_Thao);
            }

            // BƯỚC 4: XÁO TRỘN BÀN CỜ 
            // Thuật toán đổi chỗ ngẫu nhiên. Lấy từng phần tử đổi chỗ với một phần tử bất kỳ.
            for (int i_22_Thao = 0; i_22_Thao < dsHinh_22_Thao.Count; i_22_Thao++)
            {
                // Random ra một vị trí j bất kỳ trong danh sách
                int j_22_Thao = random_22_Thao.Next(dsHinh_22_Thao.Count);

                // Kỹ thuật Swap (Đổi chỗ 2 biến phải dùng 1 biến tạm)
                int tam_22_Thao = dsHinh_22_Thao[i_22_Thao];
                dsHinh_22_Thao[i_22_Thao] = dsHinh_22_Thao[j_22_Thao];
                dsHinh_22_Thao[j_22_Thao] = tam_22_Thao;
            }

            // Đổ danh sách đã xáo trộn vào ma trận logic 2 chiều để chuẩn bị cho việc vẽ UI
            maTranHinh_22_Thao = new int[soDong_22_Thao, soCot_22_Thao];
            int chiSo_22_Thao = 0;
            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao] = dsHinh_22_Thao[chiSo_22_Thao++];
                }
            }
        }
        private void VeBanCoPikachu_22_Thao()
        {
            // 1. DỌN DẸP VÀ CHUẨN BỊ
            panelPikachu_22_Thao.Controls.Clear();
            dsOAnh_22_Thao = new PictureBox[soDong_22_Thao, soCot_22_Thao];
            dsKhungAnh_22_Thao = new Panel[soDong_22_Thao, soCot_22_Thao];

            // 2. TÍNH KÍCH THƯỚC Ô
            kichThuocO_22_Thao = Math.Min(
                panelPikachu_22_Thao.Width / soCot_22_Thao,
                panelPikachu_22_Thao.Height / soDong_22_Thao
            );

            // 3. THUẬT TOÁN CĂN GIỮA (CENTERING LOGIC)
            // Tính tổng chiều rộng và chiều cao của toàn bộ bàn cờ
            int tongRongBanCo_22_Thao = soCot_22_Thao * (kichThuocO_22_Thao + khoangCachO_22_Thao);
            int tongCaoBanCo_22_Thao = soDong_22_Thao * (kichThuocO_22_Thao + khoangCachO_22_Thao);

            // Tính điểm bắt đầu (Tọa độ X, Y của ô đầu tiên) để bàn cờ nằm chính giữa Panel
            int startX_22_Thao = (panelPikachu_22_Thao.Width - tongRongBanCo_22_Thao) / 2;
            int startY_22_Thao = (panelPikachu_22_Thao.Height - tongCaoBanCo_22_Thao) / 2;

            // 4. VẼ BÀN CỜ
            int doDayVien_22_Thao = 2;

            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    int maHinh_22_Thao = maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao];

                    Panel khungAnh_22_Thao = new Panel();
                    khungAnh_22_Thao.Width = khungAnh_22_Thao.Height = kichThuocO_22_Thao;

                    // CỘNG THÊM startX và startY ĐỂ ĐẨY VÀO GIỮA
                    khungAnh_22_Thao.Left = startX_22_Thao + cot_22_Thao * (kichThuocO_22_Thao + khoangCachO_22_Thao);
                    khungAnh_22_Thao.Top = startY_22_Thao + dong_22_Thao * (kichThuocO_22_Thao + khoangCachO_22_Thao);

                    khungAnh_22_Thao.BackColor = Color.Transparent;
                    khungAnh_22_Thao.Tag = new Point(dong_22_Thao, cot_22_Thao);

                    PictureBox oAnh_22_Thao = new PictureBox();
                    oAnh_22_Thao.Width = oAnh_22_Thao.Height = kichThuocO_22_Thao - doDayVien_22_Thao * 2;
                    oAnh_22_Thao.Left = oAnh_22_Thao.Top = doDayVien_22_Thao;
                    oAnh_22_Thao.SizeMode = PictureBoxSizeMode.StretchImage;
                    oAnh_22_Thao.BackColor = Color.White;
                    oAnh_22_Thao.Image = (Image)Properties.Resources.ResourceManager.GetObject("pieces" + maHinh_22_Thao);
                    oAnh_22_Thao.Tag = new Point(dong_22_Thao, cot_22_Thao);
                    oAnh_22_Thao.Click += OAnh_Click_22_Thao;

                    khungAnh_22_Thao.Controls.Add(oAnh_22_Thao);
                    dsKhungAnh_22_Thao[dong_22_Thao, cot_22_Thao] = khungAnh_22_Thao;
                    dsOAnh_22_Thao[dong_22_Thao, cot_22_Thao] = oAnh_22_Thao;
                    panelPikachu_22_Thao.Controls.Add(khungAnh_22_Thao);
                }
            }
        }
        private void BatDauGameMoi_22_Thao()
        {
            // 1. DỪNG VÀ RESET CÁC THÀNH PHẦN GIAO DIỆN
            timer_22_Thao.Stop();
            // Ẩn bảng báo thắng (nếu ván trước vừa thắng)
            pnlWinGame_22_Thao.Visible = false;
            // Ẩn bảng báo thua
            pnlLooseGame_22_Thao.Visible = false;
            // Mở khóa bàn cờ để cho phép tương tác
            panelPikachu_22_Thao.Enabled = true;
            // Mở khóa các nút chức năng hỗ trợ
            btnSwap_22_Thao.Enabled = true;      
            btnGoiY_22_Thao.Enabled = true;

            // 2. THIẾT LẬP LẠI THÔNG SỐ LOGIC 
            diem_22_Thao = 0;
            thoiGianConLai_22_Thao = tongThoiGian_22_Thao;
            // Đưa điểm số hiển thị về 0
            CapNhatDiem_22_Thao();
            // Đặt lại thanh thời gian đầy cây
            CapNhatThanhThoiGian_22_Thao();      

            // 3. LÀM SẠCH DỮ LIỆU TẠM
            // Xóa trí nhớ về các ô đang chọn dở ở ván cũ để tránh lỗi tọa độ ảo
            DatLaiLuaChon_22_Thao();

            // 4. KÍCH HOẠT QUY TRÌNH TẠO BÀN CỜ MỚI
            // Bước A: Tính toán ma trận số (Logic Core)
            KhoiTaoMaTranHinh_22_Thao();

            // Bước B: Vẽ các ô hình lên màn hình (UI Render) dựa trên ma trận vừa tạo
            VeBanCoPikachu_22_Thao();

            // 5. BẮT ĐẦU VÁN ĐẤU
            timer_22_Thao.Start(); // Kích hoạt đồng hồ đếm ngược
        }
        
        // SỰ KIỆN CHÍNH: XỬ LÝ KHI NGƯỜI CHƠI NHẤP CHUỘT VÀO MỘT Ô HÌNH POKEMON
        // Sử dụng từ khóa 'async' để hỗ trợ xử lý bất đồng bộ (giữ độ trễ UI mà không làm đơ game)
        private async void OAnh_Click_22_Thao(object sender, EventArgs e)
        {
            // 1. Ép kiểu (Casting) đối tượng vừa được click (sender) về đúng chuẩn PictureBox
            PictureBox oDuocBam_22_Thao = sender as PictureBox;
            // 2. Bắt lỗi (Exception Handling): Ngăn chặn thao tác lỗi
            // Nếu click ra ngoài hoặc click vào ô đã ăn rồi (Image == null) thì thoát hàm ngay lập tức
            if (oDuocBam_22_Thao == null || oDuocBam_22_Thao.Image == null)
                return;
            // 3. Lấy tọa độ (Dòng, Cột) của ô vừa click đã được lưu trữ sẵn trong thuộc tính Tag
            Point viTri_22_Thao = (Point)oDuocBam_22_Thao.Tag;
            // 4. LUỒNG XỬ LÝ LẦN CLICK THỨ NHẤT
            // Nếu biến oDangChon1 đang trống, nghĩa là người chơi đang chọn hình đầu tiên của một cặp
            if (oDangChon1_22_Thao == null)
            {
                // Ghi nhận hình thứ nhất
                oDangChon1_22_Thao = oDuocBam_22_Thao;
                // Ghi nhận tọa độ thứ nhất
                viTriO1_22_Thao = viTri_22_Thao;
                // Gọi hàm đổi viền sang màu đỏ (Highlight)
                ToSangO_22_Thao(oDangChon1_22_Thao);
                // Thoát hàm để chờ người chơi click lần 2
                return;                               
            }

            // 5. NGĂN CHẶN SPAM CLICK
            // Nếu người chơi bấm liên tục 2 lần vào CÙNG MỘT Ô thì bỏ qua, không tính là chọn ô thứ 2
            if (oDangChon1_22_Thao == oDuocBam_22_Thao)
                return;
            // Ngăn chặn trường hợp hệ thống đang xử lý mà người chơi bấm loạn xạ sang ô thứ 3
            if (oDangChon2_22_Thao != null)
                return;

            // 6. LUỒNG XỬ LÝ LẦN CLICK THỨ HAI
            // Nếu đã qua được các bước lọc trên, đây chắc chắn là hình thứ 2 hợp lệ
            oDangChon2_22_Thao = oDuocBam_22_Thao;
            viTriO2_22_Thao = viTri_22_Thao;
            // Đổi viền ô thứ 2 sang màu đỏ
            ToSangO_22_Thao(oDangChon2_22_Thao);

            // 7. TỐI ƯU TRẢI NGHIỆM NGƯỜI DÙNG (UX)
            // Tạm dừng luồng thực thi 250 mili-giây (1/4 giây) nhưng không làm treo giao diện.
            // Mục đích: Cho phép người chơi kịp nhìn thấy cả 2 ô đều đã được tô đỏ trước khi gọi thuật toán xóa hình.
            await Task.Delay(250);
            // 8. Đưa dữ liệu tọa độ của 2 ô vào hàm kiểm tra thuật toán nối đường
            XuLyKiemTraCapHinh_22_Thao();
        }
        // HÀM KIỂM TRA ĐIỀU KIỆN ĂN ĐIỂM 
        // Hàm này được gọi sau khi người chơi đã chọn đủ 2 ô hợp lệ trên bàn cờ
        private void XuLyKiemTraCapHinh_22_Thao()
        {
            // 1. TRUY XUẤT MA TRẬN LOGIC
            // Lấy "Mã số hình" (ID) của ô thứ nhất và ô thứ hai từ mảng 2 chiều maTranHinh
            // Dùng tọa độ X (Dòng), Y (Cột) đã được lưu từ lúc click chuột để tra cứu
            int giaTri1_22_Thao = maTranHinh_22_Thao[viTriO1_22_Thao.X, viTriO1_22_Thao.Y];
            int giaTri2_22_Thao = maTranHinh_22_Thao[viTriO2_22_Thao.X, viTriO2_22_Thao.Y];

            // 2. GỌI THUẬT TOÁN TÌM ĐƯỜNG (BFS)
            // Truyền tọa độ 2 ô vào hàm thuật toán để kiểm tra xem có đường nối hợp lệ (<= 2 lần rẽ) hay không
            // Kết quả trả về là True (Có đường) hoặc False (Không có đường)
            bool coDuongDi_22_Thao = TimDuongNoi_22_Thao(viTriO1_22_Thao, viTriO2_22_Thao);

            // 3. ĐƯA RA PHÁN QUYẾT LOGIC
            // Luật chơi Pikachu bắt buộc phải thỏa mãn ĐỒNG THỜI 2 điều kiện:
            // - giaTri1 == giaTri2: Hai ô phải là cùng một hình Pokemon giống hệt nhau
            // - coDuongDi == true: Phải có đường nối thông thoáng, không bị vướng các hình khác
            if (giaTri1_22_Thao == giaTri2_22_Thao && coDuongDi_22_Thao == true)
            {
                // Nếu ĐÚNG: Gọi hàm "Ăn Cặp Hình" (Tàng hình 2 ô, cộng điểm, check win, check hết đường...)
                AnCapHinh_22_Thao();
            }
            else
            {
                // Nếu SAI (Một trong 2 điều kiện bị vi phạm): 
                // Gọi hàm "Bỏ Chọn" (Xóa viền đỏ, reset lại trạng thái click để người chơi chọn lại từ đầu)
                BoChon2O_22_Thao();
            }
        }
        //Logic triệt tiêu cặp hình và cập nhật trạng thái
        private void AnCapHinh_22_Thao()
        {
            // 1. TẮT HIỆU ỨNG TÔ SÁNG
            // Xóa viền đỏ của 2 ô vừa được chọn
            BoToSangO_22_Thao(oDangChon1_22_Thao);
            BoToSangO_22_Thao(oDangChon2_22_Thao);

            // 2. CẬP NHẬT GIAO DIỆN NGƯỜI DÙNG (UI UPDATE)
            // Tắt hình ảnh hiển thị
            oDangChon1_22_Thao.Image = null;
            oDangChon2_22_Thao.Image = null;

            // Ẩn hoàn toàn PictureBox khỏi màn hình (tàng hình)
            oDangChon1_22_Thao.Visible = false;
            oDangChon2_22_Thao.Visible = false;

            // Vô hiệu hóa thao tác chuột, tránh trường hợp người chơi "spam click" vào khoảng không gây lỗi
            oDangChon1_22_Thao.Enabled = false;
            oDangChon2_22_Thao.Enabled = false;

            // 3. CẬP NHẬT MA TRẬN DỮ LIỆU LOGIC
            // Gán giá trị 0 cho 2 vị trí trên ma trận. Số 0 đại diện cho "đường trống".
            //Nó giúp thuật toán tìm đường (BFS) ở các lượt sau có thể đi xuyên qua 2 tọa độ này.
            maTranHinh_22_Thao[viTriO1_22_Thao.X, viTriO1_22_Thao.Y] = 0;
            maTranHinh_22_Thao[viTriO2_22_Thao.X, viTriO2_22_Thao.Y] = 0;

            // Đảm bảo Panel khung nền phía sau cũng trở nên trong suốt
            if (dsKhungAnh_22_Thao != null)
            {
                dsKhungAnh_22_Thao[viTriO1_22_Thao.X, viTriO1_22_Thao.Y].BackColor = Color.Transparent;
                dsKhungAnh_22_Thao[viTriO2_22_Thao.X, viTriO2_22_Thao.Y].BackColor = Color.Transparent;
            }

            // 4. CẬP NHẬT ĐIỂM SỐ
            // Mỗi cặp ghép đúng cộng 10 điểm
            diem_22_Thao += 10; 
            CapNhatDiem_22_Thao();

            // 5. ĐẶT LẠI TRẠNG THÁI (RESET STATE)
            // Giải phóng dữ liệu 2 ô vừa lưu trong biến tạm để chuẩn bị đón nhận lượt click tiếp theo
            DatLaiLuaChon_22_Thao();

            // 6. KIỂM TRA ĐIỀU KIỆN CHIẾN THẮNG (WIN CONDITION)
            // Quét lại toàn bộ ma trận, nếu tất cả các ô đều mang giá trị 0 thì kết thúc game
            if (KiemTraThang_22_Thao())
            {
                //  Gọi hàm hiện Panel
                XuLyThangGame_22_Thao();
                return;
            }

            // Nếu chưa thắng thì mới kiểm tra hết đường để trộn bài
            KiemTraVaTronLaiNeuHetDuong_22_Thao();
        }
        // HÀM HỦY CHỌN KHI NỐI SAI HOẶC TÌM KHÔNG THẤY ĐƯỜNG
        // Gọi hàm này khi 2 hình không giống nhau, hoặc giống nhưng không có đường nối
        private void BoChon2O_22_Thao()
        {
            // Tắt viền đỏ của cả 2 ô 
            BoToSangO_22_Thao(oDangChon1_22_Thao);
            BoToSangO_22_Thao(oDangChon2_22_Thao);

            // Xóa trí nhớ của máy tính về 2 ô này
            DatLaiLuaChon_22_Thao();
        }

        // HÀM RESET TRẠNG THÁI BỘ NHỚ (CLEAR STATE)
        // Đưa hệ thống về trạng thái "Chưa chọn gì cả", chuẩn bị đón lượt click mới
        private void DatLaiLuaChon_22_Thao()
        {
            // Giải phóng 2 cái hình khỏi biến tạm
            oDangChon1_22_Thao = null;
            oDangChon2_22_Thao = null;

            // Point.Empty nghĩa là xóa tọa độ, đưa về tọa độ rỗng (không xác định)
            viTriO1_22_Thao = Point.Empty;
            viTriO2_22_Thao = Point.Empty;
        }

        // HÀM BẬT HIỆU ỨNG TÔ SÁNG 
        // Tạo viền đỏ cho ô hình khi người chơi click vào
        private void ToSangO_22_Thao(PictureBox oAnh_22_Thao)
        {
            // Phải check xem cái ô truyền vào có tồn tại không để tránh lỗi văng game
            if (oAnh_22_Thao != null)
            {
                // Lấy tọa độ (Dòng, Cột) đang cất giấu trong túi Tag của PictureBox
                Point viTri_22_Thao = (Point)oAnh_22_Thao.Tag;

                // Mò sang cái mảng Panel (dsKhungAnh) theo đúng tọa độ đó, đổi màu nền thành ĐỎ.
                dsKhungAnh_22_Thao[viTri_22_Thao.X, viTri_22_Thao.Y].BackColor = Color.Red;
            }
        }

        // HÀM TẮT HIỆU ỨNG TÔ SÁNG (UN-HIGHLIGHT)
        // Ngược lại với hàm trên, trả lại màu trong suốt cho cái viền
        private void BoToSangO_22_Thao(PictureBox oAnh_22_Thao)
        {
            if (oAnh_22_Thao != null)
            {
                Point viTri_22_Thao = (Point)oAnh_22_Thao.Tag;

                // Color.Transparent là màu trong suốt, làm cái viền đỏ biến mất
                dsKhungAnh_22_Thao[viTri_22_Thao.X, viTri_22_Thao.Y].BackColor = Color.Transparent;
            }
        }

        // HÀM KIỂM TRA ĐIỀU KIỆN CHIẾN THẮNG
        private bool KiemTraThang_22_Thao()
        {
            // Duyệt qua từng dòng trên bàn cờ
            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                // Duyệt qua từng cột tương ứng của dòng đó
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    // Kiểm tra giá trị logic tại tọa độ [Dòng, Cột] hiện tại
                    // QUY ƯỚC LOGIC: Số 0 đại diện cho ô trống (hình đã bị ăn).
                    // Nếu phát hiện BẤT KỲ MỘT Ô NÀO có giá trị khác 0 (tức là vẫn còn hình chưa bị ăn)
                    if (maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao] != 0)
                    {
                        // Áp dụng kỹ thuật Early Exit (Thoát sớm): 
                        // Trả về False ngay lập tức để tiết kiệm tài nguyên CPU thay vì phải quét hết ma trận.
                        return false;
                    }
                }
            }
            // Nếu hệ thống vượt qua được cả 2 vòng lặp (nghĩa là đã quét sạch toàn bộ các ô)
            // mà không hề bị vướng lệnh return false nào, thì chắc chắn 100% bàn cờ chỉ toàn số 0.
            // Kết luận: NGƯỜI CHƠI ĐÃ CHIẾN THẮNG!
            return true; 
        }
        // THUẬT TOÁN TÌM ĐƯỜNG ĐI NGẮN NHẤT (BREADTH-FIRST SEARCH - BFS)
        // Kiểm tra xem có thể nối 2 điểm bằng đường gấp khúc (tối đa 2 lần rẽ) hay không
        private bool TimDuongNoi_22_Thao(Point diem1_22_Thao, Point diem2_22_Thao)
        {
            // 1. KỸ THUẬT MỞ RỘNG MA TRẬN 
            // tạo ra một cái "hành lang" trống bao quanh bàn cờ.
            // Nhờ hành lang này, đường nối có thể đi vòng ra mép ngoài cùng của bàn cờ (trường hợp 2 hình nằm ở biên).
            int soDongMoRong_22_Thao = soDong_22_Thao + 2;
            int soCotMoRong_22_Thao = soCot_22_Thao + 2;

            int[,] maTranMoRong_22_Thao = new int[soDongMoRong_22_Thao, soCotMoRong_22_Thao];

            // Sao chép bàn cờ hiện tại vào CHÍNH GIỮA ma trận mở rộng. Các viền ngoài cùng tự động là 0 (đường trống).
            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    maTranMoRong_22_Thao[dong_22_Thao + 1, cot_22_Thao + 1] = maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao];
                }
            }

            // Tọa độ mới của 2 điểm cần nối trong ma trận mở rộng (phải cộng thêm 1)
            int dongBatDau_22_Thao = diem1_22_Thao.X + 1;
            int cotBatDau_22_Thao = diem1_22_Thao.Y + 1;
            int dongKetThuc_22_Thao = diem2_22_Thao.X + 1;
            int cotKetThuc_22_Thao = diem2_22_Thao.Y + 1;

            // Tạm thời coi 2 điểm xuất phát và đích là 0 (đường trống) để thuật toán có thể bước vào và bước ra
            maTranMoRong_22_Thao[dongBatDau_22_Thao, cotBatDau_22_Thao] = 0;
            maTranMoRong_22_Thao[dongKetThuc_22_Thao, cotKetThuc_22_Thao] = 0;

            // 2. KHAI BÁO CÁC HƯỚNG DI CHUYỂN
            // 4 hướng: Lên (-1,0), Xuống (1,0), Trái (0,-1), Phải (0,1)
            int[] huongDong_22_Thao = { -1, 1, 0, 0 };
            int[] huongCot_22_Thao = { 0, 0, -1, 1 };

            // 3. KHỞI TẠO MẢNG ĐÁNH DẤU (VISITED ARRAY) - 3 CHIỀU
            // [Dòng, Cột, Hướng đi tới ô đó]. Mảng này lưu lại SỐ LẦN RẼ ÍT NHẤT để tới được 1 ô.
            int[,,] danhDau_22_Thao = new int[soDongMoRong_22_Thao, soCotMoRong_22_Thao, 4];

            // Gán giá trị ban đầu là 99 (một số lớn rác) đại diện cho việc "chưa từng đi qua ô này"
            for (int dong_22_Thao = 0; dong_22_Thao < soDongMoRong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCotMoRong_22_Thao; cot_22_Thao++)
                {
                    for (int huong_22_Thao = 0; huong_22_Thao < 4; huong_22_Thao++)
                    {
                        danhDau_22_Thao[dong_22_Thao, cot_22_Thao, huong_22_Thao] = 99;
                    }
                }
            }

            // 4. TRIỂN KHAI HÀNG ĐỢI (QUEUE) CHO THUẬT TOÁN BFS
            Queue<TrangThaiTimDuong_22_Thao> hangDoi_22_Thao = new Queue<TrangThaiTimDuong_22_Thao>();

            // Nhét điểm xuất phát vào Hàng đợi. Bắt đầu với hướng -1 (chưa có hướng) và 0 lần rẽ.
            hangDoi_22_Thao.Enqueue(new TrangThaiTimDuong_22_Thao(dongBatDau_22_Thao, cotBatDau_22_Thao, -1, 0));

            // VÒNG LẶP CHÍNH CỦA BFS: Lan truyền tìm đường cho đến khi hàng đợi rỗng
            while (hangDoi_22_Thao.Count > 0)
            {
                // Lấy 1 trạng thái ở đầu hàng đợi ra để duyệt
                TrangThaiTimDuong_22_Thao trangThai_22_Thao = hangDoi_22_Thao.Dequeue();

                // NẾU TỚI ĐÍCH: Trả về True ngay lập tức
                if (trangThai_22_Thao.Dong_22_Thao == dongKetThuc_22_Thao &&
                    trangThai_22_Thao.Cot_22_Thao == cotKetThuc_22_Thao)
                {
                    return true;
                }

                // Tỏa ra 4 hướng từ vị trí hiện tại
                for (int huongMoi_22_Thao = 0; huongMoi_22_Thao < 4; huongMoi_22_Thao++)
                {
                    // TÍNH SỐ LẦN RẼ: Nếu hướng đang đi (-1 lúc xuất phát) khác với hướng chuẩn bị quẹo -> Nghĩa là phải Rẽ (+1)
                    int soLanReMoi_22_Thao = trangThai_22_Thao.Huong_22_Thao == -1 || trangThai_22_Thao.Huong_22_Thao == huongMoi_22_Thao
                        ? trangThai_22_Thao.SoLanRe_22_Thao
                        : trangThai_22_Thao.SoLanRe_22_Thao + 1;

                    // LUẬT PIKACHU: Không được rẽ quá 2 lần (Chữ L = 1 lần, Chữ U/Z = 2 lần). Quá 2 lần thì bỏ qua hướng này.
                    if (soLanReMoi_22_Thao > 2)
                        continue;

                    // Bước tới 1 bước theo hướng mới
                    int dongMoi_22_Thao = trangThai_22_Thao.Dong_22_Thao + huongDong_22_Thao[huongMoi_22_Thao];
                    int cotMoi_22_Thao = trangThai_22_Thao.Cot_22_Thao + huongCot_22_Thao[huongMoi_22_Thao];

                    // 5. KỸ THUẬT LOANG ĐƯỜNG THẲNG (SWEEP LINE)
                    // Thay vì đi từng bước 1 rồi dừng, vòng lặp while này sẽ "phóng" một tia đi thẳng mãi 
                    // cho đến khi đụng tường hoặc đụng một hình vật cản (khác 0).
                    while (dongMoi_22_Thao >= 0 && dongMoi_22_Thao < soDongMoRong_22_Thao &&
                           cotMoi_22_Thao >= 0 && cotMoi_22_Thao < soCotMoRong_22_Thao &&
                           (maTranMoRong_22_Thao[dongMoi_22_Thao, cotMoi_22_Thao] == 0 ||
                            (dongMoi_22_Thao == dongKetThuc_22_Thao && cotMoi_22_Thao == cotKetThuc_22_Thao)))
                    {
                        // Nếu tia này đi tới ô [dongMoi, cotMoi] với số lần rẽ ÍT HƠN KỶ LỤC CŨ đã ghi nhận tại ô đó
                        if (danhDau_22_Thao[dongMoi_22_Thao, cotMoi_22_Thao, huongMoi_22_Thao] > soLanReMoi_22_Thao)
                        {
                            // Cập nhật kỷ lục mới
                            danhDau_22_Thao[dongMoi_22_Thao, cotMoi_22_Thao, huongMoi_22_Thao] = soLanReMoi_22_Thao;

                            // Gói ghém trạng thái này nhét vào hàng đợi để lát nữa duyệt tiếp từ ô này
                            hangDoi_22_Thao.Enqueue(new TrangThaiTimDuong_22_Thao(
                                dongMoi_22_Thao,
                                cotMoi_22_Thao,
                                huongMoi_22_Thao,
                                soLanReMoi_22_Thao
                            ));
                        }

                        // Tiếp tục phóng tia đi thẳng thêm 1 ô nữa trong cùng hướng đó
                        dongMoi_22_Thao += huongDong_22_Thao[huongMoi_22_Thao];
                        cotMoi_22_Thao += huongCot_22_Thao[huongMoi_22_Thao];
                    }
                }
            }

            // Khám phá hết mọi ngóc ngách mà vẫn không đụng được Đích ->ko có đường nối hợp lệ nào cả -> Trả về False
            return false;
        }

        // HÀM QUÉT KIỂM TRA TÌNH TRẠNG BẾ TẮC CỦA BÀN CỜ 
        private bool KiemTraConDuongDi_22_Thao()
        {
            // 1. CHỌN Ô BẮT ĐẦU (Ô THỨ NHẤT)
            // 2 vòng lặp đầu tiên dùng để duyệt từng ô trên bàn cờ, đóng vai trò là "Điểm xuất phát"
            for (int dong1_22_Thao = 0; dong1_22_Thao < soDong_22_Thao; dong1_22_Thao++)
            {
                for (int cot1_22_Thao = 0; cot1_22_Thao < soCot_22_Thao; cot1_22_Thao++)
                {
                    // Nếu ô xuất phát là ô trống (đã bị ăn) thì bỏ qua ngay
                    if (maTranHinh_22_Thao[dong1_22_Thao, cot1_22_Thao] == 0)
                        continue;

                    // 2. CHỌN Ô ĐÍCH ĐẾN (Ô THỨ HAI)
                    // 2 vòng lặp lồng bên trong dùng để tìm "Điểm đích" so sánh với Điểm xuất phát
                    for (int dong2_22_Thao = 0; dong2_22_Thao < soDong_22_Thao; dong2_22_Thao++)
                    {
                        for (int cot2_22_Thao = 0; cot2_22_Thao < soCot_22_Thao; cot2_22_Thao++)
                        {
                            // Bỏ qua nếu ô đích CHÙNG TỌA ĐỘ với ô xuất phát (tự so sánh với chính mình)
                            if (dong1_22_Thao == dong2_22_Thao && cot1_22_Thao == cot2_22_Thao)
                                continue;

                            // Bỏ qua nếu ô đích là ô trống
                            if (maTranHinh_22_Thao[dong2_22_Thao, cot2_22_Thao] == 0)
                                continue; 
                            // Bỏ qua nếu giá trị (hình ảnh) của 2 ô KHÁC NHAU.
                            if (maTranHinh_22_Thao[dong1_22_Thao, cot1_22_Thao] != maTranHinh_22_Thao[dong2_22_Thao, cot2_22_Thao])
                                continue;

                            // 3. KIỂM TRA ĐƯỜNG NỐI (GỌI THUẬT TOÁN BFS)
                            // Nếu vượt qua được 4 lớp màng lọc "continue" ở trên, 2 ô này chắc chắn giống nhau.
                            // Giờ chỉ việc ném tọa độ của chúng cho hàm TimDuongNoi xử lý.
                            bool coDuongDi_22_Thao = TimDuongNoi_22_Thao(
                                new Point(dong1_22_Thao, cot1_22_Thao),
                                new Point(dong2_22_Thao, cot2_22_Thao)
                            );

                            // 4. KẾT LUẬN SỚM (EARLY EXIT)
                            // Chỉ cần phát hiện ĐÚNG MỘT CẶP có thể nối được với nhau, 
                            // lập tức báo True (Bàn cờ vẫn còn chơi được) và ngừng toàn bộ 4 vòng lặp.
                            if (coDuongDi_22_Thao == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            // Nếu chạy rã rời 4 vòng lặp, thử ghép đôi mọi con Pikachu trên bàn cờ mà vẫn không ra cặp nào nối được,
            // thì xác nhận là Bế tắc (Deadlock). Trả về False để hệ thống biết đường trộn lại bài.
            return false;
        }

        // HÀM TỰ ĐỘNG XÁO TRỘN BÀN CỜ
        // Được gọi khi hệ thống phát hiện không còn bất kỳ đường nối hợp lệ nào
        private void TronLaiCacHinhConLai_22_Thao()
        {
            // 1. THU THẬP CÁC PHẦN TỬ CÒN LẠI 
            // Tạo một danh sách động để gom tất cả các mã hình (ID) chưa bị ăn trên bàn cờ
            List<int> dsGiaTriConLai_22_Thao = new List<int>();

            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    // Chỉ nhặt những ô có giá trị khác 0 (nghĩa là đang chứa hình Pokemon)
                    if (maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao] != 0)
                    {
                        dsGiaTriConLai_22_Thao.Add(maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao]);
                    }
                }
            }

            // Nếu trên bàn cờ còn ít hơn 2 hình (không đủ tạo thành 1 cặp) thì không cần trộn
            if (dsGiaTriConLai_22_Thao.Count < 2)
                return;

            // 2. KHỞI TẠO BIẾN KIỂM SOÁT VÒNG LẶP 
            // Biến đếm số lần đã thử xáo trộn
            int soLanThu_22_Thao = 0;
            // Cờ hiệu (Flag) xác nhận xem đã có đường đi chưa
            bool daCoDuongDi_22_Thao = false; 

            // 3. VÒNG LẶP TÌM NGHIỆM KHẢ THI (Tối đa 200 lần thử)
            //  Giới hạn 200 lần để ngăn chặn lỗi Tràn bộ nhớ (Stack Overflow) 
            // hoặc Treo ứng dụng (Infinite Loop) trong trường hợp xui xẻo trộn hoài không ra.
            while (soLanThu_22_Thao < 200 && !daCoDuongDi_22_Thao)
            {
                // 3.1: Thuật toán xáo trộn ngẫu nhiên 
                for (int i_22_Thao = 0; i_22_Thao < dsGiaTriConLai_22_Thao.Count; i_22_Thao++)
                {
                    int j_22_Thao = random_22_Thao.Next(dsGiaTriConLai_22_Thao.Count);
                    int tam_22_Thao = dsGiaTriConLai_22_Thao[i_22_Thao];
                    dsGiaTriConLai_22_Thao[i_22_Thao] = dsGiaTriConLai_22_Thao[j_22_Thao];
                    dsGiaTriConLai_22_Thao[j_22_Thao] = tam_22_Thao;
                }

                // 3.2: Đổ dữ liệu đã trộn trở lại ma trận (Chỉ đổ vào các vị trí khác 0)
                int chiSo_22_Thao = 0;
                for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
                {
                    for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                    {
                        // Chỗ nào trống (0) thì vẫn là trống. Chỉ thay đổi ruột các ô còn hình.
                        if (maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao] != 0)
                        {
                            maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao] = dsGiaTriConLai_22_Thao[chiSo_22_Thao];
                            chiSo_22_Thao++;
                        }
                    }
                }

                // 3.3: Gọi lại hàm kiểm tra xem kết quả trộn vừa rồi có đẻ ra được đường đi nào không
                daCoDuongDi_22_Thao = KiemTraConDuongDi_22_Thao();

                // Tăng biến đếm lên. Nếu tới 200 mà vẫn false thì chấp nhận thoát vòng lặp.
                soLanThu_22_Thao++;
            }

            // 4. CẬP NHẬT GIAO DIỆN NGƯỜI DÙNG (UI RENDER)
            // Sau khi ma trận logic đã được trộn xong xuôi, gọi hàm này để đổi lại hình ảnh trên màn hình cho khớp.
            CapNhatLaiGiaoDienBanCo_22_Thao();
        }
        // HÀM ĐỒNG BỘ GIAO DIỆN TỪ DỮ LIỆU LOGIC 
        // Nhiệm vụ: Quét lại ma trận số và vẽ lại các ô Pikachu trên màn hình cho khớp.
        // Thường được gọi sau khi xáo trộn bàn cờ hoặc load lại game.
        private void CapNhatLaiGiaoDienBanCo_22_Thao()
        {
            // Duyệt qua toàn bộ tọa độ trên bàn cờ
            for (int dong_22_Thao = 0; dong_22_Thao < soDong_22_Thao; dong_22_Thao++)
            {
                for (int cot_22_Thao = 0; cot_22_Thao < soCot_22_Thao; cot_22_Thao++)
                {
                    // Lấy mã số hình (ID) từ ma trận dữ liệu (Core Logic)
                    int giaTriHinh_22_Thao = maTranHinh_22_Thao[dong_22_Thao, cot_22_Thao];

                    // Lấy đối tượng PictureBox tương ứng trên giao diện (UI)
                    PictureBox oAnh_22_Thao = dsOAnh_22_Thao[dong_22_Thao, cot_22_Thao];

                    // TRƯỜNG HỢP 1: Ô TRỐNG (Giá trị = 0)
                    if (giaTriHinh_22_Thao == 0)
                    {
                        // Xóa ảnh, ẩn đối tượng và khóa tương tác chuột
                        oAnh_22_Thao.Image = null;
                        oAnh_22_Thao.Visible = false;
                        oAnh_22_Thao.Enabled = false;

                        // Xóa luôn hiệu ứng viền đỏ (nếu còn sót lại do lỗi người dùng click nhanh)
                        if (dsKhungAnh_22_Thao != null)
                        {
                            dsKhungAnh_22_Thao[dong_22_Thao, cot_22_Thao].BackColor = Color.Transparent;
                        }
                    }
                    // TRƯỜNG HỢP 2: Ô CÒN HÌNH (Giá trị từ 1 đến 36)
                    else
                    {
                        // Truy xuất vào file Resources, ghép chuỗi "pieces" với mã số để nạp đúng hình Pokemon
                        oAnh_22_Thao.Image = (Image)Properties.Resources.ResourceManager.GetObject("pieces" + giaTriHinh_22_Thao);

                        // Hiển thị lại ảnh và mở khóa tương tác chuột để người chơi bấm được
                        oAnh_22_Thao.Visible = true;
                        oAnh_22_Thao.Enabled = true;

                        // Đảm bảo nền phía sau trong suốt, không bị dính màu viền cũ
                        if (dsKhungAnh_22_Thao != null)
                        {
                            dsKhungAnh_22_Thao[dong_22_Thao, cot_22_Thao].BackColor = Color.Transparent;
                        }
                    }
                }
            }
        }

        // HÀM QUẢN LÝ TRẠNG THÁI VÁN ĐẤU : KIỂM TRA VÀ XỬ LÝ BẾ TẮC
        // Hàm này hoạt động như một chốt chặn tự động chạy sau mỗi lần ghép cặp thành công.
        private void KiemTraVaTronLaiNeuHetDuong_22_Thao()
        {
            // 1. KIỂM TRA ĐIỀU KIỆN DỪNG 
            // Nếu người chơi vừa ăn cặp hình cuối cùng và Thắng game rồi, 
            // thì thoát hàm ngay lập tức.
            if (KiemTraThang_22_Thao())
                return;

            // 2. KÍCH HOẠT RADAR DÒ ĐƯỜNG 
            // Dấu chấm than (!) là phép phủ định. Câu lệnh này đọc là: 
            // "Nếu kết quả quét toàn bàn cờ là KHÔNG CÒN CẶP NÀO NỐI ĐƯỢC NỮA..."
            if (!KiemTraConDuongDi_22_Thao())
            {
                // 3. THÔNG BÁO CHO NGƯỜI CHƠI 
                MessageBox.Show("Hết đường đi, đang trộn lại bàn", "Thông báo_22_Thao");

                // 4. LÀM SẠCH BỘ NHỚ TẠM 
                // Phải reset lại 2 biến lưu tọa độ đang chọn (viTriO1, viTriO2). 
                // Nếu không reset, lát nữa trộn bài xong hình chạy đi chỗ khác, hệ thống sẽ bị lỗi tọa độ ảo.
                DatLaiLuaChon_22_Thao();

                // Gọi hàm xáo trộn lại các hình còn sống để sinh ra một thế cờ mới có thể giải được (Solvable).
                TronLaiCacHinhConLai_22_Thao();
            }
        }

        private void ptbScore_22_Thao_Click(object sender, EventArgs e)
        {

        }

        private void lbDiem_22_Thao_Click(object sender, EventArgs e)
        {

        }

        private void lbSwap_22_Thao_Click(object sender, EventArgs e)
        {

        }

        private void MainGame_22_Thao_Load(object sender, EventArgs e)
        {
            // Tắt theme của Windows trên thanhTG
            SetWindowTheme(thanhTG_22_Thao.Handle, "", "");
            // 1. GỌI HÀM KHỞI TẠO TỔNG THỂ
            // Hàm này sẽ tự động gọi KhoiTaoMaTran và VeBanCo với các thông số mặc định (hoặc tùy chỉnh sau này)
            BatDauGameMoi_22_Thao();

            // 2. THIẾT LẬP ĐIỂM SỐ BAN ĐẦU 
            CapNhatDiem_22_Thao();

            // 3. THIẾT LẬP THỜI GIAN 
            // Cài đặt tổng thời gian cho một ván chơi là 600 giây (tương đương 10 phút).
            thoiGianConLai_22_Thao = 600;

            // Đồng bộ con số 600 này lên giao diện 
            CapNhatThanhThoiGian_22_Thao();

            // 4. KÍCH HOẠT BỘ ĐẾM THỜI GIAN THỰC
            // Thuộc tính Interval quy định khoảng thời gian giữa 2 lần "Tick".
            // Đặt 1000 mili-giây (1000ms = 1 giây) để đảm bảo đồng hồ đếm ngược chuẩn xác theo thời gian thực.
            timer_22_Thao.Interval = 1000;

            // Bắt đầu cho phép Timer chạy ngầm. Cứ mỗi 1 giây nó sẽ tự động gọi hàm timer_Tick một lần.
            timer_22_Thao.Start();
            // Nạp đường dẫn file nhạc
            phatNhac_22_Thao.URL = Application.StartupPath + @"\Music\nhacMain.mp3";
            // Cho nhạc lặp lại vô tận
            phatNhac_22_Thao.settings.setMode("loop", true);
            // Chỉnh âm lượng 50% cho vừa tai
            phatNhac_22_Thao.settings.volume = 50;
            // Bắt đầu phát
            phatNhac_22_Thao.controls.play();               
        }

        private void btnSwap_22_Thao_Click(object sender, EventArgs e)
        { 
            // 1. LÀM SẠCH BỘ NHỚ TẠM (BUGS PREVENTION)
            DatLaiLuaChon_22_Thao();

            // 2. KÍCH HOẠT THUẬT TOÁN XÁO TRỘN
            TronLaiCacHinhConLai_22_Thao();
        }
        private void CapNhatDiem_22_Thao()
        {
            lbDiem_22_Thao.Text = diem_22_Thao.ToString();
        }
        // HÀM TÌM KIẾM CẶP HÌNH GỢI Ý
        // Trả về tọa độ của 2 ô đầu tiên trên bàn cờ có thể nối được với nhau.
        private Tuple<Point, Point> TimCapGoiY_22_Thao()
        {
            // Sử dụng thuật toán Vét cạn để quét các cặp ô
            for (int dong1_22_Thao = 0; dong1_22_Thao < soDong_22_Thao; dong1_22_Thao++)
            {
                for (int cot1_22_Thao = 0; cot1_22_Thao < soCot_22_Thao; cot1_22_Thao++)
                {
                    // Bỏ qua nếu ô bắt đầu là ô trống
                    if (maTranHinh_22_Thao[dong1_22_Thao, cot1_22_Thao] == 0)
                        continue;

                    for (int dong2_22_Thao = 0; dong2_22_Thao < soDong_22_Thao; dong2_22_Thao++)
                    {
                        for (int cot2_22_Thao = 0; cot2_22_Thao < soCot_22_Thao; cot2_22_Thao++)
                        {
                            // Không so sánh 1 ô với chính nó
                            if (dong1_22_Thao == dong2_22_Thao && cot1_22_Thao == cot2_22_Thao)
                                continue;

                            // Bỏ qua ô đích nếu là ô trống
                            if (maTranHinh_22_Thao[dong2_22_Thao, cot2_22_Thao] == 0)
                                continue;

                            // Bỏ qua nếu 2 ô mang hình ảnh khác nhau
                            if (maTranHinh_22_Thao[dong1_22_Thao, cot1_22_Thao] != maTranHinh_22_Thao[dong2_22_Thao, cot2_22_Thao])
                                continue;

                            // Gọi BFS kiểm tra đường đi nối 2 điểm
                            bool coDuongDi_22_Thao = TimDuongNoi_22_Thao(
                                new Point(dong1_22_Thao, cot1_22_Thao),
                                new Point(dong2_22_Thao, cot2_22_Thao)
                            );

                            // NẾU TÌM THẤY ĐƯỜNG ĐI ĐẦU TIÊN KHẢ THI
                            if (coDuongDi_22_Thao == true)
                            {
                                // CẤU TRÚC DỮ LIỆU TUPLE: 
                                // C# không cho phép return 2 biến cùng lúc. Do đó, ta gói 2 tọa độ Point 
                                // vào một cấu trúc Tuple để trả về trọn vẹn cả cặp cho nút Gợi Ý xử lý.
                                return new Tuple<Point, Point>(
                                    new Point(dong1_22_Thao, cot1_22_Thao),
                                    new Point(dong2_22_Thao, cot2_22_Thao)
                                );
                            }
                        }
                    }
                }
            }

            // Nếu chạy hết vòng lặp mà không tìm ra, trả về null.
            return null;
        }
       
        // HÀM HIỂN THỊ GỢI Ý (HINT UI HANDLER)
        // Nhiệm vụ: Tìm kiếm và tạo hiệu ứng nhấp nháy cho cặp hình có thể nối được
        // Sử dụng từ khóa 'async' để tạo độ trễ hiển thị mà không gây đơ giao diện
        private async void GoiYCapHinh_22_Thao()
        {
            // 1. TRUY VẤN DỮ LIỆU GỢI Ý
            // Gọi hàm thuật toán TimCapGoiY để lấy về một cặp tọa độ Tuple (Item1 và Item2)
            Tuple<Point, Point> capGoiY_22_Thao = TimCapGoiY_22_Thao();

            // 2. KIỂM TRA ĐIỀU KIỆN NGOẠI LỆ
            // Nếu kết quả trả về null (không tìm thấy cặp nào), thông báo cho người dùng
            if (capGoiY_22_Thao == null)
            {
                MessageBox.Show("Không còn cặp hợp lệ để gợi ý","Thông báo_22_Thao");
                return;
            }

            // 3. XÁC ĐỊNH ĐỐI TƯỢNG TRÊN GIAO DIỆN
            // Trích xuất tọa độ từ Tuple và truy xuất đối tượng PictureBox tương ứng từ mảng dsOAnh
            Point viTri1_22_Thao = capGoiY_22_Thao.Item1;
            Point viTri2_22_Thao = capGoiY_22_Thao.Item2;

            PictureBox o1_22_Thao = dsOAnh_22_Thao[viTri1_22_Thao.X, viTri1_22_Thao.Y];
            PictureBox o2_22_Thao = dsOAnh_22_Thao[viTri2_22_Thao.X, viTri2_22_Thao.Y];

            // 4. HIỂN THỊ HIỆU ỨNG THỊ GIÁC 
            // Gọi hàm ToSangO để tô viền đỏ cho cả 2 ô hình vừa tìm được
            ToSangO_22_Thao(o1_22_Thao);
            ToSangO_22_Thao(o2_22_Thao);

            // 5. TẠO ĐỘ TRỄ HIỂN THỊ 
            // Tạm dừng luồng thực thi trong 800ms (0.8 giây) để người chơi kịp quan sát vị trí gợi ý.
            // Sử dụng await Task.Delay giúp giải phóng luồng UI, đảm bảo các thành phần khác của game vẫn hoạt động.
            await System.Threading.Tasks.Task.Delay(800);

            // 6. DỌN DẸP HIỆU ỨNG 
            // Sau khi hết thời gian chờ, gọi hàm BoToSangO để trả lại trạng thái bình thường cho 2 ô
            BoToSangO_22_Thao(o1_22_Thao);
            BoToSangO_22_Thao(o2_22_Thao);
        }

        private void btnGoiY_22_Thao_Click(object sender, EventArgs e)
        {
            GoiYCapHinh_22_Thao();
        }
        // HÀM ĐỒNG BỘ GIAO DIỆN THỜI GIAN (TIME BAR & LABEL SYNC)
        // Nhiệm vụ: Cập nhật trạng thái của thanh ProgressBar và nhãn hiển thị số giây
        private void CapNhatThanhThoiGian_22_Thao()
        {
            // Đảm bảo thời gian còn lại không bao giờ bị âm
            if (thoiGianConLai_22_Thao < 0)
                thoiGianConLai_22_Thao = 0;

            // Đảm bảo thời gian còn lại không vượt quá tổng thời gian quy định
            if (thoiGianConLai_22_Thao > tongThoiGian_22_Thao)
                thoiGianConLai_22_Thao = tongThoiGian_22_Thao;

            //  CẬP NHẬT THANH TIẾN TRÌNH (PROGRESS BAR UPDATE)
            // Thiết lập giá trị nhỏ nhất và lớn nhất cho thanh thời gian 
            thanhTG_22_Thao.Minimum = 0;
            thanhTG_22_Thao.Maximum = tongThoiGian_22_Thao;

            // Gán giá trị hiện tại để thanh Progress Bar co ngắn lại hoặc dài ra tương ứng
            thanhTG_22_Thao.Value = thoiGianConLai_22_Thao;
            //gọi hàm đổi màu 
            CapNhatMauCauVong_22_Thao();
            // 3. HIỂN THỊ DẠNG VĂN BẢN (TEXTUAL FEEDBACK)
            // Cập nhật chuỗi ký tự hiển thị lên Label theo định dạng: "Thời gian hiện tại / Tổng thời gian"
            lbThoiGian_22_Thao.Text = thoiGianConLai_22_Thao.ToString() + "/" + tongThoiGian_22_Thao.ToString();
        }
        // SỰ KIỆN TICK CỦA BỘ ĐẾM THỜI GIAN (TIMER TICK EVENT)
        // Hàm này được hệ thống tự động gọi sau mỗi khoảng Interval (1000ms = 1 giây)
        // Đóng vai trò là luồng đếm ngược thời gian thực của trò chơi.
        private void timer_22_Thao_Tick(object sender, EventArgs e)
        {
            // 1. GIẢM TRỪ THỜI GIÁN
            // Cứ mỗi giây trôi qua, giảm biến đếm thời gian còn lại đi 1 đơn vị
            thoiGianConLai_22_Thao--;

            // 2. CẬP NHẬT HIỂN THỊ
            // Gọi hàm đồng bộ để vẽ lại thanh ProgressBar và số giây trên màn hình
            CapNhatThanhThoiGian_22_Thao();

            // 3. KIỂM TRA ĐIỀU KIỆN KẾT THÚC (LOOSE CONDITION)
            // Nếu thời gian đã chạm mốc 0 hoặc âm (do lỗi tính toán ngoài dự kiến)
            if (thoiGianConLai_22_Thao <= 0)
            {
                // Khóa giá trị thời gian ở mức 0 để giao diện hiển thị đẹp (không hiện số âm)
                thoiGianConLai_22_Thao = 0;
                CapNhatThanhThoiGian_22_Thao();

                // Kích hoạt kịch bản Thua cuộc: 
                // Hiện Panel Loose, dừng nhạc, dừng Timer và thông báo cho người chơi.
                XuLyThuaGame_22_Thao();
            }
        }
        // HÀM XỬ LÝ KHI NGƯỜI CHƠI THUA CUỘC 
        private void XuLyThuaGame_22_Thao()
        {
            // 1. DỪNG ĐỒNG HỒ VÀ KHÓA TƯƠNG TÁC NGAY LẬP TỨC
            timer_22_Thao.Stop();
            // Khóa bàn cờ
            panelPikachu_22_Thao.Enabled = false;
            // Khóa nút đổi bài
            btnSwap_22_Thao.Enabled = false;
            // Khóa nút gợi ý
            btnGoiY_22_Thao.Enabled = false;
            // Khóa luôn nút tùy chỉnh
            btnTuyChinh_22_Thao.Enabled = false;   

            // 2. TÍNH TOÁN VỊ TRÍ CĂN GIỮA CHO PANEL THUA
            // Công thức chuẩn để vào giữa Form
            pnlLooseGame_22_Thao.Left = (this.ClientSize.Width - pnlLooseGame_22_Thao.Width) / 2;
            pnlLooseGame_22_Thao.Top = (this.ClientSize.Height - pnlLooseGame_22_Thao.Height) / 2;

            // 3. HIỂN THỊ VÀ ĐƯA LÊN LỚP TRÊN CÙNG
            pnlLooseGame_22_Thao.Visible = true;
            pnlLooseGame_22_Thao.BringToFront();

            // 4. ĐỔ DỮ LIỆU ĐIỂM VÀ THỜI GIAN
            lbDiemThua_22_Thao.Text = diem_22_Thao.ToString();
            // Tính thời gian đã chơi 
            int thoiGianDaSai_22_Thao = tongThoiGian_22_Thao - thoiGianConLai_22_Thao;
            lbTGThua_22_Thao.Text = thoiGianDaSai_22_Thao.ToString() + "s";
        }

        // HÀM XỬ LÝ KHI NGƯỜI CHƠI CHIẾN THẮNG (HIỆN PANEL WIN)
        private void XuLyThangGame_22_Thao()
        {
            // 1. DỪNG GAME VÀ KHÓA TƯƠNG TÁC
            timer_22_Thao.Stop();
            // Khóa bàn cờ, không cho bấm thêm
            panelPikachu_22_Thao.Enabled = false;
            // Khóa nút đổi bài
            btnSwap_22_Thao.Enabled = false;
            // Khóa nút gợi ý
            btnGoiY_22_Thao.Enabled = false;
            // Khóa luôn nút tùy chỉnh
            btnTuyChinh_22_Thao.Enabled = false;   

            // 2. ĐỔ DỮ LIỆU KẾT QUẢ
            lbDiemWin_22_Thao.Text = diem_22_Thao.ToString();
            int thoiGianDaSai_22_Thao = tongThoiGian_22_Thao - thoiGianConLai_22_Thao;
            lbTGWin_22_Thao.Text = thoiGianDaSai_22_Thao.ToString() + "s";
            // 3. CĂN GIỮA PANEL THẮNG CUỘC
            // Công thức: (Rộng Form - Rộng Panel) / 2
            pnlWinGame_22_Thao.Left = (this.ClientSize.Width - pnlWinGame_22_Thao.Width) / 2;
            pnlWinGame_22_Thao.Top = (this.ClientSize.Height - pnlWinGame_22_Thao.Height) / 2;
            // 4. HIỂN THỊ VÀ ĐƯA LÊN LỚP TRÊN CÙNG
            pnlWinGame_22_Thao.Visible = true;
            pnlWinGame_22_Thao.BringToFront();
        }

        private void btnThoat_22_Thao_Click(object sender, EventArgs e)
        {
            // 1.TẠM DỪNG GAME(PAUSE)
            timer_22_Thao.Stop();

            // 2. HIỂN THỊ HỘP THOẠI XÁC NHẬN
            // Tạo một thông báo với 2 lựa chọn Yes/No và biểu tượng câu hỏi (Question)
            DialogResult ketQua_22_Thao = MessageBox.Show("Bạn có chắc là muốn thoát game không?","Xác nhận Thoát_22_Thao",MessageBoxButtons.YesNo,MessageBoxIcon.Question );

            // 3. XỬ LÝ KẾT QUẢ TRẢ VỀ
            if (ketQua_22_Thao == DialogResult.Yes)
            {
                // Nếu chọn YES: Đóng Form hiện tại và tắt ứng dụng
                this.Close();
            }
            else
            {
                // Nếu chọn NO: Cho Timer chạy tiếp để người chơi "cày" tiếp
                timer_22_Thao.Start();
            }
        }

        private void btnNhac_22_Thao_Click(object sender, EventArgs e)
        {
            if (dangBatNhac_22_Thao)
            {
                // Nếu đang bật thì Tạm dừng (Pause)
                phatNhac_22_Thao.controls.pause();
                dangBatNhac_22_Thao = false;

                // Thay đổi giao diện nút (ví dụ đổi chữ hoặc đổi ảnh icon Loa gạch chéo)
                btnNhac_22_Thao.Image = Properties.Resources.mute;
            }
            else
            {
                // Nếu đang tắt thì Phát tiếp (Play)
                phatNhac_22_Thao.controls.play();
                dangBatNhac_22_Thao = true;

                btnNhac_22_Thao.Image = Properties.Resources.sound150;
            }
        }

        private void btnTuyChinh_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. TẠM DỪNG ĐỒNG HỒ 
            timer_22_Thao.Stop();

            // 2. KHỞI TẠO VÀ HIỂN THỊ FORM TÙY CHỈNH
            // Sử dụng lệnh 'using' để Form Tùy Chỉnh tự giải phóng bộ nhớ sau khi dùng xong
            using (FormTuyChinh_22_Thao frm = new FormTuyChinh_22_Thao())
            {
                // 3. KIỂM TRA KẾT QUẢ TRẢ VỀ
                // ShowDialog sẽ chặn màn hình Main cho đến khi Form Tùy Chỉnh đóng lại
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    // 4. CẬP NHẬT THÔNG SỐ MỚI
                    // Lấy dữ liệu từ các thuộc tính public của Form Tùy Chỉnh gán cho Form Main
                    this.soDong_22_Thao = frm.SoHang_22_Thao;
                    this.soCot_22_Thao = frm.SoCot_22_Thao;
                    this.soLoaiHinh_22_Thao = frm.SoLoaiPokemon_22_Thao;
                    this.tongThoiGian_22_Thao = frm.ThoiGian_22_Thao;

                    // 5. KHỞI CHẠY VÁN MỚI VỚI CẤU HÌNH VỪA CHỈNH
                    // Hàm này sẽ tự động gọi KhoiTaoMaTran và VeBanCo theo thông số mới
                    BatDauGameMoi_22_Thao();
                }
                else
                {
                    // Nếu người dùng nhấn Hủy hoặc dấu X, cho Timer chạy tiếp ván cũ
                    timer_22_Thao.Start();
                }
            }
        }

        private void btnThoatThua_22_Thao_Click(object sender, EventArgs e)
        {
            // Hiện bảng xác nhận 
            DialogResult r_22_Thao = MessageBox.Show("Bạn muốn thoát game?", "Xác nhận_22_Thao", MessageBoxButtons.YesNo);
            if (r_22_Thao == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnChoiMoiWin_22_Thao_Click(object sender, EventArgs e)
        {
            // Ẩn cái Panel thắng đi để thấy bàn cờ mới
            pnlWinGame_22_Thao.Visible = false;

            // Mở khóa lại nút Tùy chỉnh 
            btnTuyChinh_22_Thao.Enabled = true;

            // Gọi hàm để reset game từ đầu
            BatDauGameMoi_22_Thao();
        }

        private void btnThoatWin_22_Thao_Click(object sender, EventArgs e)
        {
            // Hiện bảng xác nhận 
            DialogResult r_22_Thao = MessageBox.Show("Bạn muốn thoát game?", "Xác nhận_22_Thao", MessageBoxButtons.YesNo);
            if (r_22_Thao == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnChoiLaiThua_22_Thao_Click(object sender, EventArgs e)
        {
            // Ẩn cái Panel thua đi để thấy bàn cờ mới
            pnlLooseGame_22_Thao.Visible = false;

            // Mở khóa lại nút Tùy chỉnh 
            btnTuyChinh_22_Thao.Enabled = true;

            // Gọi hàm để reset game từ đầu
            BatDauGameMoi_22_Thao();
        }

        private void CapNhatMauCauVong_22_Thao()
        {
            // 1. DANH SÁCH 7 MÀU CẦU VỒNG (Đỏ, Cam, Vàng, Lục, Lam, Chàm, Tím)
            Color[] bangMau_22_Thao = {Color.Red, Color.Orange,Color.Yellow,Color.Green,Color.Blue,Color.Indigo, Color.Purple};

            // 2. CHỌN MÀU DỰA TRÊN THỜI GIAN
            // Dùng phép chia lấy dư (%) để màu nó xoay vòng liên tục theo giây
            int chiSoMau_22_Thao = thoiGianConLai_22_Thao % bangMau_22_Thao.Length;
            Color mauHienTai_22_Thao = bangMau_22_Thao[chiSoMau_22_Thao];

            // 3. GỬI LỆNH ĐỔI MÀU (Sử dụng SendMessage đã khai báo ở bước trước)
            int mauWindows_22_Thao = ColorTranslator.ToWin32(mauHienTai_22_Thao);
            SendMessage(thanhTG_22_Thao.Handle, PBM_SETBARCOLOR, IntPtr.Zero, (IntPtr)mauWindows_22_Thao);
        }
        private void MainGame_22_Thao_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Lệnh này sẽ đóng tất cả các Form đang chạy (kể cả cái Menu đang bị ẩn)
            // và giải phóng hoàn toàn bộ nhớ của chương trình.
            Application.Exit();
        }
    }
}
