// Created by: Unknown
// Edited by: Bill D.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Skritty.Tools.Utilities
{
    [RequireComponent(typeof(Image))]
    public class FadeImageInOnEnable : MonoBehaviour
    {
        private Image image;

        public float fadeDuration;
        float startingAlpha;
        public bool FadeOnAwake;

        private void Awake()
        {
            image = GetComponent<Image>();
            startingAlpha = image.color.a;

            if (FadeOnAwake)
                OnEnable();
        }

        private void OnEnable()
        {
            KillAllTweens();
            image.DOFade(0, 0).SetId("forceZero");
            image.DOFade(startingAlpha, fadeDuration).SetId(gameObject.name + "fadeIn");
        }

        private void OnDisable()
        {
            if(!gameObject.scene.isLoaded)
                return;

            KillAllTweens();
            image.DOFade(0, fadeDuration).SetId(gameObject.name + "fadeOut");
        }

        public IEnumerator DisableWithFadeOut()
        {
            KillAllTweens();
            image.DOFade(0, fadeDuration).SetId(gameObject.name + "fadeOut");
            yield return new WaitForSeconds(fadeDuration + .1f);
            gameObject.SetActive(false);
        }

        public void KillAllTweens()
        {
            DOTween.Kill(gameObject.name + "forceZero");
            DOTween.Kill(gameObject.name + "fadeIn");
            DOTween.Kill(gameObject.name + "fadeOut");
        }
    }
}
