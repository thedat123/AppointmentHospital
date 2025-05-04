using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.Entity;

namespace FinalProject.Services
{
    public interface ISpecialitiesService
    {
        public List<Specialities> GetAllSpecialities();
    }
}