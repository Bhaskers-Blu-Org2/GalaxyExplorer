﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Examples.InteractiveElements;
using UnityEngine;
using UnityEngine.Events;

// Galaxy Explorer button based on MRTK button
namespace GalaxyExplorer
{
    public class GEInteractiveToggle : InteractiveToggle
    {
        public UnityEvent OnGazeSelect;
        public UnityEvent OnGazeDeselect;

        protected ToolManager toolmanager = null;

        protected override void Start()
        {
            base.Start();

            toolmanager = FindObjectOfType<ToolManager>();
        }

        // On button click toggle logic, set this as the selected one or if it was select it then unselect it
        public override void ToggleLogic()
        {
            if (IsSelected)
            {
                toolmanager.SelectedTool = null;
            }
            else
            {
                toolmanager.SelectTool(this);
            }

            base.ToggleLogic();
        }

        public override void OnFocusEnter()
        {
            base.OnFocusEnter();

            if ((AllowDeselect && !IsSelected) && !PassiveMode)
            {
                OnGazeSelect?.Invoke();
            }
        }

        public override void OnFocusExit()
        {
            base.OnFocusExit();

            if ((AllowDeselect && !IsSelected) && !PassiveMode)
            {
                OnGazeDeselect?.Invoke();
            }
        }

        // Deselect Button ONLY if its not the one currently selected
        // So deselect it if another button is selected now
        public void DeselectButton()
        {
            if (IsSelected && !PassiveMode && toolmanager.SelectedTool != this)
            {
                OnDeselection?.Invoke();
                OnGazeDeselect?.Invoke();
                IsSelected = false;
                HasGaze = false;
                HasSelection = false;

                Debug.Log("Button " + gameObject.name + " was deselected because it was selected while another button got selected");
            }
        }

        // Reset is needed in buttons that the moment they are selected they are deselected as well, so they dont stay active
        // Such buttons are the controls ones, show and hide
        // This function is hooked up in editor events
        public void ResetButton()
        {
            if (IsSelected && toolmanager.SelectedTool == this)
            {
                toolmanager.SelectedTool = null;
            }

            IsSelected = false;
            HasGaze = false;
            HasSelection = false;
        }
    }
}
