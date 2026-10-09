using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using PimpMyBakkie.CameraCapture;

namespace PimpMyBakkie
{
    /// <summary>
    /// Photo-first product flow. Deliberately does not display a generic procedural vehicle:
    /// the user's actual vehicle photo is the source of truth until an exact model is available.
    /// </summary>
    public sealed class PhotoFirstHUD : MonoBehaviour
    {
        VehiclePhotoCapture capture;
        RawImage photoSurface;
        Text heading, description, status;
        Button cameraButton, identifyButton, retakeButton;
        Texture2D capturedPhoto;
        bool photoCaptured;
        bool cameraRequested;

        static readonly Color Slate = new Color(.055f, .075f, .07f, 1f);
        static readonly Color PanelFill = new Color(.09f, .12f, .105f, 1f);
        static readonly Color Accent = new Color(.43f, .59f, .37f, 1f);
        static readonly Color Ink = new Color(.94f, .96f, .92f, 1f);

        void Start()
        {
            capture = gameObject.AddComponent<VehiclePhotoCapture>();
            BuildInterface();
            ShowIntro();
        }

        void Update()
        {
            if (!photoCaptured && cameraRequested && capture != null && capture.IsRunning)
            {
                if (photoSurface.texture != capture.CurrentPreview)
                    photoSurface.texture = capture.CurrentPreview;
                if (capture.CurrentPreview != null && capture.CurrentPreview.width > 16 && capture.CurrentPreview.height > 16)
                    status.text = "CAMERA READY  •  FRAME THE WHOLE BAKKIE";
                else
                    status.text = "WAITING FOR CAMERA PERMISSION / IMAGE...";
            }
        }

        void BuildInterface()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var root = new GameObject("PhotoFirstCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Screen.height > Screen.width ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            var bg = Panel(root.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Slate);
            heading = Label(bg, "PIMPMYBAKKIE", 42, FontStyle.Bold, TextAnchor.MiddleLeft);
            heading.rectTransform.anchorMin = new Vector2(.055f, .83f);
            heading.rectTransform.anchorMax = new Vector2(.95f, .98f);

            description = Label(bg, "YOUR BAKKIE. YOUR PHOTO. REALISTIC UPGRADES.", 20, FontStyle.Normal, TextAnchor.MiddleLeft);
            description.color = new Color(.7f, .77f, .7f);
            description.rectTransform.anchorMin = new Vector2(.058f, .78f);
            description.rectTransform.anchorMax = new Vector2(.95f, .85f);

            var frame = Panel(bg.transform, "VehiclePhotoFrame", new Vector2(.045f, .23f), new Vector2(.955f, .75f), Vector2.zero, Vector2.zero, PanelFill);
            photoSurface = frame.AddComponent<RawImage>();
            photoSurface.color = Color.white;
            photoSurface.uvRect = new Rect(0, 0, 1, 1);

            var placeholder = Label(frame, "YOUR VEHICLE PHOTO\n\nTake a clear photo showing the whole bakkie.\nWe'll use your actual vehicle as the reference.", 26, FontStyle.Normal, TextAnchor.MiddleCenter);
            placeholder.name = "PhotoInstructions";
            placeholder.color = new Color(.77f, .82f, .76f);
            placeholder.raycastTarget = false;

            status = Label(bg, "", 19, FontStyle.Normal, TextAnchor.MiddleLeft);
            status.rectTransform.anchorMin = new Vector2(.055f, .16f);
            status.rectTransform.anchorMax = new Vector2(.95f, .23f);

            cameraButton = MakeButton(bg, "OPEN CAMERA", new Vector2(.05f, .045f), new Vector2(.36f, .14f));
            cameraButton.onClick.AddListener(OnCameraPressed);
            retakeButton = MakeButton(bg, "RETAKE PHOTO", new Vector2(.05f, .045f), new Vector2(.36f, .14f));
            retakeButton.onClick.AddListener(OnRetake);
            identifyButton = MakeButton(bg, "IDENTIFY MY BAKKIE", new Vector2(.39f, .045f), new Vector2(.95f, .14f));
            identifyButton.onClick.AddListener(OnIdentify);
            retakeButton.gameObject.SetActive(false);
            identifyButton.gameObject.SetActive(false);
        }

        void ShowIntro()
        {
            status.text = "STEP 1 OF 3  •  PHOTOGRAPH YOUR ACTUAL VEHICLE";
        }

        void OnCameraPressed()
        {
            if (capture == null) return;
            cameraRequested = true;
            capture.StartCamera();
            if (capture.IsRunning)
            {
                photoSurface.texture = capture.CurrentPreview;
                var instructions = photoSurface.transform.Find("PhotoInstructions");
                if (instructions != null) instructions.gameObject.SetActive(false);
                cameraButton.GetComponentInChildren<Text>().text = "CAPTURE PHOTO";
                cameraButton.onClick.RemoveAllListeners();
                cameraButton.onClick.AddListener(OnCapture);
                status.text = "CAMERA READY  •  FRAME THE WHOLE BAKKIE";
            }
            else
            {
                status.text = "NO CAMERA FOUND. Use a device with a camera to photograph the bakkie.";
            }
        }

        void OnCapture()
        {
            if (capture == null || !capture.IsRunning) return;
            var photo = capture.Capture();
            if (photo == null)
            {
                status.text = "CAMERA IS STILL STARTING. Please wait a moment and try again.";
                return;
            }

            capturedPhoto = photo;
            capture.StopCamera();
            photoCaptured = true;
            photoSurface.texture = capturedPhoto;
            var instructions = photoSurface.transform.Find("PhotoInstructions");
            if (instructions != null) instructions.gameObject.SetActive(false);
            cameraButton.gameObject.SetActive(false);
            retakeButton.gameObject.SetActive(true);
            identifyButton.gameObject.SetActive(true);
            status.text = "PHOTO CAPTURED  •  THIS PHOTO WILL BE THE VEHICLE REFERENCE";
            description.text = "CHECK YOUR PHOTO BEFORE MATCHING THE EXACT MAKE, MODEL AND VARIANT.";
        }

        void OnRetake()
        {
            if (capturedPhoto != null) Destroy(capturedPhoto);
            capturedPhoto = null;
            photoCaptured = false;
            cameraRequested = false;
            if (capture != null && capture.IsRunning) capture.enabled = false;
            if (capture != null) capture.enabled = true;

            photoSurface.texture = null;
            var instructions = photoSurface.transform.Find("PhotoInstructions");
            if (instructions != null) instructions.gameObject.SetActive(true);
            cameraButton.gameObject.SetActive(true);
            cameraButton.GetComponentInChildren<Text>().text = "OPEN CAMERA";
            cameraButton.onClick.RemoveAllListeners();
            cameraButton.onClick.AddListener(OnCameraPressed);
            retakeButton.gameObject.SetActive(false);
            identifyButton.gameObject.SetActive(false);
            description.text = "YOUR BAKKIE. YOUR PHOTO. REALISTIC UPGRADES.";
            ShowIntro();
        }

        void OnIdentify()
        {
            // Never claim an identification or substitute a generic model. The production
            // vision provider and exact-variant asset library still need to be connected.
            status.text = "PHOTO READY. AI vehicle recognition is not connected yet, so we won't guess your bakkie's model or show a fake 3D substitute.";
            description.text = "NEXT: MATCH THE EXACT VEHICLE, THEN FIT COMPATIBLE REAL-WORLD ACCESSORIES.";
        }

        GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        Text Label(GameObject parent, string value, int size, FontStyle style, TextAnchor align)
        {
            var obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 12); rect.offsetMax = new Vector2(-20, -12);
            var text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.fontStyle = style;
            text.alignment = align; text.color = Ink;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button MakeButton(GameObject parent, string caption, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(caption + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            obj.GetComponent<Image>().color = Accent;
            var button = obj.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Accent;
            colors.highlightedColor = new Color(.52f, .68f, .45f);
            colors.pressedColor = new Color(.3f, .43f, .27f);
            button.colors = colors;
            var label = Label(obj, caption, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
            return button;
        }

        void OnDestroy()
        {
            if (capturedPhoto != null) Destroy(capturedPhoto);
        }
    }
}