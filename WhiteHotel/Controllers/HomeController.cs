using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WhiteHotel.Application.Common.Interfaces;
using WhiteHotel.Application.Common.Utility;
using WhiteHotel.Models;
using WhiteHotel.Web.ViewModels;

namespace WhiteHotel.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaLIst = _unitOfWork.Villa.GetAll(includeProperites: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),

            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            Thread.Sleep(2000);
            var villaList = _unitOfWork.Villa.GetAll(includeProperites: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVilla = _unitOfWork.Booking.GetAll(u=>u.Status==SD.StatusApproved||u.Status==SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomAvaliable = SD.VillaRoomsAvaliable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVilla);
                villa.IsAvailable = roomAvaliable > 0?true : false;
            }
            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaLIst = villaList,
                Nights = nights
            };
            return PartialView("_VillaList", homeVM);
        }

        public IActionResult Details(int validId)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == validId);
            return View(villa);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
