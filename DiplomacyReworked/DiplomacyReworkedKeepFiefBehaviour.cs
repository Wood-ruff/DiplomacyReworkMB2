using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;

using TaleWorlds.CampaignSystem.Election;

namespace DiplomacyReworked
{
    class DiplomacyReworkedKeepFiefBehaviour : CampaignBehaviorBase
    {
        SettlementClaimantDecision currentDecision = null;

        public override void RegisterEvents()
        {
            CampaignEvents.KingdomDecisionAdded.AddNonSerializedListener(this, onDecisionAdded);
        }

        //Listeners
        private void onDecisionAdded(KingdomDecision arg1, bool arg2)
        {
            try
            {
                if (arg1 is TaleWorlds.CampaignSystem.Election.SettlementClaimantDecision)
                {
                    SettlementClaimantDecision decision = arg1 as SettlementClaimantDecision;
                    if (decision.Settlement.LastAttackerParty != null)
                    {
                        Hero lastAttacker = decision.Settlement.LastAttackerParty.Leader.HeroObject;
                        if (Hero.MainHero.MapFaction is Kingdom)
                        {
                            if (decision.Kingdom == Hero.MainHero.MapFaction && lastAttacker.Name.ToString() == Hero.MainHero.Name.ToString())
                            {
                                InformationManager.ShowInquiry(new InquiryData("Fief captured", DataHub.KEEP_FIEF_MENU_TEXT + DataHub.KEEP_FIEF_SETTLEMENT + " " + decision.Settlement.Name.ToString(), true, true, "Keep", "Pass", fiefKeepConfirmedAction, fiefKeepDeniedAction));
                                this.currentDecision = decision;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DataHub.DisplayInfoMsg("DiplomacyReworked:A Critical Error occurred when redistributing the Fief, continueing as defined by main game");
            }
        }

        //Consequences
        private void fiefKeepDeniedAction()
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            this.currentDecision = null;
        }

        private void fiefKeepConfirmedAction()
        {
            Campaign.Current.RemoveDecision(this.currentDecision);
            //FiefBarterable fief = new FiefBarterable(this.currentdecision.Settlement, Hero.MainHero.MapFaction.Leader,Hero.MainHero);
            FiefBarterable fief = new FiefBarterable(this.currentDecision.Settlement, this.currentDecision.Settlement.OwnerClan.Leader, this.currentDecision.Settlement.OwnerClan.Leader.OwnedParties.First(), Hero.MainHero);
            fief.Apply();
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            this.currentDecision = null;
        }
        //unused overrides
        public override void SyncData(IDataStore dataStore)
        { }
    }
}
