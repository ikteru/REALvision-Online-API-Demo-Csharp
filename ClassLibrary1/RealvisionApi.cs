using RealvisionLib;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace 
    Realvision
{
    public class RealvisionApi
    {

        public static HttpClient RealvisionClient { get; set; }

        public String ApiKey { get; set; }
        public String ApiLink { get; set; }
        public String FileToSlice { get; set; }     //Filename with extension
        public String FileFolder { get; set; }      //The folder where the file is stored, if this class isn't given a filefolder it will automatically use the Assets folder which should be supplied
        public String DownloadsFolder { get; set; } //The folder where the downloaded files will be stored 
        public String AssetsFolder { get; set; }    //The folder where the files to slice are stored.

        private String ServerResponse { get; set; }
        private String saveFileTo { get; set; }


        // ************************************************************************************* //
        // ******************************** API FUNCTIONS ************************************** //
        // ************************************************************************************* //


        public String getActivationStatus()
        {
            FormData formData = new FormData();
            return makeRequest("POST", "GetActivationStatus", formData);
        }
        public String ProvideFile()
        {
            
            FormData formData = new FormData("n","IdeaWerk-Speed","Recommended" );

            return makeRequest("POST", "ProvideFile", formData);
        }
        public String GetProgress(String uniqueID)
        {
            FormData formData = new FormData(uniqueID);
            return makeRequest("POST", "GetProgress", formData);
        }
        public String GetPrintingInformation(String uniqueID)
        {
            FormData formData = new FormData(uniqueID);
            return makeRequest("POST", "GetPrintingInformation", formData);
        }
        public void Downloadfile(String uniqueID)
        {
            FormData formData = new FormData(uniqueID);
            String progress = "0";
            while ( GetProgress(uniqueID) != "1" && !string.IsNullOrEmpty(GetProgress(uniqueID)) )
            {
                progress = GetProgress(uniqueID);
            }
            
             makeRequest("POST", "DownloadFile", formData);
            
        }

        // ************************************************************************************* //
        // ************************************************************************************* //

        private String getResult(HttpWebResponse response)
        {
            System.IO.Stream responseStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream);

            return responseReader.ReadToEnd();
        }

        private void logResponse(HttpWebResponse response, String serviceCall)
        {



            Console.WriteLine();
            Console.WriteLine("*************************************************************************");
            Console.WriteLine("METHOD       :" + response.Method);
            Console.WriteLine("BASEURL      :" + this.ApiLink);
            Console.WriteLine("SERVICECALL  :" + serviceCall);
            Console.WriteLine("REQUEST_STATUS_CODE  :" + response.StatusCode);


            if ( serviceCall == "DownloadFile")
            {
                Console.WriteLine("RESPONSE :" + " Please check the following folder: " + this.saveFileTo + " for the downloaded FCode file.");
            }
            else
            {
                Console.WriteLine("RESPONSE :" + this.ServerResponse);
            }

            Console.WriteLine("*************************************************************************");
            Console.WriteLine();
        }

        private void SaveFile(String response, String fileName, String fileExtention)
        {
            DateTime foo = DateTime.UtcNow;
            long utc = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            String timeStamp = utc.ToString();
            String fileFolderLink = (string.IsNullOrEmpty(this.DownloadsFolder) ? (string.IsNullOrEmpty(this.AssetsFolder) ? this.FileFolder : this.AssetsFolder) : this.DownloadsFolder);
            this.saveFileTo = fileFolderLink + fileName + "." + timeStamp + fileExtention;

            Console.WriteLine(" FILE LINK ::: " + this.saveFileTo);
            try
            {
                File.WriteAllText(this.saveFileTo, response);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR WHILE SAVING FILE TO FILESYSTEM", ex);
            }

        }

        //This function is used by all the API Functions to call the API 
        public String makeRequest(String method, String serviceCall, FormData formData)
        {

            String fileUrl = ( string.IsNullOrEmpty(this.FileFolder) ? this.AssetsFolder : this.FileFolder  ) + this.FileToSlice;
            String filename = Path.GetFileNameWithoutExtension(this.FileToSlice);
            String fileExtension = Path.GetExtension(this.FileToSlice);

            //Slicing configs
            String supportType = formData.SupportType;
            String printerModel = formData.PrinterModel;
            String configPresetName = formData.ConfigPresetName;
            //Used if a custom configurations file is provided which is rare because we usually use the presets and leave this one empty.
            String configFile = Path.GetFileName(formData.ConfigFile);
            //The uniqueID of the slicing process started by ProvideFile
            String uniqueID = formData.UniqueID;


            if (fileExtension != ".rvwj")
            {
                Console.WriteLine(" ---------------- Wrong File Extension --------------- ");
            }

            string boundaryString = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + boundaryString;

            // Create an http request to the API endpoint 


            //******************************************************************************************//
            //****************************** HTTP REQUEST HEADERS - START ******************************//
            //******************************************************************************************//

            HttpWebRequest requestToServerEndpoint = (HttpWebRequest)WebRequest.Create(this.ApiLink + "/" + serviceCall);

            requestToServerEndpoint.Method = WebRequestMethods.Http.Post;
            requestToServerEndpoint.ContentType = contentType;
            requestToServerEndpoint.KeepAlive = true;
            requestToServerEndpoint.Credentials = CredentialCache.DefaultCredentials;
            requestToServerEndpoint.Headers["Ocp-Apim-Subscription-Key"] = this.ApiKey;

            //******************************************************************************************//
            //****************************** HTTP REQUEST HEADERS - END ********************************//
            //******************************************************************************************//

            // Use a MemoryStream to form the post data request,
            // so that we can get the content-length attribute.
            MemoryStream postDataStream = new MemoryStream();
            StreamWriter postDataWriter = new StreamWriter(postDataStream);

            //******************************************************************************************//
            //****************************** HTTP REQUEST BODY - START *********************************//
            //******************************************************************************************//

            postDataWriter.Write("\r\n--" + boundaryString + "\r\n");


            if (serviceCall == "ProvideFile")
            {
                postDataWriter.Write("Content-Disposition: form-data; name=\"" + "file" + "\"; filename=\"" + fileUrl + "\"\r\nContent-Type: false\r\n\r\n\r\n");
                postDataWriter.Flush();

                // Read the file
                FileStream fileStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    postDataStream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();


                postDataWriter.Write("Content-Disposition: form-data; name=\"supportType\"\r\n\r\n" + formData.SupportType + "\r\n");
                postDataWriter.Write("Content-Disposition: form-data; name=\"printerModel\"\r\n\r\n" + formData.PrinterModel + "\r\n");
                postDataWriter.Write("Content-Disposition: form-data; name=\"configPresetName\"\r\n\r\n" + formData.ConfigPresetName + "\r\n");
                postDataWriter.Write("Content-Disposition: form-data; name=\"configFile\"; filename=\"" + Path.GetFileName(formData.ConfigFile) + "\"\r\nContent-Type: false\r\n\r\n");

            }
            else
            {
                postDataWriter.Write("Content-Disposition: form-data; name=\"uniqueID\"\r\n\r\n" + formData.UniqueID);
            }

            postDataWriter.Write("\r\n--" + boundaryString + "--\r\n");
            postDataWriter.Flush();


            //******************************************************************************************//
            //****************************** HTTP REQUEST BODY - END ***********************************//
            //******************************************************************************************//

            // Set the http request body content length
            requestToServerEndpoint.ContentLength = postDataStream.Length;

            // Dump the post data from the memory stream to the request stream
            using (Stream s = requestToServerEndpoint.GetRequestStream())
            {
                postDataStream.WriteTo(s);
            }
            postDataStream.Close();

            try
            {
                HttpWebResponse response = (HttpWebResponse)requestToServerEndpoint.GetResponse();
                if (HttpStatusCode.OK == response.StatusCode)
                {
                    this.ServerResponse = this.getResult(response);

                    if (serviceCall == "DownloadFile")
                    {
                        SaveFile(this.ServerResponse, filename, fileExtension);
                    }

                    this.logResponse(response, serviceCall);
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    {
                        String errorServerReply = new StreamReader(data).ReadToEnd();
                        throw new Exception("ERROR CALLING THE SERVER: "+ errorServerReply);
                    }
                    
                }
            }

            return this.ServerResponse;

        }
    }
}
