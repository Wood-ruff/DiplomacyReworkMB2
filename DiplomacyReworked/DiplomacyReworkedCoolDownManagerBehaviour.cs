using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using static DiplomacyReworked.DataHub;

namespace DiplomacyReworked
{
    class DiplomacyReworkedCoolDownManagerBehaviour : CampaignBehaviorBase
    {
        int cooldown;
        int truceDuration;
        DataHub hub;
        BasicLoggingUtil logger;

        public DiplomacyReworkedCoolDownManagerBehaviour(BasicLoggingUtil logger, DataHub hub)
        {
            this.hub = hub;
            this.logger = logger;
            this.cooldown = SettingsReader.getWarCoolDown();
            this.truceDuration = SettingsReader.getStandardTruce();
        }

        public override void RegisterEvents()
        {
            if (this.truceDuration > 0)
            {
                CampaignEvents.MakePeace.AddNonSerializedListener(this, onPeaceDeclaredDelegate);
            }
            if (this.cooldown > 0)
            {
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, onWarDeclaredDelegate);
            }
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, dayPassedDelegate);
        }

        private void onPeaceDeclaredDelegate(IFaction arg1, IFaction arg2)
        {
            this.hub.addNewTruce(new WarCooldownDataModell(arg1, arg2, 7));
        }

        private void dayPassedDelegate()
        {
            this.hub.passDayCoolDowns();
        }

        private void onWarDeclaredDelegate(IFaction arg1, IFaction arg2)
        {
            if (this.hub.isFactionOnTruce(arg1, arg2))
            {
                try
                {
                    CampaignTime duration = CampaignTime.Days(Campaign.Current.CampaignDt);
                    PeaceBarterable peace = new PeaceBarterable(arg1.Leader, arg2.Leader, arg1.Leader.OwnedParties.First(), arg2.MapFaction, duration);
                    peace.Apply();
                    DataHub.DisplayInfoMsg(this.hub.getStandard("war_declared_while_truce"));
                }
                catch (Exception e)
                {
                    this.logger.logError("DiplomacyReworkedMenuBehaviour", "onWarDeclareDelegate", e.StackTrace, null, e);
                }
            }
            else
            {
                this.hub.newWarDeclared(new WarCooldownDataModell(arg1, arg2, this.cooldown));
            }
        }

        public override void SyncData(IDataStore dataStore)
        {

        }
    }

    
}


