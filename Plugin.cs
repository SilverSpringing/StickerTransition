using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using System.IO.Compression;
using System.Collections.Generic;

namespace StickerTransition
{
    public class Plugin_Info
    {
        public const string PLUGIN_GUID = "silverspringing.stickertransition";
        public const string PLUGIN_NAME = "Sticker Transition";
        public const string PLUGIN_VERSION = "1.0.0.0";
        public const string EXTENSION = ".btsp";
    }

    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")] //Dev API
    [BepInPlugin(Plugin_Info.PLUGIN_GUID, Plugin_Info.PLUGIN_NAME, Plugin_Info.PLUGIN_VERSION)]
    public class StickerTransitionPlugin : BaseUnityPlugin
    {
        public static StickerTransitionPlugin Instance;
        string stickerPackPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "Transition Sticker Packs");
        public ConfigEntry<string> savedChoice;

        void Awake()
        {
            Harmony harmony = new Harmony(Plugin_Info.PLUGIN_GUID);
            harmony.PatchAllConditionals();
            Instance = this;

            GameObject stickerTransitionManager = new GameObject("StickerTransition");
            stickerTransitionManager.AddComponent<StickerTransitioner>();
            DontDestroyOnLoad(stickerTransitionManager);
            stickerTransitionManager.MarkAsNeverUnload();

            savedChoice = Config.Bind<string>("Save", "Saved Choice", "", "Stored index of the last transition sticker set used in-game. You shouldn't have to edit this.");

            LoadStickerPacks();

            if (StickerTransitioner.Instance.LoadedPacks.ContainsKey(savedChoice.Value)) StickerTransitioner.Instance.SwitchSet(StickerTransitioner.Instance.LoadedPacks[savedChoice.Value]);

            CustomOptionsCore.OnMenuInitialize += AddCategory;
        }

        void AddCategory(OptionsMenu __instance, CustomOptionsHandler handler)
        {
            if (Singleton<CoreGameManager>.Instance != null) return;
            handler.AddCategory<StickerOptions>("Sticker Transition");
        }

        void LoadStickerPacks()
        {
            if (Directory.Exists(stickerPackPath))
            {
                string[] folders = Directory.GetDirectories(stickerPackPath);
                string[] files = Directory.GetFiles(stickerPackPath, $"*{Plugin_Info.EXTENSION}", SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        DirectoryInfo dir = Directory.CreateDirectory(files[i].Replace($"{Plugin_Info.EXTENSION}", ""));
                        ZipFile.ExtractToDirectory(files[i], dir.FullName, true);
                        File.Delete(files[i]);
                    }
                }
                if (folders.Length > 0)
                {
                    for (int i = 0; i < folders.Length; i++)
                    {
                        TransitionStickerSetData newPack = TryParseStickerPack(folders[i]);
                        StickerTransitioner.Instance.LoadedPacks.Add(newPack.name, newPack);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(stickerPackPath);
                UnityEngine.Debug.LogWarning("Created a folder for sticker transition packs since one did not exist.");
            }
        }

        public TransitionStickerSetData TryParseStickerPack(string path)
        {
            string dataPath = Path.Combine(path, "data.json");
            TransitionStickerSetData newSet = JsonConvert.DeserializeObject<TransitionStickerSetData>(File.ReadAllText(dataPath));
            newSet.globalPath = path;
            return newSet;
        }
    }
}
