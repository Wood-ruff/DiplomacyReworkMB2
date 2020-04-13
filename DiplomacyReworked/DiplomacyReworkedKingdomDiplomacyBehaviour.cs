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

        //Initializers
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(
                 this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(
                this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
        }


        private void OnAfterNewGameCreated(CampaignGameStarter obj)
        {
            try
            {
                addGameMenus(obj);
            }
            catch (Exception e)
            {
                DataHub.DisplayInfoMsg("DiplomacyReworked:A critical error occurred when adding KingdomDiplomacy Behaviours");
            }
        }

        private void addGameMenus(CampaignGameStarter campaignGameStarter)
        {
            bool isLeave = false;
            bool isRepeatable = false;
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_TOWN_KEY, DataHub.INNER_DIPLOMACY_OPTION_ID, DataHub.MENU_INNER_DIPLOMACY_TEXT, innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, DataHub.MENU_TOWN_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_CASTLE_KEY, DataHub.INNER_DIPLOMACY_OPTION_ID, DataHub.MENU_INNER_DIPLOMACY_TEXT, innerDiplomacyCondition, innerDiplomacyConsequence, isLeave, DataHub.MENU_CASTLE_INSERT_INDEX + 1, isRepeatable);
            campaignGameStarter.AddGameMenu(DataHub.MENU_INNER_DIPLOMACY_ID, DataHub.MENU_INNER_DIPLOMACY_TITLE, DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);
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
            campaignGameStarter.AddGameMenuOption(DataHub.MENU_INNER_DIPLOMACY_ID, DataHub.MENU_INNER_DIPLOMACY_ID + "quit", "Return to Menu", DataHub.alwaysTrueDelegate, DiplomacyReworkedMenuBehaviour.selectMenuQuitConsequence, true, -1, isRepeatable);


            campaignGameStarter.AddGameMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_TITLE, DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "giftFief", "Gift this clan a Fief", giftFiefCondition, giftFiefConsequence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "conversation", "Enter Conversation with the Leader", talkToLeaderCondition, talkToLeaderConsquence, isLeave, -1, isRepeatable);
            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID, DataHub.INNER_DIPLOMACY_CLAN_MENU_ID + "quit", "Return to Clans", DataHub.alwaysTrueDelegate, innerDiplomacyClanQuitConsequence, true, -1, isRepeatable);

            campaignGameStarter.AddGameMenu(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, DataHub.INNER_DIPLOMACY_FIEF_MENU_TEXT, DataHub.MenuOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);

            foreach (Settlement settlement in Settlement.All)
            {
                if (settlement.IsCastle || settlement.IsTown)
                {

                    campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, DataHub.INNER_DIPLOMACY_FIEF_MENU_ID + settlement.Name.ToString(), settlement.Name.ToString(), isSettlementPlayerOwnedDelegate, this.settlementGiftedConsequence, isLeave, -1, isRepeatable);
                }
            }

            campaignGameStarter.AddGameMenuOption(DataHub.INNER_DIPLOMACY_FIEF_MENU_ID, DataHub.INNER_DIPLOMACY_FIEF_MENU_ID + "quit", "Return to Options", DataHub.alwaysTrueDelegate, innerDiplomacyFiefQuitConsequence, isLeave, -1, isRepeatable);
        }






        //Consequences
        private void innerDiplomacyFiefQuitConsequence(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu(DataHub.INNER_DIPLOMACY_CLAN_MENU_ID);
        }

        private void settlementGiftedConsequence(MenuCallbackArgs args)
        {
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
                if (found == null) { DataHub.DisplayInfoMsg("Error, could not find chosen fief"); return; }
                //FiefBarterable giftedFief = new FiefBarterable(found, found.OwnerClan.Leader, this.currentSelectedClan.Leader);


                FiefBarterable giftedFief = new FiefBarterable(found, found.OwnerClan.Leader, found.OwnerClan.Leader.OwnedParties.First(), this.currentSelectedClan.Leader);
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
                DataHub.DisplayInfoMsg("DiplomacyReworked:An Error occurred when gifting a Fief");
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
                DataHub.DisplayInfoMsg("Error:Could not find the Selected clan");
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
