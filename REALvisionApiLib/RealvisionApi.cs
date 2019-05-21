using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REALvisionApiLib.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace
    REALvisionApiLib
{
    public class RealvisionApi
    {

        public static HttpClient RealvisionClient { get; set; }
        public String CurrentFolder { get; set; }

        public ApiSettings ApiSettings { get; set; } 
        public String Token { get; set; }
        public String TokenExpiresOn { get; set; }

        //The Slicing Configs
        public String FileToSlice { get; set; }     //Filename with extension
        public String FileFolder { get; set; }      //The folder where the file is stored, if this class isn't given a filefolder it will automatically use the Assets folder which should be supplied
        public String SupportType { get; set; }
        public String PrinterModel { get; set; }
        public String ConfigPresetName { get; set; }

        public String DownloadsFolder { get; set; } //The folder where the downloaded files will be stored 
        public String AssetsFolder { get; set; }    //The folder where the files to slice are stored.

        private String saveFileTo { get; set; }

        public RealvisionApi(String currentFolder)
        {
            this.CurrentFolder = currentFolder;
            this.ApiSettings = this.getApiSettings(currentFolder);
            this.getToken(currentFolder).Wait();
        }

        // ************************************************************************************* //
        // ******************************** API FUNCTIONS ************************************** //
        // ************************************************************************************* //

        public String getActivationStatus()
        {
            ApiRequest ApiRequest = new ApiRequest();
            return MakeRequest("POST", "GetActivationStatus", ApiRequest).Result;
        }
        public String ProvideFile()
        {
            String fileUrl = (string.IsNullOrEmpty(this.FileFolder) ? this.AssetsFolder : this.FileFolder) + this.FileToSlice;
            string jsonString = File.ReadAllText(fileUrl, Encoding.UTF8);
            WsConfigs wsConfigs = JsonConvert.DeserializeObject<WsConfigs>(jsonString);
            WsFile file = new WsFile("calical.rvwj", wsConfigs);
            ApiRequest ApiRequest = new NoConfigApiRequest(file, this.SupportType, this.PrinterModel, this.ConfigPresetName);

            return MakeRequest("POST", "ProvideFile", ApiRequest).Result;
        }
        public String GetProgress(String TaskId)
        {
            ApiRequest ApiRequest = new TaskApiRequest(TaskId);

            String tempProgress = MakeRequest("POST", "GetProgress", ApiRequest).Result;
            return tempProgress;
        }
        public String GetPrintingInformation(String TaskId)
        {
            ApiRequest ApiRequest = new TaskApiRequest(TaskId);
            return MakeRequest("POST", "GetPrintingInformation", ApiRequest).Result;
        }
        public void Downloadfile(String TaskId)
        {
            String progress = GetProgress(TaskId);

            while (progress != "1" && progress != "-1" && !string.IsNullOrEmpty(progress) && progress != "2" )
            {
                progress = GetProgress(TaskId);
                Console.WriteLine(progress);
            }
            if(progress == "1")
            {
                var result = MakeRequest("POST", "DownloadFile?taskid=" + TaskId, new ApiRequest(), true).Result;
            } else if ( progress == "-1")
            {
                Console.WriteLine("Slicing file failed ... ");
            }
            else if (string.IsNullOrEmpty(progress))
            {
                Console.WriteLine("Progress is Empty ... ");
            } else
            {
                Console.WriteLine("Encoutered error while downloading file ... ");
            }
        }

        // ************************************************************************************* //
        // ***************************** SUPPORT FUNCTIONS ************************************* //
        // ************************************************************************************* //
        public ApiSettings getApiSettings(String currentFolder)
        {
            JToken appSettings = JToken.Parse(File.ReadAllText(currentFolder + "appsettings.json"));
            return ApiSettings = JsonConvert.DeserializeObject<ApiSettings>(JsonConvert.SerializeObject(appSettings["ApiSettings"]));
        }

        public async Task<String> requestNewToken()
        {
            using (var client = new HttpClient())
            {

                MultipartFormDataContent multipart = new MultipartFormDataContent();

                StringContent grant_type = new StringContent("client_credentials");
                StringContent client_id = new StringContent(this.ApiSettings.ClientId);
                StringContent client_secret = new StringContent(this.ApiSettings.ClientSecret);
                StringContent resource = new StringContent("https://api.createitreal.com");

                multipart.Add(grant_type, "grant_type");
                multipart.Add(client_id, "client_id");
                multipart.Add(client_secret, "client_secret");
                multipart.Add(resource, "resource");

                HttpResponseMessage result = new HttpResponseMessage();

                try
                {
                        result = client.PostAsync(this.ApiSettings.AuthServerUrl, multipart).Result;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Error while fetching token ... ");
                    throw e;
                }

                Console.WriteLine("*************************************************************************");
                String finalresult = await result.Content.ReadAsStringAsync();

                    
                if (!string.IsNullOrEmpty(JsonConvert.DeserializeObject<ApiJwtResponse>(finalresult).access_token))
                {
                    Console.WriteLine("Token successfully fetched ... ");
                }
                else
                {
                    throw new Exception("Token doesn't have an access_token property ... ");
                }

                return finalresult;
            }

        }

        public async Task<String> getToken(String currentFolder)
        {
            String tokenFile = "";
            ApiJwtResponse jwt = new ApiJwtResponse();

            try
            {
                tokenFile = File.ReadAllText(currentFolder + "/token.json");
                jwt = JsonConvert.DeserializeObject<ApiJwtResponse>(tokenFile);

                Console.WriteLine("*************************************************************************");
                Console.WriteLine("Valid token file.");
                Console.WriteLine("*************************************************************************");
            }
            catch
            {
                Console.WriteLine("*************************************************************************");
                Console.WriteLine("No token file found. requesting new token ...");
                Console.WriteLine("*************************************************************************");

                File.WriteAllText(currentFolder + "/token.json", await requestNewToken());
            }

            if (!string.IsNullOrEmpty(jwt.access_token))
            {
                this.TokenExpiresOn = jwt.expires_on;
                DateTime foo = DateTime.UtcNow;
                long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

                bool newTokenNeeded = !(Double.Parse(this.TokenExpiresOn) - unixTime > 0);

                if (!newTokenNeeded)
                {
                    Console.WriteLine("*************************************************************************");
                    Console.WriteLine("Valid token file.");
                    Console.WriteLine("*************************************************************************");

                    this.Token = jwt.access_token;
                    this.TokenExpiresOn = jwt.expires_on;

                    return this.Token;
                } else
                {
                    Console.WriteLine("*************************************************************************");
                    Console.WriteLine("Available token no longer valid, requesting new token ... ");
                    Console.WriteLine("*************************************************************************");

                    File.WriteAllText(currentFolder + "/token.json", await requestNewToken());
                    this.Token = jwt.access_token;

                    return this.Token;
                }
            } else
            {
                Console.WriteLine("*************************************************************************");
                Console.WriteLine("Token file doesn't contain token, requesting new token ... ");
                Console.WriteLine("*************************************************************************");
                String newToken = await requestNewToken();
                File.WriteAllText(currentFolder + "/token.json", newToken);
                Console.WriteLine("TOKEN    ::::: " + newToken);
                return newToken;
            }
        }

        private String readHttpResponse(HttpWebResponse response)
        {
            System.IO.Stream responseStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream);

            return responseReader.ReadToEnd();
        }

        private async void logResponse(HttpResponseMessage response, String serviceCall , bool isDownload)
        {

            Console.WriteLine();
            Console.WriteLine("*************************************************************************");
            Console.WriteLine("SERVICECALL                  :::: " + serviceCall);
            
            Console.WriteLine();
            Console.WriteLine("METHOD                       :::: " + response.RequestMessage.Method);
            Console.WriteLine("REQUEST_STATUS_CODE          :::: " + response.StatusCode);

            if ( isDownload  && HttpStatusCode.OK == response.StatusCode )
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("RESPONSE                 :::: " + " Please check the following folder for the downloaded FCode file: ");
                Console.WriteLine(this.saveFileTo);
                Console.WriteLine("--------------------");

            }
            else
            {
                Console.WriteLine("RESPONSE                     :::: " + await response.Content.ReadAsStringAsync());
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
            this.saveFileTo = fileFolderLink + fileName + "." + timeStamp + ".fcode" ;

            try
            {
                File.WriteAllText(this.saveFileTo, response);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR WHILE SAVING FILE TO FILESYSTEM", ex);
            }
        }

        // ************************************************************************************* //
        //This function is used by all the API Functions to call the API 
        // ************************************************************************************* //

        public async Task<String> MakeRequest (String method, String serviceCall, ApiRequest ApiRequest, bool isDownload = false)
        {
            using (HttpClient client = new HttpClient())
            {
                String FinalResponse = ""; 
                try
                {
                    string json = JsonConvert.SerializeObject(ApiRequest, Formatting.Indented);

                    //Authentication & Authorization
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.ApiSettings.ApiKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.Token);

                    //Making the request
                    var result = client.PostAsync(this.ApiSettings.ApiUrl + serviceCall, new StringContent(json, Encoding.UTF8, "application/json")).Result;
                    result.EnsureSuccessStatusCode();
                    //Reading the response as a string
                    var response = await result.Content.ReadAsStringAsync();

                    //Serializing the response depeding on which endpoint was called
                    if (!isDownload)
                    {
                        if (serviceCall == "ProvideFile")
                        {
                            TaskIdResponse responseObject = JsonConvert.DeserializeObject<TaskIdResponse>(response);
                            FinalResponse = responseObject.Result.TaskId;
                        }
                        else if (serviceCall == "GetProgress")
                        {
                            ProgressResponse responseObject = JsonConvert.DeserializeObject<ProgressResponse>(response);
                            FinalResponse = responseObject.Result.Progress;
                        }
                        else if (serviceCall == "GetPrintingInformation")
                        {
                            PrintingInformationResponse responseObject = JsonConvert.DeserializeObject<PrintingInformationResponse>(response);
                            FinalResponse = JsonConvert.SerializeObject(responseObject.Result);
                        } else
                        {
                            TaskIdResponse responseObject = JsonConvert.DeserializeObject<TaskIdResponse>(response);
                            FinalResponse = responseObject.Result.TaskId;
                        }

                    }
                    else
                    {
                        SaveFile(response, Path.GetFileNameWithoutExtension(this.FileToSlice), ".fcode");
                        FinalResponse = response;
                    }

                    //Logging the results of the request
                    this.logResponse(result, serviceCall, isDownload);

                    return FinalResponse;
                }
                catch (Exception e)
                {
                    throw e;
                }   
            }
        }
    }
}
