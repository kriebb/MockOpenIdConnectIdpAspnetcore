namespace ConcertApp.Ui.Models;

public record IndexViewModel(
    bool IsAuthenticated,
    string? Username,
    string? AuthenticationType,
    List<ClaimViewModel> Claims)
{

}