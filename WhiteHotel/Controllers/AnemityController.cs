using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteHotel.Application.Common.Interfaces;
using WhiteHotel.Application.Common.Utility;
using WhiteHotel.Domain.Entities;
using WhiteHotel.Infrastructure.Data;
using WhiteHotel.Web.ViewModels;

namespace WhiteHotel.Web.Controllers
{
    //[Authorize(Roles =SD.Role_Admin)]
    public class AmenityController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll(includeProperites: "Villa");
            return View(amenities);
        }

        public IActionResult Create()
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                })
            };
            
            return View(amenityVM);
        }
        [HttpPost]
        public IActionResult Create(AmenityVM obj)
        {

            if (ModelState.IsValid) 
            {
                _unitOfWork.Amenity.Add(obj.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "the amenity has been created successfully.";
                return RedirectToAction("Index");
            }
            
            obj.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            return View(obj);
        }
        public IActionResult Update(int amenityId)
        {
            AmenityVM amenitymv = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Amenity = _unitOfWork.Amenity.Get(d => d.Id == amenityId)
            };

            if(amenitymv.Amenity == null)
            {
                return RedirectToAction("Error","Home");
            }
            return View(amenitymv);
        }
        [HttpPost]
        public IActionResult Update(AmenityVM amenityVM)
        {

            if (ModelState.IsValid)
            {
               
                
                _unitOfWork.Amenity.Update(amenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "the amenity has been updated successfully.";
                return RedirectToAction(nameof(Index)); 
            }

            amenityVM.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            return View(amenityVM);
        }
        public IActionResult Delete(int amenityId)
        {
            AmenityVM amenityVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Amenity = _unitOfWork.Amenity.Get(d => d.Id == amenityId)
            };

            if (amenityVM.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(amenityVM);
        }
        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            Amenity? amenity = _unitOfWork.Amenity.Get(d => d.Id == amenityVM.Amenity.Id); 
            if (amenity is not null)
            {
                _unitOfWork.Amenity.Remove(amenity);
                _unitOfWork.Save();
                TempData["success"] = "the amenity has been deleted successfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "the amenity could not be deleted.";

            
            return View();
        }

    }
}
