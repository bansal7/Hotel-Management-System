using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // GET: Search
        public ActionResult SearchRoom()
        {
            return View(new SearchViewModels());
        }

        [HttpPost]
        public ActionResult SearchRoom(DateTime checkIn, DateTime checkOut, string roomType, int roomCount)
        {
            SearchViewModels obj = new SearchViewModels();
            obj.GetSearchResult(checkIn, checkOut, roomType, roomCount);
            return View(obj);
        }

        
      
    }
}