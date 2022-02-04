using LibraryProjectWebSite.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LibraryProjectWebSite.Models
{
    public class LibraryViewModel
    {
        public List<BookDto> Books { get; set; }

        public List<AuthorDto> Authors { get; set; }

        public List<BookLibrariesDto> BookLibraries { get; set; }

        public List<BorrowDto> Borrows { get; set; }

        public List<CategoryDto> Categories{ get; set; }

        public List<CommentDto> Comments { get; set; }

        public List<LibraryDto> Libraries { get; set; }

        public List<OfficerDto> Officers { get; set; }

        public List<UserDto> Users { get; set; }

        public List<SelectListItem> LibrarySelectListItem { get; set; }

        public BookDto Book { get; set; }

        public AuthorDto Author { get; set; }

        public BookLibrariesDto BookLibrary { get; set; }

        public BorrowDto Borrow { get; set; }

        public CategoryDto Category { get; set; }

        public CommentDto Comment { get; set; }

        public LibraryDto Library { get; set; }

        public OfficerDto Officer { get; set; }

        public UserDto User { get; set; }


    }
}