using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            _context = context;
        }
     
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("register")]     //associated route string (exclude the leading /)
            public IActionResult RegisterUser(User user)
            {
                if(ModelState.IsValid)
                {
                    // do somethng!  maybe insert into db?  then we will redirect
                    // If a User exists with provided email
                    if(_context.Users.Any(u => u.Email == user.Email))
                    {
                        //UNIQUE EMAIL VALIDATION
                        // Manually add a ModelState error to the Email field, with provided
                        // error message
                        ModelState.AddModelError("Email", "Email already in use!");
                        return View("Index");
                        // You may consider returning to the View at this point
                    }
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    _context.Add(user);
                    _context.SaveChanges();
                    ViewBag.UserFirstName = user.FirstName;
                    ViewBag.UserLastName = user.LastName;
                    ViewBag.ThisUser = user;
                    ViewBag.UserFirstName = user.FirstName;
                    ViewBag.UserLastName = user.LastName;
                    //Input all wedding info for dashboard here
                    ViewBag.AllWeddings = _context.Weddings.Include(s => s.UsersForThisWedding);
                    var test = _context.Weddings.Count();
                    ViewBag.AllWeddingsCount = test;
                    var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding);
                    var guestCount = weddingguests.Count();
                    ViewBag.GuestCount = guestCount;
                    ViewBag.WeddingGuests = weddingguests;
                    var onthelist = _context.Weddings.Include(a => a.UsersForThisWedding).Where(a => a.UserId == user.UserId);
                    return View("UserDashboard");
                }
                else
                {
                    // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                    return View("Index");
                } 
            }
        [HttpPost("login/user")]
            public IActionResult LoginUser(LoginUser user)
            {
                if(ModelState.IsValid)
                {
                    var userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                    // If no user exists with provided email
                    if(userInDb == null)
                    {
                        // Add an error to ModelState and return to View!
                        ModelState.AddModelError("Email", "Email not in Database");
                        return View("Index");
                    }
                    // Initialize hasher object
                    var hasher = new PasswordHasher<LoginUser>();
                    // verify provided password against hash stored in db
                    var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.Password);
                    // result can be compared to 0 for failure
                    if(result == 0)
                    {
                        // handle failure (this should be similar to how "existing email" is handled)
                        ModelState.AddModelError("Password", "Invalid Password");
                        return View("Index");
                    }
                    ViewBag.ThisUser = userInDb;
                    ViewBag.UserFirstName = userInDb.FirstName;
                    ViewBag.UserLastName = userInDb.LastName;
                    //Input all wedding info for dashboard here
                    ViewBag.AllWeddings = _context.Weddings.Include(s => s.UsersForThisWedding);
                    var test = _context.Weddings.Count();
                    ViewBag.AllWeddingsCount = test;
                    var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding);
                    var guestCount = weddingguests.Count();
                    ViewBag.GuestCount = guestCount;
                    ViewBag.WeddingGuests = weddingguests;
                    var onthelist = _context.Weddings.Include(a => a.UsersForThisWedding).Where(a => a.UserId == userInDb.UserId);
                    var hello = _context.Associations.Any(r => r.UserId == userInDb.UserId);
                    ViewBag.context = _context;
                    ViewBag.onthelisttest = hello;
                    Console.WriteLine(hello);

                    
                    
                    return View("UserDashboard");
                }
                else
                {
                    // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                    return View("Index");
                }
            }
        [HttpGet("userdashboard/{userId}")]
        public IActionResult UserDashboard(LoginUser user)
        { 
            if(ModelState.IsValid)
            {
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Email not in Database");
                    return View();
                }
            }
            return View("Unauthorized");
        }
        
        [HttpGet("/planwedding/{userId}")]
        public IActionResult PlanWedding(int userId)
        {
            var ThisUser = _context.Users.FirstOrDefault(b => b.UserId == userId);
            ViewBag.ThisUser = ThisUser;
            return View();
        }
        [HttpPost("createwedding")]     //associated route string (exclude the leading /)
            public IActionResult CreateWedding(Wedding wedding, int userId)
            {
                var ThisUser = _context.Users.FirstOrDefault(b => b.UserId == userId);
                ViewBag.ThisUser = ThisUser;
                if(ModelState.IsValid)
                {
                    var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
                    ViewBag.ThisUser = userInDb;
                    var thiswedding = _context.Weddings.FirstOrDefault(b => b.WeddingId == wedding.WeddingId);
                    ViewBag.Wedding = thiswedding;
                    var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding).Where(b => b.WeddingId == wedding.WeddingId);
                    var guestCount = weddingguests.Count();
                    ViewBag.GuestCount = guestCount;
                    ViewBag.WeddingGuests = weddingguests;
                    _context.Add(wedding);
                    _context.SaveChanges();
                    ViewBag.Wedding = wedding;
                    return View("ViewWedding");
                }
                else
                {
                    // Oh no!  We need to return a ViewResponse to preserve the ModelState, and the errors it now contains!
                    return View("PlanWedding");
                } 
            }
        [HttpGet("/dashboardredirect/{userId}")]
        public IActionResult DashBoardRedirect(int weddingId, int userId)
        {
            var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
            ViewBag.ThisUser = userInDb;
            ViewBag.ThisUser = userInDb;
            ViewBag.UserFirstName = userInDb.FirstName;
            ViewBag.UserLastName = userInDb.LastName;
            //Input all wedding info for dashboard here
            ViewBag.AllWeddings = _context.Weddings;
            var test = _context.Weddings.Count();
            ViewBag.AllWeddingsCount = test;
            var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding).Where(b => b.WeddingId == weddingId);
            var guestCount = weddingguests.Count();
            ViewBag.GuestCount = guestCount;
            ViewBag.WeddingGuests = weddingguests;
            ViewBag.AllWeddings = _context.Weddings.Include(x => x.UsersForThisWedding);
            return View("UserDashboard", userInDb);
        }
        [HttpGet("viewwedding/{weddingId}/{userId}")]
        public IActionResult ViewWedding(int weddingId, int userId)
        {
            var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
            ViewBag.ThisUser = userInDb;
            var wedding = _context.Weddings.FirstOrDefault(b => b.WeddingId == weddingId);
            ViewBag.Wedding = wedding;
            var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding).Where(b => b.WeddingId == weddingId);
            var guestCount = weddingguests.Count();
            ViewBag.GuestCount = guestCount;
            ViewBag.WeddingGuests = weddingguests;
            ViewBag.AllWeddings = _context.Weddings.Include(x => x.UsersForThisWedding);
            
            return View();
        }

        
        [HttpGet("rsvp/{weddingId}/{userId}/{status}")]
        public IActionResult RSVP(int weddingId, int userId, string status)
        {
            var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
            ViewBag.ThisUser = userInDb;
            var wedding = _context.Weddings.FirstOrDefault(b => b.WeddingId == weddingId);
            ViewBag.Wedding = wedding;
            var weddingguests = _context.Associations.Include(a => a.User).Include(b => b.Wedding).Where(b => b.WeddingId == weddingId);
            var guestCount = weddingguests.Count();
            ViewBag.GuestCount = guestCount;
            ViewBag.WeddingGuests = weddingguests;
            ViewBag.AllWeddings = _context.Weddings.Include(x => x.UsersForThisWedding);
            if(userInDb == null)
                return RedirectToAction("Index");

            // make sure wedding exists, redirect if not
            if(!_context.Weddings.Any(w => w.WeddingId == weddingId))
                return RedirectToAction("Index", "Wedding");

            if(status == "add")
                AddRSVP(weddingId, userId);
            else
                RemoveRSVP(weddingId, userId);

            return View("UserDashboard", userInDb);
        }
        private void AddRSVP(int weddingId, int userId)
        {
            var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
            Association newResponse = new Association()
            {
                WeddingId = weddingId,
                UserId = userInDb.UserId
            };

            _context.Associations.Add(newResponse);
            _context.SaveChanges();
        }
        private void RemoveRSVP(int weddingId, int userId)
        {
            Console.WriteLine("dssdfg");
            var userInDb = _context.Users.FirstOrDefault(b => b.UserId == userId);
            // query for response
            Wedding rsvp = _context.Weddings.FirstOrDefault(r => r.UserId == userInDb.UserId && r.WeddingId == weddingId);

            if(rsvp != null)
            {
                _context.Weddings.Remove(rsvp);
                _context.SaveChanges();
            }
                
        }
 
    }

}
