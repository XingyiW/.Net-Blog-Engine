using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class BlogPosts
    {
        [Key]
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
        public string Title
        {
            get;
            set;
        }

        [Required]
        [StringLength(1000)]
        public string ShortDescription
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

        public DateTime Posted
        {
            get;
            set;
        }

        public bool IsAvailable
        {
            get;
            set;
        }

        public List<Comments> Comments
        {
            get;
            set;
        }

        public List<Photos> Photos
        {
            get;
            set;
        }
    }
}
