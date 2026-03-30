using MTM101BaldAPI.AssetTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StickerTransition
{
    /// <summary>
    /// Contains the loaded texture, auto-generated sprite, and name of an individual transition sticker from a pack.
    /// </summary>
    public struct SelectedStickerData
    {
        public Texture2D? texture;
        public Sprite? sprite;
        public string name;
        public SelectedStickerData(string fullPath, string n)
        {
            name = n;
            texture = AssetLoader.TextureFromFile(fullPath);
            sprite = AssetLoader.SpriteFromTexture2D(texture, 1f);
        }
    }

    /// <summary>
    /// Contains loaded data for an individual transition sticker.
    /// </summary>
    public class TransitionStickerData
    {
        /// <summary>
        /// Enum of the character this transition sticker depicts.
        /// </summary>
        [JsonProperty("character")]
        public Character character { get; } = Character.Null;

        /// <summary>
        /// Name of the sticker.
        /// </summary>
        [JsonProperty("name")]
        public string name { get; } = "Sticker";

        /// <summary>
        /// Dictionary containing the filenames and weights of potential textures for this sticker.
        /// </summary>
        [JsonProperty("variants")]
        public Dictionary<string, int> variants { get; } = new Dictionary<string, int>();

        [JsonIgnore]
        public List<WeightedStickerSelection> weightedVariants = new List<WeightedStickerSelection>();
    }

    /// <summary>
    /// Contains loaded data for a transition sticker pack.
    /// </summary>
    public class TransitionStickerSetData
    {
        /// <summary>
        /// Version of the sticker set.
        /// </summary>
        [JsonProperty("version")]
        public string version { get; set; } = Plugin_Info.PLUGIN_VERSION;

        /// <summary>
        /// Human-readable name of the sticker set.
        /// </summary>
        [JsonProperty("name")]
        public string name { get; set; } = "Untitled Set";

        /// <summary>
        /// Base scale for each sticker.
        /// </summary>
        [JsonProperty("scale")]
        public float scale { get; set; } = 1.0f;

        /// <summary>
        /// Amount of stickers to place down for the transition.
        /// </summary>
        [JsonProperty("stickerCount")]
        public int[] stickerCount { get; set; } = new int[] { 15, 5 };

        /// <summary>
        /// Angle range which the stickers can be rotated between.
        /// </summary>
        [JsonProperty("angleRange")]
        public int[] angleRange { get; set; } = new int[] { -60, 70 };

        /// <summary>
        /// Artists who contributed to the sticker set.
        /// </summary>
        [JsonProperty("artists")]
        public string[] artists { get; set; } = new string[] { "Me" };

        /// <summary>
        /// Dictionary containing the filenames and weights of sticker data JSON files.
        /// </summary>
        [JsonProperty("stickers")]
        public Dictionary<string, int> stickers { get; set; } = new Dictionary<string, int>();

        [JsonIgnore]
        public string globalPath = "";

        /// <summary>
        /// Returns SelectedStickerData for every sticker in the pack, including variations.
        /// </summary>
        public SelectedStickerData[] GetStickers()
        {
            List<SelectedStickerData> selections = new List<SelectedStickerData>();
            foreach (KeyValuePair<string, int> stickerData in this.stickers)
            {
                string stickerString = File.ReadAllText(Path.Combine(this.globalPath, stickerData.Key));
                TransitionStickerData deserializedSticker = JsonConvert.DeserializeObject<TransitionStickerData>(stickerString);
                foreach (KeyValuePair<string, int> kvp in deserializedSticker.variants)
                {
                    selections.Add(new SelectedStickerData(Path.Combine(this.globalPath, kvp.Key), Path.GetFileNameWithoutExtension(kvp.Key)));
                }
            }
            return selections.ToArray();
        }

        /// <summary>
        /// Returns a list of WeightedStickerData objects for every sticker in the pack.
        /// </summary>
        public List<WeightedStickerData> GetWeightedStickers()
        {
            List<WeightedStickerData> weightedStickerDatas = new List<WeightedStickerData>();

            foreach (KeyValuePair<string, int> stickerData in this.stickers)
            {
                string stickerString = File.ReadAllText(Path.Combine(this.globalPath, stickerData.Key));
                TransitionStickerData deserializedSticker = JsonConvert.DeserializeObject<TransitionStickerData>(stickerString);
                foreach (KeyValuePair<string, int> kvp in deserializedSticker.variants)
                {
                    deserializedSticker.weightedVariants.Add(new WeightedStickerSelection()
                    {
                        selection = new SelectedStickerData(Path.Combine(this.globalPath, kvp.Key), Path.GetFileNameWithoutExtension(kvp.Key)), 
                        weight = kvp.Value
                    });
                }
                weightedStickerDatas.Add(new WeightedStickerData()
                {
                    selection = deserializedSticker,
                    weight = stickerData.Value
                });
            }
            return weightedStickerDatas;
        }
    }
    public class WeightedStickerSelection : WeightedSelection<SelectedStickerData> { }
    public class WeightedStickerData : WeightedSelection<TransitionStickerData> { }
}
