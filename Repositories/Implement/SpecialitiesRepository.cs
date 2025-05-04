using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;
using FinalProject.Entity;

namespace FinalProject.Repositories.Implement
{
    public class SpecialitiesRepository : ISpecialitiesRepository
    {
        private readonly AppDbContext _context;
        public SpecialitiesRepository(AppDbContext context){
            _context = context; 
        }
        public List<Specialities> GetAllSpecialities()
        {
            // Kiểm tra xem _context có phải null không
            if (_context == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            // Trả về danh sách các khoa, hoặc có thể trả về một danh sách rỗng nếu không có dữ liệu
            return _context.Specialities?.ToList() ?? new List<Specialities>();
        }
    }
}