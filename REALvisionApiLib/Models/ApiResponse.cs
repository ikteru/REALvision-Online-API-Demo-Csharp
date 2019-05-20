using System;
using System.Collections.Generic;
using System.Text;

namespace REALvisionApiLib
{
    public class ApiResponse
    {
        public String ApiVersion { get; set; }
        public String CoreVersion { get; set; }
        public String RealvisionHtmlVersion { get; set; }
    }

    public class TaskIdResponse : ApiResponse
    {
        public Task Result { get; set; }

    }

    public class ProgressResponse : ApiResponse
    {
        public SlicingProgress Result { get; set; }

    }
    public class PrintingInformationResponse : ApiResponse
    {
        public PrintingInformation Result { get; set; }
    }


    public class Task
    {
        public String TaskId { get; set; }

        public Task(string taskId)
        {
            TaskId = taskId;
        }
    }

    public class SlicingProgress
    {
        public String Progress { get; set; }

        public SlicingProgress(string progress)
        {
            Progress = progress;
        }
    }

    public class PrintingInformation : ApiResponse
    {
        public String Time { get; set; }
        public String Length { get; set; }
        public String Weight { get; set; }

        public PrintingInformation(string time, string length, string weight)
        {
            Time = time;
            Length = length;
            Weight = weight;
        }
    }
}
