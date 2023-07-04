using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tablebooking.Models

{
    public class Booking
    {
        public int BookingID { get; set; }
        public int DinningTableID { get; set; }
        public DinningTable DinningTable { get; set; }
        public int CustomerID { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
    }
}