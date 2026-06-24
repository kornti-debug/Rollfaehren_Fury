using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(Selectable))]
    public sealed class WwiseUIButtonAudio : MonoBehaviour,
        IPointerEnterHandler,
        ISelectHandler,
        IPointerClickHandler,
        ISubmitHandler
    {
        [SerializeField] private string hoverEvent = WwiseAudioNames.PlayUiHover;
        [SerializeField] private string clickEvent = WwiseAudioNames.PlayUiClick;

        private Selectable selectable;
        private int lastHoverFrame = -1;
        private int lastClickFrame = -1;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PostHover();
        }

        public void OnSelect(BaseEventData eventData)
        {
            PostHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PostClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            PostClick();
        }

        private void PostHover()
        {
            if (!CanPost() || lastHoverFrame == Time.frameCount)
            {
                return;
            }

            lastHoverFrame = Time.frameCount;
            WwiseAudioRuntime.Post(hoverEvent, ResolveEmitter());
        }

        private void PostClick()
        {
            if (!CanPost() || lastClickFrame == Time.frameCount)
            {
                return;
            }

            lastClickFrame = Time.frameCount;
            WwiseAudioRuntime.Post(clickEvent, ResolveEmitter());
        }

        private bool CanPost()
        {
            selectable ??= GetComponent<Selectable>();
            return selectable != null && selectable.IsInteractable();
        }

        private GameObject ResolveEmitter()
        {
            if (WwiseAudioRuntime.Instance != null)
            {
                return WwiseAudioRuntime.Instance.gameObject;
            }

            return MenuWwiseAudio.Instance != null
                ? MenuWwiseAudio.Instance.gameObject
                : gameObject;
        }
    }
}
