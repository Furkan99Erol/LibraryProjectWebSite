using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class BorrowDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int BookId { get; set; }

        public int LibraryId { get; set; }

        public DateTime BorrowDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string BorrowType { get; set; }

        public string DestinationAddress { get; set; }

        public int Status { get; set; }

        public BookDto Book { get; set; }

        public LibraryDto Library { get; set; }

        public UserDto User { get; set; }
    }
}
