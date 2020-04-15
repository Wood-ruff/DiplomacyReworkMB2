using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;

namespace DiplomacyReworked
{
    class DiplomacyReworkedCoolDownManagerBehaviour : CampaignBehaviorBase
    {
        int cooldown;
        DataHub hub;
        BasicLoggingUtil logger;

        public DiplomacyReworkedCoolDownManagerBehaviour(BasicLoggingUtil logger, DataHub hub)
        {
            this.hub = hub;
            this.logger = logger;
            this.cooldown = SettingsReader.getWarCoolDown();
        }

        public override void RegisterEvents()
        {
            if(this.cooldown > 0)
            {
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, onWarDeclaredDelegate);
                CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, dayPassedDelegate);
            }
        }

        private void dayPassedDelegate()
        {
            this.hub.passDayCoolDowns();
        }

        private void onWarDeclaredDelegate(IFaction arg1, IFaction arg2)
        {
            this.hub.newWarDeclared(new WarCooldownDataModell(arg1, arg2, this.cooldown));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }

    class WarCooldownDataModell
    {
        public WarCooldownDataModell(IFaction faction1,IFaction faction2,int totalDays)
        {
            this.faction1 = faction1;
            this.faction2 = faction2;
            this.remainingDays = totalDays;
            if (Hero.MainHero.MapFaction.IsKingdomFaction)
            {
                if(Hero.MainHero.MapFaction == faction1 || Hero.MainHero.MapFaction == faction2)
                {
                    
                    this.isPlayerRelated = true;
                }
                else
                {
                    this.isPlayerRelated = false;
                }
            }
            else
            {
                this.isPlayerRelated = false;
            }
        }

        public bool isPlayerRelated { get; }
        IFaction faction1 { get; }
        IFaction faction2 { get; }
        int remainingDays { get; set; }


        public bool dayPassed()
        {
            this.remainingDays -= 1;
            if(this.remainingDays <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool containsFaction(IFaction faction)
        {
            if(this.faction1 == faction || this.faction2 == faction)
            {
                return true;
            }
            return false;
        }
    }
}


