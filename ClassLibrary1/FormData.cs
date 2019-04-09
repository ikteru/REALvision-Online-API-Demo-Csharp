using System;
using System.Collections.Generic;
using System.Text;

namespace RealvisionLib
{
    // ************************************************************************************* //
    //  This is the Object we pass to every API call using the Initialize Request function.
    //  Each API Function has it's own FormData instance that has different values.
    //  There is a different constructor for different kinds of requests.
    // ************************************************************************************* //

    public class FormData
    {

        public String SupportType { get; set; }
        public String PrinterModel { get; set; }
        public String ConfigPresetName { get; set; }
        public String ConfigFile { get; set; }
        public String UniqueID { get; set; }

        public FormData()
        {
        }

        public FormData( String supportType, String printerModel, String configPresetName)
        {
            SupportType = supportType;
            PrinterModel = printerModel;
            ConfigPresetName = configPresetName;
        }

        public FormData(String file, String supportType, String printerModel, String configPresetName, String configFile) : this(supportType, printerModel, configPresetName)
        {
            ConfigFile = configFile;
        }

        public FormData(String uniqueID)
        {
            UniqueID = uniqueID;
        }
    }
}
