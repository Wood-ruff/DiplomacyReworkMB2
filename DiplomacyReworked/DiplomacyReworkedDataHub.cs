using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using System.Xml.Serialization;
using System.Xml.Linq;
using static DiplomacyReworked.DataHub;

namespace DiplomacyReworked 
{
    class DataHub : CampaignBehaviorBase
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
        [SaveableField(15)]
        List<WarCooldownDataModell> warCoolDown;
        [SaveableField(16)]
        List<WarCooldownDataModell> currentTruces;

        public DataHub(BasicLoggingUtil logger)
        {
            this.warCoolDown = new List<WarCooldownDataModell>();
            this.currentTruces = new List<WarCooldownDataModell>();
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

        public void passDayCoolDowns()
        {
            try
            {
                List<WarCooldownDataModell> runOutCds = new List<WarCooldownDataModell>();
                foreach (WarCooldownDataModell cooldown in this.warCoolDown)
                {
                    if (cooldown.dayPassed())
                    {
                        runOutCds.Add(cooldown);
                    }
                }

                foreach (WarCooldownDataModell runout in runOutCds)
                {
                    this.warCoolDown.Remove(runout);
                }

                runOutCds = new List<WarCooldownDataModell>();
                foreach (WarCooldownDataModell cooldown in this.currentTruces)
                {
                    if (cooldown.dayPassed())
                    {
                        runOutCds.Add(cooldown);
                    }
                }

                foreach (WarCooldownDataModell runout in runOutCds)
                {
                    this.currentTruces.Remove(runout);
                }

            }
            catch (Exception e)
            {
                logger.logError("DataHub", "passDayCooldown", e.StackTrace, null, e);
            }
        }

        public bool isPlayerOnCooldown(IFaction relatedFaction)
        {
            foreach (WarCooldownDataModell data in this.warCoolDown)
            {
                if (data.containsFaction(relatedFaction) && data.isPlayerRelated)
                {
                    return true;
                }
            }
            return false;
        }

        public int isPlayerOnTruce(IFaction relatedFaction)
        {
            foreach (WarCooldownDataModell data in this.currentTruces)
            {
                if (data.containsFaction(relatedFaction) && data.isPlayerRelated)
                {
                    return data.remainingDays;
                }
            }
            return -1;
        }

        public bool isFactionOnTruce(IFaction origFaction, IFaction relatedFaction)
        {
            foreach (WarCooldownDataModell data in this.currentTruces)
            {
                if (data.containsFaction(relatedFaction) && data.containsFaction(origFaction))
                {
                    return true;
                }
            }
            return false;
        }

        public void newWarDeclared(WarCooldownDataModell data)
        {
            this.warCoolDown.Add(data);
        }

        public void addNewTruce(WarCooldownDataModell data)
        {
            foreach(WarCooldownDataModell truce in currentTruces)
            {
                if (truce.equals(data)){
                    truce.addDays(data.remainingDays);
                    return;
                }
            }

            this.currentTruces.Add(data);
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

        public override void RegisterEvents()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData<List<WarCooldownDataModell>>("warCoolDown", ref this.warCoolDown);
                dataStore.SyncData<List<WarCooldownDataModell>>("currentTruces", ref this.currentTruces);

            }
            catch (Exception e)
            {
                DisplayInfoMsg(this.getError("error_loading_data"));
                this.logger.logError("DataHub", "SyncData", e.StackTrace, null, e);
            }

        }
    }
}

[SaveableClass(747942846)]
public class WarCooldownDataModell
{


    public WarCooldownDataModell(IFaction faction1, IFaction faction2, int totalDays)
    {
        this.faction1 = faction1;
        this.faction2 = faction2;
        this.remainingDays = totalDays;
        if (Hero.MainHero.MapFaction.IsKingdomFaction)
        {
            if (Hero.MainHero.MapFaction == faction1 || Hero.MainHero.MapFaction == faction2)
            {

                this.isPlayerRelated = true;
            }
            else
            {
                this.isPlayerRelated = false;
            }
        }
        else
        {
            this.isPlayerRelated = false;
        }
    }

    [SaveableProperty(10)]
    public bool isPlayerRelated { get; set; }

    [SaveableProperty(11)]
    public IFaction faction1 { get; set; }
    [SaveableProperty(12)]
    public IFaction faction2 { get; set; }
    [SaveableProperty(13)]
    public int remainingDays { get; set; }


    public bool dayPassed()
    {
        this.remainingDays -= 1;
        if (this.remainingDays <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void addDays(int days)
    {
        this.remainingDays += days;
    }

    public bool containsFaction(IFaction faction)
    {
        if (this.faction1 == faction || this.faction2 == faction)
        {
            return true;
        }
        return false;
    }

    public bool equals(WarCooldownDataModell model)
    {
        bool faction1 = false;
        bool faction2 = false;
        if (model.faction1 == this.faction1 || model.faction1 == this.faction2) faction1 = true;
        if (model.faction2 == this.faction1 || model.faction2 == this.faction2) faction2 = true;

        return faction1 && faction2;
    }

}
public class WarCooldownDataModellSaveDefiner : SaveableTypeDefiner
{
    public WarCooldownDataModellSaveDefiner() : base(0xdf3229)
    {
    }

    protected override void DefineClassTypes()
    {
        try
        {
            AddClassDefinition(typeof(WarCooldownDataModell), 747942846);
        }
        catch (Exception e)
        {

        }
    }

    protected override void DefineContainerDefinitions()
    {
        base.ConstructContainerDefinition(typeof(List<WarCooldownDataModell>));
    }
}
