using WhiteHotel.Domain.Entities;

namespace WhiteHotel.Web.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Villa>? VillaLIst { get; set; }
        public DateOnly CheckInDate {  get; set; }
        public DateOnly CheckOutDate { get; set; }
        public int Nights { get; set; }
    }
}
