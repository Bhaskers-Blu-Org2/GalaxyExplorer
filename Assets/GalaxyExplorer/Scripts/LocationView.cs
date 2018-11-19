﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class LocationView : MonoBehaviour
    {
        [SerializeField]
        private string MusicEvent;

        [SerializeField]
        private float MusicDelayInSeconds = 1.0f;

        private bool playMusic = true;
        private float delayTimer = 0.0f;

        private MusicManager musicManager = null;
        private TransitionManager transitionManager = null;

        void Start()
        {
            delayTimer = MusicDelayInSeconds;
            musicManager = FindObjectOfType<MusicManager>();
            transitionManager = FindObjectOfType<TransitionManager>();
        }

        void Update()
        {
            if (playMusic && !transitionManager.IsInIntroFlow)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0.0f)
                {
                    musicManager.FindSnapshotAndTransition(MusicEvent);
                    playMusic = false;
                }
            }
        }
    }
}
