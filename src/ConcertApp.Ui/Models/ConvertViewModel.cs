
namespace ConcertApp.Ui.Models;

public record ConvertViewModel( List<ConvertViewModel.ConcertItem> Concerts)
{
    public record ConcertItem(string Artist, string Venue, DateTime Date, string ImageUrl, string Name, string Description, string Id)
    {
    }
}