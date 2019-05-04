using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class ViewModel
    {
            public Models.BlogPosts Blogs { get; set; }
            public Models.Users Users { get; set; }
            
            public List<CommentList> Comments { get; set; }
            public List<BadWords> Badwords { get; set; }
        }

        public class CommentList
        {
            public Comments Comments { get; set; }
            public string Author { get; set; }
            public string AuthorEmail { get; set; }
        }
}
