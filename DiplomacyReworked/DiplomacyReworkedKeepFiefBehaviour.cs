using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

using TaleWorlds.CampaignSystem.Barterables;

using TaleWorlds.Core;


using TaleWorlds.CampaignSystem.Election;

namespace DiplomacyReworked
{
    class DiplomacyReworkedKeepFiefBehaviour : CampaignBehaviorBase
    {
        SettlementClaimantDecision currentDecision = null;
        public DataHub hub;
        private BasicLoggingUtil logger;

        public DiplomacyReworkedKeepFiefBehaviour(BasicLoggingUtil logger,DataHub hub)
        {
            this.hub = hub;
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

                this.logger.logError("DiplomacyReworkedKeepFiefBehaviour", "OnDecisionAdded", e.StackTrace, values,e);
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
            Dictionary<String, object> values = new Dictionary<string, object>();
            try
            {
                Campaign.Current.RemoveDecision(this.currentDecision);
                values.Add("SettlementName", this.currentDecision.Settlement.Name);
                values.Add("SettlementPossessor", this.currentDecision.Settlement.OwnerClan.Name);
                values.Add("HeroFactionIsKingdom",Hero.MainHero.MapFaction.IsKingdomFaction);
                values.Add("HeroFaction", Hero.MainHero.MapFaction.Name);
                values.Add("Hero", Hero.MainHero.Name);
                FiefBarterable fief = new FiefBarterable(this.currentDecision.Settlement, Hero.MainHero.MapFaction.Leader, Hero.MainHero);
                fief.Apply();
                values.Add("SettlementPossessorAfterTrade", fief.TargetSettlement.OwnerClan.Leader.Name);
                Campaign.Current.GameMenuManager.MenuLocations.Clear();
                this.currentDecision = null;
            }catch(Exception e){
                DataHub.DisplayInfoMsg(this.hub.getError("gift_fief_failed"));
                this.logger.logError("DiplomacyReworkedKeepFiefBehaviour", "fiefKeepConfirmedAction", e.StackTrace, values,e);
            }
        }
        //unused overrides
        public override void SyncData(IDataStore dataStore)
        { }
    }
}
