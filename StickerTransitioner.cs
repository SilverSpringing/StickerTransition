using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StickerTransition
{
    public class StickerTransitioner : Singleton<StickerTransitioner>
    {
        const int ROOT_X = -320;
        const int ROOT_Y = -180;
        const int SCREEN_RESOLUTION_X = 640;
        const int SCREEN_RESOLUTION_Y = 360;
        const float IN_TIME = 0.5f;
        const float OUT_TIME = 0.5f;

        int clampedMaxX = 100;
        int clampedMaxY = 50;
        bool busy;

        public TransitionStickerSetData CurrentPack { get; private set; }
        public Dictionary<string, TransitionStickerSetData> LoadedPacks = new Dictionary<string, TransitionStickerSetData>();

        public event StickerTransitioner.OnTransitionMidpointReached TransitionMidpointReached;
        public delegate void OnTransitionMidpointReached();
        public delegate void OnTransitionComplete();

        AudioSource stickerSfx;
        List<AudioClip> stickerSounds = new List<AudioClip>();
        List<Image> stickerImages = new List<Image>();
        List<Vector2> storedStickerPositions = new List<Vector2>();
        List<Vector2> stickerPositions = new List<Vector2>();
        Canvas canvas;


        WeightedStickerData[] currentStickers;

        void Start()
        {
            stickerSfx = this.gameObject.AddComponent<AudioSource>();
            stickerSfx.ignoreListenerPause = true;

            canvas = UIHelpers.CreateBlankUIScreen("StickerCanvas", true);
            canvas.transform.parent = this.transform;
            DontDestroyOnLoad(canvas);

            foreach (string path in Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(StickerTransitionPlugin.Instance))))
            {
                stickerSounds.Add(AssetLoader.AudioClipFromFile(path));
            }
        }

        public void SwitchSet(TransitionStickerSetData newSet)
        {
            CurrentPack = newSet;
            currentStickers = CurrentPack.GetWeightedStickers().ToArray();
            clampedMaxX = Mathf.Clamp(CurrentPack.stickerCount[0], 1, 50);
            clampedMaxY = Mathf.Clamp((int)CurrentPack.stickerCount[1], 1, 10);
            GeneratePositions();
        }

        void CreateSticker(int slot)
        {
            TransitionStickerData stickerSelection = WeightedStickerData.RandomSelection(currentStickers);
            SelectedStickerData stickerData = WeightedStickerSelection.RandomSelection(stickerSelection.weightedVariants.ToArray()); 
            Image newSticker = UIHelpers.CreateImage(stickerData.sprite, canvas.transform, new Vector3(stickerPositions[slot].x, stickerPositions[slot].y, 0), false);
            newSticker.transform.transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(CurrentPack.angleRange[0], CurrentPack.angleRange[1]));
            newSticker.transform.transform.localScale = Vector3.one * CurrentPack.scale;
            stickerImages.Add(newSticker);
        }

        public void StartTransition(float time)
        {
            if (!busy)
            {
                if (CursorController.Instance) CursorController.Instance.DisableClick(true);
                StartCoroutine(TransitionEnumerator(time));
                busy = true;
            }
        }

        public void GeneratePositions()
        {
            int offset_x = (SCREEN_RESOLUTION_X / clampedMaxX) / 2;
            int offset_y = (SCREEN_RESOLUTION_Y / clampedMaxY) / 2;
            storedStickerPositions.Clear();

            for (int y = 0; y < clampedMaxY; y++)
            {
                for (int x = 0; x < clampedMaxX; x++)
                {
                    storedStickerPositions.Add(new Vector2(ROOT_X + ((SCREEN_RESOLUTION_X / clampedMaxX) * x) + offset_x, ROOT_Y + ((SCREEN_RESOLUTION_Y / clampedMaxY) * y) + offset_y));
                }
            }
        }

        IEnumerator TransitionEnumerator(float waitTime)
        {
            stickerPositions = new List<Vector2>(storedStickerPositions);
            yield return StartCoroutine(In());
            yield return new WaitForSecondsRealtime(waitTime);
            if (TransitionMidpointReached != null) this.TransitionMidpointReached();
            while (AdditiveSceneManager.Instance.Busy)
            {
                yield return null;
            }
            TransitionMidpointReached = null;
            yield return StartCoroutine(Out());
            busy = false;
            yield break;
        }

        public void ExitTransition()
        {
            StartCoroutine(TransitionOut());
            if (CursorController.Instance) CursorController.Instance.DisableClick(false);
        }

        IEnumerator TransitionOut()
        {
            yield return StartCoroutine(Out());
        }

        IEnumerator In()
        {
            while (stickerImages.Count < (clampedMaxX * clampedMaxY))
            {
                Vector2 chosenPosition = stickerPositions[UnityEngine.Random.Range(0, stickerPositions.Count)];
                CreateSticker(stickerPositions.IndexOf(chosenPosition));
                stickerPositions.Remove(chosenPosition);
                stickerSfx.PlayOneShot(stickerSounds[UnityEngine.Random.Range(0, stickerSounds.Count)]);
                yield return new WaitForSecondsRealtime(IN_TIME / (clampedMaxX * clampedMaxY));
            }
            yield break;
        }

        IEnumerator Out() 
        {
            while (stickerImages.Count > 0)
            {
                GameObject.Destroy(stickerImages.First().gameObject);
                stickerImages.RemoveAt(0);
                stickerSfx.PlayOneShot(stickerSounds[UnityEngine.Random.Range(0, stickerSounds.Count)]);
                yield return new WaitForSecondsRealtime(OUT_TIME / (clampedMaxX * clampedMaxY));
            }
            yield break;
        }
    }
}
