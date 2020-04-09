using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;

namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
        private const int MENU_TOWN_INSERT_INDEX = 5;
        private const int MENU_CASTLE_INSERT_INDEX = 3;
        private const string MENU_TOWN_KEY = "town";
        private const string MENU_CASTLE_KEY = "castle";
        private const string MENU_TITLE = "Diplomacy";
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
            campaignGameStarter.AddGameMenuOption(MENU_TOWN_KEY, "make_diplomacy_town", MENU_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), false, MENU_TOWN_INSERT_INDEX, false);
            campaignGameStarter.AddGameMenuOption(MENU_CASTLE_KEY, "make_diplomacy_castle", MENU_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), false, MENU_CASTLE_INSERT_INDEX, false);
        }

        private void menuConsequence(MenuCallbackArgs args)
        {
            List<IFaction> enemyFactions = FactionManager.GetEnemyFactions(Hero.MainHero.MapFaction).ToList();
            foreach (IFaction faction in enemyFactions)
            {
                InformationManager.DisplayMessage(new InformationMessage(faction.Name.ToString()));
            }  
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
