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
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem.Election;

namespace DiplomacyReworked
{
    class DiplomacyReworkedKingdomDiplomacyBehaviour : CampaignBehaviorBase
    {
        Clan currentSelectedClan = null;
        public DataHub hub;
        private BasicLoggingUtil logger;
        //Initializers
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
        }

        public DiplomacyReworkedKingdomDiplomacyBehaviour(BasicLoggingUtil logger)
        {
            this.logger = logger;
        }
       

        private void OnAfterNewGameCreated(CampaignGameStarter obj)
        {
            try
            {
                addGameMenus(obj);
            }
            catch (Exception e)
            {
                DataHub.DisplayInfoMsg(this.hub.getError("critical_load_diplomacy"));
                this.logger.logError("DiplomacyReworkedKingdomDiplomacyBehaviour", "OnAfterNewGameCreated", e.StackTrace, null);
            }
        }

        private void addGameMenus(CampaignGameStarter campaignGameStarter)
        {
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_TOWN_KEY, DataHub.INNER_DIPLOMACY_OPTION_ID, this.hub.getStandard("kingdom_diplomacy_title"), innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, DataHub.MENU_TOWN_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_CASTLE_KEY, DataHub.INNER_DIPLOMACY_OPTION_ID, this.hub.getStandard("kingdom_diplomacy_title"), innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, DataHub.MENU_CASTLE_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenu(DataHub.MENU_INNER_DIPLOMACY_ID, this.hub.getStandard("select_clan_text"), DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);
            if (Hero.MainHero.MapFaction is Kingdom)
            {
                foreach (Clan clan in Campaign.Current.Clans)
                {
                    if (clan.Name.ToString() != Hero.MainHero.Clan.Name.ToString())
                    {
                        campaignGameStarter.AddGameMenuOption(DataHub.MENU_INNER_DIPLOMACY_ID, DataHub.MENU_INNER_DIPLOMACY_ID + clan.Name.ToString(), clan.Name.ToString(), clanEnableDelegate, innerDiplomacyClanConsequence, isLeave, -1, isRepeatable);
                    }
                }
            }
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_INNER_DIPLOMACY_ID, DataHub.MENU_INNER_DIPLOMACY_ID + "quit", this.hub.getButton("return_menu"), DataHub.alwaysTrueDelegate, DiplomacyReworkedMenuBehaviour.selectMenuQuitConsequence, true, -1, isRepeatable);


            campaignGameStarter.AddGameMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, this.hub.getStandard("what_to_do"), DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "giftFief", this.hub.getButton("gift_fief_clan"), giftFiefCondition, giftFiefConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "conversation", this.hub.getButton("conversation_leader"), talkToLeaderCondition, talkToLeaderConsquence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "quit", this.hub.getButton("return_clans"), DataHub.alwaysTrueDelegate, innerDiplomacyClanQuitConsequence, true, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, this.hub.getStandard("which_fief_to_gift"), DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);

            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsCastle || settlement.IsTown)
                {

                    campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, DataHub.INNER_DIPLOMACY_FIEF_MENU_ID + settlement.Name.ToString(), settlement.Name.ToString(), isSettlementPlayerOwnedDelegate, this.settlementGiftedConsequence, isLeave, -1, isRepeatable);
                }
            }

            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, DataHub.INNER_DIPLOMACY_FIEF_MENU_ID + "quit", this.hub.getButton("return_options"), DataHub.alwaysTrueDelegate, innerDiplomacyFiefQuitConsequence, isLeave, -1, isRepeatable);
        }






        //Consequences
        private void innerDiplomacyFiefQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID);
        }

        private void settlementGiftedConsequence(MenuCallbackArgs args)
        {
            Dictionary<String, object> values = new Dictionary<string, object>();
            try
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
                if (found == null) { DataHub.DisplayInfoMsg(this.hub.getError("chosen_fief_not_found")); return; }
                values.Add("SettlementName", found.Name.ToString());
                values.Add("SettlementCastle", found.IsCastle);
                values.Add("SettlementCity", found.IsTown);

                values.Add("Owner", found.OwnerClan.Leader.Name);
                values.Add("Receiver", this.currentSelectedClan.Leader.Name);
                FiefBarterable giftedFief = new FiefBarterable(found, found.OwnerClan.Leader, this.currentSelectedClan.Leader);
                //FiefBarterable giftedFief = new FiefBarterable(found, found.OwnerClan.Leader, found.OwnerClan.Leader.OwnedParties.First(), this.currentSelectedClan.Leader);
                giftedFief.Apply();
                if (found.IsCastle)
                {
                    currentSelectedClan.Leader.SetPersonalRelation(Hero.MainHero, (int)currentSelectedClan.Leader.GetRelationWithPlayer() + 30);
                }
                else
                {
                    currentSelectedClan.Leader.SetPersonalRelation(Hero.MainHero, (int)currentSelectedClan.Leader.GetRelationWithPlayer() + 50);
                }
                GameMenu.SwitchToMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID);
            }
            catch (Exception e)
            {
                this.logger.logError("DiplomacyReworkedKingdomDiplomacyBehaviour", "settlementGiftedConsequence", e.StackTrace, values);
                DataHub.DisplayInfoMsg(this.hub.getError("gift_fief_failed"));
            }
        }

        private void innerDiplomacyClanQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.MENU_INNER_DIPLOMACY_ID);
        }

        private void giftFiefConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID);
        }

        private void talkToLeaderConsquence(MenuCallbackArgs args)
        {
            Hero player = Hero.MainHero;
            Hero other = this.currentSelectedClan.Leader;


            DiplomacyReworkedMenuBehaviour.startConversation(player, other);
        }

        private void innerDiplomacyConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.MENU_INNER_DIPLOMACY_ID);
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
            if (currentSelectedClan != null)
            {
                GameMenu.SwitchToMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID);
            }
            else
            {
                DataHub.DisplayInfoMsg(this.hub.getError("selectedClan_not_found"));
            }
        }

        //Conditions

        private bool innerDiplomacyCondition(MenuCallbackArgs args)
        {
            return (Hero.MainHero.MapFaction != null && Hero.MainHero.MapFaction.IsKingdomFaction);
        }

        private bool giftFiefCondition(MenuCallbackArgs args)
        {
            return !Hero.MainHero.Clan.Settlements.IsEmpty();
        }

        private bool talkToLeaderCondition(MenuCallbackArgs args)
        {
            return this.currentSelectedClan.Leader.IsAlive && !this.currentSelectedClan.Leader.IsPrisoner;
        }

        private bool clanEnableDelegate(MenuCallbackArgs args)
        {
            Clan found = null;

            foreach (Clan clan in Campaign.Current.Clans)
            {
                if (Hero.MainHero.MapFaction.IsKingdomFaction)
                {
                    if (clan.Name.ToString() == args.Text.ToString())
                    {
                        found = clan;
                        break;
                    }
                }
            }


            return found.MapFaction == Hero.MainHero.MapFaction;
        }

        private bool isSettlementPlayerOwnedDelegate(MenuCallbackArgs args)
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
            if (found == null) return false;
            return found.OwnerClan.Name.ToString() == Hero.MainHero.Clan.Name.ToString();
        }

        //unused overrides
        public override void SyncData(IDataStore dataStore)
        {
        }

    }
}
