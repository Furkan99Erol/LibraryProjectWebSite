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
        // GET: Home
        public ActionResult Index()
        {
            UserData userData = new UserData();
            userData = GetValidationData();
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            if (userData == null || userData.Role == "user")
            {
                List<BookDto> booksDto = request.Get<List<BookDto>>("/api/Book/Get").Result;
                libraryViewModel.Books = booksDto;
            }
            else if (userData.Role == "officer")
            {
                libraryViewModel.Officer = request.Get<OfficerDto>("api/Officer/Get".SetQueryParams(new { id = userData.Id }), GetHeaderWithToken()).Result;
            }
            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult Index(string searchKey)
        {
            UserData userData = new UserData();
            userData = GetValidationData();
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            if (searchKey == "")
            {
                List<BookDto> booksDto = request.Get<List<BookDto>>("/api/Book/Get").Result;
                libraryViewModel.Books = booksDto;

            }
            else
            {
                List<BookDto> booksDto = request.Get<List<BookDto>>("/api/Book/Get".SetQueryParams(new { searchKey = searchKey })).Result;
                libraryViewModel.Books = booksDto;
            }

            return View(libraryViewModel);
        }


        public ActionResult _UnapprovedBorrow(string searchKey = null)
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "officer")
            {
                List<BorrowDto> borrowsDto = request.Get<List<BorrowDto>>("api/Officer/PendingApprovalBorrow".SetQueryParams(new { searchKey = searchKey }), GetHeaderWithToken()).Result;
                libraryViewModel.Borrows = borrowsDto;
            }

            return PartialView(libraryViewModel);
        }

        public ActionResult _UnapprovedComment()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            List<CommentDto> commentsDto = request.Get<List<CommentDto>>("api/Officer/PendingApprovalComment", GetHeaderWithToken()).Result;
            libraryViewModel.Comments = commentsDto;

            return PartialView(libraryViewModel);
        }

        public ActionResult _UnapprovedOfficer()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            List<OfficerDto> officerDto = request.Get<List<OfficerDto>>("api/Officer/GetUnapprovedOfficers", GetHeaderWithToken()).Result;
            libraryViewModel.Officers = officerDto;

            return PartialView(libraryViewModel);
        }


        [HttpPost]
        public ActionResult BorrowConfirmation(int id, string borrowString, int userId)
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "officer")
            {
                var postData = new
                {
                    userId = userId,
                    bookId = id,
                    borrowString = borrowString
                };
                request.PostJson<bool>("/api/Borrow/BorrowConfirmation".SetQueryParams(postData), null, GetHeaderWithToken());
            }
            return Json("");
        }

        [HttpPost]
        public ActionResult CommentConfirmation(int id, bool status)
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "officer")
            {
                if (status == true)
                {
                    request.PostJson<bool>("/api/Officer/CommentConfirmation".SetQueryParams(new { commentId = id }), null, GetHeaderWithToken());
                }
                else if (status == false)
                {
                    request.PostJson<bool>("/api/Officer/CommentDisapprovement".SetQueryParams(new { commentId = id }), null, GetHeaderWithToken());
                }
            }

            return Json("");
        }

        [HttpPost]
        public ActionResult OfficerConfirmation(int id, bool status)
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "admin")
            {
                if (status == true)
                {
                    request.PostJson<bool>("/api/Officer/ApproveOfficer".SetQueryParams(new { id = id }), null, GetHeaderWithToken());
                }
                else if (status == false)
                {
                    request.PostJson<bool>("/api/Officer/DisapproveOfficer".SetQueryParams(new { id = id }), null, GetHeaderWithToken());
                }
            }
            return Json("");
        }
        public ActionResult GetById(int id)
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null && (userData.Role == "officer" || userData.Role == "admin"))
            {
                return RedirectToAction("Index");
            }
            try
            {
                BookDto book = request.Get<BookDto>($"/api/Book/Get/{id}").Result;
                if (book.Users.Where(x => x.Id == userData.Id).Any())
                {
                    ViewBag.isFavourite = true;
                }
                else
                {
                    ViewBag.isFavourite = false;
                }
                request.PostJson<bool>("api/Book/IncreaseClickCounter".SetQueryParams(new { bookId = id }), null, GetHeaderWithToken());
                libraryViewModel.Comments = request.Get<List<CommentDto>>("/api/Comment/GetAllByBookId".SetQueryParams(new { id = id }), null).Result;
                libraryViewModel.Book = book;
                if (userData != null)
                {
                    bool doesHaveAlready = request.PostJson<bool>($"/api/Borrow/DoesHaveAlready".SetQueryParams(new { bookId = id }), null, GetHeaderWithToken()).Result;
                    if (doesHaveAlready)
                    {
                        ViewBag.doesHaveAlready = doesHaveAlready;
                        List<BorrowDto> borrows = request.Get<List<BorrowDto>>("/api/Borrow/BorrowHistory", GetHeaderWithToken()).Result;
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
                    else
                    {
                        ViewBag.doesHaveLibrary = false;
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
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }

            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult UserLogin(LibraryViewModel _libraryViewModel)
        {
            object userData = new
            {
                grant_type = "password",
                username = _libraryViewModel.User.Email + "|user",
                password = _libraryViewModel.User.Password
            };
            try
            {
                var token = request.PostUrlEncoded<Token>("/token", userData).Result;
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            catch (Exception ex)
            {
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.alertMessage = "Username or password is invalid.";
                TempData["model"] = libraryViewModel;
                return RedirectToAction("UserLogin");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AdminLogin()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }

            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult AdminLogin(LibraryViewModel _libraryViewModel)
        {
            object userData = new
            {
                grant_type = "password",
                username = _libraryViewModel.User.Email + "|admin",
                password = _libraryViewModel.User.Password
            };
            try
            {
                var token = request.PostUrlEncoded<Token>("/token", userData).Result;
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            catch (Exception ex)
            {
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.alertMessage = "Username or password is invalid.";
                TempData["model"] = libraryViewModel;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult OfficerLogin()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }

            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult OfficerLogin(LibraryViewModel _libraryViewModel)
        {
            object userData = new
            {
                grant_type = "password",
                username = _libraryViewModel.Officer.Email + "|officer",
                password = _libraryViewModel.Officer.Password
            };
            try
            {
                var token = request.PostUrlEncoded<Token>("/token", userData).Result;
                HttpCookie token_type = new HttpCookie("token_type", token.token_type);
                HttpCookie access_token = new HttpCookie("access_token", token.access_token);
                token_type.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                access_token.Expires = DateTime.Now.AddMinutes(Convert.ToDouble(token.expires_in));
                Response.SetCookie(token_type);
                Response.SetCookie(access_token);
            }
            catch (Exception ex)
            {
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.alertMessage = "Username or password is invalid.";
                TempData["model"] = libraryViewModel;
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
                LibraryId = libraryViewModel.Borrow.LibraryId,
                BookId = libraryViewModel.Borrow.BookId,
                BorrowType = libraryViewModel.Borrow.BorrowType,
                BorrowDate = libraryViewModel.Borrow.BorrowDate,
                ReturnDate = libraryViewModel.Borrow.ReturnDate,
                DestinationAddress = libraryViewModel.Borrow.DestinationAddress,
            };

            bool x = request.PostJson<bool>("api/Borrow/Borrow", borrowDto, GetHeaderWithToken()).Result;
            if (x == true)
            {
                libraryViewModel.alertMessage = "Borrow Completed";
            }
            else
            {
                libraryViewModel.alertMessage = "Borrow Uncompleted";
            }
            TempData["model"] = libraryViewModel;

            return RedirectToAction($"GetById/{libraryViewModel.Borrow.BookId}");
        }

        public UserData GetValidationData()
        {
            if (HttpContext.Request.Cookies["access_token"] != null && HttpContext.Request.Cookies["token_type"] != null)
            {

                UserData userData = request.Get<UserData>("/api/Validation/GetValidationData", GetHeaderWithToken()).Result;
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
                bool result = request.PostJson<bool>("api/Book/AddByISBN".SetQueryParams(new { ISBN = libraryViewModel.Book.ISBN10 }), null, GetHeaderWithToken()).Result;
                ViewBag.isBookAdded = result;
            }
            else if (libraryViewModel.Book != null && libraryViewModel.Book.ISBN13 != null)
            {
                bool result = request.PostJson<bool>("api/Book/AddByISBN".SetQueryParams(new { ISBN = libraryViewModel.Book.ISBN13 }), null, GetHeaderWithToken()).Result;
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
            List<LibraryDto> librariesDto = request.Get<List<LibraryDto>>("/api/Library/GetByLocation".SetQueryParams(new { location = currentLocation, sortingString = sortingString })).Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel() { Libraries = librariesDto };
            return PartialView(libraryViewModel);
        }

        [HttpGet]
        public ActionResult UserRegister()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult UserRegister(LibraryViewModel libraryViewModel)
        {
            var anyEmail = request.PostJson<bool>("/api/User/EmailCheck".SetQueryParams(new { email = libraryViewModel.User.Email }), null).Result;
            var anyNickname = request.PostJson<bool>("/api/User/NicknameCheck".SetQueryParams(new { nickname = libraryViewModel.User.Nickname }), null).Result;
            LibraryViewModel _libraryViewModel = new LibraryViewModel();
            if (!anyEmail)
            {
                if (!anyNickname)
                {
                    bool response = request.PostJson<bool>("/api/User/Add", libraryViewModel.User).Result;
                    if (response)
                    {
                        _libraryViewModel.alertMessage = "Successfully Registered";
                    }
                    else
                    {
                        _libraryViewModel.alertMessage = "Something Went Wrong";
                    }
                    TempData["model"] = _libraryViewModel;

                }
                else
                {
                    _libraryViewModel.alertMessage = "Nickname is already taken";
                    TempData["model"] = _libraryViewModel;

                    return RedirectToAction("UserRegister");
                }

            }
            else
            {
                _libraryViewModel.alertMessage = "Email is already taken";
                TempData["model"] = _libraryViewModel;
                return RedirectToAction("UserRegister");
            }
            return RedirectToAction("UserLogin");
        }

        [HttpGet]
        public ActionResult OfficerRegister()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData != null)
            {
                return RedirectToAction("Index");
            }
            libraryViewModel.Libraries = request.Get<List<LibraryDto>>("/api/Library/Get").Result;
            return View(libraryViewModel);
        }

        [HttpPost]
        public ActionResult OfficerRegister(LibraryViewModel libraryViewModel)
        {
            var anyEmail = request.PostJson<bool>("/api/Officer/EmailCheck".SetQueryParams(new { email = libraryViewModel.Officer.Email }), null).Result;
            var anyIdentityNumber = request.PostJson<bool>("/api/Officer/IdentityNumberCheck".SetQueryParams(new { identityNumber = libraryViewModel.Officer.IdentityNumber }), null).Result;
            var anyPhoneNumber = request.PostJson<bool>("/api/Officer/PhoneNumberCheck".SetQueryParams(new { phoneNumber = libraryViewModel.Officer.PhoneNumber }), null).Result;
            LibraryViewModel _libraryViewModel = new LibraryViewModel();
            if (!anyEmail)
            {
                if (!anyIdentityNumber)
                {
                    if (!anyPhoneNumber)
                    {
                        bool response = request.PostJson<bool>("/api/Officer/Add", libraryViewModel.Officer).Result;
                        if (response)
                        {
                            _libraryViewModel.alertMessage = "Successfully registered, you should wait admin to approve your account";
                        }
                        else
                        {
                            _libraryViewModel.alertMessage = "Something Went Wrong";
                        }
                        TempData["model"] = _libraryViewModel;
                        return RedirectToAction("OfficerLogin");
                    }
                    else
                    {
                        _libraryViewModel.alertMessage = "Phone Number is already taken";
                        TempData["model"] = _libraryViewModel;
                    }
                }
                else
                {
                    _libraryViewModel.alertMessage = "Identity Number is already taken";
                    TempData["model"] = _libraryViewModel;
                }
            }
            else
            {
                _libraryViewModel.alertMessage = "Email is already taken";
                TempData["model"] = _libraryViewModel;
            }
            return RedirectToAction("OfficerRegister");
        }

        [HttpPost]
        public ActionResult AddComment(LibraryViewModel libraryViewModel)
        {
            UserData userData = GetValidationData();
            object postData = new
            {
                UserId = userData.Id,
                Date = DateTime.Now,
                BookId = libraryViewModel.Comment.BookId,
                Comment1 = libraryViewModel.Comment.CommentString
            };
            request.PostJson<bool>("api/Comment/Add", postData, GetHeaderWithToken());
            return RedirectToAction($"GetById/{libraryViewModel.Comment.BookId}");
        }

        public ActionResult BorrowHistory()
        {
            UserData userData = GetValidationData();
            if (userData == null || userData.Role == "officer")
            {
                return RedirectToAction("Index");
            }
            else
            {
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                List<BorrowDto> borrowsDto = request.Get<List<BorrowDto>>("api/Borrow/BorrowHistory", GetHeaderWithToken()).Result;
                libraryViewModel.Borrows = borrowsDto;
                return View(libraryViewModel);
            }

        }

        public ActionResult EditUserAccount()
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "user")
            {
                UserDto user = request.Get<UserDto>("/api/User/Get".SetQueryParams(new { id = userData.Id }), GetHeaderWithToken()).Result;
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.User = user;
                return View(libraryViewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult EditUserAccount(LibraryViewModel libraryViewModel)
        {
            UserDto userDto = libraryViewModel.User;

            request.PostJson<bool>("/api/User/Update", userDto, GetHeaderWithToken());
            return RedirectToAction("EditUserAccount");
        }
        public ActionResult EditOfficerAccount()
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "officer")
            {
                OfficerDto officerDto = request.Get<OfficerDto>("/api/Officer/Get".SetQueryParams(new { id = userData.Id }), GetHeaderWithToken()).Result;
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.Officer = officerDto;
                return View(libraryViewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult EditOfficerAccount(LibraryViewModel libraryViewModel)
        {
            OfficerDto officerDto = libraryViewModel.Officer;

            request.PostJson<bool>("/api/Officer/Update", officerDto, GetHeaderWithToken());
            return RedirectToAction("EditOfficerAccount");
        }

        public ActionResult UserFavouriteBooks()
        {
            UserData userData = GetValidationData();
            if (userData != null && userData.Role == "user")
            {
                List<BookDto> favouriteBooks = request.Get<List<BookDto>>("api/User/GetFavouriteBook", GetHeaderWithToken()).Result;
                LibraryViewModel libraryViewModel = new LibraryViewModel();
                libraryViewModel.Books = favouriteBooks;
                return View(libraryViewModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddUserFavouriteBooks(int bookId)
        {
            request.PostJson<bool>("api/User/AddFavouriteBook".SetQueryParams(new { bookId = bookId }), null, GetHeaderWithToken());
            return RedirectToAction($"GetById/{bookId}");
        }

        [HttpPost]
        public ActionResult DeleteUserFavouriteBooks(int bookId)
        {
            var x = request.Delete<bool>("api/User/DeleteFavouriteBook".SetQueryParams(new { bookId = bookId }), GetHeaderWithToken()).Result;
            return RedirectToAction($"UserFavouriteBooks");
        }

        [HttpPost]
        public ActionResult DeleteUserFavouriteBooksFromPage(int bookId)
        {
            var x = request.Delete<bool>("api/User/DeleteFavouriteBook".SetQueryParams(new { bookId = bookId }), GetHeaderWithToken()).Result;
            return RedirectToAction($"GetById/{bookId}");
        }

        public ActionResult UserResetPassword()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData == null)
            {
                return View(libraryViewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public ActionResult UserResetPassword(string resetString, string password1, string password2)
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            if (password1 == password2)
            {
                bool response = request.PostJson<bool>("api/User/ResetPassword".SetQueryParams(new { resetString = resetString, password = password1 }), null).Result;
                if (response)
                {
                    libraryViewModel.alertMessage = "Password has been changed";
                }
                else
                {
                    libraryViewModel.alertMessage = "Something went wrong";
                }
                TempData["model"] = libraryViewModel;
                return RedirectToAction("UserLogin");
            }
            else
            {
                libraryViewModel.alertMessage = "Passwords are not matching.";
                TempData["model"] = libraryViewModel;
                return View(libraryViewModel);
            }
        }



        [HttpPost]
        public ActionResult UserResetPasswordSendEmail(string email)
        {
            var x = request.PostJson<string>("/api/User/ResetPasswordSendEmail".SetQueryParams(new { email = email }), null).Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            libraryViewModel.alertMessage = "Email has been sent. You can reset your password with key in 180 seconds.";
            TempData["model"] = libraryViewModel;
            return RedirectToAction("UserResetPassword");
        }

        public ActionResult OfficerResetPassword()
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            var model = TempData["model"] as LibraryViewModel != null ? TempData["model"] as LibraryViewModel : new LibraryViewModel();
            libraryViewModel.alertMessage = model.alertMessage;
            UserData userData = GetValidationData();
            if (userData == null)
            {
                return View(libraryViewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult OfficerResetPassword(string resetString, string password1, string password2)
        {
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            if (password1 == password2)
            {
                bool response = request.PostJson<bool>("api/Officer/ResetPassword".SetQueryParams(new { resetString = resetString, password = password1 }), null).Result;
                if (response)
                {
                    libraryViewModel.alertMessage = "Password has been changed";
                }
                else
                {
                    libraryViewModel.alertMessage = "Something went wrong";
                }
                TempData["model"] = libraryViewModel;
                return RedirectToAction("OfficerLogin");
            }
            else
            {
                libraryViewModel.alertMessage = "Passwords are not matching.";
                TempData["model"] = libraryViewModel;
                return View(libraryViewModel);
            }
        }

        [HttpPost]
        public ActionResult OfficerResetPasswordSendEmail(string email)
        {
            var x = request.PostJson<string>("/api/Officer/ResetPasswordSendEmail".SetQueryParams(new { email = email }), null).Result;
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            libraryViewModel.alertMessage = "Email has been sent. You can reset your password with key in 180 seconds.";
            TempData["model"] = libraryViewModel;
            return RedirectToAction("OfficerResetPassword");
        }

        public ActionResult UserTrack()
        {
            UserData userData = GetValidationData();
            LibraryViewModel libraryViewModel = new LibraryViewModel();
            if (userData != null && userData.Role == "user")
            {
                libraryViewModel.Borrows = request.Get<List<BorrowDto>>("api/Borrow/BorrowHistory", GetHeaderWithToken()).Result;
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View(libraryViewModel);
        }



    }
}