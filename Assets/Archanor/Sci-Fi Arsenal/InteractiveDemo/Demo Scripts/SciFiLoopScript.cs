﻿using System.Collections;
using UnityEngine;

namespace SciFiArsenal
{
    public class SciFiLoopScript : MonoBehaviour
    {
        public GameObject chosenEffect;
        public float loopTimeLimit = 2.0f;

        private void Start()
        {
            PlayEffect();
        }

        public void PlayEffect()
        {
            StartCoroutine("EffectLoop");
        }

        private IEnumerator EffectLoop()
        {
            GameObject effectPlayer = (GameObject)Instantiate(chosenEffect, transform.position, transform.rotation);

            yield return new WaitForSeconds(loopTimeLimit);

            Destroy(effectPlayer);
            PlayEffect();
        }
    }
}