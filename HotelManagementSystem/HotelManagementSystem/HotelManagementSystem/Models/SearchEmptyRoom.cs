using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace HotelManagementSystem.Models
{
    public class SearchEmptyRoom
    {
        static readonly string connectionString = "Server=db-instances.cyiy68i5rl4p.us-west-2.rds.amazonaws.com;Database=DBMS_HMS;User Id=harshil_03; password=Vasani99";
        public List<int> resultData = new List<int>();
        public string message = null;
        public int totalRoomCount { get; set; }
        public int checkedInRoomCount { get; set; }

        public SearchEmptyRoom()
        {

        }

        public SearchEmptyRoom(string confirmationCode, int count)
        {
            checkedInRoomCount = new BookingDetails().GetRoomCountCheckedIn(confirmationCode);
            totalRoomCount = count;
        }

        public void GetEmptyRooms(string confirmationCode)
        {
            if (confirmationCode == null)
                return;

            string queryString = @"SELECT r.roomNumber as 'Available Room'
	                                    FROM DBMS_HMS.[dbproject].[Room] r, DBMS_HMS.[dbproject].[OccupancyDates] od, DBMS_HMS.[dbproject].[Booking] b
	                                    WHERE b.confirmationCode = @confirmationCode
	                                    AND r.roomType = b.roomType
	                                    AND r.isOccupied = 0
	                                    AND r.id = od.roomId
	                                    AND od.inDate <= b.checkInDate
	                                    AND od.outDate <= b.checkOutDate
									UNION
									SELECT r.roomNumber as 'Available Room'
	                                    FROM DBMS_HMS.[dbproject].[Room] r, DBMS_HMS.[dbproject].[Booking] b
									    WHERE b.confirmationCode = @confirmationCode
	                                    AND r.isOccupied = 0
									    AND r.roomType = b.roomType";

            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@confirmationCode", confirmationCode);

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
                            resultData.Add(myReader.GetInt32(0));
                        }
                    }
                }
            }
        }

        public void CheckInRoom(string confirmationCode, int roomNumber = 0)
        {
            if (confirmationCode == null && roomNumber == 0)
                return;

            string queryString = @"BEGIN TRANSACTION
                                    UPDATE DBMS_HMS.[dbproject].[OccupancyDates] 
                                            SET roomId = (SELECT r.id FROM DBMS_HMS.dbproject.Room r WHERE r.roomNumber = @roomNumber)
                                            WHERE id in(
                                          SELECT TOP(1) od.id FROM  [DBMS_HMS].[dbproject].[OccupancyDates] od, [DBMS_HMS].[dbproject].[Room] r1, [DBMS_HMS].[dbproject].[Room] r2, [DBMS_HMS].dbproject.Booking b
                                            WHERE b.confirmationCode = @confirmationCode
											AND b.checkInDate = od.inDate
											AND b.checkOutDate = od.outDate
											AND r1.roomNumber = @roomNumber /*room number from UI*/
                                            AND r2.id = od.roomId  
                                            AND r1.roomType = r2.roomType
                                            AND r2.isOccupied = 0
											AND not exists (select * from  [DBMS_HMS].[dbproject].[OccupancyDates] od1
											Where r1.id = od1.roomId))                                    

                                    INSERT INTO DBMS_HMS.[dbproject].CheckIn(roomId,checkInBy,checkInDate)
                                          SELECT r.id, b.bookingOf, b.checkInDate
                                          FROM DBMS_HMS.[dbproject].[Booking] b JOIN DBMS_HMS.[dbproject].[OccupancyDates] od ON ( od.inDate = b.checkInDate)
                                          JOIN DBMS_HMS.[dbproject].[Room] r ON (od.roomId = r.id)
                                          WHERE b.confirmationCode = @confirmationCode
                                          AND r.roomNumber = @roomNumber;

                                    UPDATE DBMS_HMS.[dbproject].Room
                                          SET isOccupied = 1
                                          WHERE roomNumber = @roomNumber;
                                    COMMIT;";

            SqlParameter[] param = new SqlParameter[2];
            param[0] = new SqlParameter("@confirmationCode", confirmationCode);
            param[1] = new SqlParameter("@roomNumber", roomNumber);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Connection.Open();
                    if (param != null)
                        command.Parameters.AddRange(param);


                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        message = "Something went wrong while CheckIn";
                    }
                }
            }
        }
    }
}