using System.ComponentModel.DataAnnotations;

namespace MVC_SSL_Chat.Models
{
    /// <summary>
    /// The model for wrapping the data from the javascript code.
    /// </summary>
    public class MessageViewModel
    {
        public string? AuthorName { get; set; }
        [Required]
        public string? Content { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
