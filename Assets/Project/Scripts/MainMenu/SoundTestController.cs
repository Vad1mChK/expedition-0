using System.Collections.Generic;
using UnityEngine;
using Expedition0.Save;
using Expedition0.Audio;
using TMPro;

namespace Expedition0.MainMenu
{
    public sealed class SoundTestController : MonoBehaviour
    {
        [Header("Tracks")]
        [SerializeField] private ProgressBasedConditionalMultiSelector<MusicTrackAsset> conditionalTracks;
        [SerializeField] private MusicTrackAsset defaultTrack;

        [Header("UI")]
        [SerializeField] private TMP_Text titleLabel;

        [Header("Options")]
        [SerializeField] private bool filterByProgress = true;

        private List<MusicTrackAsset> _tracks;
        private int _index;
        private MusicTrackAsset _beforeTest;

        // Call this from the panel's OnEnable event, or a menu button "Open Sound Test"
        public void Open()
        {
            _tracks = conditionalTracks.SelectForCurrentProgress();
            if (_tracks == null)
            {
                _tracks = new List<MusicTrackAsset>();
            }
            if (_tracks.Count == 0)
            {
                _tracks.Add(defaultTrack);
            }
            _index = Mathf.Clamp(_index, 0, Mathf.Max(0, _tracks.Count - 1));
            CacheCurrent();
            Show(_index);
            
            Debug.Log($"Sound Test: Built pool of size {_tracks.Count}");
        }

        // Call this from the panel's OnDisable event, or a "Back" button
        public void Close()
        {
            if (MusicPlayer.Instance && _beforeTest)
                MusicPlayer.Instance.Play(_beforeTest);
        }

        // — Buttons hook these directly —
        public void Prev()
        {
            if (_tracks.Count == 0) return;
            _index = (_index - 1 + _tracks.Count) % _tracks.Count;
            Show(_index);
        }
        
        public void PrevAndPlay()
        {
            Prev();
            Debug.Log($"Sound Test: Switched to music track '{_tracks[_index]}'");
            PlayCurrent();
        }
        
        public void Next()
        {
            if (_tracks.Count == 0) return;
            _index = (_index + 1) % _tracks.Count;
            Show(_index);
        }
        
        public void NextAndPlay()
        {
            Next();
            Debug.Log($"Sound Test: Switched to music track '{_tracks[_index]}'");
            PlayCurrent();
        }

        public void RandomPick()
        {
            if (_tracks.Count == 0) return;
            _index = UnityEngine.Random.Range(0, _tracks.Count);
            Show(_index);
        }

        public void PlayCurrent()
        {
            if (_tracks.Count == 0 || !MusicPlayer.Instance) return;
            MusicPlayer.Instance.Play(_tracks[_index]); // hard switch, no crossfade
        }

        public void StopPlayback()
        {
            if (MusicPlayer.Instance) MusicPlayer.Instance.Stop();
        }

        private void CacheCurrent()
        {
            /* if you track current music elsewhere, set _beforeTest here */
        }

        private void Show(int i)
        {
            if (!titleLabel) return;
            titleLabel.text = _tracks.Count > 0 ? _tracks[i].EffectiveName : "(no tracks)";
        }
    }
}