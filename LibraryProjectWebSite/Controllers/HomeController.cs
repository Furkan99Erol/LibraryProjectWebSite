using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Flurl;
using Flurl.Http;
using LibraryProjectWebSite.DataTransferObject;
using LibraryProjectWebSite.Models;

namespace LibraryProjectWebSite.Controllers
{
    public class HomeController : Controller
    {
        Request.Request request = new Request.Request();
        public static bool showWrongUserDataPopUp;
        // GET: Home
        public ActionResult Index()
        {
            GetValidationData();
            List<BookDto> booksDto = request.GetAsync<List<BookDto>>("/api/Book/Get").Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel() { Books = booksDto };
            return View(libraryViewModel);
        }

        public ActionResult GetById(int id)
        {
            UserData userData = GetValidationData();
            if (userData.Role == "officer")
            {
                return RedirectToAction("Index");
            }
            try
            {
                BookDto book = request.GetAsync<BookDto>($"/api/Book/Get/{id}").Result;
                var y = request.PostJsonAsync<bool>("api/Book/IncreaseClickCounter".SetQueryParams(new { bookId = id }), null, GetHeaderWithToken());
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.Book = book;
                if (userData != null)
                {
                    bool doesHaveAlready = request.PostJsonAsync<bool>($"/api/Borrow/DoesHaveAlready".SetQueryParams(new { bookId = id }), null, GetHeaderWithToken()).Result;
                    if (doesHaveAlready)
                    {
                        ViewBag.doesHaveAlready = doesHaveAlready;
                        List<BorrowDto> borrows = request.GetAsync<List<BorrowDto>>("/api/Borrow/BorrowHistory", GetHeaderWithToken()).Result;
                        BorrowDto borrowDto = borrows.FirstOrDefault(x => x.Status == 0 || x.Status == 1 && x.BookId == id);
                        libraryViewModel.Borrow = new BorrowDto();

                        libraryViewModel.Borrow.ReturnDate = borrowDto.ReturnDate;

                        libraryViewModel.Borrow.BorrowDate = borrowDto.BorrowDate;
                        libraryViewModel.Library = new LibraryDto()
                        {
                            Name = borrowDto.Library.Name,
                        };
                    }
                }
                if (book.Libraries.Count != 0)
                {
                    bool doesHaveLibrary = book.Libraries.Where(x => x.Quantity > 0).Any();
                    if (doesHaveLibrary)
                    {
                        ViewBag.doesHaveLibrary = true;
                    }
                }
                else
                {
                    ViewBag.doesHaveLibrary = false;
                }

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
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            UserDto userDto = new LibraryViewModel().User;
            if (showWrongUserDataPopUp == true)
            {
                ViewBag.showWrongUserDataPopUp = true;
                showWrongUserDataPopUp = false;
            }
            return View(userDto);
        }

        [HttpPost]
        public ActionResult UserLogin(UserDto userDto)
        {
            object userData = new
            {
                grant_type = "password",
                username = userDto.Email + "|user",
                password = userDto.Password
            };
            try
            {
                var token = request.PostUrlEncodedAsync<Token>("/token", userData).Result;
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            catch (Exception ex)
            {
                showWrongUserDataPopUp = true;
                return RedirectToAction("UserLogin");
            }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult OfficerLogin()
        {
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            OfficerDto officerDto = new LibraryViewModel().Officer;
            if (showWrongUserDataPopUp == true)
            {
                ViewBag.showWrongUserDataPopUp = true;
                showWrongUserDataPopUp = false;
            }
            return View(officerDto);
        }

        [HttpPost]
        public ActionResult OfficerLogin(OfficerDto officerDto)
        {
            object userData = new
            {
                grant_type = "password",
                username = officerDto.Email + "|officer",
                password = officerDto.Password
            };
            try
            {
                var token = request.PostUrlEncodedAsync<Token>("/token", userData).Result;
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            catch (Exception ex)
            {
                showWrongUserDataPopUp = true;
                return RedirectToAction("OfficerLogin");
            }

            return RedirectToAction("Index");
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
            GetValidationData();
            var borrowDto = new
            {
                libraryId = libraryViewModel.Borrow.LibraryId,
                bookId = libraryViewModel.Borrow.BookId,
                BorrowType = libraryViewModel.Borrow.BorrowType,
                borrowDate = libraryViewModel.Borrow.BorrowDate,
                returnDate = libraryViewModel.Borrow.ReturnDate,
                destinationAddress = libraryViewModel.Borrow.DestinationAddress,
            };

            var result = request.PostJsonAsync<string>("api/Borrow/Borrow", borrowDto, GetHeaderWithToken()).Result;

            return RedirectToAction($"GetById/{libraryViewModel.Borrow.BookId}");
        }

        public UserData GetValidationData()
        {
            if (HttpContext.Request.Cookies["access_token"] != null && HttpContext.Request.Cookies["token_type"] != null)
            {

                UserData userData = request.GetAsync<UserData>("/api/Validation/GetValidationData", GetHeaderWithToken()).Result;
                if (userData.Id != 0 && userData.Role != null)
                {
                    ViewBag.Id = userData.Id;
                    ViewBag.Role = userData.Role;
                    ViewBag.isLoggedIn = true;

                    return userData;
                }
                else
                {
                    Logout();
                    ViewBag.isLoggedIn = false;
                    return null;
                }
            }
            else
            {
                ViewBag.isLoggedIn = false;
                Logout();
                return null;
            }
        }

        public Dictionary<string, string> GetHeaderWithToken()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"{HttpContext.Request.Cookies["token_type"].Value} {HttpContext.Request.Cookies["access_token"].Value}");
            return headers;
        }

        public ActionResult AddBook(LibraryViewModel libraryViewModel)
        {
            GetValidationData();
            ViewBag.isBookAdded = null;
            if (libraryViewModel.Book != null && libraryViewModel.Book.ISBN10 != null)
            {
                bool result = request.PostJsonAsync<bool>("api/Book/AddByISBN".SetQueryParams(new { ISBN = libraryViewModel.Book.ISBN10 }), null, GetHeaderWithToken()).Result;
                ViewBag.isBookAdded = result;
            }
            return View();
        }

        public ActionResult NearestLibraries()
        {
            GetValidationData();
            return View();
        }

        public ActionResult _NearestLibraries(string currentLocation, string sortingString = null)
        {

            object body = new
            {
                location = currentLocation,
                sortingString = "duration"
            };
            List<LibraryDto> librariesDto = request.GetAsync<List<LibraryDto>>("/api/Library/GetByLocation".SetQueryParams(new { location = currentLocation, sortingString = sortingString })).Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel() { Libraries = librariesDto };
            return PartialView(libraryViewModel);
        }

        [HttpGet]
        public ActionResult UserRegister()
        {
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult UserRegister(LibraryViewModel libraryViewModel)
        {
            var anyEmail = request.PostJsonAsync<bool>("/api/User/EmailCheck".SetQueryParams(new { email = libraryViewModel.User.Email }), null).Result;
            var anyNickname = request.PostJsonAsync<bool>("/api/User/NicknameCheck".SetQueryParams(new { nickname = libraryViewModel.User.Nickname }), null).Result;
            if (!anyEmail && !anyNickname)
            {
                object adminData = new
                {
                    grant_type = "password",
                    username = "furkanerol@outlook.com" + "|admin",
                    password = "deneme"
                };
                try
                {
                    var token = request.PostUrlEncodedAsync<Token>("/token", adminData).Result;
                    var headers = new Dictionary<string, string> {
                        {"Content-Type", "application/json" },
                        { "Authorization", $"{token.token_type} {token.access_token}"}
                    };
                    bool response = request.PostJsonAsync<bool>("/api/User/Add", libraryViewModel.User, headers).Result;
                }
                catch (Exception ex)
                {
                    
                }
            }
            return RedirectToAction("UserLogin");
        }

        [HttpGet]
        public ActionResult OfficerRegister()
        {
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            libraryViewModel.Libraries = request.GetAsync<List<LibraryDto>>("/api/Library/Get").Result;
            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult OfficerRegister(LibraryViewModel libraryViewModel)
        {
            var anyEmail = request.PostJsonAsync<bool>("/api/Officer/EmailCheck".SetQueryParams(new { email = libraryViewModel.Officer.Email }), null).Result;
            var anyIdentityNumber = request.PostJsonAsync<bool>("/api/Officer/IdentityNumberCheck".SetQueryParams(new { identityNumber = libraryViewModel.Officer.IdentityNumber }), null).Result;
            var anyPhoneNumber = request.PostJsonAsync<bool>("/api/Officer/PhoneNumberCheck".SetQueryParams(new { phoneNumber = libraryViewModel.Officer.PhoneNumber }), null).Result;
            if (!anyEmail && !anyIdentityNumber && !anyPhoneNumber)
            {
                object adminData = new
                {
                    grant_type = "password",
                    username = "furkanerol@outlook.com" + "|admin",
                    password = "deneme"
                };
                try
                {
                    var token = request.PostUrlEncodedAsync<Token>("/token", adminData).Result;
                    var headers = new Dictionary<string, string> {
                        {"Content-Type", "application/json" },
                        { "Authorization", $"{token.token_type} {token.access_token}"}
                    };
                    bool response = request.PostJsonAsync<bool>("/api/Officer/Add", libraryViewModel.Officer, headers).Result;
                }
                catch (Exception ex)
                {

                }
            }
            return RedirectToAction("OfficerLogin");
        }

    }
}