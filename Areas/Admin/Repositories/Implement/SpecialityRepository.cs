using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;
using AppointmentHospital.Helpers;
using Microsoft.EntityFrameworkCore;


namespace AppointmentHospital.Areas.Admin.Repositories.Implement
{
    public class SpecialityRepository : ISpecialityRepository
    {
        private readonly AppDbContext _context;

        public SpecialityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pagination<Specialities>> GetAllSpeciality(int page, string searchTerm)
        {
            var query = _context.Specialities.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.SpecialityName.ToLower().Contains(searchTerm.ToLower()));
            }
            var paginatedList = await Pagination<Specialities>.PaginatedList(query, page);
            return paginatedList;
        }

        public async Task CreateSpecialityAsync(Specialities speciality)
        {
            await _context.Specialities.AddAsync(speciality);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteSpecialityAsync(int id)
        {
            var speciality = await _context.Specialities.FindAsync(id);
            if (speciality != null)
            {
                // Kiểm tra xem có bác sĩ nào thuộc chuyên khoa này không
                var hasDoctors = await _context.Doctors.AnyAsync(d => d.SpecialityId == id);
                
                if (hasDoctors)
                {
                    return false; // Không thể xóa vì còn có bác sĩ
                }
                
                _context.Specialities.Remove(speciality);
                await _context.SaveChangesAsync();
                return true; // Xóa thành công
            }
            
            return false; // Không tìm thấy chuyên khoa
        }

        public async Task<Specialities> GetSpecialityAsync(int id)
        {
            var speciality = await _context.Specialities.FindAsync(id);
            return speciality;
        }

        public async Task EditSpecialityAsync(int id, Specialities speciality)
        {
            var existingSpeciality = await _context.Specialities.FindAsync(id);
            Console.WriteLine($"Speciality {existingSpeciality} is being edited.");
            if (existingSpeciality != null)
            {
                existingSpeciality.SpecialityName = speciality.SpecialityName;
                await _context.SaveChangesAsync();
            }
        }
    }
}