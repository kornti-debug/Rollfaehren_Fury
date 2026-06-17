using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public sealed class RiverWaterScroller : MonoBehaviour
    {
        [SerializeField] private Vector2 textureTiling = new Vector2(14f, 14f);
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0.025f, 0.012f);
        [SerializeField] private string baseMapTransformProperty = "_BaseMap_ST";
        [SerializeField] private string fallbackMapTransformProperty = "_MainTex_ST";

        private Renderer waterRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Vector2 offset;
        private int baseMapTransformId;
        private int fallbackMapTransformId;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            CacheReferences();
            ApplyOffset();
        }

        private void Update()
        {
            offset.x = Mathf.Repeat(offset.x + scrollSpeed.x * Time.deltaTime, 1f);
            offset.y = Mathf.Repeat(offset.y + scrollSpeed.y * Time.deltaTime, 1f);
            ApplyOffset();
        }

        private void OnDisable()
        {
            if (waterRenderer != null)
            {
                waterRenderer.SetPropertyBlock(null);
            }
        }

        private void OnValidate()
        {
            textureTiling.x = Mathf.Max(0.01f, textureTiling.x);
            textureTiling.y = Mathf.Max(0.01f, textureTiling.y);
        }

        private void CacheReferences()
        {
            if (waterRenderer == null)
            {
                waterRenderer = GetComponent<Renderer>();
            }

            propertyBlock ??= new MaterialPropertyBlock();
            baseMapTransformId = Shader.PropertyToID(baseMapTransformProperty);
            fallbackMapTransformId = Shader.PropertyToID(fallbackMapTransformProperty);
        }

        private void ApplyOffset()
        {
            if (waterRenderer == null)
            {
                return;
            }

            Vector4 textureTransform = new Vector4(textureTiling.x, textureTiling.y, offset.x, offset.y);
            waterRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(baseMapTransformId, textureTransform);
            propertyBlock.SetVector(fallbackMapTransformId, textureTransform);
            waterRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
