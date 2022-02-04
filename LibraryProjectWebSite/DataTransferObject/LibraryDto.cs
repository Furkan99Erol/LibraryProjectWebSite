using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class LibraryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Location { get; set; }

        public int Status { get; set; }

        public int Distance { get; set; }

        public int Duration { get; set; }
        public int Quantity { get; set; }
        public List<OfficerDto> Officers { get; set; }

        public List<BorrowDto> Borrows { get; set; }

        public List<BookDto> Books { get; set; }

        public List<UserDto> Users { get; set; }
    }
}
