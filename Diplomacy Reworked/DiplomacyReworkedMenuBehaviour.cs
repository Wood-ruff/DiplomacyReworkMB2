using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;

namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
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
            DisplayInfoMsg("Loading Mod");
            campaignGameStarter.AddGameMenuOption("village", "diplomacy_reworked", "Make Diplomacy", new GameMenuOption.OnConditionDelegate(this.menuCondition), this.menuConsequence, false, 3, false);
            DisplayInfoMsg("Mod Loaded");
        }

        private void menuConsequence(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("Placeholder Text"));
        }

        private bool menuCondition(MenuCallbackArgs args)
        {
            return true;
        }

        public override void SyncData(IDataStore dataStore)
        {
            throw new NotImplementedException();
        }

        private static void DisplayInfoMsg(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg));
        }
    }
}
