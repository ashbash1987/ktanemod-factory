using UnityEngine;

namespace FactoryAssembly
{
    public class FakeStrike : MonoBehaviour
    {
        private KMAudio _audio = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
        }

        public void DoFakeStrike()
        {
            _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
        }        
    }
}
