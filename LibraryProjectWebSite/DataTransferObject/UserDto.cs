using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Nickname { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public List<CommentDto> Comments { get; set; }

        public List<BorrowDto> Borrows { get; set; }
        public List<BookDto> Books { get; set; }

        public List<LibraryDto> Libraries { get; set; }
    }
}
