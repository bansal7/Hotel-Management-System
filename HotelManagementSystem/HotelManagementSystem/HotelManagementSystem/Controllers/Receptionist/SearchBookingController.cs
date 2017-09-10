using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class SearchBookingController : Controller
    {
        static BookingDetails obj { get; set; }

        // GET: SearchBooking
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string confirmationCode)
        {
            obj = new BookingDetails();

            obj.GetBookingDetails(confirmationCode);

            obj.resultData.roomCountCheckedIn = obj.GetRoomCountCheckedIn(confirmationCode);

            return View(obj);
        }

        [HttpPost]
        public ActionResult SearchEmptyRoom()
        {
            int roomCount = obj.resultData.roomCount;
            string confirmationCode = obj.resultData.confirmationCode;

            SearchEmptyRoom serObj = new SearchEmptyRoom(confirmationCode, roomCount);

            if (serObj.checkedInRoomCount == roomCount)
            {
                ViewBag.Message = "CheckIn successfully completed for all room(s)";
                return View();
            }

            ViewBag.Message = "Available Rooms";

            serObj.GetEmptyRooms(confirmationCode);

            return View(serObj);
        }

        [HttpPost]
        public ActionResult CheckIn(int roomNumber = 0)
        {
            SearchEmptyRoom serObj = new SearchEmptyRoom();

            serObj.CheckInRoom(obj.resultData.confirmationCode, roomNumber);

            if (serObj.message == null)
            {
                ViewBag.Message = "CheckIn Done successfuly for room number: " + roomNumber;
            }
            else
            {
                ViewBag.Message = serObj.message;
            }

            return View(serObj);
        }
    }
}