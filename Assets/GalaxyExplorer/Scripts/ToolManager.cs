// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{

    public class ToolManager : MonoBehaviour
    {
        public GEInteractiveToggle SelectedTool = null;
        public GameObject BackButton;
        public GameObject ShowButton;
        public GameObject HideButton;
        public float TargetMinZoomSize = 0.15f;
        public float LargestZoom = 3.0f;

        [SerializeField]
        private AnimationCurve toolsOpacityChange;

        [SerializeField]
        private float FadeToolsDuration = 1.0f;

        [HideInInspector]
        public bool ToolsVisible = false;

        private bool locked = false;
        private ToolPanel panel;

        private List<GEInteractiveToggle> allButtons = new List<GEInteractiveToggle>();
        private List<Collider> allButtonColliders = new List<Collider>();
        private ViewLoader loader = null;
        private TransitionManager transition = null;
        private BoundingBox boundingBox = null;
        private BoundingBoxHandler boundingBoxHandler = null;
        private GEFadeManager fadeManager = null;

        public bool IsLocked
        {
            get { return locked; }
        }

        private float smallestZoom;

        public float SmallestZoom
        {
            get { return smallestZoom; }
        }

        private void Start()
        {
            smallestZoom = TargetMinZoomSize;

            panel = GetComponentInChildren<ToolPanel>(true) as ToolPanel;
            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }

            // FInd all button scripts
            GEInteractiveToggle[] buttonsArray = GetComponentsInChildren<GEInteractiveToggle>(true);
            foreach (var button in buttonsArray)
            {
                allButtons.Add(button);
            }

            // Find all button colliders
            Collider[] allColliders = GetComponentsInChildren<Collider>(true);
            foreach (var collider in allColliders)
            {
                allButtonColliders.Add(collider);
            }

            ShowButton?.SetActive(false);
            BackButton?.SetActive(false);

            transition = FindObjectOfType<TransitionManager>();
            fadeManager = FindObjectOfType<GEFadeManager>();
            boundingBox = FindObjectOfType<BoundingBox>();
            boundingBoxHandler = FindObjectOfType<BoundingBoxHandler>();

            loader = FindObjectOfType<ViewLoader>();
            if (loader)
            {
                loader.OnSceneIsLoaded += OnSceneIsLoaded;
                loader.OnLoadNewScene += OnLoadNewScene;
            }

            // if its unity editor and a non intro scene is active on start then make the menu visible
#if UNITY_EDITOR
            OnSceneIsLoaded();
#endif
        }

        public void OnSceneIsLoaded()
        {
            StartCoroutine(OnSceneIsLoadedCoroutine());
        }

        // Callback when a new scene is requested to be loaded
        public void OnLoadNewScene()
        {
            if (ToolsVisible)
            {
                HideTools();

                // If button is selected then need to be deselected 
                SelectedTool = null;
                UnselectAllTools();
            }
        }

        // Callback when a new scene is loaded
        private IEnumerator OnSceneIsLoadedCoroutine()
        {
            // waiting necessary for events in flow manager to be called and stage of intro flow to be correct when executing following code
            yield return new WaitForSeconds(1);

            if (!ToolsVisible && !transition.IsInIntroFlow)
            {
                // If tools/menu is not visible and intro flow has finished then make menu visible
                while (transition.InTransition)
                {
                    yield return null;
                }

                ShowTools();

                // If there is previous scene then user is able to go back so activate the back button
                BackButton?.SetActive(loader.IsTherePreviousScene());

            }

            yield return null;
        }

        // prevents tools from being accessed
        public void LockTools()
        {
            if (!locked)
            {
                UnselectAllTools();
                locked = true;
            }
        }

        // re-enables tool access
        public void UnlockTools()
        {
            locked = false;
        }

        public void UnselectAllTools()
        {
            // Deselect any other button that might be selected
            foreach (var button in allButtons)
            {
                button.DeselectButton();
            }
        }

        public bool SelectTool(GEInteractiveToggle tool)
        {
            if (locked)
            {
                return false;
            }

            bool isAnyToolSelected = (SelectedTool != null);
            SelectedTool = tool;

            // if Any tool was selected before this one was, then need to deselect the previous one
            if (isAnyToolSelected)
            {
                UnselectAllTools();
            }

            // TODO set cursor to select tool state

            return true;
        }

        public bool DeselectTool(GEInteractiveToggle tool)
        {
            if (locked)
            {
                return false;
            }

            // TODO set cursor normal state

            if (SelectedTool == tool)
            {
                SelectedTool = null;
                return true;
            }

            return false;
        }

        public void LowerTools()
        {
            panel.IsLowered = true;

            if (ShowButton && HideButton)
            {
                ShowButton.SetActive(true);
                HideButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            panel.IsLowered = false;

            if (ShowButton && HideButton)
            {
                ShowButton.SetActive(false);
                HideButton.SetActive(true);
            }
        }

        // Toggle tools by lowering and raising them, this is happening when show and hide button is being pressed
        public void ToggleTools()
        {
            if (panel.IsLowered)
            {
                RaiseTools();
            }
            else
            {
                LowerTools();
            }
        }

        [ContextMenu("Hide Tools")]
        public void HideTools()
        {
            StartCoroutine(HideToolsAsync());
        }

        [ContextMenu("Show Tools")]
        public void ShowTools()
        {
            StartCoroutine(ShowToolsAsync());
        }

        // Hide tools by deactivating button colliders and fade out button materials
        public IEnumerator HideToolsAsync()
        {
            ToolsVisible = false;
            SetCollidersEnabled(false);

            Fader[] allToolFaders = GetComponentsInChildren<Fader>();
            fadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeOut, FadeToolsDuration, toolsOpacityChange);

            yield return null;
        }

        // Show tools by activating button colliders and fade in button materials
        public IEnumerator ShowToolsAsync()
        {
            if (GalaxyExplorerManager.IsHoloLens || GalaxyExplorerManager.IsImmersiveHMD)
            {
                panel.gameObject.SetActive(true);
                ToolsVisible = true;
                SetCollidersEnabled(true);
             
                Fader[] allToolFaders = GetComponentsInChildren<Fader>();
                fadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeIn, FadeToolsDuration, toolsOpacityChange);
                yield return null;
            }
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (var collider in allButtonColliders)
            {
                collider.enabled = isEnabled;
            }
        }

        public void ShowBackButton()
        {
            if (BackButton)
            {
                BackButton.SetActive(true);
            }
        }

        public void HideBackButton()
        {
            if (BackButton)
            {
                BackButton.SetActive(false);
            }
        }

        // On manipulate scene button pressed from menu
        // Enable bounding box and scale it appropriately so in coves the whole scene
        public void OnManipulateButtonPressed(bool enable)
        {
            if (boundingBox)
            {
                boundingBox.Target.GetComponentInChildren<Collider>().enabled = enable;

                if (enable)
                {
                    boundingBox.Target.GetComponent<BoundingBoxRig>().Activate();
                    StartCoroutine(boundingBoxHandler.OnBoundingBoxCreated());
                }
                else
                {
                    boundingBox.Target.GetComponent<BoundingBoxRig>().Deactivate();
                }
            }
        }
    }
}
