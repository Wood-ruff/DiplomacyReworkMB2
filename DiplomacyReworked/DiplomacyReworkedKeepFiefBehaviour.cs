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
        public DataHub hub;
        private BasicLoggingUtil logger;

        public DiplomacyReworkedKeepFiefBehaviour(BasicLoggingUtil logger)
        {
            this.logger = logger;
        }

        public override void RegisterEvents()
        {
            if (SettingsReader.getKeepFiefOn() == "1")
            {
                CampaignEvents.KingdomDecisionAdded.AddNonSerializedListener(this, onDecisionAdded);
            }
        }

        //Listeners
        private void onDecisionAdded(KingdomDecision arg1, bool arg2)
        {
            Dictionary<String, object> values = new Dictionary<string, object>();
            try
            {
                if (arg1 is TaleWorlds.CampaignSystem.Election.SettlementClaimantDecision)
                {
                    SettlementClaimantDecision decision = arg1 as SettlementClaimantDecision;
                    if (decision.Settlement.LastAttackerParty != null)
                    {
                        Hero lastAttacker = decision.Settlement.LastAttackerParty.Leader.HeroObject;
                    values.Add("LastAttacker", decision.Settlement.LastAttackerParty.Name.ToString());
                        if (Hero.MainHero.MapFaction is Kingdom)
                        {
                            values.Add("HeroName", Hero.MainHero.Name.ToString());
                            values.Add("HeroMapFaction", Hero.MainHero.MapFaction.Name.ToString());
                            values.Add("HeroMapFactionisKingdom", Hero.MainHero.MapFaction.IsKingdomFaction.ToString());
                            if (decision.Kingdom == Hero.MainHero.MapFaction && lastAttacker.Name.ToString() == Hero.MainHero.Name.ToString())
                            {
                                InformationManager.ShowInquiry(new InquiryData(this.hub.getStandard("fief_captured_title"), this.hub.getStandard("fief_keep_pass_text") + "\n" + this.hub.getStandard("settlement_name") + " " + decision.Settlement.Name.ToString(), true, true, this.hub.getButton("button_keep"), this.hub.getButton("button_pass"), fiefKeepConfirmedAction, fiefKeepDeniedAction));
                                this.currentDecision = decision;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DataHub.DisplayInfoMsg(this.hub.getError("redistribute_fief_failed"));

                this.logger.logError("DiplomacyReworkedKeepFiefBehaviour", "OnDecisionAdded", e.StackTrace, values);
            }
            values = null;
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
            FiefBarterable fief = new FiefBarterable(this.currentDecision.Settlement, Hero.MainHero.MapFaction.Leader,Hero.MainHero);
            //FiefBarterable fief = new FiefBarterable(this.currentDecision.Settlement, this.currentDecision.Settlement.OwnerClan.Leader, this.currentDecision.Settlement.OwnerClan.Leader.OwnedParties.First(), Hero.MainHero);
            fief.Apply();
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            this.currentDecision = null;
        }
        //unused overrides
        public override void SyncData(IDataStore dataStore)
        { }
    }
}
