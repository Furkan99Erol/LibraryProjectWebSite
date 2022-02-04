using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class BookLibrariesDto
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int LibraryId { get; set; }

        public int Quantity { get; set; }

        public BookDto Book { get; set; }

        public LibraryDto Library { get; set; }
    }
}
