using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Photos
    {
        [Key]
        public int PhotoId
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

        [Required]
        [StringLength(1000)]
        public string Filename
        {
            get;
            set;
        }

        [Required]
        [StringLength(1000)]
        public string Url
        {
            get;
            set;
        }
    }
}
