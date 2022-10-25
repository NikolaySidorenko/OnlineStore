using System.Collections.Generic;

namespace OnlineStore.Data.Providers.Context.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
    }
}
