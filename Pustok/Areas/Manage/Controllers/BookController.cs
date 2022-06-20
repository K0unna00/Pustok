using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Helpers;
using Pustok.Models;
using System.Linq;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BookController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            var model = _context.Books.Include(x => x.Genre).Include(x => x.Author).ToList();
            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            if (!ModelState.IsValid)
            {
                return View();
            }


            if (!_context.Authors.Any(x => x.Id == book.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "Author notfound");
                

                return View();
            }

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
            {
                ModelState.AddModelError("GenreId", "Genre notfound");
                

                return View();
            }

            if (book.PosterFile == null)
            {
                ModelState.AddModelError("PosterFile", "PosterFile is required");
                
                return View();
            }
            else
            {
                if (book.PosterFile.ContentType != "image/png" && book.PosterFile.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("PosterFile", "File format must be image/png or image/jpeg");
                }

                if (book.PosterFile.Length > 2097152)
                {
                    ModelState.AddModelError("PosterFile", "File size must be less than 2MB");
                }

                if (!ModelState.IsValid)
                {
                    
                    return View();
                }

                BookImage bookImage = new BookImage
                {
                    Name = FileManager.Save(_env.WebRootPath, "uploads/books", book.PosterFile),
                    PosterStatus = true
                };

                book.BookImages.Add(bookImage);
            }
            if(book.TagIds != null)
            {
                foreach (var tagId in book.TagIds)
                {
                    BookTag bookTag = new BookTag()
                    {
                        TagId = tagId
                    };
                    book.BookTags.Add(bookTag);
                }
                
            }


            if (book.ImageFiles != null)
            {
                foreach (var file in book.ImageFiles)
                {
                    if (file.ContentType != "image/png" && file.ContentType != "image/jpeg")
                    {
                        ModelState.AddModelError("ImageFiles", "File format must be image/png or image/jpeg");
                    }

                    if (file.Length > 2097152)
                    {
                        ModelState.AddModelError("ImageFiles", "File size must be less than 2MB");
                    }

                    if (!ModelState.IsValid)
                    {
                        
                        return View();
                    }
                }

                foreach (var file in book.ImageFiles)
                {
                    BookImage bookImage = new BookImage
                    {
                        Name = FileManager.Save(_env.WebRootPath, "uploads/books", file),
                        PosterStatus = null
                    };

                    book.BookImages.Add(bookImage);
                }
            }



            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public IActionResult Edit(int id)
        {
            Book book = _context.Books.Include(x=>x.BookImages).Include(x=>x.BookTags).FirstOrDefault(x => x.Id == id);

            if (book == null)
                return RedirectToAction("error", "dashboard");

            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            
            book.TagIds = book.BookTags.Select(x=>x.TagId).ToList();
            return View(book);
        }
        [HttpPost]
        public IActionResult Edit(Book book)
        {
            var existsBook=_context.Books.FirstOrDefault(x=>x.Id == book.Id);

            existsBook.BookTags.RemoveAll(bt=>!book.TagIds.Contains(bt.TagId));

            foreach (var tagId in book.TagIds.Where(x=>!existsBook.BookTags.Any(bt=>bt.TagId == x)))
            {
                BookTag bookTag = new BookTag
                {
                    TagId = tagId
                };
                existsBook.BookTags.Add(bookTag);
            }
            existsBook.Name = book.Name;
            existsBook.AuthorId= book.AuthorId;
            existsBook.Desc = book.Desc;
            existsBook.CostPrice = book.CostPrice;
            existsBook.SalePrice = book.SalePrice;
            existsBook.DiscountPercent = book.DiscountPercent;
            existsBook.PageSize = book.PageSize;
            existsBook.SubDesc= book.SubDesc;
            existsBook.GenreId= book.GenreId;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Test()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Test(Tag tag)
        {
            _context.Tags.Add(tag);
            _context.SaveChanges();
            return RedirectToAction("index");
        }
    }
}
