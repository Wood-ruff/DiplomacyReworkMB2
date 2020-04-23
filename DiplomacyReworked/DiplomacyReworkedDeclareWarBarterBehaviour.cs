using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
namespace DiplomacyReworked
{
    class DiplomacyReworkedDeclareWarBarterBehaviour : CampaignBehaviorBase
    {
        DataHub hub;
        BasicLoggingUtil logger;
        public DiplomacyReworkedDeclareWarBarterBehaviour(DataHub hub, BasicLoggingUtil logger)
        {
            this.hub = hub;
            this.logger = logger;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, onSessionLaunched);
        }

        private void onSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddPlayerLine("paid_war_conversation_option", "hero_main_options", "lord_declare_war_conversation", this.hub.getStandard("paid_war_menu_option"), paidWarCondition, paid_war_consequence);
            obj.AddDialogLine("paid_war_menu", "lord_declare_war_conversation", "lord_choose_paid_war_target", this.hub.getStandard("paid_war_selection_line"), condition, consequence);
            foreach (Kingdom kingdom in Campaign.Current.Kingdoms)
            {
                obj.AddPlayerLine("paid_war_conversation_choose_target_" + kingdom.Name.ToString(), "lord_choose_paid_war_target", "lord_start", this.hub.getStandard("paid_war_barter_start") +" "+ kingdom.Name.ToString() + " ? ", paidWarCondition, paidWarActionDelegate);
            }
            obj.AddPlayerLine("paid_war_conversation_choose_target_back", "lord_choose_paid_war_target", "lord_start", this.hub.getButton("return_options"), condition, consequence);
        }

        private void paid_war_consequence()
        {
        }

        private void consequence()
        {
        }


        private bool condition()
        {
            return true;
        }

        private void paidWarActionDelegate()
        {
            try
            {
                Hero mainHero = Hero.MainHero;
                Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
                PartyBase mainParty = PartyBase.MainParty;
                Kingdom target = Campaign.Current.Kingdoms[Campaign.Current.ConversationManager.LastSelectedButtonIndex];
                if (target == oneToOneConversationHero.MapFaction)
                {
                    DataHub.DisplayInfoMsg(this.hub.getError("no_war_on_self"));
                    return;
                }
                if (this.hub.isFactionOnTruce(target, oneToOneConversationHero.MapFaction))
                {
                    DataHub.DisplayInfoMsg(this.hub.getError("factions_at_truce"));
                    return;
                }

                MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;
                PartyBase otherParty = (partyBelongedTo != null) ? partyBelongedTo.Party : null;
                BarterManager.BarterContextInitializer contextInitializer = new BarterManager.BarterContextInitializer(BarterManager.Instance.InitializeMakePeaceBarterContext);
                Barterable[] array = new Barterable[1]; ;
                MobileParty conversationParty = MobileParty.ConversationParty;
                array[0] = new CustomDeclareWarBarterable(oneToOneConversationHero, otherParty, target, oneToOneConversationHero, this.hub);
                BarterManager.Instance.StartBarterOffer(mainHero, oneToOneConversationHero, otherParty, mainParty, null, contextInitializer, 0, false, array);
            }
            catch (Exception e)
            {
                this.logger.logError("DiplomacyReworkedDeclareWarBarterBehaviour", "paidWarActionDelegate", e.StackTrace, null, e);
            }
        }

        private bool paidWarCondition()
        {
            Hero other = Hero.OneToOneConversationHero;
            return other.MapFaction != Hero.MainHero.MapFaction && other.IsFactionLeader && !other.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction);
        }



        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
