using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryRoomData : MonoBehaviour
    {
        public Transform[] ConveyorBeltNodes;
        public Transform InitialSpawn;
        public Transform LeftDoor;
        public Transform RightDoor;
        public Transform ConveyorTop;

        public AudioClip DoorLongAudio;
        public AudioClip DoorShortAudio;
        public AudioClip ConveyorAudio;

        public Transform VanillaBombSpawn;

        public Light[] NormalLights;
        public WarningLight WarningLight;

        public bool EnableAmbient = true;
        public bool EnableNormalLights = true;
        public bool EnableWarningLights = true;

        public Color AmbientOffColor = new Color(0.01f, 0.01f, 0.01f);
        public Color AmbientWarningColor = new Color(0.3f, 0.3f, 0.3f);
        public Color AmbientOnColor = new Color(0.4f, 0.4f, 0.4f);

        [Range(0.0f, 1.0f)]
        public float LightOffIntensity = 0.02f;

        [Range(0.0f, 1.0f)]
        public float LightWarningIntensity = 0.6f;

        [Range(0.0f, 1.0f)]
        public float LightOnIntensity = 0.8f;

        [Range(0.0f, 180.0f)]
        public float WarningTime = 60.0f;

        public Renderer TVDisplay;
    }
}
