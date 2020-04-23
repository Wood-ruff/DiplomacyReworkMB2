using HarmonyLib;
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

        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("bannerlord.nightmaremen.diplomacyreworked");
            harmony.PatchAll();
        }

        private void AddBehaviours(Game game, CampaignGameStarter gameInitializer)
        {
            DiplomacyReworkedInitializer init = new DiplomacyReworkedInitializer();
            try
            {
                if (game.GameType is Campaign)
                {

                    gameInitializer.AddBehavior(init.menuBehaviour);
                    gameInitializer.AddBehavior(init.kingdomDiplomacyBehaviour);
                    gameInitializer.AddBehavior(init.keepFiefBehaviour);
                    gameInitializer.AddBehavior(init.cdManager);
                    gameInitializer.AddBehavior(init.dataHub);
                    gameInitializer.AddBehavior(init.deccingBehaviour);
                }
            }
            catch (Exception e)
            {
                //not loading the mod should the Game not be a campaign
                DataHub.DisplayInfoMsg("DiplomacyReworked: An Error occurred, when trying to load the mod into your current game.");
                init.logger.logError("----", "-----", e.StackTrace, null, e);
            }
        }

    }

}
