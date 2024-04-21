using ConcertApp.WeatherManagement.Controllers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertApp.WeatherManagement.Controllers;

[ApiController]
[Authorize(Policy = "OnlyBelgium")]
[Route("api/[controller]")]
public class ConcertController : ControllerBase
{
    // Constructor and other dependencies would be injected here

    public ConcertController()
    {
    }

    // GET api/concert/availability
    [HttpGet("availability")]
    public IActionResult CheckAvailability()
    {
        // Placeholder method to check seat availability
        // This could be non-protected if you just want to show available seats without booking
        return Ok("Seats available: 100");
    }
    
    // POST api/concert/buy
    [HttpPost("buy")]
    [Authorize(Policy = "LoggedInCustomer")] // Protect this endpoint so that only authenticated users can access it
    public IActionResult BuyTicket([FromBody] TicketPurchase purchase)
    {
        // Logic to handle ticket purchase
        if (User.Identity?.IsAuthenticated == true)
        {
            // Process the purchase with user's details
            // For example, verify sufficient seats, deduct availability, issue ticket, etc.
                
            return Ok($"Ticket for {purchase.ConcertId} purchased successfully!");
        }

        return Unauthorized("You need to be logged in to buy a ticket.");
    }
}