using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.OptionsAPI;
using StickerTransition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerTransition
{
    public class StickerOptions : CustomOptionsCategory
    {
        public TextMeshProUGUI packName;
        public TextMeshProUGUI creditsText;
        public SoundObject applySoundObject;
        public int current;

        public override void Build()
        {
            if (StickerTransitioner.Instance.LoadedPacks.Count > 0)
            {
                applySoundObject = Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "NotebookCollect");

                packName = CreateText("SelectedPack", "N/A", new Vector3(0f, -30f, 0f), MTM101BaldAPI.UI.BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(260f, 20f), Color.black, false);
                creditsText = CreateText("CreditsText", "", new Vector3(0f, -60f, 0f), MTM101BaldAPI.UI.BaldiFonts.ComicSans12, TextAlignmentOptions.Center, new Vector2(260f, 200f), Color.gray, false);

                CreateApplyButton(() => {
                    ApplyConfig();
                    SoundObject obj = applySoundObject;
                    optionsMenu.GetComponent<AudioManager>().PlaySingle(obj);
                });

                CreateButton(() => { if (current > 0) current--; UpdateChoice(); }, menuArrowLeft, menuArrowLeftHighlight, "PreviousPage", new Vector3(-120f, -30f, 0f));
                CreateButton(() => { if (current < StickerTransitioner.Instance.LoadedPacks.Count - 1) current++; UpdateChoice(); }, menuArrowRight, menuArrowRightHighlight, "Next", new Vector3(120f, -30f, 0f));

                SetDefaultIndex();
            }
            else
            {
                TextMeshProUGUI NoEditText = CreateText("NoPacks", "You have no packs installed!\n\nInstall or create\na pack first.", new Vector3(0f, -30f, 0f), MTM101BaldAPI.UI.BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(200f, 20f), Color.red, false);
            }
        }

        public void SetDefaultIndex()
        {
            if (StickerTransitioner.Instance.LoadedPacks.Keys.ToList().IndexOf(StickerTransitionPlugin.Instance.savedChoice.Value) == -1)
            {
                current = 0;
                UpdateChoice();
                return;
            }
            current = StickerTransitioner.Instance.LoadedPacks.Keys.ToList().IndexOf(StickerTransitionPlugin.Instance.savedChoice.Value);
            UpdateChoice();
            return;
        }

        public void UpdateChoice()
        {
            //set data
            packName.text = StickerTransitioner.Instance.LoadedPacks.Values.ToArray()[current].name;
            creditsText.text = StickerTransitioner.Instance.LoadedPacks.Values.ToArray()[current].artists[0];
        }

        void ApplyConfig()
        {
            StickerTransitioner.Instance.SwitchSet(StickerTransitioner.Instance.LoadedPacks.Values.ToArray()[current]);
            StickerTransitionPlugin.Instance.savedChoice.Value = StickerTransitioner.Instance.CurrentPack.name;
        }
    }
}