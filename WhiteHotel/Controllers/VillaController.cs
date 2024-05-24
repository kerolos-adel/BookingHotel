using Microsoft.AspNetCore.Mvc;
using WhiteHotel.Application.Common.Interfaces;
using WhiteHotel.Domain.Entities;
using WhiteHotel.Infrastructure.Data;

namespace WhiteHotel.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment; 
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name==villa.Description)
            {
                ModelState.AddModelError("Description", "the description cannot br exactly match the Name.");
            }
            if (ModelState.IsValid) 
            {
                if (villa.Image!=null)
                {
                    string fileName = Guid.NewGuid().ToString()+ Path.GetExtension(villa.Image.FileName);
                    string ImagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                    using var fileStream = new FileStream(Path.Combine(ImagePath,fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://th.bing.com/th/id/OIP.LHY-huRGQF7Xcn4GvYjz9AAAAA?rs=1&pid=ImgDetMain";
                }
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "the villa has been created successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Update(int validId)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == validId);
            if(villa == null)
            {
                return RedirectToAction("Error","Home");
            }
            return View(villa);
        }
        [HttpPost]
        public IActionResult Update(Villa villa)
        {

          
            if (ModelState.IsValid && villa.Id>0)
            {
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string ImagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");
                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using var fileStream = new FileStream(Path.Combine(ImagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = @"\images\VillaImage\" + fileName;
                }

                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();
                TempData["success"] = "the villa has been updated successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int validId)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == validId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }
        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaFromDb = _unitOfWork.Villa.Get(x=>x.Id == villa.Id);
            if (villaFromDb is not null)
            {
                if (!string.IsNullOrEmpty(villaFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaFromDb.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Villa.Remove(villaFromDb);
                _unitOfWork.Save();
                TempData["success"] = "the villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            TempData["erorr"] = "the villa could not be deleted.";

            return View();
        }

    }
}
