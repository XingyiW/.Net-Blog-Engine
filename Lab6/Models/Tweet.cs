using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab6.Models
{
    public class Tweet
    {
        public int TweetId
        {
            get;
            set;
        }

        /// <summary>
        /// The person who created the tweet
        /// </summary>
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// The content of the tweet
        /// </summary>
        public string Content
        {
            get;
            set;
        }
    }
}
