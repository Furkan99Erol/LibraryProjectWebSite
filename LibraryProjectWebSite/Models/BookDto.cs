using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.Models
{

    public class BookDto
    {

        public int Id { get; set; }

        public string ISBN10 { get; set; }

        public string ISBN13 { get; set; }

        public string Name { get; set; }

        public string Publisher { get; set; }

        public int? NumberOfPages { get; set; }

        public int? Revision { get; set; }

        public int? LatestRevision { get; set; }

        public string Language { get; set; }

        public DateTime? CreateDate { get; set; }

        public string Description { get; set; }

        public int ClickCounter { get; set; }

        public List<CommentDto> Comments { get; set; }

        public List<BorrowDto> Borrows { get; set; }

        public List<UserDto> Users { get; set; }

        public List<AuthorDto> Authors { get; set; }

        public List<CategoryDto> Categories { get; set; }

        public List<LibraryDto> Libraries { get; set; }



    }


}


