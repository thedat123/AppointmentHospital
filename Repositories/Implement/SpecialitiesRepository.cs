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

        public async Task<Pagination<Specialities>> GetAllSpecialitiesAsync(int page, string searchTerm, string filter)
        {
            if (_context == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            var query = _context.Specialities.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.SpecialityName.Contains(searchTerm));
            }

            // Apply specific filter
            if (!string.IsNullOrWhiteSpace(filter) && filter.ToLower() != "all")
            {
                var lowerFilter = filter.ToLower();

                query = lowerFilter switch
                {
                    "trung-tam" => query.Where(s => s.SpecialityName.ToLower().Contains("trung tâm")),
                    "khoa" => query.Where(s => s.SpecialityName.ToLower().Contains("khoa")),
                    _ => query
                };
            }

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