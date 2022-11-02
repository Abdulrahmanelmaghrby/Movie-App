using MovieAppCRUD.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace MovieAppCRUD.ViewModels
{
    public class MovieFormViewModel
    {

        public int Id { get; set; }
        [Required,StringLength(255)]
        public string Title { get; set; }

        [Required,StringLength(2500)]
        public string StoryLine { get; set; }

        [Range(1,10)]
        public double Rate { get; set; }
        public int Year { get; set; }

        public byte[] Poster { get; set; }

        [Display(Name ="Genre")]
        public byte GenreId { get; set; }
        
        public IEnumerable<Genre> Genres { get; set; }
    }
}
