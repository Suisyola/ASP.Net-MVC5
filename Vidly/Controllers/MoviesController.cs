﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vidly.Models;
using Vidly.ViewModel;

namespace Vidly.Controllers
{
    public class MoviesController : Controller
    {
        private ApplicationDbContext _context;

        public MoviesController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ViewResult Index()
        {
            // Checks if user has role, "CanManageMovies", then display Views that is accessible by admins
            if (User.IsInRole(RoleName.CanManageMovies))
            {
                return View("List");
            }

            return View("ReadOnlyList");
            
        }

        public ActionResult Details(int id)
        {
            // SingleOrDefault will return at most 1. If more than 1, throws exception.
            var movie = _context.Movies.Include(m => m.MovieType).SingleOrDefault(c => c.Id == id);

            if (movie == null)
                return HttpNotFound();

            return View(movie);
        }

        // GET: Movies/Random
        public ActionResult Random()
        {
            var movie = new Movie() {Name = "Shrek!"};

            var customers = new List<Customer>
            {
                new Customer {Name = "Customer 1"},
                new Customer {Name = "Customer 2"}
            };

            var viewModel = new RandomMovieViewModel
            {
                Movie = movie,
                Customers = customers
            };

            return View(viewModel);
        }

        
        
        [Route("movies/released/{year}/{month:regex(\\d{2}):range(1,12)}")]
        public ActionResult ByReleaseDate(int month, int year)
        {
            return Content(year + "/" + month);
        }

        [Authorize(Roles = RoleName.CanManageMovies)]
        public ActionResult New()
        {
            // get list of all movie types
            var movieTypes = _context.MovieTypes.ToList();

            var viewmodel = new MovieFormViewModel
            {
                Movie = new Movie(),
                MovieTypes = movieTypes
            };

            return View("MovieForm", viewmodel);
        }

        [Authorize(Roles = RoleName.CanManageMovies)]
        public ActionResult Edit(int Id)
        {
            var movie = _context.Movies.Include(m => m.MovieType).Single(m => m.Id == Id);

            if (movie == null)
            {
                return HttpNotFound();
            }

            var viewmodel = new MovieFormViewModel
            {
                Movie = movie,
                MovieTypes = _context.MovieTypes.ToList()
            };

            return View("MovieForm", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleName.CanManageMovies)]
        public ActionResult Save(MovieFormViewModel viewmodel)
        {
            if (!ModelState.IsValid)
            {
                var movieFormViewModel = new MovieFormViewModel
                {
                    Movie = viewmodel.Movie,
                    MovieTypes = _context.MovieTypes.ToList()
                };

                return View("MovieForm", movieFormViewModel);
            }

            // Id = 0 implies new movie
            if (viewmodel.Movie.Id == 0)
            {
                viewmodel.Movie.DateAdded = DateTime.Now;

                // Add new movie. This only stores changes in memory
                _context.Movies.Add(viewmodel.Movie);
            }
            else
            {
                // Retrieves the movie which matches the Id
                var movie = _context.Movies.Single(c => viewmodel.Movie.Id == c.Id);

                // Modify existing movie's data
                movie.Name = viewmodel.Movie.Name;
                movie.ReleaseDate = viewmodel.Movie.ReleaseDate;
                movie.MovieTypeId = viewmodel.Movie.MovieTypeId;
                movie.NumInStock = viewmodel.Movie.NumInStock;
            }


            // To persist changes into Database
            _context.SaveChanges();

            return RedirectToAction("Index", "Movies");
        }
    }
}