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
        /*   public override void OnCampaignStart(Game game, object starterObject)
           {
               AddBehaviors(game, (CampaignGameStarter)starterObject);
           }

           public override void OnGameLoaded(Game game, object initializerObject)
           {
               AddBehaviors(game, (CampaignGameStarter)initializerObject);
           }
           */
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            try
            {
                if(gameStarterObject is CampaignGameStarter)
                {
                    AddBehaviors(game, (CampaignGameStarter)gameStarterObject);
                }
            }
            catch (Exception e)
            {
                //not loading the mod should the selected mode not be a campaign
            }
        }

        private void AddBehaviors(Game game, CampaignGameStarter gameInitializer)
        {
            try
            {
                if (game.GameType is Campaign)
                {
                    gameInitializer.AddBehavior(new DiplomacyReworkedMenuBehaviour());
                }
            }
            catch (Exception e)
            {
                //not loading the mod should the Game not be a campaign
            }
        }

    }

}
