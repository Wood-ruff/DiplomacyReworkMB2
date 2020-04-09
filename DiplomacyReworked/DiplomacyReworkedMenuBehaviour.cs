using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
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

            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "quit", "Return to Selection", new GameMenuOption.OnConditionDelegate(selectMenuCondition), new GameMenuOption.OnConsequenceDelegate(selectActionQuitConsequence), false, -1, isRepeatable);

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
            if(this.currentSelectedFaction.Name.ToString() == "")
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
