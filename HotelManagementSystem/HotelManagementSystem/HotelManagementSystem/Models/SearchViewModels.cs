using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace HotelManagementSystem.Models
{
    public class SearchResult
    {
        public string roomType { get; set; }
        public int count { get; set; }
        public double pricePerRoom { get; set; }
    }

    public class BookingDates
    {
        public DateTime checkInDate { get; set; }
        public DateTime checkOutDate { get; set; }
        
    }

    public class RoomBookingDetails
    {
        public string roomType { get; set; }
        public int count { get; set; }
    }

    public class SearchViewModels
    {
        static readonly string connectionString = "Server=db-instances.cyiy68i5rl4p.us-west-2.rds.amazonaws.com;Database=DBMS_HMS;User Id=harshil_03; password=Vasani99";
        public List<SearchResult> searchResult { get; set; }
        public BookingDates bookingDates { get; set; }
        public RoomBookingDetails roomBookingDetails { get; set; }
       

        public void GetSearchResult(DateTime checkInDate, DateTime checkOutDate, string roomType, int roomCount)
        {
            
            if (checkInDate == null || checkOutDate == null)
                return;

            searchResult = new List<SearchResult>();
            bookingDates = new BookingDates();
            roomBookingDetails = new RoomBookingDetails();

            bookingDates.checkInDate = checkInDate.Add(new TimeSpan(14, 0, 0));
            bookingDates.checkOutDate = checkOutDate.Add(new TimeSpan(11, 0, 0));

            roomBookingDetails.roomType = roomType;
            roomBookingDetails.count = roomCount;

            roomType += "%";

            string queryString = @"SELECT r.roomType, COUNT(DISTINCT r.roomNumber), r.price
	                                FROM DBMS_HMS.[dbproject].Room r LEFT JOIN DBMS_HMS.[dbproject].OccupancyDates od ON (r.id = od.roomId)
	                                WHERE ((od.inDate > @checkOutDate
		                                  OR od.outDate < @checkInDate)
		                                OR od.roomId IS NULL )                                       
                                        AND r.isOccupied = 0
		                                AND r.roomType like @roomType   
	                                GROUP BY r.roomType, r.price
	                                HAVING COUNT(*) >= @roomCount;";

            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@checkInDate", checkInDate);
            param[1] = new SqlParameter("@checkOutDate", checkOutDate);
            param[2] = new SqlParameter("@roomType", roomType);
            param[3] = new SqlParameter("@roomCount", roomCount);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Connection.Open();
                    if (param != null)
                        command.Parameters.AddRange(param);

                    using (SqlDataReader myReader = command.ExecuteReader())
                    {
                       
                        while (myReader.Read())
                        {
                            SearchResult sr = new SearchResult();

                            sr.roomType = myReader.GetString(0);
                            sr.count = myReader.GetInt32(1);
                            sr.pricePerRoom = myReader.GetDouble(2);
                            searchResult.Add(sr);
                        }
                    }
                }
            }
        }

    }
}