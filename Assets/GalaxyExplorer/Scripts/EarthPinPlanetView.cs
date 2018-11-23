﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace GalaxyExplorer
{
    public class EarthPinPlanetView : PlanetView
    {
        [SerializeField]
        private string SceneName = "";

        public string  GetSceneName 
        {
            get { return SceneName; }
        }

        public override void OnInputDown(InputEventData eventData)
        {

        }

        public override void OnInputUp(InputEventData eventData)
        {

        }
    }
}
