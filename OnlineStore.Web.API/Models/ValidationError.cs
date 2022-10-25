namespace OnlineStore.Web.API.Models
{
    public class ValidationError : Error
    {
        public string Field { get; set; }
    }
}
