using System;

using System.Linq;


using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;

using TaleWorlds.CampaignSystem.Election;


namespace DiplomacyReworked
{
    class DiplomacyReworkedMenuBehaviour : CampaignBehaviorBase
    {
        private const string LOGGING_PATH = "./DiplomacyReworkedLog.txt";
        //diplo
        private const int MENU_TOWN_INSERT_INDEX = 5;
        private const int MENU_CASTLE_INSERT_INDEX = 3;
        private const string MENU_TOWN_KEY = "town";
        private const string MENU_CASTLE_KEY = "castle";
        private const string MENU_ID = "diplomacy";
        private const string MENU_BUTTON_TITLE = "Diplomacy";
        private const string MENU_TEXT = "Select a Kingdom to interact";
        private const string MENU_FACTION_DIPLOMACY_ID = "faction_diplomacy";
        private const string MENU_FACTION_DIPLOMACY_TEXT = "What do you want to do?";


        //innerdiple
        private const string INNER_DIPLOMACY_OPTION_ID = "inner_diplomacy_option";
        private const string MENU_INNER_DIPLOMACY_ID = "inner_diplomacy";
        private const string MENU_INNER_DIPLOMACY_TEXT = "Kingdom Diplomacy";
        private const string MENU_INNER_DIPLOMACY_TITLE = "Select a Clan to interact";
        private const string INNER_DIPLOMACY_CLAN_MENU_ID = "inner_diplomacy_clan";
        private const string INNER_DIPLOMACY_CLAN_MENU_TITLE = "What do you want to do?";
        private const string INNER_DIPLOMACY_FIEF_MENU_ID = "inner_diplomacy_fiefs";
        private const string INNER_DIPLOMACY_FIEF_MENU_TEXT = "Which Fief do you want to gift?";


        private IFaction currentSelectedFaction = null;
        private Clan currentSelectedClan = null;


        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, onDecisionConcludedDelegate);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, settlementchangedOwnerDelegate);
        }

        private void settlementchangedOwnerDelegate(Settlement arg1, bool arg2, Hero arg3, Hero arg4, Hero arg5, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail arg6)
        {
            //if(Campaign.Current.GameMenuManager.GetMenuOverlayType(new MenuContext()))
        }

        private void onDecisionConcludedDelegate(KingdomDecision arg1, DecisionOutcome arg2, bool arg3)
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
                                /*if(Hero.MainHero.MapFaction.IsKingdomFaction)
                                {
                                    Kingdom kingdom = Hero.MainHero.MapFaction as Kingdom;
                                    kingdom.
                                }*/
                            }
                        }
                    }
                }
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
                logString(e.StackTrace);
                DisplayInfoMsg("DiplomacyReworked: Menus could not be added, an error occured when adding menus.");
            }
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {

            //Adding in menus and buttons for outer diplomacy
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(MENU_TOWN_KEY, MENU_ID + MENU_TOWN_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, MENU_TOWN_INSERT_INDEX, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_CASTLE_KEY, MENU_ID + MENU_CASTLE_KEY, MENU_BUTTON_TITLE, new GameMenuOption.OnConditionDelegate(menuCondition), new GameMenuOption.OnConsequenceDelegate(menuConsequence), isLeave, MENU_CASTLE_INSERT_INDEX, isRepeatable);

            campaignGameStarter.AddGameMenu(MENU_ID, MENU_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters);


            string factionName = "";
            int currentMaxIndex = 0;
            foreach (IFaction faction in Campaign.Current.Factions.ToList())
            {
                if (faction.IsKingdomFaction && faction != Hero.MainHero.MapFaction && faction.Name.ToString() != "Return")
                {
                    factionName = faction.Name.ToString();
                    campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + factionName, factionName, alwaystrueDelegate, selectMenuConsequence, isLeave, currentMaxIndex, isRepeatable);
                    currentMaxIndex++;
                }
            }
            campaignGameStarter.AddGameMenuOption(MENU_ID, MENU_ID + "quit", "Return to Menu", alwaystrueDelegate, selectMenuQuitConsequence, isLeave, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_TEXT, new OnInitDelegate(MenuOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "conversation", "Enter Conversation", selectActionPeaceCondition, selectActionPeaceConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "war", "Declare War", selectActionWarCondition, selectActionWarConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_FACTION_DIPLOMACY_ID, MENU_FACTION_DIPLOMACY_ID + "quit", "Return to Selection", alwaystrueDelegate, selectActionQuitConsequence, isLeave, -1, isRepeatable);





            //Adding in menus and buttons for inner diplomacy
            campaignGameStarter.AddGameMenuOption(MENU_TOWN_KEY, INNER_DIPLOMACY_OPTION_ID, MENU_INNER_DIPLOMACY_TEXT, innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, MENU_TOWN_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(MENU_CASTLE_KEY, INNER_DIPLOMACY_OPTION_ID, MENU_INNER_DIPLOMACY_TEXT, innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, MENU_CASTLE_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenu(MENU_INNER_DIPLOMACY_ID, MENU_INNER_DIPLOMACY_TITLE, MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithCharacters);
            if (Hero.MainHero.MapFaction is Kingdom)
            {
                foreach (Clan clan in (Hero.MainHero.MapFaction as Kingdom).Clans)
                {
                    if (clan.Name.ToString() != Hero.MainHero.Clan.Name.ToString())
                    {
                        campaignGameStarter.AddGameMenuOption(MENU_INNER_DIPLOMACY_ID, MENU_INNER_DIPLOMACY_ID + clan.Name.ToString(), clan.Name.ToString(), alwaystrueDelegate, innerDiplomacyClanConsequence, isLeave, -1, isRepeatable);
                    }
                }
            }
            campaignGameStarter.AddGameMenuOption(MENU_INNER_DIPLOMACY_ID, MENU_INNER_DIPLOMACY_ID + "quit", "Return to Menu", alwaystrueDelegate, selectMenuQuitConsequence, isLeave, -1, isRepeatable);


            campaignGameStarter.AddGameMenu(INNER_DIPLOMACY_CLAN_MENU_ID, INNER_DIPLOMACY_CLAN_MENU_TITLE, MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithCharacters);
            campaignGameStarter.AddGameMenuOption(INNER_DIPLOMACY_CLAN_MENU_ID, INNER_DIPLOMACY_CLAN_MENU_ID + "giftFief", "Gift this clan a Fief", giftFiefCondition, giftFiefConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(INNER_DIPLOMACY_CLAN_MENU_ID, INNER_DIPLOMACY_CLAN_MENU_ID + "conversation", "Enter Conversation with the Leader", talkToLeaderCondition, talkToLeaderConsquence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(INNER_DIPLOMACY_CLAN_MENU_ID, INNER_DIPLOMACY_CLAN_MENU_ID + "quit", "Return to Clans", alwaystrueDelegate, innerDiplomacyClanQuitConsequence, isLeave, -1, isRepeatable);


            campaignGameStarter.AddGameMenu(INNER_DIPLOMACY_FIEF_MENU_ID, INNER_DIPLOMACY_FIEF_MENU_TEXT, MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithCharacters);
            
            foreach(Settlement settlement in Settlement.All)
            {
                if(settlement.IsCastle || settlement.IsTown)
                {

                campaignGameStarter.AddGameMenuOption(INNER_DIPLOMACY_FIEF_MENU_ID, INNER_DIPLOMACY_FIEF_MENU_ID + settlement.Name.ToString(), settlement.Name.ToString(), isSettlementPlayerOwnedDelegate, settlementGiftedConsequence, isLeave, -1, isRepeatable);
                }
            }

            campaignGameStarter.AddGameMenuOption(INNER_DIPLOMACY_FIEF_MENU_ID, INNER_DIPLOMACY_FIEF_MENU_ID + "quit", "Return to Options", alwaystrueDelegate, innerDiplomacyFiefQuitConsequence, isLeave, -1, isRepeatable);
        }

        private void talkToLeaderConsquence(MenuCallbackArgs args)
        {
            Hero player = Hero.MainHero;
            Hero other = this.currentSelectedClan.Leader;


            startConversation(player, other);
        }

        private bool talkToLeaderCondition(MenuCallbackArgs args)
        {
            return this.currentSelectedClan.Leader.IsAlive && !this.currentSelectedClan.Leader.IsPrisoner;
        }

        private void innerDiplomacyClanQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(MENU_INNER_DIPLOMACY_ID);
        }

        private void settlementGiftedConsequence(MenuCallbackArgs args)
        {
            Settlement found = null;
            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.Name.ToString() == args.Text.ToString())
                {
                    found = settlement;
                    break;
                }
            }
            if (found == null) { DisplayInfoMsg("Error, could not find chosen fief"); return; }
            FiefBarterable giftedFief = new FiefBarterable(found,found.OwnerClan.Leader,found.OwnerClan.Leader.OwnedParties.First(),currentSelectedClan.Leader);
            giftedFief.Apply();
            if (found.IsCastle)
            {
                currentSelectedClan.Leader.SetPersonalRelation(Hero.MainHero,(int) currentSelectedClan.Leader.GetRelationWithPlayer() + 30);
            }
            else
            {
                currentSelectedClan.Leader.SetPersonalRelation(Hero.MainHero, (int)currentSelectedClan.Leader.GetRelationWithPlayer() + 50);
            }
            GameMenu.SwitchToMenu(INNER_DIPLOMACY_CLAN_MENU_ID);
        }

        private bool isSettlementPlayerOwnedDelegate(MenuCallbackArgs args)
        {
            Settlement found = null;
            foreach(Settlement settlement in Settlement.All)
            {
                if(settlement.Name.ToString() == args.Text.ToString())
                {
                    found = settlement;
                    break;
                } 
            }
            if (found == null) return false;
            return found.OwnerClan.Name.ToString() == Hero.MainHero.Clan.Name.ToString();
        }


        private void innerDiplomacyFiefQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(INNER_DIPLOMACY_CLAN_MENU_ID);
        }

        private void giftFiefConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(INNER_DIPLOMACY_FIEF_MENU_ID);
        }

        private bool giftFiefCondition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.Clan.Settlements.IsEmpty();
        }

        private void innerDiplomacyClanConsequence(MenuCallbackArgs args)
        {
            foreach (Clan clan in (Hero.MainHero.MapFaction as Kingdom).Clans)
            {
                if (clan.Name.ToString() == args.Text.ToString())
                {
                    this.currentSelectedClan = clan;
                }
            }
            if(currentSelectedClan != null)
            {
                GameMenu.SwitchToMenu(INNER_DIPLOMACY_CLAN_MENU_ID);
            }
            else
            {
                DisplayInfoMsg("Could not find the Selected clan");
            }
        }

        private void innerDiplomacyConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(MENU_INNER_DIPLOMACY_ID);
        }

        private bool innerDiplomacyCondition(MenuCallbackArgs args)
        {
            return (Hero.MainHero.MapFaction != null && Hero.MainHero.MapFaction.IsKingdomFaction);
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
                GameMenu.SwitchToMenu(MENU_FACTION_DIPLOMACY_ID);
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



        private void logString(String log)
        {
            System.IO.File.WriteAllText(@LOGGING_PATH, log);

        }


        private void selectActionQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(MENU_ID);
        }

        private void selectMenuQuitConsequence(MenuCallbackArgs args)
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
                    GameMenu.SwitchToMenu(MENU_FACTION_DIPLOMACY_ID);
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

        private void startConversation(Hero player, Hero other)
        {

            try
            {
                Campaign.Current.CampaignMissionManager.OpenConversationMission(new ConversationCharacterData(player.CharacterObject), new ConversationCharacterData(other.CharacterObject));
            }
            catch (Exception e)
            {
                DisplayInfoMsg("An Error occurred when initializing the conversation");
                logString(e.ToString());
            }
        }
    }
}
