using System;
using System.Collections;
using System.Collections.Generic;
using Expedition0.Save;
using Expedition0.Visuals;
using Expedition0.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Expedition0.MainMenu.Credits
{
    public class CreditsSceneChanger: MonoBehaviour
    {
        [SerializeField] private VisualEffectsController vfxController;
        [SerializeField] private bool fadeInOnStart = true;
        [SerializeField] private bool fadeOutOnEnd = true;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;
        [SerializeField] private ProgressBasedConditionalResolver<string> sceneResolver;
        
        private Coroutine _fadeInCoroutine;
        private Coroutine _fadeOutCoroutine;
        private float _t = 0;
        
        private void Start()
        {
            if (vfxController)
            {
                vfxController.SetFade01(1f);
            }
        }

        private void Awake()
        {
            if (fadeInOnStart && vfxController != null)
            {
                _fadeInCoroutine = StartCoroutine(FadeIn());
            }
        }
        
        public void BeginFadeOut()
        {
            if (_fadeInCoroutine != null)
            {
                StopCoroutine(_fadeInCoroutine);
                _fadeInCoroutine = null;
            }

            if (vfxController != null)
            {
                vfxController.SetFade01(0f);
            }

            if (fadeOutOnEnd)
            {
                _t = 0;
                _fadeOutCoroutine = StartCoroutine(FadeOut());
            }
            else
            {
                LoadScene();
            }
        }

        private IEnumerator FadeIn()
        {
            while (_t < fadeInDuration)
            {
                vfxController.SetFade01(MathUtils.SinLerp(1f, 0f, _t / fadeInDuration));
                _t += Time.deltaTime;
                yield return null;
            }
            
            vfxController.SetFade01(0f);
        }
        
        private IEnumerator FadeOut()
        {
            while (_t < fadeOutDuration)
            {
                vfxController.SetFade01(MathUtils.SinLerp(0f, 1f, _t / fadeOutDuration));
                _t += Time.deltaTime;
                yield return null;
            }
            
            vfxController.SetFade01(1f);
            LoadScene();
        }

        private void LoadScene()
        {
            var scene = sceneResolver.ResolveForCurrentProgress();
            SceneManager.LoadScene(scene);
        }
    }
}