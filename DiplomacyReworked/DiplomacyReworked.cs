using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace DiplomacyReworked
{
    public class Main : MBSubModuleBase
    {
        public override void OnCampaignStart(Game game, object starterObject)
        {
            AddBehaviors(game, (CampaignGameStarter)starterObject);
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            AddBehaviors(game, (CampaignGameStarter)initializerObject);
        }

        private void AddBehaviors(Game game, CampaignGameStarter gameInitializer)
        {
            if (game.GameType is Campaign)
            {
                gameInitializer.AddBehavior(new DiplomacyReworkedMenuBehaviour());
            }
        }

    }

}
