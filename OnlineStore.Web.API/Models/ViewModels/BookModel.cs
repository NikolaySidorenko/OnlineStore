using System.Collections.Generic;

namespace OnlineStore.Web.API.Models.ViewModels
{
    public class BookModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<int> Categories { get; set; }
    }
}
