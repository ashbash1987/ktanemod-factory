using UnityEngine;

namespace FactoryAssembly
{
    public class CameraPostProcess : MonoBehaviour
    {
        public static CameraPostProcess AddPostProcess(Material material)
        {
            CameraPostProcess postProcess = Camera.main.gameObject.AddComponent<CameraPostProcess>();
            postProcess.PostProcessMaterial = material;

            return postProcess;
        }

        public Material PostProcessMaterial
        {
            get;
            set;
        }

        private void Awake()
        {
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, PostProcessMaterial);
        }
    }
}
