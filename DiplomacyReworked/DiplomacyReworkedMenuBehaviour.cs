using System;

using System.Linq;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem.Election;


namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    { 
        private IFaction currentSelectedFaction = null;
        public DataHub currentHub;


        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, onDecisionConcludedDelegate);
            

        }


        private void onDecisionConcludedDelegate(KingdomDecision arg1, DecisionOutcome arg2, bool arg3)
        {
            try
            {

                float mayority = 0;
                foreach (DecisionOutcome outcome in arg1.DetermineInitialCandidates().ToList())
                {
                    if (outcome.Support > mayority) mayority = outcome.Support;
                }

                if (arg2.Support >= mayority)
                {
                    if (arg1 is TaleWorlds.CampaignSystem.Election.SettlementClaimantDecision)
                    {
                        if (arg1.DetermineChooser() == Hero.MainHero.Clan)
                        {
                            foreach (Clan clan in arg1.Kingdom.Clans)
                            {
                                int change = (int)arg1.CalculateRelationshipEffectWithSponsor(clan);
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
                DisplayInfoMsg("An Error occurred when resetting your relationship");
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
                DataHub.logString(e.StackTrace);
                DisplayInfoMsg("DiplomacyReworked: Menus could not be added, an error occured when adding menus.");
            }
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            //Adding in menus and buttons for outer diplomacy
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_TOWN_KEY, DataHub.MENU_ID + DataHub.MENU_TOWN_KEY, DataHub.MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, DataHub.MENU_TOWN_INSERT_INDEX, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_CASTLE_KEY, DataHub.MENU_ID + DataHub.MENU_CASTLE_KEY, DataHub.MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, DataHub.MENU_CASTLE_INSERT_INDEX, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.MENU_ID, DataHub.MENU_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithBoth);
            string factionName = "";
            int currentMaxIndex = 0;
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.IsKingdomFaction && faction != Hero.MainHero.MapFaction && faction.Name.ToString() != "Return")
                {
                    factionName = faction.Name.ToString();
                    campaignGameStarter.AddGameMenuOption(DataHub.MENU_ID, DataHub.MENU_ID + factionName, factionName, KingdomDisplayDelegate, selectMenuConsequence, isLeave, currentMaxIndex, isRepeatable);
                    currentMaxIndex++;
                }
            }
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_ID, DataHub.MENU_ID + "quit", "Return to Menu", alwaystrueDelegate, selectMenuQuitConsequence, isLeave, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "conversation", "Enter Conversation", selectActionPeaceCondition, selectActionPeaceConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "war", "Declare War", selectActionWarCondition, selectActionWarConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_FACTION_DIPLOMACY_ID, DataHub.MENU_FACTION_DIPLOMACY_ID + "quit", "Return to Selection", alwaystrueDelegate, selectActionQuitConsequence, true, -1, isRepeatable);
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
                DisplayInfoMsg("This Factions Leader is currently either dead, or someone took him prisoner");
            }
        }

        private bool selectActionPeaceCondition(MenuCallbackArgs args)
        {
            return true;
        }

        private bool selectActionWarCondition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction);
        }

        private void selectActionWarConsequence(MenuCallbackArgs args)
        {
            if (!Hero.MainHero.MapFaction.IsAtWarWith(this.currentSelectedFaction))
            {
                DeclareWarAction.Apply(Hero.MainHero.MapFaction, this.currentSelectedFaction);
                Campaign.Current.GameMenuManager.MenuLocations.Clear();
                GameMenu.SwitchToMenu(DataHub.MENU_FACTION_DIPLOMACY_ID);
            }
            else
            {
                DisplayInfoMsg("You are already at war with this faction");
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


        private bool alwaystrueDelegate(MenuCallbackArgs args)
        {
            return true;
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
                DataHub.DisplayInfoMsg("An Error occurred when initializing the conversation");
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
