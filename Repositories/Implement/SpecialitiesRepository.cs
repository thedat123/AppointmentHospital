using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AppointmentHospital.Models;
using AppointmentHospital.Helpers;

namespace AppointmentHospital.Repositories.Implement
{
    public class SpecialitiesRepository : ISpecialitiesRepository
    {
        private readonly AppDbContext _context;
        public SpecialitiesRepository(AppDbContext context){
            _context = context; 
        }

        public async Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page)
        {
            if (_context == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            var query = _context.Specialities.AsQueryable();
            return await Pagination<Specialities>.PaginatedList(query, page);
        }

        public Specialities GetSpecialityById(int specialityId)
        {
            var speciality = _context.Specialities
                .Include(s => s.Doctor)
                .FirstOrDefault(s => s.Id == specialityId);
            if (speciality == null)
            {
                throw new InvalidOperationException("Speciality not found.");
            }
            return speciality;
        }
    }
}