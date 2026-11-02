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
    public partial class FormHuongDan_22_Thao : Form
    {
        public FormHuongDan_22_Thao()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnChoi_22_Thao_Click(object sender, EventArgs e)
        {
            // 1. TÌM VÀ ẨN FORM MENU
            // Tìm trong danh sách các Form đang mở, nào tên là Menu_22_Thao thì ẩn nó đi
            Form frmMenu_22_Thao = Application.OpenForms["Menu_22_Thao"];
            if (frmMenu_22_Thao != null)
            {
                frmMenu_22_Thao.Hide();
            }

            // 2. MỞ GAME CHÍNH
            MainGame_22_Thao frmGame_22_Thao = new MainGame_22_Thao();
            frmGame_22_Thao.Show();

            // 3. ĐÓNG FORM HƯỚNG DẪN
            this.Close();
        }

        private void btnQuayLai_22_Thao_Click(object sender, EventArgs e)
        {
        // Đóng Form Hướng dẫn và quay lại Form Menu
            this.Close();
        }
    }
}
