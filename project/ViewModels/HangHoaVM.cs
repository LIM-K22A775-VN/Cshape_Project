namespace project.ViewModels
{
	public class HangHoaVM
	{
		public int MaHh {  get; set; }
		public String TenHH { get; set; }
		public String Hinh { get; set; }
		public double Dongia { get; set; }
		public String Motangan	 { get; set; }
		public String Tenloai { get; set; }
	}

    public class ChiTietHangHoaVM
    {
        public int MaHh { get; set; }
        public String TenHH { get; set; }
        public String Hinh { get; set; }
        public double Dongia { get; set; }
        public String Motangan { get; set; }
        public String Tenloai { get; set; }
        public String ChiTiet { get; set; }
        public int DiemDanhGia { get; set; }
        public int SoLuongTon  { get; set; }
    }
}
