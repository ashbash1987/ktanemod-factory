using System;
using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class SpoopyScares : MonoBehaviour
    {
        [Header("Timing")]
        public float MinimumScareWait = 0.0f;
        public float MaximumScareWait = 0.0f;

        public TorchLight TorchLight = null;
        public GlitchDistort GlitchDistort = null;
        public FakeStrike FakeStrike = null;
        public FakeNeedy FakeNeedy = null;
        public Bump Bump = null;
        public WibblyWobblyTime WibblyWobblyTime = null;

        private FactoryRoomData _data = null;
        private Action[] _scares = null;

        private void Awake()
        {
            _data = GetComponent<FactoryRoomData>();
            _scares = new Action[]
            {
                DoTorchLightScare,
                DoTorchLightScare,
                DoTorchLightScare,
                DoTorchLightScare,
                DoGlitchDistortScare,
                DoGlitchDistortScare,
                DoGlitchDistortScare,
                DoBump,
                DoBump,
                DoBump,
                DoBump,
                DoWibblyWobblyTime,
                DoWibblyWobblyTime,
                DoWibblyWobblyTime,
                DoFakeStrike,
                DoFakeNeedy,
            };
        }

        private void OnEnable()
        {
            StartCoroutine(ScareLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator ScareLoop()
        {
            while (true)
            {
                float scareWait = UnityEngine.Random.Range(MinimumScareWait, MaximumScareWait);
                yield return new WaitForSeconds(scareWait);

                Action scare = _scares[UnityEngine.Random.Range(0, _scares.Length)];
                scare();

                yield return null;
            }
        }

        private void DoTorchLightScare()
        {
            TorchLight.DoFlicker(_data, MinimumScareWait * 0.6f);
        }

        private void DoGlitchDistortScare()
        {
            GlitchDistort.DoDistort();
        }

        private void DoFakeStrike()
        {
            FakeStrike.DoFakeStrike();
        }

        private void DoFakeNeedy()
        {
            FakeNeedy.DoFakeNeedy();
        }

        private void DoBump()
        {
            Bump.DoBump(MinimumScareWait * 0.15f);
        }

        private void DoWibblyWobblyTime()
        {
            WibblyWobblyTime.DoWibblyWobblyTime(MinimumScareWait * 0.2f);
        }
    }
}
