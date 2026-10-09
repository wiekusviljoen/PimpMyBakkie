[Reading 144 lines from start (total: 144 lines, 0 remaining)]

[Reading 140 lines from start (total: 140 lines, 0 remaining)]

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using PimpMyBakkie.Prototype;

namespace PimpMyBakkie
{
    public sealed class ConfiguratorHUD : MonoBehaviour
    {
        readonly Dictionary<string, bool> enabledUpgrades = new();
        Text statsText;
        Text statusText;
        ProceduralBakkie vehicle;

        void Start()
        {
            vehicle = FindFirstObjectByType<ProceduralBakkie>();
            BuildInterface();
            RefreshStats();
        }

        void BuildInterface()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystem);

            var canvasObject = new GameObject("ConfiguratorCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;

            var header = Panel(canvasObject.transform, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -210), new Vector2(0, 0), new Color(.035f, .055f, .047f, .96f));
            var title = Label(header, "PIMPMYBAKKIE", 48, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.rectTransform.offsetMin = new Vector2(46, 100);
            title.rectTransform.offsetMax = new Vector2(-30, -22);
            var subtitle = Label(header, "BUILD YOUR BAKKIE  •  ROTATE TO EXPLORE", 22, FontStyle.Normal, TextAnchor.MiddleLeft);
            subtitle.color = new Color(.69f, .78f, .71f);
            subtitle.rectTransform.offsetMin = new Vector2(48, 34);
            subtitle.rectTransform.offsetMax = new Vector2(-30, -104);

            var footer = Panel(canvasObject.transform, "BuildPanel", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 570), new Color(.035f, .055f, .047f, .97f));
            var label = Label(footer, "YOUR BUILD  /  ESTIMATES", 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            label.rectTransform.offsetMin = new Vector2(36, 490);
            label.rectTransform.offsetMax = new Vector2(-30, -18);
            statsText = Label(footer, "", 24, FontStyle.Normal, TextAnchor.UpperLeft);
            statsText.rectTransform.offsetMin = new Vector2(38, 300);
            statsText.rectTransform.offsetMax = new Vector2(-24, -88);

            statusText = Label(footer, "Choose upgrades below", 20, FontStyle.Italic, TextAnchor.MiddleLeft);
            statusText.color = new Color(.72f, .82f, .74f);
            statusText.rectTransform.offsetMin = new Vector2(38, 252);
            statusText.rectTransform.offsetMax = new Vector2(-24, -300);

            string[] names = { "BULLBAR", "LIFT KIT", "SNORKEL", "CANOPY", "SPOTLIGHTS" };
            string[] keys = { "Bullbar", "Lift", "Snorkel", "Canopy", "Lights" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                var button = MakeButton(footer, names[i], new Vector2(.02f + i * .196f, 0), new Vector2(.02f + i * .196f + .18f, 0), new Vector2(0, 34), new Vector2(0, 220));
                button.onClick.AddListener(() => ToggleUpgrade(keys[index], button));
            }
        }

        GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        Text Label(GameObject parent, string value, int size, FontStyle style, TextAnchor alignment)
        {
            var obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var text = obj.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size; text.fontStyle = style; text.alignment = alignment;
            text.color = new Color(.94f, .96f, .93f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button MakeButton(GameObject parent, string caption, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(caption + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var image = obj.GetComponent<Image>();
            image.color = new Color(.16f, .25f, .19f);
            var button = obj.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(.16f, .25f, .19f);
            colors.highlightedColor = new Color(.28f, .4f, .29f);
            colors.pressedColor = new Color(.38f, .55f, .37f);
            button.colors = colors;
            var label = Label(obj, caption, 18, FontStyle.Bold, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
            return button;
        }

        void ToggleUpgrade(string key, Button button)
        {
            bool enabled = !enabledUpgrades.ContainsKey(key) || !enabledUpgrades[key];
            enabledUpgrades[key] = enabled;
            if (vehicle != null) vehicle.SetUpgrade(key, enabled);
            button.GetComponent<Image>().color = enabled ? new Color(.35f, .52f, .32f) : new Color(.16f, .25f, .19f);
            statusText.text = key.ToUpperInvariant() + (enabled ? " ADDED TO BUILD" : " REMOVED FROM BUILD");
            RefreshStats();
        }

        void RefreshStats()
        {
            if (statsText == null) return;
            int count = 0;
            foreach (var value in enabledUpgrades.Values) if (value) count++;
            bool bar = enabledUpgrades.TryGetValue("Bullbar", out var barOn) && barOn;
            bool liftOn = enabledUpgrades.TryGetValue("Lift", out var lift) && lift;
            bool canopy = enabledUpgrades.TryGetValue("Canopy", out var canopyOn) && canopyOn;
            bool lights = enabledUpgrades.TryGetValue("Lights", out var lightsOn) && lightsOn;
            float clearance = 286 + (liftOn ? 50 : 0);
            float mass = 2100 + (bar ? 55 : 0) + (canopy ? 65 : 0) + (lights ? 8 : 0);
            float price = (bar ? 14500 : 0) + (liftOn ? 18500 : 0)
                        + (enabledUpgrades.TryGetValue("Snorkel", out var snorkel) && snorkel ? 4200 : 0)
                        + (canopy ? 22000 : 0) + (lights ? 3800 : 0);
            statsText.text = string.Format("150 kW    •    500 Nm\n{0:N0} kg    •    {1:0} mm ground clearance\n{2} upgrades selected    •    {3:N0} NAD estimated parts cost", mass, clearance, count, price);
        }
    }
}

[executed on device: DESKTOP-2IPJ9P8 (59869c7c-cbf0-49d3-b774-8bcd0132f317)]

[executed on device: DESKTOP-2IPJ9P8 (59869c7c-cbf0-49d3-b774-8bcd0132f317)]