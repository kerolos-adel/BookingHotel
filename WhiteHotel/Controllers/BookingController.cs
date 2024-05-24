using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WhiteHotel.Application.Common.Interfaces;
using System.Security.Claims;
using WhiteHotel.Domain.Entities;
using WhiteHotel.Application.Common.Utility;
using Stripe.Checkout;
using Stripe;

namespace WhiteHotel.Web.Controllers
{

    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId,DateOnly checkInDate,int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.User.Get(d=>d.Id== userId);
            Booking booking = new Booking()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperites: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId= userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,

            };
            booking.TotalCost = booking.Villa.Price*nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);

            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;


            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVilla = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();

          
                int roomAvaliable = SD.VillaRoomsAvaliable_Count(villa.Id, villaNumberList, booking.CheckInDate, booking.Nights, bookedVilla);
            
            if(roomAvaliable == 0)
            {
                TempData["error"] = "Room has been sold out";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights,
                });
            }


            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),


                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
            };


            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> { domain + villa.ImageUrl},
                    }
                },
                Quantity = 1,
            });

           
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id,session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location",session.Url);
            return new StatusCodeResult(303);
            
        }
        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromdb = _unitOfWork.Booking.Get(d=>d.Id == bookingId,includeProperites:"User,Villa");
            if (bookingFromdb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromdb.StripeSessionId);
                if(session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromdb.Id, SD.StatusApproved,0);
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingFromdb.Id, session.Id,session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }
        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking booking = _unitOfWork.Booking.Get(d => d.Id == bookingId, includeProperites: "User,Villa");
            if (booking.VillaNumber == 0 && booking.Status == SD.StatusApproved)
            {
                var AvailableVillaNumber = AssignVillaNumberByVilla(booking.VillaId);
                booking.VillaNumbers = _unitOfWork.VillaNumber.GetAll(u=>u.VillaId == booking.VillaId
                &&AvailableVillaNumber.Any(x=>x==u.Villa_Number)
                ).ToList();
            }
            return View(booking);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBooking;
            if (User.IsInRole(SD.Role_Admin))
            {
                objBooking = _unitOfWork.Booking.GetAll(includeProperites:"User,Villa");
            }
            else 
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objBooking = _unitOfWork.Booking
                    .GetAll(u=>u.UserId==userId,includeProperites: "User,Villa");

            }
            if(!string.IsNullOrEmpty(status))
            {
                objBooking = objBooking.Where(u=>u.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new {data = objBooking});
        }
        [HttpPost]
        [Authorize(Roles =(SD.Role_Admin))]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";

            return RedirectToAction(nameof(BookingDetails), new {bookingId = booking.Id});
        }

        [HttpPost]
        [Authorize(Roles = (SD.Role_Admin))]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking completed Successfully.";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = (SD.Role_Admin))]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Cancelled Successfully.";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }
        private List<int> AssignVillaNumberByVilla(int villaId)
        {
            List<int> AvailableVillaNumber = new();
            var villaNumbers = _unitOfWork.VillaNumber.GetAll(u=>u.VillaId==villaId);
            var checkInVilla = _unitOfWork.Booking.GetAll(u=>u.VillaId== villaId && u.Status==SD.StatusCheckedIn).Select(u=>u.VillaNumber);
            foreach (var villaNumber in villaNumbers)
            {
                if(!checkInVilla.Contains(villaNumber.Villa_Number))
                {
                    AvailableVillaNumber.Add(villaNumber.Villa_Number);
                }

            }
            return AvailableVillaNumber;
        }


    }
}
