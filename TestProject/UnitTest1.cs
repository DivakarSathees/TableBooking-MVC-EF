using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Tablebooking.Controllers;
using Tablebooking.Models;
using Tablebooking.Exceptions;

namespace Tablebooking.Tests
{
    [TestFixture]
    public class TableControllerTests
    {
        private ApplicationDbContext _dbContext;
        private TableController _tableController;
        private BookingController _bookingController;


        [SetUp]
        public void Setup()
        {
            // Initialize a new in-memory ApplicationDbContext for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _tableController = new TableController(_dbContext);
            _bookingController = new BookingController(_dbContext);

        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the ApplicationDbContext and reset the database
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public void AvailableSlots_ReturnsViewResult()
        {
            // Arrange
            var table1 = new DinningTable { DinningTableID = 1, SeatingCapacity = 4, Availability = true };
            var table2 = new DinningTable { DinningTableID = 2, SeatingCapacity = 2, Availability = true };
            _dbContext.DinningTables.AddRange(table1, table2);
            _dbContext.SaveChanges();

            // Act
            var result = _tableController.AvailableSlots() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void AvailableSlots_ReturnsViewResult_WithListOfAvailableTables()
        {
            // Arrange
            var table1 = new DinningTable { DinningTableID = 1, SeatingCapacity = 4, Availability = true };
            var table2 = new DinningTable { DinningTableID = 2, SeatingCapacity = 2, Availability = true };
            _dbContext.DinningTables.AddRange(table1, table2);
            _dbContext.SaveChanges();

            // Act
            var result = _tableController.AvailableSlots() as ViewResult;

            // Assert            
            var model = result.Model as List<DinningTable>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
            CollectionAssert.Contains(model, table1);
            CollectionAssert.Contains(model, table2);
        }

        [Test]
        public void BookedSlots_ReturnsViewResult()
        {
            // Arrange
            var table1 = new DinningTable { DinningTableID = 1, SeatingCapacity = 4, Availability = false };
            var table2 = new DinningTable { DinningTableID = 2, SeatingCapacity = 2, Availability = false };
            _dbContext.DinningTables.AddRange(table1, table2);
            _dbContext.SaveChanges();

            // Act
            var result = _tableController.BookedSlots() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void BookedSlots_ReturnsViewResult_WithListOfBookedTables()
        {
            // Arrange
            var table1 = new DinningTable { DinningTableID = 1, SeatingCapacity = 4, Availability = false };
            var table2 = new DinningTable { DinningTableID = 2, SeatingCapacity = 2, Availability = false };
            _dbContext.DinningTables.AddRange(table1, table2);
            _dbContext.SaveChanges();

            // Act
            var result = _tableController.BookedSlots() as ViewResult;

            var model = result.Model as List<DinningTable>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
            CollectionAssert.Contains(model, table1);
            CollectionAssert.Contains(model, table2);
        }

    
        [Test]  
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = true };
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            // Act
            var result = _bookingController.Create(tableId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Create_Get_InvalidTableId_ReturnsNotFound()
        {
            // Arrange
            var tableId = 1;

            // Act
            var result = _bookingController.Create(tableId) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Create_Post_ValidBooking_Success()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = true };
            var reservationDate = new DateTime(2023, 7, 5);
            var timeSlot = TimeSpan.FromHours(10);
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            // Act
            var result = _bookingController.Create(tableId, reservationDate, timeSlot) as RedirectToActionResult;
            var booking = _dbContext.Bookings.Include(b => b.DinningTable).FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Confirmation", result.ActionName);
            Assert.IsNotNull(booking);
            Assert.AreEqual(tableId, booking.DinningTable.DinningTableID);
            Assert.AreEqual(reservationDate.Date, booking.ReservationDate.Date);
            Assert.AreEqual(timeSlot, booking.TimeSlot);
            Assert.IsFalse(booking.DinningTable.Availability);
        }

        [Test]
        public void Create_Post_InvalidTableId_ReturnsNotFound()
        {
            // Arrange
            var tableId = 1;
            var reservationDate = DateTime.Now.AddDays(1);
            var timeSlot = TimeSpan.FromHours(10);

            // Act
            var result = _bookingController.Create(tableId, reservationDate, timeSlot) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Create_Post_TableAlreadyBooked_ThrowsException()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = false };
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            Assert.Throws<TableBookingException>(() => _bookingController.Create(tableId));
        }

        [Test]
        public void Create_Post_TableAlreadyBooked_ThrowsException_with_Message()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = false };
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            var msg = Assert.Throws<TableBookingException>(() => _bookingController.Create(tableId));
            Assert.AreEqual("Table already booked", msg.Message);
        }

        [Test]
        public void Create_Post_InvalidReservationDate_ThrowsException()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = true };
            var reservationDate = new DateTime(2023, 1, 1);
            var timeSlot = TimeSpan.FromHours(10);
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            // Act & Assert
            Assert.Throws<TableBookingException>(() =>
            {
                _bookingController.Create(tableId, reservationDate, timeSlot);
            });
        }

        [Test]
        public void Create_Post_InvalidReservationDate_ThrowsException_with_message()
        {
            // Arrange
            var tableId = 1;
            var table = new DinningTable { DinningTableID = tableId, SeatingCapacity = 4, Availability = true };
            var reservationDate = new DateTime(2023, 1, 1);
            var timeSlot = TimeSpan.FromHours(10);
            _dbContext.DinningTables.Add(table);
            _dbContext.SaveChanges();

            // Act & Assert
            var msg = Assert.Throws<TableBookingException>(() =>
            {
                _bookingController.Create(tableId, reservationDate, timeSlot);
            });
            Assert.AreEqual("Invalid reservation date", msg.Message);
        }


        [Test]
        public void Confirmation_InvalidBookingId_ReturnsNotFound()
        {
            // Arrange
            var bookingId = 1;

            // Act
            var result = _bookingController.Confirmation(bookingId) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
        }

         [Test]
        public void Booking_Properties_BookingID_GetSetCorrectly()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.BookingID = 1;

            // Assert
            Assert.AreEqual(1, booking.BookingID);
        }
        [Test]
        public void Booking_Properties_DinningTableID_GetSetCorrectly()
        {
            // Arrange
            var booking = new Booking();

            // Act
            booking.DinningTableID = 2;

            // Assert
            Assert.AreEqual(2, booking.DinningTableID);
        }

        [Test]
        public void Booking_Properties_ReservationDate_GetSetCorrectly()
        {
            // Arrange
            var booking = new Booking();

            booking.ReservationDate = new DateTime(2023, 7, 1);
            
            Assert.AreEqual(new DateTime(2023, 7, 1), booking.ReservationDate);
        }

        [Test]
        public void Booking_Properties_TimeSlot_GetSetCorrectly()
        {
            // Arrange
            var booking = new Booking();

            booking.TimeSlot = new TimeSpan(14, 0, 0);

            Assert.AreEqual(new TimeSpan(14, 0, 0), booking.TimeSlot);
        }

        [Test]
        public void Booking_Properties_BookingID_HaveCorrectDataTypes()
        {
            // Arrange
            var booking = new Booking();

            // Assert
            Assert.That(booking.BookingID, Is.TypeOf<int>());
        }
        [Test]
        public void Booking_Properties_DinningTableID_HaveCorrectDataTypes()
        {
            // Arrange
            var booking = new Booking();

            // Assert
            Assert.That(booking.DinningTableID, Is.TypeOf<int>());
        }

        [Test]
        public void Booking_Properties_ReservationDate_HaveCorrectDataTypes()
        {
            // Arrange
            var booking = new Booking();
            Assert.That(booking.ReservationDate, Is.TypeOf<DateTime>());
        }

        [Test]
        public void Booking_Properties_TimeSlot_HaveCorrectDataTypes()
        {
            // Arrange
            var booking = new Booking();

            Assert.That(booking.TimeSlot, Is.TypeOf<TimeSpan>());
        }

        
        [Test]
        public void DinningTableClassExists()
        {
            var dinningTable = new DinningTable();
        
            Assert.IsNotNull(dinningTable);
        }
        
        [Test]
        public void BookingClassExists()
        {
            var booking = new Booking();
        
            Assert.IsNotNull(booking);
        }
        
        [Test]
        public void ApplicationDbContextContainsDbSetSlotProperty()
        {
            // using (var context = new ApplicationDbContext(_dbContextOptions))
            //         {
            // var context = new ApplicationDbContext();
        
            var propertyInfo = _dbContext.GetType().GetProperty("Bookings");
        
            Assert.IsNotNull(propertyInfo);
            Assert.AreEqual(typeof(DbSet<Booking>), propertyInfo.PropertyType);
                    // }
        }
        
        [Test]
        public void ApplicationDbContextContainsDbSetBookingPropertyInfo()
        {
            // using (var context = new ApplicationDbContext(_dbContextOptions))
            //         {
            // var context = new ApplicationDbContext();
        
            var propertyInfo = _dbContext.GetType().GetProperty("DinningTables");
        
            Assert.IsNotNull(propertyInfo);
            Assert.AreEqual(typeof(DbSet<DinningTable>), propertyInfo.PropertyType);
        }

        [Test]
        public void ApplicationDbContextContainsDbSetBookingProperty()
        {
            // using (var context = new ApplicationDbContext(_dbContextOptions))
            //         {
            // var context = new ApplicationDbContext();
        
            var propertyInfo = _dbContext.GetType().GetProperty("DinningTables");
        
            Assert.AreEqual(typeof(DbSet<DinningTable>), propertyInfo.PropertyType);
        }

        [Test]
        public void DinningTable_Properties_GetSetCorrectly()
        {
            // Arrange
            var dinningTable = new DinningTable();

            // Act
            dinningTable.DinningTableID = 1;
            dinningTable.SeatingCapacity = 4;

            // Assert
            Assert.AreEqual(1, dinningTable.DinningTableID);
            Assert.AreEqual(4, dinningTable.SeatingCapacity);
        }

        [Test]
        public void DinningTable_Properties_Availability_GetSetCorrectly()
        {
            // Arrange
            var dinningTable = new DinningTable();

            dinningTable.Availability = true;

            Assert.IsTrue(dinningTable.Availability);
        }

        [Test]
        public void DinningTable_Properties_HaveCorrectDataTypes()
        {
            // Arrange
            var dinningTable = new DinningTable();

            // Assert
            Assert.That(dinningTable.DinningTableID, Is.TypeOf<int>());
            Assert.That(dinningTable.SeatingCapacity, Is.TypeOf<int>());
            Assert.That(dinningTable.Availability, Is.TypeOf<bool>());
            // Assert.That(dinningTable.Bookings, Is.TypeOf<List<Booking>>());
        }
        [Test]
        public void DinningTable_Properties_Availability_HaveCorrectDataTypes()
        {
            // Arrange
            var dinningTable = new DinningTable();

            Assert.That(dinningTable.Availability, Is.TypeOf<bool>());
        }



    }
}
