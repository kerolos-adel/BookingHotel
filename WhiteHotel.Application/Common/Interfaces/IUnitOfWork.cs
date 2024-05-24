using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteHotel.Domain.Entities;


namespace WhiteHotel.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
      IVillaRepository Villa {  get; }
      IVillaNumberRepository VillaNumber {  get; }
      IAmenityRepository Amenity { get; }
      IBookingRepository Booking { get; }
      IApplicationUserRepository User { get; }
      void Save();


    }
}
