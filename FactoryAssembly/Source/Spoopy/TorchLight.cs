using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class TorchLight : MonoBehaviour
    {
        public AudioClip PowerOffSound = null;
        public AudioClip PowerOnSound = null;
        public AudioClip SwitchSound = null;
        public float PowerOffDelay = 2.0f;
        public float PowerOnDelay = 2.0f;
        public Texture TVBlack = null;

        private KMAudio _audio = null;
        private Light _light = null;
        private Camera _camera = null;
        private Transform _cameraTransform = null;

        private Material _oldTVMaterial = null;

        private void Awake()
        {
            _audio = GetComponent<KMAudio>();
            _light = GetComponent<Light>();

            _camera = Camera.main;
            _cameraTransform = _camera.transform;
        }

        public void DoFlicker(FactoryRoomData data, float duration)
        {
            gameObject.SetActive(true);

            StartCoroutine(Flicker(data, duration));
        }

        private IEnumerator Flicker(FactoryRoomData data, float duration)
        {
            _audio.PlaySoundAtTransform(PowerOffSound.name, transform);
            data.EnableAmbient = data.EnableNormalLights = data.EnableWarningLights = false;
            _light.enabled = false;

            _oldTVMaterial = data.TVDisplay.material;
            Material material = new Material(_oldTVMaterial);
            material.mainTexture = TVBlack;
            data.TVDisplay.material = material;

            yield return new WaitForSeconds(PowerOffDelay);

            _audio.PlaySoundAtTransform(SwitchSound.name, transform);
            _light.enabled = true;

            while (duration >= 0.0f)
            {
                yield return new WaitForSeconds(0.1f);
                duration -= 0.1f;
                _light.intensity = Mathf.MoveTowards(_light.intensity, Random.Range(0.0f, 1.0f), 0.05f);
            }

            _audio.PlaySoundAtTransform(PowerOnSound.name, transform);
            data.EnableAmbient = data.EnableNormalLights = data.EnableWarningLights = true;

            data.TVDisplay.material = _oldTVMaterial;

            yield return new WaitForSeconds(PowerOnDelay);

            _audio.PlaySoundAtTransform(SwitchSound.name, transform);
            _light.enabled = false;

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            _light.spotAngle = _camera.fieldOfView * 0.7f;
            transform.SetPositionAndRotation(_cameraTransform.position, _cameraTransform.rotation);
        }
    }
}
