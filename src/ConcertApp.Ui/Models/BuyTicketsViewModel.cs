using System.Collections.Generic;

namespace ConcertApp.Ui.Models
{
    public class BuyTicketsViewModel
    {
        public List<ConcertItem> Concerts { get; set; }

        public class ConcertItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public decimal Price { get; set; }
        }
    }
}