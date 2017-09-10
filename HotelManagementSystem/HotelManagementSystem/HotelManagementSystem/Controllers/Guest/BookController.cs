using HotelManagementSystem.Models;
using System;
using System.Web.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class BookController : Controller
    {
        //[HttpPost]
        public ActionResult BookRoom(string bookingDate, string roomType, int roomCount, double roomPrice)
        {
            string[] bookinDates = bookingDate.Split(' ');
            DateTime checkInDate = Convert.ToDateTime(bookinDates[0]).Add(new TimeSpan(14, 0, 0));
            DateTime checkOutDate = Convert.ToDateTime(bookinDates[1]).Add(new TimeSpan(11, 0, 0));

            BookViewModel bvmObj = new BookViewModel(new RoomDetail(checkInDate, checkOutDate, roomType.Trim(), roomCount, roomPrice));

            return View(bvmObj);
        }

        [HttpPost]
        public ActionResult ConfirmBooking(BookViewModel bvmObj)
        {
            if (bvmObj != null)
            {
                BookingDetail bdObj = bvmObj.AddBookingDetails();
                return RedirectToAction("BookingDetails", bdObj);
                //return View(bdObj);
            }

            return RedirectToAction("BookingDetails", new BookingDetail());

        }

        public ActionResult BookingDetails(BookingDetail bdObj)
        {
            string body = $"<h2>Thank you for your booking...!!!</h2> <hr><h4>Booking Details</h4> Confirmation Code: <b>{bdObj.confirmationCode}</b> <br> Check-in Date: <b>{bdObj.checkInDate}</b> <br> Check-out Date: <b>{bdObj.checkOutDate}</b> <br> Room-Type: <b>{bdObj.roomType}</b> <br> Room Count: <b>{bdObj.roomCount}</b> <br> Amount Paid: <b>{bdObj.totalAmountPaid}</b>";
            SMTP.SMTP.SendSMTPMail(bdObj.emailAddr, body);
            return View(bdObj);
        }
    }
}