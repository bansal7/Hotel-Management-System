using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelManagementSystem.Models
{
    public class RoomExpense
    {
        public int roomNumber { get; set; }
        public DateTime expenseDT { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        static readonly string connectionString = "Server=db-instances.cyiy68i5rl4p.us-west-2.rds.amazonaws.com;Database=DBMS_HMS;User Id=harshil_03; password=Vasani99";
        public IEnumerable<SelectListItem> occupiedRoom { get; set; }

        public RoomExpense()
        {
            expenseDT = DateTime.Now.AddHours(0);
            
            GetOccupiedRoom();
        }

        public RoomExpense(int roomNumber, string description, double price, DateTime? expenseDT)
        {
            this.roomNumber = roomNumber;
            this.description = description;
            this.price = price;
            if (expenseDT == null)
                this.expenseDT = DateTime.Now.AddHours(0);
            else
                this.expenseDT = expenseDT.Value;

            GetOccupiedRoom();
        }

        public int AddExpense()
        {
            string queryString = @"INSERT INTO DBMS_HMS.[dbproject].[Expense] ([description], [charge], [dateOfExpense] ,[expenseOf])
                                      (SELECT @description, @price, @expenseDT, id 
                                      FROM DBMS_HMS.[dbproject].[ROOM]
                                      WHERE roomNumber = @roomNumber)";

            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@description", description);
            param[1] = new SqlParameter("@price", price);
            param[2] = new SqlParameter("@expenseDT", expenseDT);
            param[3] = new SqlParameter("@roomNumber", roomNumber);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Connection.Open();
                    if (param != null)
                        command.Parameters.AddRange(param);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public void GetOccupiedRoom()
        {
            List<int> roomNums = new List<int>();

            string queryString = @"SELECT [roomNumber]      
                                      FROM DBMS_HMS.[dbproject].[Room]
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
}