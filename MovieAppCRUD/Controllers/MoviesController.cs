using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppCRUD.Models;
using MovieAppCRUD.ViewModels;
using System.Linq;
using System.IO;
using NToastNotify;

namespace MovieAppCRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context; //injection
        private readonly IToastNotification _toastNotification;
        private new List<string> _allowedExtentions = new List<string> { ".png", ".jpg" };
        private long _MaxImageSize = 1048576;

        public MoviesController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m=>m.Rate).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(m=>m.Name).ToListAsync(),
            };
            return View("MovieForm",viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View(model);
            }

            var files = Request.Form.Files;

            if(!files.Any())
            {
                model.Genres=await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please add Poster.....");
                return View(model);
            }

            var poster = files.FirstOrDefault();
            

            if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "only .png or .jpg are allowed");
                return View(model);
            }

            if (poster.Length>_MaxImageSize)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster can not be more than 1MB!");
                return View("MovieForm", model);
            }

            using var datastream = new MemoryStream();

            await poster.CopyToAsync(datastream);

            var movie = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                StoryLine = model.StoryLine,
                Year = model.Year,
                Rate = model.Rate,
               
                Poster = datastream.ToArray()
            };

            try
            {
                _context.Movies.Add(movie);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
            _toastNotification.AddSuccessToastMessage("Movie Created Successfully");

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit (int? id)
        {

            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(id);
            if(movie == null)
                return NotFound();

            var viewModel = new MovieFormViewModel
            { 
                Id=movie.Id,
                Title=movie.Title,
                GenreId=movie.GenreId,
                Rate=movie.Rate,
                Year=movie.Year,
                StoryLine=movie.StoryLine,
                Poster=movie.Poster,
                Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync(),
            };
            return View("MovieForm", viewModel);

            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }
            var movie = await _context.Movies.FindAsync(model.Id);
            if (movie == null)
                return NotFound();
            var file = Request.Form.Files;
            if (file.Any())
            {
                var poster = file.FirstOrDefault();
                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);
                model.Poster= dataStream.ToArray();

                if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName.ToLower())))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "only .png or .jpg are allowed");
                    return View(model);
                }

                if (poster.Length > _MaxImageSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster can not be more than 1MB!");
                    return View("MovieForm", model);
                }
                movie.Poster = model.Poster;
            }
             
            movie.Title=model.Title;
            movie.GenreId=model.GenreId;
            movie.Rate=model.Rate;
            movie.Year=model.Year;
            movie.StoryLine=model.StoryLine;

            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Updated Successfully");

            return RedirectToAction(nameof(Index));
         }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

        //   var movie=await _context.Movies.FindAsync(id);
             var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m=>m.Id==id);
            if (movie == null) 
                return NotFound();
            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

              var movie=await _context.Movies.FindAsync(id);
     
            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }




    }
}
