using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tablebooking.Models;

namespace Tablebooking.Controllers
{
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public TableController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult AvailableSlots()
        {
            var tables = _dbContext.DinningTables
                .Where(t => t.Availability)
                .ToList();
            return View(tables);
        }

        public IActionResult BookedSlots()
        {
            var tables = _dbContext.DinningTables
                .Where(t => !t.Availability)
                .Include(t => t.Bookings)
                .ToList();
            return View(tables);
        }
    }
}
