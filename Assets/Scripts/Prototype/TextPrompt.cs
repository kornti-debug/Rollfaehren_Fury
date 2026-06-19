using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    internal static class TextPrompt
    {
        public static void Set(GameObject promptObject, string text, bool active)
        {
            if (promptObject == null)
            {
                return;
            }

            Text label = promptObject.GetComponent<Text>();
            if (label != null)
            {
                label.text = text;
            }

            promptObject.SetActive(active);
        }
    }
}
