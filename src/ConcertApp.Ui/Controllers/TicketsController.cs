using ConcertApp.Ui.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ConcertApp.Ui.Controllers
{
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            var model = new BuyTicketsViewModel
            {
                Concerts = new List<BuyTicketsViewModel.ConcertItem>
                {
                    // Add your concerts here
                }
            };

            return View(model);
        }
        

            public async Task<IActionResult> Buy(int id)
            {
                // Retrieve the concert with the given id from the database
                // var concert = await _concertService.GetConcertById(id);

                // Check if the concert exists
                // if (concert == null)
                // {
                //     return NotFound();
                // }

                // Process the ticket buying

                // Redirect the user to a confirmation page
                return RedirectToAction("Confirmation");
            }
        
            
            public IActionResult Confirmation()
            {
                var model = new ConfirmationViewModel
                {
                    Message = "Your ticket purchase was successful!"
                };

                return View(model);
            }
    
    }
}