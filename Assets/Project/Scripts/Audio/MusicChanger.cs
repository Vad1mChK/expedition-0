using UnityEngine;
using Expedition0.Save;
using Expedition0.Save.Experimental;
using Expedition0.Save.Registries;

namespace Expedition0.Audio
{
    public class MusicChanger : MonoBehaviour
    {
        [Header("Resolution")]
        [SerializeField] private ProgressBasedConditionalResolver<string> musicResolver;
        
        [Header("Registry & Unlocking")]
        [SerializeField] private MusicRegistry musicRegistry;
        [SerializeField] private bool autoUnlockResolvedMusicGamewide = false;

        private static MusicTrackAsset _last;

        private void Start()
        {
            Reevaluate();
        }

        /// <summary>
        /// Resolves the correct track based on current playthrough progress.
        /// </summary>
        public void Reevaluate()
        {
            var bestIdentifier = musicResolver.Resolve();
            if (bestIdentifier == null) return;
            
            if (autoUnlockResolvedMusicGamewide)
            {
                UnlockMusicById(bestIdentifier);
            }
            
            Debug.Log($"[<b>MusicChanger</b>] Best music ID for this level: {bestIdentifier}");
            
            var bestMusic = musicRegistry.GetItem(bestIdentifier);
            if (bestMusic == null) return;
            
            SetMusicTrack(bestMusic);
        }

        public void SetMusicTrack(MusicTrackAsset trackAsset)
        {
            if (_last == trackAsset) return; 

            _last = trackAsset;
            Debug.Log($"[<b>MusicChanger</b>] Switching music to: {trackAsset.displayName}");
            
            if (MusicPlayer.Instance) 
                MusicPlayer.Instance.Play(trackAsset);
        }

        /// <summary>
        /// Explicitly unlocks a track in the Gamewide save file.
        /// </summary>
        public void UnlockMusicById(string musicId)
        {
            if (GamewideLifecycleManager.Instance != null)
            {
                GamewideLifecycleManager.Instance.UnlockMusic(musicId);
            }
        }

        // --- Playback Controls ---

        public void FadeOutMusic() => MusicPlayer.Instance?.FadeOutWithPause();
        public void FadeInMusic() => MusicPlayer.Instance?.FadeInWithResume();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetLast() => _last = null;
    }
}