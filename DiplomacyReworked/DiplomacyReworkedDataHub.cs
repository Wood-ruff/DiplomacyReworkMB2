using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;

namespace DiplomacyReworked
{
    class DataHub : CampaignBehaviorBase
    {
        public const string LOGGING_PATH = "./DiplomacyReworkedLog.txt";
        //diplo
        public static int MENU_TOWN_INSERT_INDEX = 5;
        public static int MENU_CASTLE_INSERT_INDEX = 3;
        public static string MENU_TOWN_KEY = "town";
        public static string MENU_CASTLE_KEY = "castle";
        public static string MENU_ID = "diplomacy";
        public static string MENU_BUTTON_TITLE = "Diplomacy";
        public static string MENU_TEXT = "Select a Kingdom to interact";
        public static string MENU_FACTION_DIPLOMACY_ID = "faction_diplomacy";
        public static string MENU_FACTION_DIPLOMACY_TEXT = "What do you want to do?";


        //innerdiple
        public static string INNER_DIPLOMACY_OPTION_ID = "inner_diplomacy_option";
        public static string MENU_INNER_DIPLOMACY_ID = "inner_diplomacy";
        public static string MENU_INNER_DIPLOMACY_TEXT = "Kingdom Diplomacy";
        public static string MENU_INNER_DIPLOMACY_TITLE = "Select a Clan to interact";
        public static string INNER_DIPLOMACY_CLAN_MENU_ID = "inner_diplomacy_clan";
        public static string INNER_DIPLOMACY_CLAN_MENU_TITLE = "What do you want to do?";
        public static string INNER_DIPLOMACY_FIEF_MENU_ID = "inner_diplomacy_fiefs";
        public static string INNER_DIPLOMACY_FIEF_MENU_TEXT = "Which Fief do you want to gift?";

        public static string KEEP_FIEF_MENU_ID = "keep_fief_menu";
        public static string KEEP_FIEF_MENU_TEXT = "Do you wish to keep the fief or let it be passed by vote?";
        public static string KEEP_FIEF_FRIENDLY_MENU_TEXT = "A Fief has become available in your Kingdom, do you wish to claim it for yourself or let it be passed by vote?";
        public static string KEEP_FIEF_SETTLEMENT = "SETTLEMENT NAME:";

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

        public override void RegisterEvents()
        {}

        public override void SyncData(IDataStore dataStore)
        { }

        public static void logString(String log)
        {
            System.IO.File.WriteAllText(DataHub.LOGGING_PATH, log);

        }
    }
}
