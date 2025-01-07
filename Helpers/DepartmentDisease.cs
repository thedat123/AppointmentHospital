using System;

namespace AppointmentHospital.Helpers;

public class DepartmentDisease
{
    public static Dictionary<string, List<string>> GetDiseaseByDepartment()
        {
            return new Dictionary<string, List<string>>
            {
                { "TaiMuiHong", new List<string> { "Common Cold", "Vertigo", "Sinusitis", "Tonsillitis", "Otitis media" } },
                { "Nhi", new List<string> { "Chicken pox", "Impetigo", "Measles", "Mumps", "Hand-foot-mouth disease" } },
                { "DaLieu", new List<string> { "Psoriasis", "Acne", "Fungal infection", "Eczema", "Vitiligo" } },
                { "NgoaiKhoa", new List<string> { "Appendicitis", "Hernia", "Gallstones", "Trauma injuries", "Osteoarthritis", "Dimorphic hemorrhoids (piles)" } },
                { "NoiKhoa", new List<string> { "Diabetes", "Hypertension", "GERD", "Hypothyroidism", "Hyperthyroidism", "Anemia", "Asthma", "Hypoglycemia" } },
                { "RangHamMat", new List<string> { "Dental caries", "Gingivitis", "Oral thrush" } },
                { "SanKhoa", new List<string> { "Pregnancy complications", "Pre-eclampsia", "Postpartum hemorrhage" } },
                { "NhanKhoa", new List<string> { "Cataract", "Glaucoma", "Conjunctivitis" } },
                { "TimMach", new List<string> { "Heart attack", "Varicose veins", "Cardiac arrhythmia", "Congestive heart failure", "Jaundice" } },
                { "ThanKinh", new List<string> { "Migraine", "Paralysis (brain hemorrhage)", "Epilepsy", "Parkinson's disease" } },
                { "PhuKhoa", new List<string> { "Polycystic ovary syndrome (PCOS)", "Endometriosis", "Fibroids" } },
                { "TruyenNhiem", new List<string> { "Covid", "Hepatitis A", "Hepatitis B", "Hepatitis C", "Tuberculosis", "Dengue", "AIDS", "Malaria", "Hepatitis E", "Hepatitis D" } },
                { "TieuHoa", new List<string> { "Gastroenteritis", "Peptic ulcer disease", "Chronic cholestasis", "Irritable bowel syndrome (IBS)", "Crohn's disease", "Typhoid" } }
            };
        }
}
