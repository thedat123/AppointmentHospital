using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.Entity;
using FinalProject.Repositories;

namespace FinalProject.Services.Implement
{
    public class SpecialitiesService : ISpecialitiesService
    {
        private readonly ISpecialitiesRepository specialitiesRepository;
        public SpecialitiesService(ISpecialitiesRepository specialitiesRepository){
            this.specialitiesRepository = specialitiesRepository;
        }
        public List<Specialities> GetAllSpecialities(){
            return specialitiesRepository.GetAllSpecialities();
        }
    }
}