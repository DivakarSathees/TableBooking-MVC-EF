using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tablebooking.Models

{
    public class DinningTable
    {
        public int DinningTableID { get; set; }
        public int SeatingCapacity { get; set; }
        public bool Availability { get; set; }
        public List<Booking> Bookings { get; set; }
    }
}