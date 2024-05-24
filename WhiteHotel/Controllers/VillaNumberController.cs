using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteHotel.Application.Common.Interfaces;
using WhiteHotel.Domain.Entities;
using WhiteHotel.Infrastructure.Data;
using WhiteHotel.Web.ViewModels;

namespace WhiteHotel.Web.Controllers
{
    public class VillaNumberController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.VillaNumber.GetAll(includeProperites: "Villa");
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberViewModel villaNumber = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                })
            };
            
            return View(villaNumber);
        }
        [HttpPost]
        public IActionResult Create(VillaNumberViewModel obj)
        {

           bool roomNumberExist = _unitOfWork.VillaNumber.Any(u=>u.Villa_Number==obj.VillaNumber.Villa_Number);
            if (ModelState.IsValid && !roomNumberExist ) 
            {
                _unitOfWork.VillaNumber.Add(obj.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "the villa has been created successfully.";
                return RedirectToAction("Index");
            }
            if (roomNumberExist)
            {
                TempData["error"] = "this villa number already exists";
            }
            obj.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            return View(obj);
        }
        public IActionResult Update(int villaNumberId)
        {
            VillaNumberViewModel villaNumbermv = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(d => d.Villa_Number == villaNumberId)
            };

            if(villaNumbermv.VillaNumber == null)
            {
                return RedirectToAction("Error","Home");
            }
            return View(villaNumbermv);
        }
        [HttpPost]
        public IActionResult Update(VillaNumberViewModel villaNumberViewModel)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumberViewModel.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "the villa has been updated successfully.";
                return RedirectToAction("Index");
            }
           
            villaNumberViewModel.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            return View(villaNumberViewModel);
        }
        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberViewModel villaNumbermv = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(d => d.Villa_Number == villaNumberId)
            };

            if (villaNumbermv.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumbermv);
        }
        [HttpPost]
        public IActionResult Delete(VillaNumberViewModel villaNumberViewModel)
        {
            VillaNumber? villaNumber = _unitOfWork.VillaNumber.Get(d => d.Villa_Number == villaNumberViewModel.VillaNumber.Villa_Number); 
            if (villaNumber is not null)
            {
                _unitOfWork.VillaNumber.Remove(villaNumber);
                _unitOfWork.Save();
                TempData["success"] = "the villa number has been deleted successfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "the villa number could not be deleted.";

            
            return View();
        }

    }
}
