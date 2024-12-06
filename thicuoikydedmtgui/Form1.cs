using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace thicuoikydedmtgui
{
    public partial class Form1 : Form
    {
        string connectString = @"Data Source=hai\SQLEXPRESS;Initial Catalog=dmtgui;Integrated Security=True;Encrypt=False";
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter adt;//cau noi giua datatable vs sql server
        DataTable dt = new DataTable();//tuong tac vs grid data view
        public Form1()
        {
            InitializeComponent();
            con = new SqlConnection(connectString);
            loadData();

            LoadChatLieuComboBox();

        }



        private void loadData()
        {
            try
            {
                dt.Clear();
                con.Open(); // Mở kết nối thủ công

                // Sử dụng INNER JOIN để lấy tên phòng ban từ bảng PhongBan
                cmd = new SqlCommand(
                    "SELECT NhanVien.MaNV, NhanVien.TenNV, NhanVien.SoDT, NhanVien.GioiTinh, " +
                    "NhanVien.Anh, PhongBan.TenPhongBan AS TenPhongBan, NhanVien.MucLuong " +
                    "FROM NhanVien " +
                    "INNER JOIN PhongBan ON NhanVien.MaPhongBan = PhongBan.MaPhongBan", con);

                adt = new SqlDataAdapter(cmd);
                adt.Fill(dt);
                con.Close(); // Đóng kết nối thủ công

                dataGridView1.DataSource = dt;

                // Đặt tiêu đề cho các cột trong DataGridView
                dataGridView1.Columns["MaNV"].HeaderText = "Mã Nhân Viên";
                dataGridView1.Columns["TenNV"].HeaderText = "Tên Nhân Viên";
                dataGridView1.Columns["SoDT"].HeaderText = "Số điện thoại";
                dataGridView1.Columns["GioiTinh"].HeaderText = "Giới tính";
                dataGridView1.Columns["Anh"].HeaderText = "Ảnh";
                dataGridView1.Columns["TenPhongBan"].HeaderText = "Tên Phòng Ban"; // Đổi tên cột
                dataGridView1.Columns["MucLuong"].HeaderText = "Mức Lương";

                // Đặt màu chữ cho tất cả ô
                dataGridView1.DefaultCellStyle.ForeColor = Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }


        private void LoadChatLieuComboBox()
        {
            try
            {
                con.Open();
                string query = "SELECT MaPhongBan, TenPhongBan FROM PhongBan";
                SqlCommand command = new SqlCommand(query, con);

                SqlDataReader reader = command.ExecuteReader();
                List<PhongBan> dsPhongBan = new List<PhongBan>();

                while (reader.Read())
                {
                    string maPB =reader["MaPhongBan"].ToString();
                    string tenPB = reader["TenPhongBan"].ToString();

                    PhongBan phongBan = new PhongBan(maPB, tenPB);
                    dsPhongBan.Add(phongBan);
                }
                reader.Close();

                comboBoxPhongBan.DataSource = dsPhongBan;
                comboBoxPhongBan.DisplayMember = "TenPhongBan"; // Hiển thị tên
                comboBoxPhongBan.ValueMember = "MaPhongBan";   // Giá trị thực là mã phòng ban
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách phòng ban: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }




        private void btnAnh_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.Title = "Chọn ảnh đại diện";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string imagePath = openFileDialog1.FileName;
                pictureBox1.Image = Image.FromFile(imagePath);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }


        private void btnThem_Click(object sender, EventArgs e)
        {
            // Kiểm tra các trường dữ liệu cần nhập
            if (string.IsNullOrEmpty(textBoxMaNV.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã NV.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxTenNV.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên NV.");
                return;
            }

            // Kiểm tra Giới tính
            if (!radioButtonNam.Checked && !radioButtonNu.Checked)
            {
                MessageBox.Show("Vui lòng chọn Giới tính.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxMucLuong.Text))
            {
                MessageBox.Show("Vui lòng nhập Mức lương.");
                return;
            }

            // Nếu vượt qua tất cả các kiểm tra, tiếp tục xử lý thêm nhân viên.


            foreach (DataRow row in dt.Rows)
            {
                if (row["MaNV"].ToString() == textBoxMaNV.Text)
                {
                    MessageBox.Show("Mã nhân viên đã tồn tại. Vui lòng nhập mã khác.");
                    textBoxMaNV.Focus();
                    return;
                }
            }






            string maNV = textBoxMaNV.Text;
            string tenNV = textBoxTenNV.Text;
            string soDT = textBoxSDT.Text;
            string gioiTinh = radioButtonNam.Checked ? "Nam" : "Nữ";
            string anh = openFileDialog1.FileName != "" ? SaveImageToLocal(openFileDialog1.FileName) : ""; // Lưu ảnh vào thư mục và lấy tên ảnh
            decimal mucLuong = Convert.ToDecimal(textBoxMucLuong.Text);


            // Kiểm tra xem người dùng có chọn phòng ban trong ComboBox không


            string maPhongBan = (string)comboBoxPhongBan.SelectedValue;

            // Tiếp tục xử lý


            try
            {
                con.Open();

                cmd = new SqlCommand("INSERT INTO NhanVien (MaNV, TenNV,SoDT, GioiTinh, Anh,MaPhongBan,MucLuong) VALUES (@MaNV, @TenNV, @SoDT, @GioiTinh, @Anh, @MaPhongBan, @MucLuong)", con);
                cmd.Parameters.AddWithValue("@MaNV", maNV);
                cmd.Parameters.AddWithValue("@TenNV", tenNV);
                cmd.Parameters.AddWithValue("@SoDT", soDT);
                cmd.Parameters.AddWithValue("@GioiTinh", gioiTinh);
                cmd.Parameters.AddWithValue("@Anh", anh);
                cmd.Parameters.AddWithValue("@MaPhongBan", maPhongBan);
                cmd.Parameters.AddWithValue("@MucLuong", mucLuong);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Thêm vật liệu thành công!");
                con.Close(); // Đóng kết nối thủ công
                clearTextBoxes();
                loadData();


            }

            finally { con.Close(); }

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            // Kiểm tra các trường dữ liệu cần nhập
            if (string.IsNullOrEmpty(textBoxMaNV.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã NV.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxTenNV.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên NV.");
                return;
            }

            // Kiểm tra Giới tính
            if (!radioButtonNam.Checked && !radioButtonNu.Checked)
            {
                MessageBox.Show("Vui lòng chọn Giới tính.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxMucLuong.Text))
            {
                MessageBox.Show("Vui lòng nhập Mức lương.");
                return;
            }

            string maNV = textBoxMaNV.Text;
            string tenNV = textBoxTenNV.Text;
            string soDT = textBoxSDT.Text;
            string gioiTinh = radioButtonNam.Checked ? "Nam" : "Nữ";
            string anh = openFileDialog1.FileName != "" ? SaveImageToLocal(openFileDialog1.FileName) : ""; // Lưu ảnh vào thư mục và lấy tên ảnh
            decimal mucLuong = Convert.ToDecimal(textBoxMucLuong.Text);
            string currentImage = GetCurrentImagePath(maNV);
            if (string.IsNullOrEmpty(anh) && !string.IsNullOrEmpty(currentImage))
            {
                anh = currentImage; // Giữ nguyên ảnh cũ nếu không chọn ảnh mới
            }

            string maPhongBan = (string)comboBoxPhongBan.SelectedValue;


            try
            {
                con.Open();
                // Cập nhật thông tin nhân viên
                cmd = new SqlCommand("UPDATE NhanVien SET TenNV = @TenNV, SoDT = @SoDT, GioiTinh = @GioiTinh, Anh = @Anh, MaPhongBan = @MaPhongBan, MucLuong = @MucLuong WHERE MaNV = @MaNV", con);
                cmd.Parameters.AddWithValue("@MaNV", maNV);
                cmd.Parameters.AddWithValue("@TenNV", tenNV);
                cmd.Parameters.AddWithValue("@SoDT", soDT);
                cmd.Parameters.AddWithValue("@GioiTinh", gioiTinh);
                cmd.Parameters.AddWithValue("@Anh", anh);
                cmd.Parameters.AddWithValue("@MaPhongBan", maPhongBan);
                cmd.Parameters.AddWithValue("@MucLuong", mucLuong);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Sửa thông tin nhân viên thành công!");
                con.Close(); // Đóng kết nối thủ công
                clearTextBoxes();
                loadData(); // Tải lại dữ liệu
                textBoxMaNV.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa thông tin: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }


        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Kiểm tra Mã NV đã được chọn
            if (string.IsNullOrEmpty(textBoxMaNV.Text))
            {
                MessageBox.Show("Vui lòng chọn nhân viên để xóa.");
                return;
            }

            string maNV = textBoxMaNV.Text;

            // Xác nhận việc xóa
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên {maNV}?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                try
                {
                    con.Open();
                    // Câu lệnh xóa nhân viên
                    cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV = @MaNV", con);
                    cmd.Parameters.AddWithValue("@MaNV", maNV);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Xóa nhân viên thành công!");

                    con.Close();
                    clearTextBoxes();
                    loadData(); // Tải lại dữ liệu sau khi xóa
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa nhân viên: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }


        private void btnThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có muốn thoát không?", "Xác nhận", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }



        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra xem có click vào hàng không
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex]; // Lấy hàng vừa click

                textBoxMaNV.Text = row.Cells["MaNV"].Value.ToString();
                textBoxTenNV.Text = row.Cells["TenNV"].Value.ToString();
                textBoxSDT.Text = row.Cells["SoDT"].Value.ToString();

                string gioiTinh = row.Cells["GioiTinh"].Value.ToString();
                if (gioiTinh.Equals("Nam"))
                    radioButtonNam.Checked = true;
                else
                    radioButtonNu.Checked = true;


                if (row.Cells["Anh"].Value != DBNull.Value)
                {
                    string imageFile = row.Cells["Anh"].Value.ToString();
                    string imagePath = Path.Combine(Application.StartupPath, "Images", imageFile);
                    if (!File.Exists(imagePath))
                    {
                        pictureBox1.Image = null;

                    }
                    else
                    {
                        pictureBox1.Image = Image.FromFile(imagePath);
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;


                    }

                }

                textBoxMucLuong.Text = row.Cells["MucLuong"].Value.ToString();

                string maNhanVien = dataGridView1.Rows[e.RowIndex].Cells["MaNV"].Value.ToString();

                // Lấy mã phòng ban từ cơ sở dữ liệu dựa vào mã nhân viên
                string maPB = GetMaPhongBan(maNhanVien);
                // Hiển thị lại ComboBox với Phòng Ban đã được chọn
                //int maPhongBan = Convert.ToInt32(row.Cells["MaPhongBan"].Value); // Lấy MaPhongBan từ dòng hiện tại

                string maPhongBan =maPB;
                // Đặt giá trị cho ComboBox
                comboBoxPhongBan.SelectedValue = maPhongBan;

                // Kích hoạt các nút
                btnSua.Enabled = true;
                btnXoa.Enabled = true;
                textBoxMaNV.Enabled = false;
                btnThem.Enabled = false;
            }
        }

        private string GetMaPhongBan(string maNhanVien)
        {
            string maPhongBan = string.Empty;
            string query = "SELECT MaPhongBan FROM NhanVien WHERE MaNV = @MaNhanVien";

            using (SqlConnection conn = new SqlConnection(connectString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaNhanVien", maNhanVien);

                conn.Open();
                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    maPhongBan = result.ToString();
                }
            }

            return maPhongBan;
        }

        private string GetCurrentImagePath(string maNV)
        {
            string currentImagePath = null;

            try
            {
                using (SqlConnection con = new SqlConnection(connectString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Anh FROM NhanVien WHERE MaNV = @MaNV", con))
                    {
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            currentImagePath = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy ảnh cũ: " + ex.Message);
            }

            return currentImagePath;
        }


        private string SaveImageToLocal(string imagePath)
        {

            if (string.IsNullOrEmpty(imagePath))
            {
                return "";  // Hoặc bạn có thể trả về một giá trị khác tùy theo nhu cầu
            }

            if (!File.Exists(imagePath))
            {
                return "";  // Trả về null nếu file không tồn tại
            }

            string imageDirectory = Path.Combine(Application.StartupPath, "Images");
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);  // Tạo thư mục nếu chưa tồn tại
            }

            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string extension = Path.GetExtension(imagePath);
            TimeSpan timeNow = DateTime.Now.TimeOfDay;
            int totalSeconds = (int)timeNow.TotalSeconds;
            string newFileName = $"{fileName}_{totalSeconds}{extension}";

            string destinationPath = Path.Combine(imageDirectory, newFileName);
            File.Copy(imagePath, destinationPath, true);  // Lưu ảnh vào thư mục Images

            return newFileName;  // Trả về tên file để lưu vào CSDL
        }



        private void clearTextBoxes()
        {
            textBoxMaNV.Clear();
            textBoxTenNV.Clear();
            textBoxSDT.Clear();
            textBoxMucLuong.Clear();
            radioButtonNam.Checked = false;
            radioButtonNu.Checked = false;

            // Đặt lại ComboBox (nếu có)
            comboBoxPhongBan.SelectedIndex = -1;
            pictureBox1.Image = null;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThem.Enabled = true;
        }




        private void btnInRaExcel_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("DanhSachMatHang");

                    // Thêm tiêu đề
                    worksheet.Cell("B1").Value = "Đào Minh Hải";
                    worksheet.Range("B1:H1").Merge();
                    worksheet.Range("B1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range("B1:H1").Style.Font.FontColor = XLColor.Blue;

                    worksheet.Cell("B2").Value = "Địa Chỉ: VIETNAMESE";
                    worksheet.Range("B2:H2").Merge();
                    worksheet.Range("B2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("B4").Value = "HÓA ĐƠN BÁN HÀNG";
                    worksheet.Range("B4:H4").Merge();
                    worksheet.Range("B4:H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range("B4:H4").Style.Font.FontColor = XLColor.Red;

                    // Thêm tiêu đề cột
                    worksheet.Cell("B6").Value = "STT";
                    worksheet.Cell("C6").Value = "Mã nhân viên";
                    worksheet.Cell("D6").Value = "Tên nhân viên";
                    worksheet.Cell("E6").Value = "Số điện thoại";
                    worksheet.Cell("F6").Value = "Giới tính";

                    worksheet.Cell("G6").Value = "Tên phòng ban";
                    worksheet.Cell("H6").Value = "Mức lương";

                    worksheet.Range("B6:H6").Style.Font.Bold = true;
                    worksheet.Range("B6:H6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Điền dữ liệu từ DataGridView
                    decimal totalAmount = 0;
                    int row = 7;
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        worksheet.Cell(row, 2).Value = (i + 1).ToString();
                        worksheet.Cell(row, 3).Value = dataGridView1.Rows[i].Cells["MaNV"].Value?.ToString();
                        worksheet.Cell(row, 4).Value = dataGridView1.Rows[i].Cells["TenNV"].Value?.ToString();
                        worksheet.Cell(row, 5).Value = dataGridView1.Rows[i].Cells["SoDT"].Value?.ToString();
                        worksheet.Cell(row, 6).Value = dataGridView1.Rows[i].Cells["GioiTinh"].Value?.ToString();
                        worksheet.Cell(row, 7).Value = dataGridView1.Rows[i].Cells["TenPhongBan"].Value?.ToString();

                        worksheet.Cell(row, 8).Value = dataGridView1.Rows[i].Cells["MucLuong"].Value?.ToString(); ; // Để trống cho ghi chú nếu cần

                        // Tính toán "Thành Tiền"
                        decimal MucLuong = Convert.ToDecimal(dataGridView1.Rows[i].Cells["MucLuong"].Value);
                        //   decimal donGiaBan = Convert.ToDecimal(dataGridView1.Rows[i].Cells["DonGiaBan"].Value);
                        //  decimal thanhTien = soLuong * donGiaBan;
                        //   worksheet.Cell(row, 8).Value = thanhTien;
                        //   worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00"; // Định dạng thành tiền

                        totalAmount += MucLuong; // Cộng dồn vào tổng tiền
                        row++;
                    }

                    // Tính tổng tiền
                    worksheet.Cell(row, 7).Value = "TỔNG TIỀN:";
                    worksheet.Cell(row, 8).Value = totalAmount;
                    worksheet.Cell(row, 8).Style.Font.Bold = true;

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Columns().AdjustToContents();

                    // Lưu file
                    string filePath = @"E:\zalo\trucquan\ktragiuaky\xuatFile\DanhSachNhanVien.xlsx"; // Đổi đường dẫn nếu cần
                    workbook.SaveAs(filePath);

                    MessageBox.Show("Lưu thành công tại " + filePath);
                }
            }
            else
            {
                MessageBox.Show("Không có danh sách hàng để in.");
            }
        }



    }
}
