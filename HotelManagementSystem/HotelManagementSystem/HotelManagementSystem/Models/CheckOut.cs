using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace HotelManagementSystem.Models
{
    public class CheckOut
    {
        public int roomNumber { get; set; }
        public IEnumerable<SelectListItem> occupiedRoom { get; set; }
        public DateTime? checkOutDate { get; set; }
        static readonly string connectionString = "Server=db-instances.cyiy68i5rl4p.us-west-2.rds.amazonaws.com;Database=DBMS_HMS;User Id=harshil_03; password=Vasani99";
        public List<ExpenseSet> resultData = new List<ExpenseSet>();

        public string message = null;

        public CheckOut()
        {
            checkOutDate = null;
            GetOccupiedRoom();
        }

        public void CheckOutFromRoom()
        {
            if (roomNumber == 0 && checkOutDate == null)
                return;

            string queryString = @"BEGIN TRANSACTION
                                   INSERT INTO DBMS_HMS.[dbproject].[CheckOut] ([checkOutDate],[roomId],[checkOutBy],[totalCharge])
                                    SELECT @checkOutDate ,r.id, ci.checkInBy, SUM(NULLIF(e.charge, 0))
                                        FROM DBMS_HMS.[dbproject].[Room] r JOIN DBMS_HMS.[dbproject].CheckIn ci ON(r.id = ci.roomId)
																  JOIN DBMS_HMS.[dbproject].[OccupancyDates] od ON (od.inDate = ci.checkInDate AND od.roomId = r.id)
																  LEFT JOIN DBMS_HMS.[dbproject].[Expense] e ON (e.expenseOf = r.id AND od.inDate <= e.dateOfExpense AND @checkOutDate >= e.dateOfExpense)										
                                        WHERE r.roomNumber = @roomNumber                                     
										AND od.outDate >= @checkOutDate
                                        GROUP BY r.id, od.inDate, ci.checkInBy;
                                   UPDATE DBMS_HMS.dbproject.Room
                                        SET isOccupied = 0
                                        WHERE roomNumber = @roomNumber;
                                   COMMIT;";

            SqlParameter[] param = new SqlParameter[2];
            param[0] = new SqlParameter("@roomNumber", roomNumber);
            param[1] = new SqlParameter("@checkOutDate", checkOutDate);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Connection.Open();
                    if (param != null)
                        command.Parameters.AddRange(param);
                    int rowsAffected = 0;
                    try
                    {
                        rowsAffected = command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        rowsAffected = 0;
                    }

                    if (rowsAffected == 0)
                    {
                        message = "Something went wrong while Check Out";
                    }
                }
            }
        }

        public void GenerateReceipt()
        {
            if (roomNumber == 0 && checkOutDate == null)
                return;

            string queryString = @"SELECT e.description, e.dateOfExpense, e.charge
                                   FROM DBMS_HMS.dbproject.Expense e
                                   WHERE expenseOf = (SELECT id FROM DBMS_HMS.[dbproject].Room WHERE roomNumber = @roomNumber)";

            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@roomNumber", roomNumber);

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
                            resultData.Add(new ExpenseSet
                            {
                                description = myReader.GetString(0),
                                dateOfExpense = myReader.GetDateTime(1),
                                charge = myReader.GetDouble(2)
                            });
                        }
                    }
                }
            }
        }


        public void GetOccupiedRoom()
        {
            List<int> roomNums = new List<int>();

            string queryString = @"SELECT [roomNumber]      
                                      FROM [DBMS_HMS].[dbproject].[Room]
                                      WHERE isOccupied = 1";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Connection.Open();

                    using (SqlDataReader myReader = command.ExecuteReader())
                    {

                        while (myReader.Read())
                        {
                            roomNums.Add(myReader.GetInt32(0));
                        }
                        occupiedRoom = new SelectList(roomNums);
                    }
                }
            }
        }
    }

    public class ExpenseSet
    {
        public string description { get; set; }
        public DateTime dateOfExpense { get; set; }
        public double charge { get; set; }
    }

    public static class DateTimeUtil
    {
        public static bool IsEmpty(this DateTime dateTime)
        {
            return dateTime == default(DateTime);
        }
    }
}