using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class CheckOutController : Controller
    {
        // GET: CheckOut
        public ActionResult CheckOut()
        {
            ViewBag.Message = "CheckOut a guest";
            ViewBag.Message2 = "Enter the room number and check out date to proceed:";
            return View(new CheckOut());
        }

        [HttpPost]
        public ActionResult CheckOut(CheckOut obj)
        {
            obj.CheckOutFromRoom();
            if (obj.message != null)
            {
                ViewBag.Message = obj.message;
                return View(obj);
            }

            else
            {
                ViewBag.Message = "CheckOut successfully done";
                return RedirectToAction("ReceiptPage", obj);
            }
        }

        public ActionResult ReceiptPage(CheckOut coObj)
        {
            coObj.GenerateReceipt();
            return View(coObj);
        }
    }
}