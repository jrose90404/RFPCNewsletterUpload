using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
using iTextSharp.text.pdf;
using System.Net;

namespace RFPCNewsletterUpload
{
    class Program
    {

        static void Main(string[] args)
        {
            //connection string credentials
            var str = @"server=IPAddress;database=DBName;userid=DBUserName;password=DBPassword;";
            
            MySqlConnection con = null;

            //look in this folder for pdf documents
            string[] pdfFiles = GetFileNames("C:\\Users\\Joe\\Desktop\\RFPCNewsletters", "*.pdf");
            foreach(var newsletterPdf in pdfFiles)
            {
                //set properties
                var post_title = newsletterPdf.ToString();
                var post_name = newsletterPdf.ToString();

                //upload the pdf
                UploadFile("ftp://reinbeckfirstpres.com/", newsletterPdf, "n79c4827", "Adminrfpc01!", "wp-content/uploads/Console/");

                //create record after successfull PDF upload
                try
                {
                    //create and open connection
                    con = new MySqlConnection(str);
                    con.Open();

                    //sql to insert
                    var cmdText = "INSERT INTO wp_posts(post_author, post_date, post_date_gmt, post_title, post_status, comment_status, ping_status, post_name, post_modified, post_modified_gmt, guid, menu_order, post_type, post_mime_type, comment_count) VALUES(@post_author, @post_date, @post_date_gmt, @post_title, @post_status, @comment_status, @ping_status, @post_name, @post_modified, @post_modified_gmt, @guid, @menu_order, @post_type, @post_mime_type, @comment_count)";

                    //bind values to sql variables/placeholders
                    var cmd = new MySqlCommand(cmdText, con);
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@post_author", 1);
                    cmd.Parameters.AddWithValue("@post_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@post_date_gmt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@post_title", post_title);
                    cmd.Parameters.AddWithValue("@post_status", "inherit");
                    cmd.Parameters.AddWithValue("@comment_status", "closed");
                    cmd.Parameters.AddWithValue("@ping_status", "closed");
                    cmd.Parameters.AddWithValue("@post_name", post_name);
                    cmd.Parameters.AddWithValue("@post_modified", DateTime.Now);
                    cmd.Parameters.AddWithValue("@post_modified_gmt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@guid", "http://reinbeckfirstpres.com/wp-content/uploads/Console/"+ newsletterPdf);
                    cmd.Parameters.AddWithValue("@menu_order", 0);
                    cmd.Parameters.AddWithValue("@post_type", "attachment");
                    cmd.Parameters.AddWithValue("@post_mime_type", "application/pdf");
                    cmd.Parameters.AddWithValue("@comment_count", 0);
                    cmd.ExecuteNonQuery(); //execute the mysql command
                }
                catch (MySqlException err)
                {
                    Console.WriteLine("Error: " + err.ToString());
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close(); //close the connection
                    }
                }
            }
            //remember to close the connection after accessing the database
        }

        private static string[] GetFileNames(string path, string filter)
        {
            string[] files = Directory.GetFiles(path, filter);
            for(int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }

        public static string UploadFile(string FtpUrl, string fileName, string userName, string password, string UploadDirectory = "")
        {
            var PureFileName = new FileInfo(fileName).Name;
            var uploadUrl = String.Format("{0}{1}{2}", FtpUrl,UploadDirectory,PureFileName);
            var req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.Credentials = new NetworkCredential(userName,password);
            req.UseBinary = true;
            req.UsePassive = true;
            byte[] data = File.ReadAllBytes(fileName);
            req.ContentLength = data.Length;
            var stream = req.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            var res = (FtpWebResponse)req.GetResponse();
            return res.StatusDescription;
        }

    }
}
