using System.Collections;

namespace ConcertApp.Ui.Models;

public record ManageAccountViewModel(
    bool IsAuthenticated,
    string? Username,
    string? AuthenticationType,
    List<ClaimViewModel> Claims)
{

}