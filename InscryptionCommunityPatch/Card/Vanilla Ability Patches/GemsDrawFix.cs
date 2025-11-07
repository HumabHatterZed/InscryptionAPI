using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace InscryptionCommunityPatch.Card;

// Fixes the PackMule special ability so it works when used by the player
[HarmonyPatch(typeof(GemsDraw), nameof(GemsDraw.OnOtherCardResolve))]
internal class GemsDrawFix
{
    [HarmonyPrefix]
    private static bool FixGemsDraw(GemsDraw __instance, ref IEnumerator __result)
    {
        __result = BetterGemsDraw(__instance);
        return false;
    }

    public static IEnumerator BetterGemsDraw(GemsDraw __instance)
    {
        yield return __instance.PreSuccessfulTriggerSequence();
        Singleton<ViewManager>.Instance.SwitchToView(SaveManager.SaveFile.IsMagnificus ? View.WizardBattleSlots : View.Default);
        yield return new WaitForSeconds(0.1f);

        int numGems = Singleton<BoardManager>.Instance.PlayerSlotsCopy.Count(x => x.Card != null && x.Card.HasTrait(Trait.Gem));

        if (numGems == 0)
        {
            yield return new WaitForSeconds(0.1f);
            __instance.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.45f);
            yield break;
        }
        
        for (int i = 0; i < numGems; i++)
        {
            if (Singleton<CardDrawPiles3D>.Instance != null && Singleton<CardDrawPiles3D>.Instance.Pile != null) {
                Singleton<CardDrawPiles3D>.Instance.Pile.Draw();
                yield return Singleton<CardDrawPiles3D>.Instance.DrawCardFromDeck();
            }
            else
                yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck();
            
        }
        yield return __instance.LearnAbility(0.5f);
    }
}