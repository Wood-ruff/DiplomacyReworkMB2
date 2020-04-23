using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;

namespace DiplomacyReworked
{
    [HarmonyPatch(typeof(PlayerTownVisitCampaignBehavior))]
    public class HostileActionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("game_menu_village_hostile_action_on_condition")]
        private static void PostfixActivate(PlayerTownVisitCampaignBehavior __instance,ref bool __result, MenuCallbackArgs args)
        {
            if(Globals.isPlayerOnTruce(Hero.MainHero.CurrentSettlement.MapFaction) != -1)
            { 
                __result = false;
            }
        }

    }
}
