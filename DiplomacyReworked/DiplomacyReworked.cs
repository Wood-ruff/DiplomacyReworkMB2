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
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            try
            {
                if(gameStarterObject is CampaignGameStarter)
                {
                    AddBehaviours(game, (CampaignGameStarter)gameStarterObject);
                }
            }
            catch (Exception e)
            {
                //not loading the mod should the selected mode not be a campaign
                DataHub.DisplayInfoMsg("DiplomacyReworked: An Error occurred, when trying to load the mod into your current game.");
            }
        }

        private void AddBehaviours(Game game, CampaignGameStarter gameInitializer)
        {
            try
            {
                if (game.GameType is Campaign)
                {

                    DiplomacyReworkedInitializer init = new DiplomacyReworkedInitializer();
                    gameInitializer.AddBehavior(init.menuBehaviour);
                    gameInitializer.AddBehavior(init.kingdomDiplomacyBehaviour);
                    gameInitializer.AddBehavior(init.keepFiefBehaviour);
                    gameInitializer.AddBehavior(init.cdManager);
                }
            }
            catch (Exception e)
            {
                //not loading the mod should the Game not be a campaign
                DataHub.DisplayInfoMsg("DiplomacyReworked: An Error occurred, when trying to load the mod into your current game.");
            }
        }

    }

}
