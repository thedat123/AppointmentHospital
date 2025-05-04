using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.Entity;

namespace FinalProject.Repositories
{
    public interface ISpecialitiesRepository
    {
        public List<Specialities> GetAllSpecialities();
    }
}