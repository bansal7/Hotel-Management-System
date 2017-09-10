using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelManagementSystem.Models
{
    public class RoomDetail
    {
        public DateTime checkInDate { get; set; }
        public DateTime checkOutDate { get; set; }
        public string roomType { get; set; }
        public int roomCount { get; set; }
        public double roomPrice { get; set; }

        public RoomDetail()
        {

        }
        public RoomDetail(DateTime checkInDate, DateTime checkOutDate, string roomType, int roomCount, double roomPrice)
        {
            this.checkInDate = checkInDate;
            this.checkOutDate = checkOutDate;
            this.roomType = roomType;
            this.roomCount = roomCount;
            this.roomPrice = roomPrice;
        }

    }

    public class CreditCardDetail
    {
        public string cardHolderName { get; set; }
        public string bankName { get; set; }
        public string cardNumber { get; set; }
        public string expiryDate { get; set; }
        public string code { get; set; }

        public CreditCardDetail()
        {

        }

        public CreditCardDetail(string cardHolderName, string bankName, string cardNumber, string expiryDate, string code)
        {
            this.cardHolderName = cardHolderName;
            this.bankName = bankName;
            this.cardNumber = cardNumber;
            this.expiryDate = expiryDate;
            this.code = code;
        }
    }

    public class GuestDetail
    {
        public string fullName { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pinCode { get; set; }
        public string idType { get; set; }
        public string idNumber { get; set; }
        public string emailAddr { get; set; }

        public GuestDetail()
        {

        }

        public GuestDetail(string fullName, string address, string city, string state, string pinCode, string idType, string idNumber, string emailAddr)
        {
            this.fullName = fullName;
            this.address = address;
            this.city = city;
            this.state = state;
            this.pinCode = pinCode;
            this.idType = idType;
            this.idNumber = idNumber;
            this.emailAddr = emailAddr;
        }
    }

    public class BookingDetail
    {
        public string confirmationCode { get; set; }
        public DateTime checkInDate { get; set; }
        public DateTime checkOutDate { get; set; }
        public string roomType { get; set; }
        public int roomCount { get; set; }
        public double totalAmountPaid { get; set; }
        public string guestName { get; set; }
        public string emailAddr { get; set; }

        public BookingDetail()
        {

        }

        public BookingDetail(string confirmationCode, DateTime checkInDate, DateTime checkOutDate, string roomType, int roomCount, double totalAmountPaid, string guestName, string emailAddr)
        {
            this.confirmationCode = confirmationCode;
            this.checkInDate = checkInDate;
            this.checkOutDate = checkOutDate;
            this.roomType = roomType;
            this.roomCount = roomCount;
            this.totalAmountPaid = totalAmountPaid;
            this.guestName = guestName;
            this.emailAddr = emailAddr;
        }
    }

    public class BookViewModel
    {
        static readonly string connectionString = "Server=db-instances.cyiy68i5rl4p.us-west-2.rds.amazonaws.com;Database=DBMS_HMS;User Id=harshil_03; password=Vasani99";

        public RoomDetail roomDetail { get; set; }
        public CreditCardDetail creditCardDetail { get; set; }
        public GuestDetail guestDetail { get; set; }

        public IEnumerable<SelectListItem> bankNames
        {
            get { return new SelectList(new List<String>() { "BOFA", "AMEX", "Discover", "Santander" }); }
        }
        public IEnumerable<SelectListItem> idTypes
        {
            get { return new SelectList(new List<String>() { "SSN", "PassPort", "State Id", "Driving Licence" }); }
        }

        public BookViewModel()
        {

        }
        public BookViewModel(RoomDetail roomDetail)
        {
            this.roomDetail = roomDetail;
            creditCardDetail = new CreditCardDetail();
            guestDetail = new GuestDetail();
        }

        public BookingDetail AddBookingDetails()
        {
            string uniqueCode = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);

            if (AddCreditCard(creditCardDetail) == 1)
            {
                if (AddGuest(guestDetail, creditCardDetail.cardNumber) == 1)
                {
                    double amtPaid = roomDetail.roomCount * roomDetail.roomPrice;
                    BookingDetail bdObj = new BookingDetail(uniqueCode, roomDetail.checkInDate, roomDetail.checkOutDate, roomDetail.roomType, roomDetail.roomCount, amtPaid, guestDetail.fullName, guestDetail.emailAddr);

                    if (AddBooking(bdObj, guestDetail.idNumber) == 1)
                    {
                        AddOccupancyDates(bdObj);
                        return bdObj;
                    }
                }
            }

            return new BookingDetail();
        }

        private int AddCreditCard(CreditCardDetail crdObj)
        {
            if (crdObj == null)
                return 0;

            string queryString = @"IF (NOT EXISTS(SELECT id FROM DBMS_HMS.[dbproject].[CreditCard] WHERE [cardNumber] = @cardNumber)) 
	                                    BEGIN 
	                                       INSERT INTO DBMS_HMS.[dbproject].[CreditCard]([cardHolderName], [cardNumber], [expiryDate], [code], [bankName])
	                                        VALUES(@cardHolderName, @cardNumber, @expiryDate, @code, @bankName) 
                                    END 
                                    ELSE 
	                                    BEGIN 
	                                       UPDATE DBMS_HMS.[dbproject].[CreditCard] 
			                                    SET [cardHolderName] = @cardHolderName,
				                                    [expiryDate] = @expiryDate,
				                                    [code] = @code,
				                                    [bankName] = @bankName
	                                       WHERE [cardNumber] = @cardNumber
                                    END ";

            SqlParameter[] param = new SqlParameter[5];
            param[0] = new SqlParameter("@cardHolderName", crdObj.cardHolderName);
            param[1] = new SqlParameter("@cardNumber", crdObj.cardNumber);
            param[2] = new SqlParameter("@expiryDate", crdObj.expiryDate);
            param[3] = new SqlParameter("@code", crdObj.code);
            param[4] = new SqlParameter("@bankName", crdObj.bankName);

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

        private int AddGuest(GuestDetail gdObj, string ccNum)
        {
            if (gdObj == null || string.IsNullOrEmpty(ccNum))
                return 0;

            string queryString = @"IF (NOT EXISTS(SELECT id FROM DBMS_HMS.[dbproject].[Guest] WHERE validIdNumber = @idNumber AND validIdType = @idType)) 
	                                    BEGIN 
		                                    INSERT INTO DBMS_HMS.[dbproject].[Guest](name, address, validIdType, validIdNumber, gmailAddress, creditCardId)
		                                    VALUES(@fullName, @fullAddress, @idType, @idNumber, @gmailAddress, (SELECT id FROM DBMS_HMS.[dbproject].[CreditCard] WHERE [cardNumber] = @ccNum)) 
	                                    END 
                                    ELSE 
	                                    BEGIN 
	                                      UPDATE DBMS_HMS.[dbproject].[Guest] 
			                                    SET [name] = @fullName,
				                                    [address] = @fullAddress,
				                                    [creditCardId] = (SELECT id FROM DBMS_HMS.[dbproject].[CreditCard] WHERE [cardNumber] = @ccNum)
			                                    WHERE validIdNumber = @idNumber
			                                    AND validIdType = @idType
                                    END ";

            SqlParameter[] param = new SqlParameter[6];
            param[0] = new SqlParameter("@idNumber", gdObj.idNumber);
            param[1] = new SqlParameter("@idType", gdObj.idType);
            param[2] = new SqlParameter("@fullName", gdObj.fullName);
            param[3] = new SqlParameter("@fullAddress", gdObj.address + ", " + gdObj.city + ", " + gdObj.state + ", " + gdObj.pinCode);
            param[4] = new SqlParameter("@ccNum", ccNum);
            param[5] = new SqlParameter("@gmailAddress", gdObj.emailAddr);

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

        private int AddBooking(BookingDetail bdObj, string guestIdNum)
        {
            if (bdObj == null || string.IsNullOrEmpty(guestIdNum))
                return 0;

            string queryString = @"INSERT INTO DBMS_HMS.[dbproject].[Booking] ([checkInDate]
                                          ,[checkOutDate]
                                          ,[roomType]
                                          ,[numOfRoom]
                                          ,[confirmationCode]
                                          ,[bookingOf])
	                                      VALUES(@checkInDate, @checkOutDate, @roomType, @numOfRoom, @confirmationCode, 
	                                      (SELECT id FROM DBMS_HMS.[dbproject].[Guest]  WHERE validIdNumber=@guestIdNum))";

            SqlParameter[] param = new SqlParameter[6];
            param[0] = new SqlParameter("@checkInDate", bdObj.checkInDate);
            param[1] = new SqlParameter("@checkOutDate", bdObj.checkOutDate);
            param[2] = new SqlParameter("@roomType", bdObj.roomType);
            param[3] = new SqlParameter("@numOfRoom", bdObj.roomCount);
            param[4] = new SqlParameter("@confirmationCode", bdObj.confirmationCode);
            param[5] = new SqlParameter("@guestIdNum", guestIdNum);

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

        private int AddOccupancyDates(BookingDetail bdObj)
        {
            if (bdObj == null)
                return 0;

            string queryString = @"INSERT INTO DBMS_HMS.[dbproject].[OccupancyDates] ([inDate],[outDate],[roomId])
                                    (SELECT DISTINCT TOP (@roomCount)  @checkInDate, @checkOutDate, r.id
									FROM DBMS_HMS.[dbproject].Room r LEFT JOIN DBMS_HMS.[dbproject].OccupancyDates od ON (r.id = od.roomId)
									WHERE((od.inDate > @checkOutDate
										  OR od.outDate < @checkInDate)
                                        OR od.roomID IS NULL)
										AND r.isOccupied = 0
										AND r.roomType = @roomType)";

            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@checkInDate", bdObj.checkInDate);
            param[1] = new SqlParameter("@checkOutDate", bdObj.checkOutDate);
            param[2] = new SqlParameter("@roomType", bdObj.roomType);
            param[3] = new SqlParameter("@roomCount", bdObj.roomCount);
            

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
    }
}