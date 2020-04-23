using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace DiplomacyReworked
{
    public class BasicLoggingUtil
    {
        private static string LOGGING_PATH = @"./../../Modules/DiplomacyReworked/ErrorLogs/";
        private static string FILEPATH = @"./../../Modules/DiplomacyReworked/ErrorLogs/Errors.log";

        public BasicLoggingUtil()
        {

            if (!Directory.Exists(LOGGING_PATH))
            {
                Directory.CreateDirectory(LOGGING_PATH);
            }

            if (!File.Exists(FILEPATH))
            {
                File.Create(FILEPATH);
            }
        }

        public void log(string log)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(FILEPATH, true))
            {
                file.WriteLine(log);
            }
        }

        public void logArray(IEnumerable<String> logs) {
            foreach(String log in logs)
            {
                this.log(log);
            }
        }
        
        public void logError(String origin,String function,String stacktrace,Dictionary<string,Object> values,Exception e = null)
        {
            this.log("Error at " + origin.ToString() + " in function " + function);
            this.log("With stacktrace :\n" + stacktrace);
            if(values != null)
            {
                this.log("Values being:\n");
                foreach(String key in values.Keys)
                {
                    this.log(key + ":" + values[key].ToString());
                }
            }
            if(e != null)
            {
                this.log("With Message: "+e.Message);
            }
            this.log("----------------------------------------------------");
        }

    }
}
