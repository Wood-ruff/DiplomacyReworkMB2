using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace DiplomacyReworked
{
    class DataHub
    {
        private BasicLoggingUtil logger = null;
        public const string LOGGING_PATH = "./DiplomacyReworkedLog.txt";
        //diplo
        public static int MENU_TOWN_INSERT_INDEX = 5;
        public static int MENU_CASTLE_INSERT_INDEX = 3;
        public static string MENU_TOWN_KEY = "town";
        public static string MENU_CASTLE_KEY = "castle";
        public static string MENU_ID = "diplomacy";
        public static string MENU_FACTION_DIPLOMACY_ID = "faction_diplomacy";


        //innerdiple
        public static string INNER_DIPLOMACY_OPTION_ID = "inner_diplomacy_option";
        public static string MENU_INNER_DIPLOMACY_ID = "inner_diplomacy";
        public static string INNER_DIPLOMACY_CLAN_MENU_ID = "inner_diplomacy_clan";
        public static string INNER_DIPLOMACY_FIEF_MENU_ID = "inner_diplomacy_fiefs";

        public static string KEEP_FIEF_MENU_ID = "keep_fief_menu";

        private const string LANGUAGE_SUPPORT_FOLDER = "./../../Modules/DiplomacyReworked/i18n/";
        List<XDocument> languages = null;
        XDocument currentLang = null;

        String selectedLang = "English";

        public DataHub(BasicLoggingUtil logger)
        {
            this.logger = logger;
            this.selectedLang = SettingsReader.getLang();
            this.languages = new List<XDocument>();
            foreach (String path in Directory.GetFiles(LANGUAGE_SUPPORT_FOLDER))
            {
                if (File.Exists(path))
                {
                    try
                    {
                        XDocument language = XDocument.Parse(File.ReadAllText(path));
                        if (language.Root.Element("Language").Value.ToString() == this.selectedLang)
                        {
                            this.currentLang = language;
                        }
                        this.languages.Add(language);
                    }
                    catch (Exception e)
                    {
                        this.logger.logError("DataHub", "DataHub", e.StackTrace, null);
                    }
                }
            }
            if (this.currentLang == null)
            {
                this.currentLang = languages[0];
            }
        }

        public string getError(String key)
        {
            return this.getmessage(key, "ErrorMessages");
        }

        public string getStandard(String key)
        {
            return this.getmessage(key, "Standard");
        }
        public string getButton(String key)
        {
            return this.getmessage(key, "Buttons");
        }

        private string getmessage(String key, String subsection)
        {
            string message;
            try
            {
                message = this.currentLang.Root.Element(subsection).Element(key).Value.ToString();
            }
            catch (Exception e)
            {
                return "translationkey not found";
            }
            return message;
        }

        List<String> getLanguages()
        {
            List<String> language = new List<string>();
            foreach (XDocument doc in this.languages)
            {
                language.Add(doc.Root.Element("Language").Value.ToString());
            }
            return language.IsEmpty() ? null : language;
        }


        public static void DisplayInfoMsg(String message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message));
        }

        public static void MenuOnInit(Object obj)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
        }

        public static bool alwaysTrueDelegate(Object obj)
        {
            return true;
        }

        public static void logString(String log)
        {
            System.IO.File.WriteAllText(DataHub.LOGGING_PATH, log);

        }
    }
}
