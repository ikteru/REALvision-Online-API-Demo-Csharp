using System;
using System.IO;
using REALvisionApiLib;

namespace 
    DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Don't forget to add the REALvisionApiLib to this App's reference
            REALvisionApiLib.RealvisionApi realvisionInstance = new REALvisionApiLib.RealvisionApi();

            String currentFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\")); //The folder in which Program.cs exists
            realvisionInstance.CurrentFolder = currentFolder;

            ////Specify your Credentials : Subscription Key, client id and client secret .
            realvisionInstance.ApiKey = "0055df69240944e5a2edf6470344fee2";
            //realvisionInstance.ApiKey = "cddc9d03c546487f84e8adac2cedfbe5";
            //realvisionInstance.ClientId = "c2962643-a534-4d84-a298-6fb709af1bf4";
            //realvisionInstance.ClientSecret = "lH642TrZAWMRUXZnkJb8BtuAmZ6c6ds4my/DPCN2/hg=";

            //Set the authentication server URL and the API Url
            //realvisionInstance.AuthServerUrl = "https://login.microsoftonline.com/186eb8de-01f4-4c87-9d83-4946009f1791/oauth2/token";
            realvisionInstance.ApiUrl = "https://realvisiononline.azure-api.net";
            //realvisionInstance.ApiUrl = "https://test-getway.azure-api.net/realvision";

            //Get and set access token 
            //realvisionInstance.getToken();

            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/GetActivationStatus
            // ********************************************************************************//

            String activatoinStatus = realvisionInstance.getActivationStatus();

            //// ********************************************************************************//
            ////  To call https://realvisiononline.azure-api.net/ProvideFile
            //// ********************************************************************************//


            //Specify the name of the file you want to slice with it's extension.
            realvisionInstance.FileToSlice = "calicat.rvwj";

            //Specify where the file is stored
            //If it's stored in the Assets folder, use the Assets folder property
            //If not use the FileFolder property and specify the link to the file folder

            realvisionInstance.AssetsFolder = currentFolder + @"\Assets\";
            //realvisionInstance.FileFolder = currentFolder + @"\Assets\";

            //Specify where you want the downloaded FCode file to be stored.
            //If you don't specify it, the downloaded file will be stored in the same folder as the file you provided to slice

            realvisionInstance.DownloadsFolder = currentFolder + @"\Downloads\";

            //Specify the slicing configs
            realvisionInstance.SupportType = "n";
            realvisionInstance.PrinterModel = "IdeaWerk-Speed";
            realvisionInstance.ConfigPresetName = "Recommended";

            String uniqueID = realvisionInstance.ProvideFile();

            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/GetProgress
            // ********************************************************************************//

            String progress = realvisionInstance.GetProgress(uniqueID);

            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/GetPrintingInformation
            // ********************************************************************************//

            String printingInfos = realvisionInstance.GetPrintingInformation(uniqueID);

            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/DownloadFile
            // ********************************************************************************//

            //Note: DownloadFile will first check the progress of the slicing process before downloading the file
            //      which is why you'll notice in the Console that GetProgress is executed a few times before DownloadFile is executed
            realvisionInstance.Downloadfile(uniqueID);


        }
    }
}
