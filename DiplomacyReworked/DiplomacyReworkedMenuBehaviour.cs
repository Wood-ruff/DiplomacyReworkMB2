﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Barter;

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

            isLeave = true;
            // Adding the "Make Peace" Menu entry
            campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + "_negotiate_peace", "Negotiate Peace", new GameMenuOption.OnConditionDelegate(canNegotiatePeace), new GameMenuOption.OnConsequenceDelegate(attemptPeaceBarter), isLeave, 0, isRepeatable);


            // string factionName = "";
            // List<IFaction> enemyFactions = FactionManager.GetEnemyFactions(Hero.MainHero.MapFaction).ToList();
            // for (int i = 0; i < enemyFactions.Capacity; i++)
            // {
            //    factionName = enemyFactions.ElementAt(i).Name.ToString();
            // campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + factionName, "Make peace with " + factionName, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, MENU_TOWN_INSERT_INDEX, isRepeatable);
            //}
        }

        // Debug method to attempt to do a barter with an enemy kingdom
        private void attemptPeaceBarter(MenuCallbackArgs args)
        {
            List<IFaction> enemyFactions = FactionManager.GetEnemyFactions(Hero.MainHero.MapFaction).ToList();

            Hero owner = Hero.MainHero;
            Hero other = enemyFactions.ElementAt(0).Leader;

            PartyBase offererParty = PartyBase.MainParty;
            MobileParty otherParty = other.PartyBelongedTo;

            // BarterData 
            //PeaceBarterable barterable =  PeaceBarterable(Hero.MainHero, enemyFactions.ElementAt(0).Leader, enemyFactions.ElementAt(0), null);
            CampaignTime duration = CampaignTime.Days(Campaign.Current.CampaignDt);
            PeaceBarterable barterable = new PeaceBarterable(owner, other, offererParty, enemyFactions.ElementAt(0), duration);
            // barterable.Apply();

            BarterData data = new BarterData(owner, other, offererParty, (otherParty != null) ? otherParty.Party : null);

            try
            {
                // IAsyncResult result;
                // AsyncCallback callback = new AsyncCallback(demo);
                // BarterManager.Instance.BarterBegin.BeginInvoke(data, callback, null);

                BarterManager.Instance.StartBarterOffer(other, owner, (otherParty != null) ? otherParty.Party : null, offererParty);

                BarterManager.Instance.InitializeMakePeaceBarterContext(barterable, data, null);
                Campaign.Current.BarterManager.BeginNewBarter(data);
                // CampaignMission.Current.SetMissionMode(MissionMode.Barter, false);
                BarterManager.Instance.ExecuteAIBarter(data, owner.MapFaction, other.MapFaction, owner, other);
            }
            catch (Exception e)
            {
                DisplayInfoMsg(e.Message);
                DisplayInfoMsg(e.StackTrace);
            }

            


            // BarterManager.Instance.InitializeMakePeaceBarterContext(barterable, data, null);            
            // Campaign.Current.BarterManager.BeginNewBarter(data);
            // CampaignMission.Current.SetMissionMode(MissionMode.Barter, false);

            

            // BarterManager.Instance.ExecuteAIBarter(data, owner.MapFaction, other.MapFaction, owner, other);

            // BarterItemVisualWidget();
            // Campaign.Current.BarterManager.ExecuteAiBarter(enemyFactions.ElementAt(0), owner.MapFaction, other, owner, barterable);


            //BarterManager.Instance.StartBarterOffer(other, owner, (otherParty != null) ? otherParty.Party : null, offererParty);
            //enemyFactions.ElementAt(0).Leader
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
            InformationManager.DisplayMessage(new InformationMessage("Placeholder Text"));
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
