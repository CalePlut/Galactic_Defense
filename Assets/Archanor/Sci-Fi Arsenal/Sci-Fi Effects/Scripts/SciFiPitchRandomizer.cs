using UnityEngine;

namespace SciFiArsenal
{
    public class SciFiPitchRandomizer : MonoBehaviour
    {
        public float randomPercent = 10;

        private void Start()
        {
            transform.GetComponent<AudioSource>().pitch *= 1 + Random.Range(-randomPercent / 100, randomPercent / 100);
        }
    }
}