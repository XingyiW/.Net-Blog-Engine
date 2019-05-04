using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Comments
    {
        [Key]
        public int CommentId
        {
            get;
            set;
        }

        [ForeignKey("BlogPostId")]
        public BlogPosts BlogPosts
        {
            get;
            set;
        }
        public int BlogPostId
        {
            get;
            set;
        }

        [ForeignKey("UserId")]
        public Users Users
        {
            get;
            set;
        }
        public int UserId
        {
            get;
            set;
        }

        [Required]
        [StringLength(1000)]
        public string Content
        {
            get;
            set;
        }

        [Required]
        [StringLength(1000)]
        public int Rating
        {
            get;
            set;
        }
    }
}
