using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiplomacyReworked
{
    class SettingsReader
    {
        public static string getLang()
        {
            try
            {
                XDocument language = XDocument.Parse(File.ReadAllText("./../../Modules/DiplomacyReworked/Settings.xml"));
                return language.Root.Element("Language").Value.ToString();
            }
            catch(Exception e)
            {
                return "English";
            }
            
        }

        public static string getKeepFiefOn()
        {
            try
            {
                 XDocument language = XDocument.Parse(File.ReadAllText("./../../Modules/DiplomacyReworked/Settings.xml"));
                 return language.Root.Element("KeepFiefOn").Value.ToString();
            }catch(Exception e)
            {
                return "1";
            }
        }
    }
}
