using Microsoft.AspNetCore.Mvc;
using Pustok.DAL;
using Pustok.Models;
using System.Linq;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var data = _context.Settings.ToList();
            return View(data);
        }
        public IActionResult Update(int Id)
        {
            var data = _context.Settings.FirstOrDefault(x => x.Id == Id);
            return View(data);
        }
        [HttpPost]
        public IActionResult Update(Setting setting)
        {
            var data =_context.Settings.FirstOrDefault(x => x.Id == setting.Id);
            data.Value = setting.Value;
            _context.SaveChanges();
            return RedirectToAction("index", "setting");
        }
    }
}
