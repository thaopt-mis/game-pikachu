using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22_Thao_N2_DoAnPikachu
{
    public partial class FormTuyChinh_22_Thao : Form
    {
        // Tạo các thuộc tính để Form Main có thể lấy dữ liệu sau khi nhấn Xác nhận
        public int SoHang_22_Thao { get; set; }
        public int SoCot_22_Thao { get; set; }
        public int SoLoaiPokemon_22_Thao { get; set; }
        public int ThoiGian_22_Thao { get; set; }
        public FormTuyChinh_22_Thao()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // 1. Chỉnh giới hạn cho ô Thời gian 
            nudThoiGian_22_Thao.Maximum = 3600;
            nudThoiGian_22_Thao.Minimum = 10; 

            // 2. Chỉnh luôn cho số hàng và số cột
            nudSoHang_22_Thao.Maximum = 50;
            nudSoCot_22_Thao.Maximum = 50;

            // 3. Số loại Pokemon thì vẫn nên để tối đa là 36 
            nudSoLoaiPokemon_22_Thao.Maximum = 36;
            nudSoLoaiPokemon_22_Thao.Minimum = 1;
        }

        private void FormTuyChinh_22_Thao_Load(object sender, EventArgs e)
        {

        }

        private void ptbXacNhan_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. Xóa các thông báo lỗi cũ
            errorProvider_22_Thao.Clear();
            bool coLoi_22_Thao = false;

            int h = (int)nudSoHang_22_Thao.Value;
            int c = (int)nudSoCot_22_Thao.Value;
            int soLoai = (int)nudSoLoaiPokemon_22_Thao.Value;
            int tongSoO = h * c;

            // 2. KIỂM TRA TỔNG SỐ Ô (Phải chẵn)
            if (tongSoO % 2 != 0)
            {
                errorProvider_22_Thao.SetError(nudSoHang_22_Thao, "Tổng số ô (Hàng x Cột) phải là số chẵn!");
                errorProvider_22_Thao.SetError(nudSoCot_22_Thao, "Tổng số ô (Hàng x Cột) phải là số chẵn!");
                coLoi_22_Thao = true;
            }

            // 3. KIỂM TRA SỐ LOẠI POKEMON (QUAN TRỌNG NÈ)
            // Công thức: Số loại <= Tổng số ô / 2
            if (soLoai > tongSoO / 2)
            {
                errorProvider_22_Thao.SetError(nudSoLoaiPokemon_22_Thao,
                    $"Bàn cờ chỉ có {tongSoO / 2} cặp, chọn {soLoai} loại là không đủ chỗ chứa!");
                coLoi_22_Thao = true;
            }

            // 4. NẾU CÓ LỖI THÌ DỪNG LẠI, KHÔNG CHO LƯU
            if (coLoi_22_Thao) return;

            // Nếu mọi thứ OK thì mới đóng Form và truyền dữ liệu về
            this.SoHang_22_Thao = h;
            this.SoCot_22_Thao = c;
            this.SoLoaiPokemon_22_Thao = soLoai;
            this.ThoiGian_22_Thao = (int)nudThoiGian_22_Thao.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ptbHuy_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. HIỂN THỊ HỘP THOẠI XÁC NHẬN
            // Tạo thông báo với 2 nút Yes/No và biểu tượng cảnh báo (Warning)
            DialogResult ketQua_22_Thao = MessageBox.Show( "Bạn có chắc muốn hủy bỏ các thay đổi và quay lại game không?", "Xác nhận Hủy_22_Thao", MessageBoxButtons.YesNo,MessageBoxIcon.Warning );

            // 2. KIỂM TRA LỰA CHỌN CỦA NGƯỜI DÙNG
            if (ketQua_22_Thao == DialogResult.Yes)
            {
                // Nếu chọn Yes: Đặt kết quả là Cancel (để Form Main biết không cần reset game)
                this.DialogResult = DialogResult.Cancel;

                // Đóng Form tùy chỉnh
                this.Close();
            }
            // Nếu chọn No: Không làm gì cả, người dùng ở lại Form để chỉnh tiếp
        }
    }
}
