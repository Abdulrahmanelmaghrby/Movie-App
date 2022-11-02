using System.ComponentModel.DataAnnotations;

namespace MovieAppCRUD.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Title { get; set; }
        
        public string StoryLine { get; set; }
        public double Rate { get; set; }
        public int Year { get; set; }
        
        public byte[] Poster { get; set; }
        public byte GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
