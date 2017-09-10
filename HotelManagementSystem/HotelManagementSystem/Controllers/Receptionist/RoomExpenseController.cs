using HotelManagementSystem.Models;
using System;
using System.Web.Mvc;

namespace HotelManagementSystem.Controllers.Receptionist
{
    public class RoomExpenseController : Controller
    {
        // GET: RoomExpense
        public ActionResult Index()
        {    
            return View(new RoomExpense());
        }

        [HttpPost]
        public ActionResult Index(RoomExpense reObj)
        { 
            return RedirectToAction("RoomExpenseFeedback", new {rowsAdded = reObj.AddExpense(), roomNum= reObj.roomNumber, description = reObj.description});
        }

        public ActionResult RoomExpenseFeedback(int rowsAdded, int roomNum, string description)
        {
            ViewBag.description = description;
            ViewBag.rowsAdded = rowsAdded;
            ViewBag.roomNum = roomNum;

            return View();
        }

    }
}