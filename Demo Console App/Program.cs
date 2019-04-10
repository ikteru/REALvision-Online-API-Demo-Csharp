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
            String currentFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\")); //The folder in which Program.cs exists

            //Don't forget to add the REALvisionApiLib to this App's reference
            REALvisionApiLib.RealvisionApi realvisionInstance = new REALvisionApiLib.RealvisionApi();

            //Specify the API link and the SUBSCRIPTION_KEY
            realvisionInstance.ApiKey = "46a51b685f1e4fbca8cfc98397fdcb8c";
            realvisionInstance.ApiLink = "https://realvisiononline.azure-api.net";


            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/GetActivationStatus
            // ********************************************************************************//

            String activatoinStatus = realvisionInstance.getActivationStatus();

            // ********************************************************************************//
            //  To call https://realvisiononline.azure-api.net/ProvideFile
            // ********************************************************************************//


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
