using System.ComponentModel.DataAnnotations;

namespace AppointmentHospital.EnumStatus
{
    public enum Specialization
    {
        [Display(Name = "Ear, Nose and Throat")]
        TaiMuiHong = 0,
        [Display(Name = "Pediatrics")]
        Nhi = 1,
        [Display(Name = "Dermatology")]
        DaLieu = 2,
        [Display(Name = "Surgery")]
        NgoaiKhoa = 3,
        [Display(Name = "Internal Medicine")]
        NoiKhoa = 4,
        [Display(Name = "Dentistry")]
        RangHamMat = 5,
        [Display(Name = "Obstetrics")]
        SanKhoa = 6,
        [Display(Name = "Ophthalmology")]
        NhanKhoa = 7,
        [Display(Name = "Cardiology")]
        TimMach = 8,
        [Display(Name = "Neurology")]
        ThanKinh = 9,
        [Display(Name = "Ophthalmology")]
        Mat = 10,
        [Display(Name = "Gynecology")]
        PhuKhoa = 11
    }

}
