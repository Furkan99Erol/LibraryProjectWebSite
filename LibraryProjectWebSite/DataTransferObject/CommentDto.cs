using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectWebSite.DataTransferObject
{
    public class CommentDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int BookId { get; set; }

        public string CommentString { get; set; }

        public DateTime Date { get; set; }

        public int Status { get; set; }

        public BookDto Book { get; set; }

        public UserDto User { get; set; }
    }
}
