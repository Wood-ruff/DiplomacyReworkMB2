using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace DiplomacyReworked
{
    public class CustomDeclareWarBarterable : Barterable
    {
        Kingdom target;
        DataHub hub;
        Hero conversationPartner;
        public CustomDeclareWarBarterable(Hero originalOwner, PartyBase originialParty, Kingdom targetKingdom, Hero conversationPartner, DataHub hub) : base(originalOwner, originialParty)
        {
            this.hub = hub;
            this.target = targetKingdom;
            this.conversationPartner = conversationPartner;
        }

        public override int MaxAmount => 1;

        public override string StringID => "Paid_War_Barterable";

        public override TextObject Name => new TextObject(this.hub.getStandard("paid_war_barterable_name") + " "+ target.Name.ToString());


        public override void Apply()
        {
            DeclareWarAction.Apply(conversationPartner.MapFaction, target);
        }

        public override int GetUnitValueForFaction(IFaction faction)
        {
            int value = -800000;
            try
            {
                value += Int32.Parse(Hero.OneToOneConversationHero.GetRelationWithPlayer().ToString()) * 2000;
                value -= Int32.Parse(Hero.OneToOneConversationHero.GetRelation(this.target.Leader).ToString()) * 2000;
                DefaultDiplomacyModel model = new DefaultDiplomacyModel();
                float fullStrengthPlayer = 0.0f;
                foreach (Clan clan in (Hero.MainHero.MapFaction as Kingdom).Clans)
                {
                    fullStrengthPlayer += model.GetClanStrength(clan);
                }
                float fullStrengthOther = 0.0f;
                foreach (Clan clan in (faction.MapFaction as Kingdom).Clans)
                {
                    fullStrengthOther += model.GetClanStrength(clan);
                }
                float fullStrengthTarget = 0.0f;
                foreach (Clan clan in (this.target as Kingdom).Clans)
                {
                    fullStrengthTarget += model.GetClanStrength(clan);
                }

                value += Convert.ToInt32((fullStrengthPlayer - fullStrengthOther) * 0.6);
                value -= Convert.ToInt32((fullStrengthTarget - fullStrengthOther) * 0.6);
            }
            catch (Exception e)
            {
                DataHub.DisplayInfoMsg(this.hub.getError("error_calculating_barter_value"));
                value = -800000;
            }
            return value;
        }

        public override ImageIdentifier GetVisualIdentifier()
        {
            return null;
        }
    }
}
