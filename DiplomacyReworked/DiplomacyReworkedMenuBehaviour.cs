using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
        private const string LOGGING_PATH = "./DiplomacyReworkedLog.txt";

        private const int MENU_TOWN_INSERT_INDEX = 5;
        private const int MENU_CASTLE_INSERT_INDEX = 3;
        private const string MENU_TOWN_KEY = "town";
        private const string MENU_CASTLE_KEY = "castle";
        private const string MENU_ID = "diplomacy";
        private const string MENU_BUTTON_TITLE = "Diplomacy";
        private const string MENU_TEXT = "Select a Kingdom to negotiate";
        private const string MENU_FACTION_DIPLOMACY_ID = "faction_diplomacy";
        private const string MENU_FACTION_DIPLOMACY_TEXT = "What do you want to do?";
        private IFaction currentSelectedFaction = null;


        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            AddGameMenus(campaignGameStarter);
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(MENU_TOWN_KEY, MENU_ID + MENU_TOWN_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, MENU_TOWN_INSERT_INDEX, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_CASTLE_KEY, MENU_ID + MENU_CASTLE_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, MENU_CASTLE_INSERT_INDEX, isRepeatable);


            campaignGameStarter.AddGameMenu(MENU_ID, MENU_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters);


            string factionName = "";
            int currentMaxIndex = 0;
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.IsKingdomFaction && faction != Hero.MainHero.MapFaction && faction.Name.ToString() != "Return")
                {
                    factionName = faction.Name.ToString();
                    campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + factionName, factionName, new GameMenuOption.OnConditionDelegate(selectMenuCondition), new GameMenuOption.OnConsequenceDelegate(selectMenuConsequence), isLeave, currentMaxIndex, isRepeatable);
                    currentMaxIndex++;
                }
            }
            campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + "quit", "Return to Menu", new GameMenuOption.OnConditionDelegate(selectMenuCondition), new GameMenuOption.OnConsequenceDelegate(selectMenuQuitConsequence), false, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "peace", "Negotiate Peace", new GameMenuOption.OnConditionDelegate(selectActionPeaceCondition), new GameMenuOption.OnConsequenceDelegate(selectActionPeaceConsequence), false, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "war", "Declare War", new GameMenuOption.OnConditionDelegate(selectActionWarCondition), new GameMenuOption.OnConsequenceDelegate(selectActionWarConsequence), false, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "quit", "Return to Selection", new GameMenuOption.OnConditionDelegate(selectMenuCondition), new GameMenuOption.OnConsequenceDelegate(selectActionQuitConsequence), false, -1, isRepeatable);

        }

        private void selectActionPeaceConsequence(MenuCallbackArgs args)
        {
            if (Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction))
            {
                attemptPeaceBarter(args);
                Campaign.Current.GameMenuManager.MenuLocations.Clear();
                GameMenu.SwitchToMenu(MENU_FACTION_DIPLOMACY_ID);
            }
            else
            {
                DisplayInfoMsg("You are already at peace with this faction");
            }
        }

        private bool selectActionPeaceCondition(MenuCallbackArgs args)
        {
            return Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction);
        }

        private bool selectActionWarCondition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction);
        }

        private void selectActionWarConsequence(MenuCallbackArgs args)
        {
            if (!Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction))
            {
                DeclareWarAction.Apply(Hero.MainHero.MapFaction, this.currentSelectedFaction);
                Campaign.Current.GameMenuManager.MenuLocations.Clear();
                GameMenu.SwitchToMenu(MENU_FACTION_DIPLOMACY_ID);
            }
            else
            {
                DisplayInfoMsg("You are already at war with this faction");
            }
        }

        // Debug method to attempt to do a barter with an enemy kingdom
        private void attemptPeaceBarter(MenuCallbackArgs args)
        {
            List<IFaction> enemyFactions = FactionManager.GetEnemyFactions(Hero.MainHero.MapFaction).ToList();

            Hero owner = Hero.MainHero;
            Hero other = this.currentSelectedFaction.Leader;


            PartyBase offererParty = PartyBase.MainParty;
            PartyBase otherParty = other.OwnedParties.First();

            /*
            CampaignTime duration = CampaignTime.Days(200);
            Campaign.Current.BarterManager.BarterBegin = barterBeginDelegate;
            PeaceBarterable barterable = new PeaceBarterable(owner,other, offererParty, this.currentSelectedFaction, duration);
            barterable.Initialize(new DefaultsBarterGroup(), false);
            barterable.Apply();
            BarterData data = new BarterData(other, owner, offererParty, other.OwnedParties.First(), new BarterManager.BarterContextInitializer(initializer), 0,false);
            */
            try
            {
                Campaign.Current.CampaignMissionManager.OpenConversationMission(new ConversationCharacterData(owner.CharacterObject), new ConversationCharacterData(other.CharacterObject));
            }
            catch (Exception e)
            {
                DisplayInfoMsg("Error");
                logString(e.ToString());
            }
        }

        private void barterBeginDelegate(BarterData args)
        {

        }

        private void printAllParties(Hero hero)
        {
            foreach (PartyBase party in hero.OwnedParties)
            {
                DisplayInfoMsg(party.Name.ToString());
            }
        }



        private void logString(String log)
        {
            System.IO.File.WriteAllText(@LOGGING_PATH, log);

        }


        private void logArray(IEnumerable<String> logs)
        {
            foreach (String log in logs)
            {
                logString(log);
            }
        }
        private void selectActionQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(MENU_ID);
        }

        private void selectMenuQuitConsequence(MenuCallbackArgs args)
        {
            if (Hero.MainHero.CurrentSettlement.IsTown)
            {
                GameMenu.SwitchToMenu("town");
            }
            else
            {
                GameMenu.SwitchToMenu("castle");
            }
        }

        private bool selectMenuCondition(MenuCallbackArgs args)
        {
            return true;
        }
        private void selectMenuConsequence(MenuCallbackArgs args)
        {
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.Name.ToString() == args.Text.ToString())
                {
                    this.currentSelectedFaction = faction;
                    GameMenu.SwitchToMenu(MENU_FACTION_DIPLOMACY_ID);
                    DisplayInfoMsg(faction.Name.ToString());
                    break;
                }
            }
            if (this.currentSelectedFaction.Name.ToString() == "")
            {
                DisplayInfoMsg("Something went wrong selecting your faction");
            }
        }

        // Debug Method to display all factions
        private void printAllFactions(MenuCallbackArgs args)
        {
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                DisplayInfoMsg(faction.Name.ToString());
            }
        }

        private bool canNegotiatePeace(MenuCallbackArgs args)
        {
            return !FactionManager.GetEnemyFactions(Hero.MainHero.MapFaction).IsEmpty();
        }

        private void MenuOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
        }

        private void menuConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(MENU_ID);
        }

        private bool menuCondition(MenuCallbackArgs args)
        {
            return Settlement.CurrentSettlement.MapFaction == Hero.MainHero.MapFaction && Hero.MainHero.IsFactionLeader;
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private static void DisplayInfoMsg(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg));
        }
    }
}
