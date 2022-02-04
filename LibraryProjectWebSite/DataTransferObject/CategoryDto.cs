using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class CategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ClickCounter { get; set; }

        public List<BookDto> Books { get; set; }

    }
}
