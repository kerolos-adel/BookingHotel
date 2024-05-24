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
    public class VillaRepositry : Repositry<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _context;
        public VillaRepositry(ApplicationDbContext context):base(context)
        {
            _context = context;
        }


        public void Update(Villa entity)
        {
            _context.Villas.Update(entity);
        }
    }
}
