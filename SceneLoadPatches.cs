using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StickerTransition
{
    [HarmonyPatch(typeof(WarningScreen), "Advance")]
    internal class WarningScreenLoadPatch
    {
        private static bool Prefix(WarningScreen __instance)
        {
            if (StickerTransitioner.Instance.CurrentPack != null)
            {
                __instance.gameObject.GetComponent<AudioSource>().Stop();
                StickerTransitioner.Instance.TransitionMidpointReached += () => AdditiveSceneManager.Instance.LoadScene("MainMenu");
                StickerTransitioner.Instance.StartTransition(1f);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CoreGameManager), "Quit")]
    internal class QuitPatch
    {
        private static bool Prefix(CoreGameManager __instance)
        {
            if (StickerTransitioner.Instance.CurrentPack != null)
            {
                Singleton<GlobalCam>.Instance.SetListener(true);
                Singleton<SubtitleManager>.Instance.DestroyAll();
                StickerTransitioner.Instance.TransitionMidpointReached += () => CoreGameManager.Instance.ReturnToMenu();
                StickerTransitioner.Instance.StartTransition(0.25f);
                return false;
            }
            return true;
        }
    }
}
