using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace DiplomacyReworked
{
    class TruceBarterable : Barterable
    {
        DataHub hub;
        Kingdom givingFaction;
        Kingdom receivingFaction;
        public TruceBarterable(Hero origOwner, PartyBase origParty, Kingdom givingFaction, Kingdom receivingFaction, DataHub toApplyTo) : base(origOwner, origParty)
        {
            this.hub = toApplyTo;
            this.givingFaction = givingFaction;
            this.receivingFaction = receivingFaction;
        }

        public override string StringID => "truce_Barterable";

        public override int MaxAmount => 31;

        public override TextObject Name => new TextObject(this.hub.getStandard("truce"));

        public override void Apply()
        {
            if (givingFaction.IsAtWarWith(receivingFaction))
            {
                PeaceBarterable peace = new PeaceBarterable(givingFaction.Leader, receivingFaction.Leader, givingFaction.Leader.PartyBelongedTo.Party, receivingFaction, CampaignTime.Days(this.CurrentAmount));
                peace.Apply();
            }

            if (this.CurrentAmount > 0)
            {
                this.hub.addNewTruce(new WarCooldownDataModell(givingFaction, receivingFaction, this.CurrentAmount));
            }

        }

        public override int GetUnitValueForFaction(IFaction faction)
        {
            try
            {
                int value = -10000;
                float fullStrengthPlayer = 0.0f;
                DefaultDiplomacyModel model = new DefaultDiplomacyModel();
                foreach (Clan clan in (Hero.MainHero.MapFaction as Kingdom).Clans)
                {
                    fullStrengthPlayer += model.GetClanStrength(clan);
                }
                float fullStrengthOther = 0.0f;
                foreach (Clan clan in (faction.MapFaction as Kingdom).Clans)
                {
                    fullStrengthOther += model.GetClanStrength(clan);
                }

                value += Convert.ToInt32((fullStrengthPlayer - fullStrengthOther) * 0.04);
                /*
                if (Hero.MainHero.MapFaction.IsAtWarWith(faction))
                {
                    value += Convert.ToInt32(model.GetScoreOfDeclaringPeace(Hero.MainHero.MapFaction, faction));
                }*/

                return value;
            }
            catch (Exception e)
            {
                return -30000;
            }
        }


        public override ImageIdentifier GetVisualIdentifier()
        {
            return null;
        }
    }
}
