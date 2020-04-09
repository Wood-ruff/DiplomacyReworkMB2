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
            campaignGameStarter.AddGameMenuOption(MENU_TOWN_KEY, MENU_ID+MENU_TOWN_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), false, MENU_TOWN_INSERT_INDEX, false);
            campaignGameStarter.AddGameMenuOption(MENU_CASTLE_KEY, MENU_ID + MENU_CASTLE_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), false, MENU_CASTLE_INSERT_INDEX, false);


            campaignGameStarter.AddGameMenu(MENU_ID, MENU_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters);
        }

        private void MenuOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
        }

        private void menuConsequence(MenuCallbackArgs args)
        {
            
            //BarterManager.Instance.InitializeMakePeaceBarterContext(new PeaceBarterable(Hero.MainHero,Hero.MainHero,Hero.MainHero.OwnedParties.First(),Hero.MainHero.MapFaction,CampaignTime.Now),data,null);
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
