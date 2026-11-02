using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// Thêm thư viện xử lý âm thanh
using WMPLib;
namespace _22_Thao_N2_DoAnPikachu
{
    public partial class Menu_22_Thao : Form
    {
        // Khởi tạo đối tượng phát nhạc
        WindowsMediaPlayer nhacNen_22_Thao = new WindowsMediaPlayer();
        // Cờ kiểm soát trạng thái (mặc định mở form là có nhạc)
        bool isMusicPlaying_22_Thao = true;
        public Menu_22_Thao()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // Gắn đường dẫn file nhạc 
            nhacNen_22_Thao.URL = Application.StartupPath + @"\Music\The Pikachu Song Pokemon.wav";
            // Thiết lập chế độ lặp lại (loop) khi kết thúc bài
            nhacNen_22_Thao.settings.setMode("loop", true);
            // Bắt đầu phát
            nhacNen_22_Thao.controls.play(); 
        }

        private void btnCachChoi_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. Khởi tạo đối tượng Form Hướng dẫn
            FormHuongDan_22_Thao formHuongDan_22_Thao = new FormHuongDan_22_Thao();

            // 2. Hiển thị Form dưới dạng cửa sổ Modal (bắt buộc tương tác để đóng)
            formHuongDan_22_Thao.ShowDialog();
        }

        private void btnChoi_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. Dừng nhạc của Menu (nếu có)
            if (isMusicPlaying_22_Thao)
            {
                nhacNen_22_Thao.controls.stop();
            }
            // 1. Tạo đối tượng Form chơi game mới
            MainGame_22_Thao formGame_22_Thao = new MainGame_22_Thao();

            // 2. Hiển thị Form Game lên màn hình
            formGame_22_Thao.Show();

            // 3. ẩn Form Menu hiện tại đi 
            this.Hide();
        }

        private void btnThoat_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. Hiển thị hộp thoại xác nhận và lưu kết quả người dùng chọn vào biến xacNhan
            DialogResult xacNhan_22_Thao = MessageBox.Show("Bạn có chắc chắn muốn thoát khỏi trò chơi?","Xác nhận thoát_22_Thao", MessageBoxButtons.YesNo,MessageBoxIcon.Question);

            // 2. Kiểm tra nếu người dùng nhấn vào nút Yes
            if (xacNhan_22_Thao == DialogResult.Yes)
            {
                // Thực hiện đóng toàn bộ ứng dụng 
                Application.Exit();
            }
        }

        private void btnNhac_22_Thao_Click(object sender, EventArgs e)
        {
            // Trường hợp nhạc ĐANG PHÁT
            if (isMusicPlaying_22_Thao)
            {
                // Đang hát -> Bấm thì TẠM DỪNG
                nhacNen_22_Thao.controls.pause();

                // Thay hình loa tắt
                btnNhac_22_Thao.Image = Properties.Resources.mute;
                isMusicPlaying_22_Thao = false;
            }
            // Trường hợp nhạc ĐANG DỪNG
            else
            {
                // Đang tắt -> Bấm thì HÁT TIẾP KHÚC ĐANG DỪNG
                nhacNen_22_Thao.controls.play();

                // Thay hình loa bật
                btnNhac_22_Thao.Image = Properties.Resources.sound150;
                isMusicPlaying_22_Thao = true;
            }
        }
    }
}
