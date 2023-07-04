using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tablebooking.Exceptions;
using Tablebooking.Models;

namespace Tablebooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var tables = _dbContext.DinningTables.ToList();
            return View(tables);
        }

        [HttpGet]
        public IActionResult Create(int tableId)
        {
            var table = _dbContext.DinningTables
                .Include(t => t.Bookings)
                .FirstOrDefault(t => t.DinningTableID == tableId);
            
            

            if (table == null)
            {
                return NotFound();
            }
            if (!table.Availability)
            {
                throw new TableBookingException("Table already booked");
            }

            return View(table);
        }

        [HttpPost]
        public IActionResult Create(int tableId, DateTime reservationDate, TimeSpan timeSlot)
        {
            // try
            // {
                var table = _dbContext.DinningTables
                    .Include(t => t.Bookings)
                    .FirstOrDefault(t => t.DinningTableID == tableId);

                if (table == null)
                {
                    return NotFound();
                }

                if (!table.Availability)
                {
                    throw new TableBookingException("Table already booked");
                }

                DateTime targetDate = new DateTime(2023, 7, 1); // Replace with your desired date

                if (reservationDate < targetDate)
                {
                    throw new TableBookingException("Invalid reservation date");
                }

                // Console.WriteLine(timeSlot.Duration());
                // Console.WriteLine(TimeSpan.FromHours(2));
                // if (timeSlot.Duration() < TimeSpan.FromHours(2))
                // {
                //     throw new TableBookingException("Duration exceeded");
                // }

                if (table.Bookings.Any(b =>
                    b.ReservationDate.Date == reservationDate.Date &&
                    b.TimeSlot == timeSlot))
                {
                    throw new TableBookingException("Table already booked for the selected time slot");
                }

                var booking = new Booking
                {
                    DinningTableID = table.DinningTableID,
                    ReservationDate = reservationDate.Date,
                    TimeSlot = timeSlot
                };

                _dbContext.Bookings.Add(booking);
                table.Availability = false;
                _dbContext.SaveChanges();

                return RedirectToAction("Confirmation", new { bookingId = booking.BookingID });
            // }
            // catch (TableBookingException ex)
            // {
            //     ModelState.AddModelError(string.Empty, ex.Message);
            // }

            var selectedTable = _dbContext.DinningTables
                .Include(t => t.Bookings)
                .FirstOrDefault(t => t.DinningTableID == tableId);

            return View(selectedTable);
        }

        public IActionResult Confirmation(int bookingId)
        {
            var booking = _dbContext.Bookings
                .Include(b => b.DinningTable)
                .FirstOrDefault(b => b.BookingID == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}
