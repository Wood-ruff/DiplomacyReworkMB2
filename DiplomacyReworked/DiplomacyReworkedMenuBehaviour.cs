using System;

using System.Linq;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;


using TaleWorlds.CampaignSystem.Election;
using System.Threading;

namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
        private IFaction currentSelectedFaction = null;
        public DataHub hub;
        private BasicLoggingUtil logger;

        public DiplomacyReworkedMenuBehaviour(BasicLoggingUtil logger, DataHub hub)
        {
            this.hub = hub;
            this.logger = logger;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, onSessionLaunched);
            //CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, onDecisionConcludedDelegate);
        }

        private void onSessionLaunched(CampaignGameStarter obj)
        {
            obj.AddPlayerLine("offer_truce_conversation_option", "hero_main_options", "lord_start", this.hub.getStandard("offer_truce_dialog_option"), truceLineCondition, selectTruceActionDelegate);
        }

        private bool truceLineCondition()
        {
            return Hero.OneToOneConversationHero.MapFaction != Hero.MainHero.MapFaction &&Hero.OneToOneConversationHero.IsFactionLeader;
        }

        private void selectTruceActionDelegate()
        {
            selectTruceActionDelegate(null);
        }

        private void onDecisionConcludedDelegate(KingdomDecision arg1, DecisionOutcome arg2, bool arg3)
        {
            Dictionary<String, Object> values = new Dictionary<string, object>();
            try
            {

                float mayority = 0;
                foreach (DecisionOutcome outcome in arg1.DetermineInitialCandidates().ToList())
                {
                    if (outcome.Support > mayority) mayority = outcome.Support;
                }
                values.Add("mayority", mayority);

                if (arg2.Support >= mayority)
                {
                    values.Add("type", arg1.GetType());
                    if (arg1 is TaleWorlds.CampaignSystem.Election.SettlementClaimantDecision)
                    {
                        if (arg1.DetermineChooser() == Hero.MainHero.Clan)
                        {
                            foreach (Clan clan in arg1.Kingdom.Clans)
                            {
                                int change = (int)arg1.CalculateRelationshipEffectWithSponsor(clan);
                                values.Add("change", change);
                                values.Add("current", clan.Leader.GetRelationWithPlayer());
                                if (change < 0)
                                {
                                    clan.Leader.SetPersonalRelation(Hero.MainHero, (int)clan.Leader.GetRelationWithPlayer() + (change * -1));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.logger.logError("DiplomacyReworkedMenuBehaviour", "onDecisionConcludedDelegate", e.StackTrace, values, e);
                this.hub.getError("error_setting_relation");
            }
        }

        public void OnAfterNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            try
            {
                AddGameMenus(campaignGameStarter);
            }
            catch (Exception e)
            {
                this.logger.logError("DiplomacyReworkedMenuBehaviour", "OnAfterNewGameCreated", e.StackTrace, null, e);
                this.hub.getError("critical_load_Menus");
            }
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            //Adding in menus and buttons for outer diplomacy
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_TOWN_KEY, DataHub.MENU_ID + DataHub.MENU_TOWN_KEY, this.hub.getStandard("diplomacy_title"), new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, DataHub.MENU_TOWN_INSERT_INDEX, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_CASTLE_KEY, DataHub.MENU_ID + DataHub.MENU_CASTLE_KEY, this.hub.getStandard("diplomacy_title"), new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, DataHub.MENU_CASTLE_INSERT_INDEX, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.MENU_ID, this.hub.getStandard("select_kingdom"), new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithBoth);
            string factionName = "";
            int currentMaxIndex = 0;
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.IsKingdomFaction && faction != Hero.MainHero.MapFaction)
                {
                    factionName = faction.Name.ToString();
                    campaignGameStarter.AddGameMenuOption(DataHub.MENU_ID, DataHub.MENU_ID + factionName, factionName, KingdomDisplayDelegate, selectMenuConsequence, isLeave, currentMaxIndex, isRepeatable);
                    currentMaxIndex++;
                }
            }
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_ID, DataHub.MENU_ID + "quit", this.hub.getButton("return_menu"), alwaystrueDelegate, selectMenuQuitConsequence, isLeave, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.MENU_FACTION_DIPLOMACY_ID, this.hub.getStandard("what_to_do"), new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "conversation", this.hub.getButton("conversation_leader"), selectActionPeaceCondition, selectActionPeaceConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "war", this.hub.getButton("declare_war"), selectActionWarCondition, selectActionWarConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "quit", this.hub.getButton("return_selection"), alwaystrueDelegate, selectActionQuitConsequence, true, -1, isRepeatable);
        }

        private void selectTruceActionDelegate(MenuCallbackArgs args)
        {
            //FiefBarterable peace = new FiefBarterable(Hero.MainHero.Clan.Settlements.First(), Hero.MainHero, this.currentSelectedFaction.Leader);
            //BarterData data = new BarterData(Hero.MainHero, this.currentSelectedFaction.Leader, this.currentSelectedFaction.Leader.OwnedParties.First(), Campaign.Current.MainParty.Party);
            //    data.AddBarterable<FiefBarterable>(peace);

            try
            {

                Hero mainHero = Hero.MainHero;
                Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
                PartyBase mainParty = PartyBase.MainParty;
                MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;
                PartyBase otherParty = (partyBelongedTo != null) ? partyBelongedTo.Party : null;
                Hero beneficiaryOfOtherHero = null;
                BarterManager.BarterContextInitializer contextInitializer = new BarterManager.BarterContextInitializer(BarterManager.Instance.InitializeMakePeaceBarterContext);
                Barterable[] array = new Barterable[1];;
                MobileParty conversationParty = MobileParty.ConversationParty;
                array[0] = new TruceBarterable(oneToOneConversationHero, otherParty,mainHero.MapFaction as Kingdom,otherParty.MapFaction as Kingdom,this.hub);
                BarterManager.Instance.StartBarterOffer(mainHero, oneToOneConversationHero, otherParty, mainParty, beneficiaryOfOtherHero, contextInitializer, 0, false, array);
            }
            catch (Exception e)
            {
                this.logger.logError("DiplomacyReworkedMenuBehaviour", "selectTruceActionDelegate", e.StackTrace, null, e);
            }
        }
        private bool KingdomDisplayDelegate(MenuCallbackArgs args)
        {
            Kingdom found = null;

            foreach (Kingdom kingdom in Campaign.Current.Kingdoms)
            {
                if (kingdom.Name.ToString() == args.Text.ToString())
                {
                    found = kingdom;
                    break;
                }
            }

            return found.MapFaction != Hero.MainHero.MapFaction;
        }


        private void selectActionPeaceConsequence(MenuCallbackArgs args)
        {
            if (!this.currentSelectedFaction.Leader.IsDead || !this.currentSelectedFaction.Leader.IsPrisoner)
            {
                this.startConversation(args);
            }
            else
            {
                DataHub.DisplayInfoMsg("This Factions Leader is currently either dead, or someone took him prisoner");
            }
        }

        private bool selectActionPeaceCondition(MenuCallbackArgs args)
        {
            return !this.hub.isPlayerOnCooldown(this.currentSelectedFaction);
        }

        private bool selectActionWarCondition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction);
        }

        private void selectActionWarConsequence(MenuCallbackArgs args)
        {
            int trucetime = this.hub.isPlayerOnTruce(this.currentSelectedFaction);
            if (trucetime >= 0)
            {
                if(trucetime > 1)
                {
                    DataHub.DisplayInfoMsg(this.hub.getStandard("still_at_truce_multiday_1")+ " " +trucetime.ToString()+" " + this.hub.getStandard("still_at_truce_multiday_2"));
                }
                else
                {
                    DataHub.DisplayInfoMsg(this.hub.getStandard("still_at_truce_singleday"));
                }
                return;
            }

            if (!Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction))
            {
                DeclareWarAction.Apply(Hero.MainHero.MapFaction, this.currentSelectedFaction);
                Campaign.Current.GameMenuManager.MenuLocations.Clear();
                GameMenu.SwitchToMenu(DataHub.MENU_FACTION_DIPLOMACY_ID);
            }
            else
            {
                DisplayInfoMsg(this.hub.getError("already_war"));
            }
        }

        // Debug method to attempt to do a barter with an enemy kingdom
        private void startConversation(MenuCallbackArgs args)
        {
            Hero owner = Hero.MainHero;
            Hero other = this.currentSelectedFaction.Leader;


            startConversation(owner, other);
        }

        private void printAllParties(Hero hero)
        {
            foreach (PartyBase party in hero.OwnedParties)
            {
                DisplayInfoMsg(party.Name.ToString());
            }
        }

        private void selectActionQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.MENU_ID);
        }

        public static void selectMenuQuitConsequence(MenuCallbackArgs args)
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
        private void selectMenuConsequence(MenuCallbackArgs args)
        {
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.Name.ToString() == args.Text.ToString())
                {
                    this.currentSelectedFaction = faction;
                    GameMenu.SwitchToMenu(DataHub.MENU_FACTION_DIPLOMACY_ID);
                    DisplayInfoMsg(faction.Name.ToString());
                    break;
                }
            }
            if (this.currentSelectedFaction.Name.ToString() == "")
            {
                DisplayInfoMsg(this.hub.getError("select_faction_failed"));
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


        private bool alwaystrueDelegate(MenuCallbackArgs args)
        {
            return true;
        }

        private void MenuOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
        }

        private void menuConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.MENU_ID);
        }

        private bool menuCondition(MenuCallbackArgs args)
        {
            return Settlement.CurrentSettlement.MapFaction == Hero.MainHero.MapFaction && Hero.MainHero.IsFactionLeader;
        }

        private static void DisplayInfoMsg(string msg)
        {
            InformationManager.DisplayMessage(new InformationMessage(msg));
        }

        public static void startConversation(Hero player, Hero other)
        {

            try
            {
                Campaign.Current.CampaignMissionManager.OpenConversationMission(new ConversationCharacterData(player.CharacterObject), new ConversationCharacterData(other.CharacterObject));
            }
            catch (Exception e)
            {
                Dictionary<String, Object> values = new Dictionary<string, object>();
                values.Add("player", player.Name.ToString());
                if (other != null)
                {
                    values.Add("Other", other.Name);
                    values.Add("OtherIsPrisoner", other.IsPrisoner);
                    values.Add("OtherIsDead", other.IsDead);
                    values.Add("OtherIsOccupied", other.IsOccupiedByAnEvent());
                }
                else
                {
                    values.Add("Other", "Isnull");
                }
                BasicLoggingUtil logger = new BasicLoggingUtil();
                logger.logError("DiplomacyReworkedMenuBehaviour", "startConversation", e.StackTrace, values, e);
                DataHub.DisplayInfoMsg("An Error occurred when initializing the conversation");
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
