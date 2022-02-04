using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class AuthorDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<BookDto> Books { get; set; }
    }
}
