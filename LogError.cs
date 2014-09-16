using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Net.Mail;

namespace WebFolderSync
{
    class LogError
    {

        //public static void Log(string LogDescription)//GENERIC LogError
        //{
        //    SqlConnection conn = new SqlConnection(Program.InternetConnectionString());

        //    LogDescription = LogDescription.Replace("'", "\"");//so that it doesn't cause anotother exception
            
        //    string sql = "Insert INTO UsersLog (UserId,IpAddress,LogType,LogDate,LogDescription,RelatedID) VALUES (" +
        //        0 + //UserId int
        //        "," + "'" + "127.0.0.1" + "'" + //IpAddress string(25)
        //        "," + 6 + //LogType smallInt
        //        "," + "'" + DateTime.Now.AddMinutes(1) + "'" + //LogDate datetime ( add a minute so that the it is later than the updateTimeStamp )
        //        "," + "'" + "\"" + LogDescription + "\"" + "'" + //LogDescription string
        //        "," + 0 + ")"; //RelatedID int

        //    SqlCommand cmd = new SqlCommand(sql, conn);

        //    try
        //    {
        //        conn.Open();
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (Exception e) { SendEmail(" EmailAlertN LogError - Log():\n" + e.Message + "\n\n" + "SQL:\n" + sql); }
        //    conn.Close();
        //    conn.Dispose();
        //}
        
        public static void Log(string body)//simple email
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress("address");
            message.To.Add("mailAddressItem");
            message.Subject = "WebFolderSync - error  ";
            message.Body = body;
            SmtpClient smtp = new SmtpClient("host");
            smtp.Credentials = new System.Net.NetworkCredential("username", "password");
            smtp.EnableSsl = true;
            smtp.Send(message);
        }


    }
}
