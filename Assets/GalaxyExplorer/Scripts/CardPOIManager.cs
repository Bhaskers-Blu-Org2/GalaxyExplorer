﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using MRS.Audui;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GalaxyExplorer
{
    public class CardPOIManager : MonoBehaviour, IInputClickHandler, IControllerTouchpadHandler
    {
        [Header("Galaxy Card POI Fading")]
        [Tooltip("The time it takes for all points of interest to completely fade out when a card point of interest is selected.")]
        public float POIFadeOutTime = 1.0f;
        [Tooltip("How the opacity changes when all points of interest fade out when a card is selected.")]
        public AnimationCurve POIOpacityCurve;

        [Header("Galaxy Card Text Sliding")]
        [Tooltip("The time it takes for the card description to move from its unselected position to its selected position.")]
        public float DescriptionSlideOutTime = 1.0f;
        [Tooltip("The time it takes for the card description to move from its selected position to its unselected/highlight position.")]
        public float DescriptionSlideInTime = 0.5f;
        [Tooltip("The vector (local space) that descripts where the description card moves to when selected.")]
        public Vector3 DescriptionSlideDirection;
        [Tooltip("How the description card moves when it slides to selected and unselected positions.")]
        public AnimationCurve DescriptionSlideCurve;

        private List<PointOfInterest> allPOIs = new List<PointOfInterest>();
        private ToolManager toolsManager = null;
        private AuduiEventWrangler audioEventWrangler = null;
        private GEMouseInputSource mouseInput = null;
        private TransitionManager transitionManager = null;


        private void Start()
        {
            InputManager.Instance.AddGlobalListener(gameObject);

            toolsManager = FindObjectOfType<ToolManager>();
            if (toolsManager)
            {
                toolsManager.OnAboutSlateOnDelegate += OnAboutSlateOnDelegate;
            }

            InputRouter inputRouter = FindObjectOfType<InputRouter>();
            if (inputRouter)
            {
                inputRouter.OnKeyboadSelection += OnKeyboadSelection;
            }

            mouseInput = FindObjectOfType<GEMouseInputSource>();
            if (mouseInput)
            {
                mouseInput.OnMouseClickDelegate += OnMouseClickDelegate;
                mouseInput.OnMouseClickUpDelegate += OnMouseClickUpDelegate;
                mouseInput.OnMouseOnHoverDelegate += OnMouseOnHoverDelegate;
                mouseInput.OnMouseOnUnHoverDelegate += OnMouseOnUnHoverDelegate;
            }

            audioEventWrangler = FindObjectOfType<AuduiEventWrangler>();
            transitionManager = FindObjectOfType<TransitionManager>();
        }

        private void OnMouseOnUnHoverDelegate(GameObject selectedObject)
        {
            audioEventWrangler.OverrideFocusedObject(null);
        }

        private void OnMouseOnHoverDelegate(GameObject selectedObject)
        {
            if (selectedObject)
            {
                audioEventWrangler.OnFocusEnter(selectedObject);
            }
        }

        private void OnMouseClickUpDelegate(GameObject selectedObject)
        {
            audioEventWrangler.OverrideFocusedObject(null);
        }

        private void OnMouseClickDelegate(GameObject selectedObject)
        {
            if (selectedObject)
            {
                audioEventWrangler.OverrideFocusedObject(selectedObject);
            }
        }

        // Space bar was tapped callback
        // If any poi card is on then close it else if space tap was to select a poi, then select it and trigger audio
        private void OnKeyboadSelection()
        {
            bool isAnyCardActive = IsAnyCardActive();
            if (isAnyCardActive)
            {
                StartCoroutine(CloseAnyOpenCard(null));
                audioEventWrangler.OnInputClicked(null);
            }
            else
            {
                GameObject selected = mouseInput.FocusedObject;
                if (selected)
                {
                    IInputClickHandler handler = selected.GetComponentInParent<IInputClickHandler>();
                    handler?.OnInputClicked(null);

                    PointOfInterest poi = selected.GetComponentInParent<PointOfInterest>();
                    if (poi)
                    {
                        audioEventWrangler.OverrideFocusedObject(poi.IndicatorCollider.gameObject);
                    }

                    audioEventWrangler.OnInputClicked(null);
                }
            }

            StartCoroutine(UpdateActivationOfPOIColliders());
        }

        public void RegisterPOI(PointOfInterest poi)
        {
            allPOIs.Add(poi);
        }

        public void UnRegisterPOI(PointOfInterest poi)
        {
            allPOIs.Remove(poi);
        }

        // Find if a card POI is activa and its card is on/visible
        private bool IsAnyCardActive()
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    return true;
                }
            }

            return false;
        }

        // If a poi card is active then deactivate all poi colliders so user cant activate another one during card presentation
        // If no poi card is active then activate poi colliders
        private IEnumerator UpdateActivationOfPOIColliders()
        {
            yield return new WaitForEndOfFrame();

            if (transitionManager.InTransition)
            {
                yield break;
            }

            bool isAnyCardActive = IsAnyCardActive();
            if (isAnyCardActive)
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = false;
                    }
                }
            }
            else
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider.enabled == true)
                    {
                        yield break;
                    }

                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = true;
                    }
                }
            }
        }

        // Find if a card POI is active and its card is on/visible, close the card and trigger audio
        private IEnumerator CloseAnyOpenCard(InputEventData eventData)
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    // eventData needs to be used in case that we are clocing the card because we dont want this click to propagate into the focused handler
                    eventData?.Use();

                    CardPOI cardPoi = (CardPOI)poi;
                    audioEventWrangler.OverrideFocusedObject((cardPoi) ? cardPoi.GetCardObject.GetComponentInChildren<Collider>().gameObject : poi.IndicatorCollider.gameObject);

                    poi.OnInputClicked(null);
                    Debug.Log("Close card because of input");
                    break;
                }
            }

            yield return null;
        }

        // When a card poi is on, if About Slate gets activated through menu or desktop button then card poi need to be deactivated
        public void OnAboutSlateOnDelegate(bool enable)
        {
            if (enable)
            {
                // Find if a card POI is activa and its card is on/visible
                bool isAnyCardActive = false;
                foreach (var poi in allPOIs)
                {
                    if (poi.IsCardActive)
                    {
                        isAnyCardActive = true;
                        poi.OnInputUp(null);
                        break;
                    }
                }

                if (isAnyCardActive)
                {
                    foreach (var poi in allPOIs)
                    {
                        if (poi.IndicatorCollider)
                        {
                            poi.IndicatorCollider.enabled = true;
                        }
                    }
                }
            }
        }

        // Deactivate all pois that might have active card description except the one that is currently focused/touched
        // Note that the focused/touched object could be a planet and not its poi indicator
        private void DeactivateAllDescriptionsHandlers(GameObject focusedObject)
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IndicatorCollider.gameObject != focusedObject)
                {
                    PlanetPOI planetPOI = poi as PlanetPOI;
                    if (planetPOI)
                    {
                        // If planet sphere object is the focused object then dont unfocus from it
                        Planet planet = planetPOI.PlanetObject.GetComponentInChildren<Planet>();
                        if (planet && planet.gameObject == focusedObject)
                        {
                            continue;
                        }
                    }

                    poi.OnFocusExit();
                }
            }
        }

        public void OnTouchpadTouched(InputEventData eventData)
        {

        }

        public void OnTouchpadReleased(InputEventData eventData)
        {
            // GETouchScreenInputSource sets InputManager.Instance.OverrideFocusedObject on collider touch
            GameObject focusedObj = InputManager.Instance.OverrideFocusedObject; 
            DeactivateAllDescriptionsHandlers(focusedObj);

            bool isAnyCardActive = IsAnyCardActive();
            StartCoroutine(CloseAnyOpenCard(eventData));
            StartCoroutine(UpdateActivationOfPOIColliders());

            if (isAnyCardActive)
            {
                audioEventWrangler.OnInputClicked(null);
            }
        }

        public void OnInputPositionChanged(InputPositionEventData eventData)
        {

        }

        // OnInputClicked is triggered with airtap and mouse click
        public void OnInputClicked(InputClickedEventData eventData)
        {
            StartCoroutine(CloseAnyOpenCard(eventData));
            StartCoroutine(UpdateActivationOfPOIColliders());
        }
    }
}