namespace JustShareBackend.Models
{
    public class Post
    {
        public int Id { get; set; }
        public String Description { get; set; }
        public String? ImageId { get; set; }
        public String UserId { get; set; }
    }
}
