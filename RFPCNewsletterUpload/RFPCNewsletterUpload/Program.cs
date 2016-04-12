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
            String str = @"server=184.168.154.14;database=rei1330509541036;userid=rei1330509541036;password=Adminrfpc01!;";
            MySqlConnection con = null;

            string[] pdfFiles = GetFileNames("C:\\Users\\Joe\\Desktop\\RFPCNewsletters", "*.pdf");
            foreach(var newsletterPdf in pdfFiles)
            {
                //set properties
                var post_title = newsletterPdf.ToString();
                var post_name = newsletterPdf.ToString();

                UploadFile("ftp://reinbeckfirstpres.com/", newsletterPdf, "n79c4827", "Adminrfpc01!", "wp-content/uploads/Console/");

                // Get the object used to communicate with the server.
                //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://reinbeckfirstpres.com/wp-content/uploads/Console/" + newsletterPdf);
                //request.Method = WebRequestMethods.Ftp.UploadFile;

                //// This example assumes the FTP site uses anonymous logon.
                //request.Credentials = new NetworkCredential("n79c4827", "Adminrfpc01!");

                //// Copy the contents of the file to the request stream.
                //StreamReader sourceStream = new StreamReader(newsletterPdf);
                //byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                //sourceStream.Close();
                //request.ContentLength = fileContents.Length;

                //Stream requestStream = request.GetRequestStream();
                //requestStream.Write(fileContents, 0, fileContents.Length);
                //requestStream.Close();

                //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                ////Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                //String uriString = request.ToString();

                //// Create a new WebClient instance.
                //WebClient myWebClient = new WebClient();
                //myWebClient.Credentials = new NetworkCredential("n79c4827", "Adminrfpc01");
                ////Console.WriteLine("\nPlease enter the fully qualified path of the file to be uploaded to the URI");
                //string fileName = uriString;
                //Console.WriteLine("Uploading {0} to {1} ...", fileName, uriString);

                //// Upload the file to the URI.
                //// The 'UploadFile(uriString,fileName)' method implicitly uses HTTP POST method.
                //byte[] responseArray = myWebClient.UploadFile(uriString, fileName);

                //response.Close();


                try
                {
                    con = new MySqlConnection(str);
                    con.Open();

                    String cmdText = "INSERT INTO wp_posts(post_author, post_date, post_date_gmt, post_title, post_status, comment_status, ping_status, post_name, post_modified, post_modified_gmt, guid, menu_order, post_type, post_mime_type, comment_count) VALUES(@post_author, @post_date, @post_date_gmt, @post_title, @post_status, @comment_status, @ping_status, @post_name, @post_modified, @post_modified_gmt, @guid, @menu_order, @post_type, @post_mime_type, @comment_count)";
                    MySqlCommand cmd = new MySqlCommand(cmdText, con);
                    cmd.Prepare();
                    //we will bound a value to the placeholder
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

        public static string UploadFile(string FtpUrl, string fileName, string userName, string password,string UploadDirectory="")
        {
            string PureFileName = new FileInfo(fileName).Name;
            String uploadUrl = String.Format("{0}{1}{2}", FtpUrl,UploadDirectory,PureFileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
            req.Proxy = null;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.Credentials = new NetworkCredential(userName,password);
            req.UseBinary = true;
            req.UsePassive = true;
            byte[] data = File.ReadAllBytes(fileName);
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            FtpWebResponse res = (FtpWebResponse)req.GetResponse();
            return res.StatusDescription;
        }

    }
}
