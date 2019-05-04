using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class BadWords
    {
        [Key]
        public int BadWordId
        {
            get;
            set;
        }

        public string Word
        {
            get;
            set;
        }

    }
}
