using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flurl;
using Flurl.Http;
using LibraryProjectWebSite.Models;

namespace LibraryProjectWebSite.Controllers
{
    public class HomeController : Controller
    {
        Request.Request request = new Request.Request();
        // GET: Home
        public ActionResult Index()
        {
            CheckValidation();
            List<BookDto> booksDto = request.GetAsync<List<BookDto>>("/api/Book/Get").Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel() { Books = booksDto };
            return View(libraryViewModel);
        }

        public ActionResult GetById(int id)
        {
            try
            {
                BookDto book = request.GetAsync<BookDto>($"/api/Book/Get/{id}").Result;
                LibraryViewModel libraryViewModel = new LibraryViewModel() { Book = book };
                libraryViewModel.LibrarySelectListItem = book.Libraries.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
                return View(libraryViewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }

        }



        [HttpGet]
        public ActionResult UserLogin()
        {
            UserDto userDto = new LibraryViewModel().User;
            return View(userDto);
        }

        [HttpPost]
        public ActionResult UserLogin(UserDto userDto)
        {
            object postdata = new
            {
                grant_type = "password",
                username = userDto.Email + "|user",
                password = userDto.Password
            };
            var token = request.PostAsync<Token>("/token", postdata).Result;
            if (token != null)
            {
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult OfficerLogin()
        {
            OfficerDto officerDto = new LibraryViewModel().Officer;
            return View(officerDto);
        }

        [HttpPost]
        public ActionResult OfficerLogin(OfficerDto officerDto)
        {
            object postdata = new
            {
                grant_type = "password",
                username = officerDto.Email + "|officer",
                password = officerDto.Password
            };
            var token = request.PostAsync<Token>("/token", postdata).Result;
            if (token != null)
            {
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            var token_type = new HttpCookie("token_type");
            var access_token = new HttpCookie("access_token");
            token_type.Expires = DateTime.Now.AddDays(-1);
            access_token.Expires = DateTime.Now.AddDays(-1);
            Response.SetCookie(access_token);
            Response.SetCookie(token_type);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Borrow(LibraryViewModel libraryViewModel)
        {
            if (CheckValidation())
            {
                

            }

            return RedirectToAction("Index");
        }

        public bool CheckValidation()
        {
            if (HttpContext.Request.Cookies["access_token"] != null && HttpContext.Request.Cookies["token_type"] != null)
            {

                UserData userData = request.GetAsync<UserData>("/api/Validation/GetValidationData", GetHeader()).Result;
                if (userData.Id != 0 && userData.Role != null)
                {
                    ViewBag.Id = userData.Id;
                    ViewBag.Role = userData.Role;
                    ViewBag.isLoggedIn = true;
                    return true;
                }
                else
                {
                    Logout();
                    return false;
                }
            }
            else
            {
                ViewBag.isLoggedIn = false;
                return false;
            }
        }

        public Dictionary<string, string> GetHeader()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"{HttpContext.Request.Cookies["token_type"].Value} {HttpContext.Request.Cookies["access_token"].Value}");
            return headers;
        }
    }
}