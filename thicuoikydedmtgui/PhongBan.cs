using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thicuoikydedmtgui
{
    public class PhongBan
    {
        public string MaPhongBan { get; set; }
        public string TenPhongBan { get; set; }

        public PhongBan() { }

        public PhongBan(string maPhongBan, string tenPhongBan)
        {
            MaPhongBan = maPhongBan;
            TenPhongBan = tenPhongBan;
        }

        public override string ToString()
        {
            return TenPhongBan; // Dùng để hiển thị tên phòng ban trong ComboBox
        }
    }

}
