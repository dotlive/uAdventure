﻿using System;
using System.Collections;
using uAdventure.Core;
using UnityEngine;
using Action = System.Action;

namespace uAdventure.Runner
{
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private Material transitionMaterial;
        private Texture transitionTexture;
        private RenderTexture renderTexture;
        private bool transitioning;
        private Transition transition;

        void Awake()
        {
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            renderTexture.Create();
        }

        public void UseMaterial(Material material)
        {
            this.transitionMaterial = material;
        }

        public void PrepareTransition(Transition transition)
        {
            CreateTransitionTexture();
            PrepareTransition(transition, renderTexture);
        }

        public void PrepareTransition(Transition transition, Texture transitionTexture)
        {
            this.transitionTexture = transitionTexture;
            this.transition = transition;
            this.transitioning = true;
        }

        public void DoTransition(Action<Transition, Texture> onFinish)
        {
            StartCoroutine(TransitionRoutine(transition, onFinish));
        }

        private void ResetMaterial()
        {
            transitionMaterial.SetFloat("_DirectionX", 0);
            transitionMaterial.SetFloat("_DirectionY", 0);
            transitionMaterial.SetFloat("_Progress", 0);
            transitionMaterial.SetFloat("_Blend", 0);
        }

        private IEnumerator TransitionRoutine(Transition transition, Action<Transition, Texture> onFinish)
        {
            transitioning = true;
            var timeLeft = transition.getTime()/1000f;
            var totalTime = timeLeft;
            var fade = false;
            ResetMaterial();
            transitionMaterial.SetTexture("_TransitionTex", transitionTexture);

            switch ((NextSceneEnumTransitionType) transition.getType())
            {
                case NextSceneEnumTransitionType.NO_TRANSITION:
                    FinalizeTransition(transition, transitionTexture, onFinish);
                    yield break;
                case NextSceneEnumTransitionType.FADE_IN:
                    fade = true;
                    break;
                case NextSceneEnumTransitionType.BOTTOM_TO_TOP:
                    transitionMaterial.SetFloat("_DirectionY", -1);
                    break;
                case NextSceneEnumTransitionType.TOP_TO_BOTTOM:
                    transitionMaterial.SetFloat("_DirectionY", 1);
                    break;
                case NextSceneEnumTransitionType.RIGHT_TO_LEFT:
                    transitionMaterial.SetFloat("_DirectionX", -1);
                    break;
                case NextSceneEnumTransitionType.LEFT_TO_RIGHT:
                    transitionMaterial.SetFloat("_DirectionX", 1);
                    break;
            }

            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                transitionMaterial.SetFloat(fade ? "_Blend" : "_Progress", Mathf.Clamp01(1 - (timeLeft / totalTime)));
                yield return null;
            }

            FinalizeTransition(transition, transitionTexture, onFinish);
        }

        private void FinalizeTransition(Transition transition, Texture transitionTexture, Action<Transition, Texture> onFinish)
        {
            transitioning = false;
            ResetMaterial();
            transitionMaterial.SetTexture("_MainTex", transitionTexture);
            transitionMaterial.SetTexture("_TransitionTex", null);
            onFinish(transition, transitionTexture);
        }


        private void CreateTransitionTexture()
        {
            var mainCamera = Camera.main;
            mainCamera.targetTexture = renderTexture;

            RenderTexture.active = renderTexture;
            mainCamera.Render();

            RenderTexture.active = null;
            mainCamera.targetTexture = null;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (transitioning)
            {
                transitionMaterial.SetTexture("_TransitionTex", source);
                Graphics.Blit(transitionTexture, destination, transitionMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }
    }
}