using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class ControllerButtonHintHelper
    {
        internal static Sprite circleButtonSprite;
        internal static Sprite rectButtonSprite;

        internal static void AddControllerTooltipToTrashCan(Button button, Transform parent)
        {
            var uiGamePad = InventoryGui.instance.m_takeAllButton.GetComponent<UIGamePad>();

            var controllerHint = Object.Instantiate(uiGamePad.m_hint, parent);
            var uiGamePadNew = button.gameObject.AddComponent<UIGamePad>();
            uiGamePadNew.m_hint = controllerHint;

            InventoryGui.instance.StartCoroutine(WaitAFrameToSetupControllerHint(uiGamePadNew));
        }

        private static IEnumerator WaitAFrameToSetupControllerHint(UIGamePad uiGamePad)
        {
            yield return null;

            if (uiGamePad == null)
            {
                yield break;
            }

            if (!uiGamePad.m_hint)
            {
                yield break;
            }

            SetupControllerHint(uiGamePad, KeybindChecker.joyTrash);
        }

        internal static void FixTakeAllButtonControllerHint(InventoryGui instance)
        {
            var uiGamePad = instance.m_takeAllButton.GetComponent<UIGamePad>();

            if (!uiGamePad)
            {
                return;
            }

            bool shouldShowHint = !ControllerConfig.RemoveControllerButtonHintFromTakeAllButton.Value;

            uiGamePad.enabled = shouldShowHint;

            var toMoveUp = uiGamePad.m_hint.gameObject;

            if (!shouldShowHint)
            {
                toMoveUp.SetActive(false);
            }
            else
            {
                toMoveUp.SetActive(true);

                var canvas = toMoveUp.GetComponent<Canvas>();

                if (canvas == null)
                {
                    canvas = toMoveUp.AddComponent<Canvas>();
                }

                if (!toMoveUp.GetComponent<GraphicRaycaster>())
                {
                    toMoveUp.AddComponent<GraphicRaycaster>();
                }

                instance.StartCoroutine(DelayedOverrideSorting(canvas));
            }
        }

        private static IEnumerator DelayedOverrideSorting(Canvas canvas)
        {
            yield return null;

            while (canvas != null && !canvas.isActiveAndEnabled)
            {
                yield return null;
            }

            if (canvas == null)
            {
                yield break;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 1;
        }

        internal static IEnumerator WaitAFrameToSetupControllerHint(Button button, string joyHint)
        {
            yield return null;

            if (button == null)
            {
                yield break;
            }

            var uiGamePad = button.GetComponent<UIGamePad>();

            if (!uiGamePad)
            {
                yield break;
            }

            if (!uiGamePad.m_hint)
            {
                yield break;
            }

            SetupControllerHint(uiGamePad, joyHint);
        }

        internal static void SetupControllerHint(UIGamePad uiGamePad, string joyHint)
        {
            var hint = uiGamePad.m_hint;

            if (!uiGamePad.m_hint)
            {
                return;
            }

            uiGamePad.m_zinputKey = null;

            if (ControllerConfig.UseHardcodedControllerSupport.Value)
            {
                hint.gameObject.SetActive(true);

                var text = hint.GetComponentInChildren<TextMeshProUGUI>();

                if (text)
                {
                    text.text = Localization.instance.Translate(KeybindChecker.joyTranslationPrefix + joyHint);
                }
            }
            else
            {
                uiGamePad.enabled = false;
                hint.gameObject.SetActive(false);
            }
        }
    }
}