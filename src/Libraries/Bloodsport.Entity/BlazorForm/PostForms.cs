namespace Bloodsport.Entity.BlazorForm
{
    public class PostForm
    {
        public string Title { get; set; } = string.Empty;

        public string Markdown { get; set; } = string.Empty;
    }

    public class CommentForm
    {
        public string Body { get; set; } = string.Empty;
    }
}
