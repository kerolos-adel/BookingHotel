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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IVillaRepository Villa { get; private set; }
        public IVillaNumberRepository VillaNumber { get; private set; }
        public IAmenityRepository Amenity { get; private set; }

        public IBookingRepository Booking { get; private set; }

        public IApplicationUserRepository User { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Villa = new VillaRepositry(_context);
            VillaNumber = new VillaNumberRepositry(_context);
            User = new ApplicationUserRepository(_context);
            Amenity = new AmenityRepositry(_context);
            Booking = new BookingRepositry(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
