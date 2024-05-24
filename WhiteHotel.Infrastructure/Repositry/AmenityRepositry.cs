using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteHotel.Application.Common.Interfaces;
using WhiteHotel.Domain.Entities;
using WhiteHotel.Infrastructure.Data;

namespace WhiteHotel.Infrastructure.Repositry
{
    public class AmenityRepositry : Repositry<Amenity>, IAmenityRepository
    {
        private readonly ApplicationDbContext _context;
        public AmenityRepositry(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
        public void Save()
        {
            _context.SaveChanges();
        }
        public void Update(Amenity entity)
        {
            _context.Amenities.Update(entity);
        }
        
    }
}
