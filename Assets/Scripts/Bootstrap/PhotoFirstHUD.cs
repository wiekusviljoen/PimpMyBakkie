using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using PimpMyBakkie.CameraCapture;
using PimpMyBakkie.Services;

namespace PimpMyBakkie
{
    /// <summary>Photo-first bakkie configurator. The user's real photo is never replaced by a generic 3D vehicle.</summary>
    public sealed class PhotoFirstHUD : MonoBehaviour
    {
        VehiclePhotoCapture capture;
        RawImage photoSurface;
        AspectRatioFitter photoAspect;
        Text description, status;
        Button importButton, cameraButton, identifyButton, manualButton, retakeButton, confirmButton;
        Button previewButton, originalButton, settingsButton, saveSettingsButton, testSettingsButton;
        GameObject photoInstructions, accessoryPanel, vehiclePanel, settingsPanel;
        Text settingsStatus;
        InputField makeField, modelField, yearField, variantField, apiUrlField, apiTokenField;
        readonly Dictionary<string, Button> accessoryButtons = new();
        readonly HashSet<string> selectedAccessories = new();
        Texture2D capturedPhoto, previewPhoto;
        string confirmedVehicle = "";
        string apiUrl = "http://127.0.0.1:5078";
        string apiToken = "";
        bool photoCaptured, vehicleConfirmed, cameraRequested, busy, showingPreview;

        static readonly Color Slate = new(.045f, .06f, .052f, 1f);
        static readonly Color PanelFill = new(.09f, .12f, .105f, 1f);
        static readonly Color Accent = new(.34f, .48f, .29f, 1f);
        static readonly Color Selected = new(.48f, .63f, .39f, 1f);
        static readonly Color Ink = new(.95f, .96f, .92f, 1f);
        static readonly string[] Accessories =
        {
            "Bullbar", "All-terrain tyres", "Black alloy wheels",
            "Suspension lift", "Snorkel", "Canopy",
            "Spotlights", "Dark window tint", "Paint: white",
            "Paint: black", "Paint: graphite", "Paint: sand"
        };

        void Start()
        {
            capture = gameObject.AddComponent<VehiclePhotoCapture>();
            apiUrl = PlayerPrefs.GetString("PimpMyBakkie.ApiUrl", apiUrl);
            apiToken = PlayerPrefs.GetString("PimpMyBakkie.ApiToken", "");
            BuildInterface();
            SetStatus("START HERE  •  IMPORT A REAL PHOTO OF YOUR BAKKIE");
        }

        void Update()
        {
            if (!photoCaptured && cameraRequested && capture != null && capture.IsRunning)
            {
                if (capture.CurrentPreview != null && capture.CurrentPreview.width > 16)
                {
                    photoSurface.texture = capture.CurrentPreview;
                    photoSurface.color = Color.white;
                    photoAspect.aspectRatio = (float)capture.CurrentPreview.width / capture.CurrentPreview.height;
                    SetStatus("CAMERA READY  •  FRAME THE WHOLE BAKKIE");
                }
                else SetStatus("WAITING FOR CAMERA PERMISSION / IMAGE...");
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

            var bg = MakePanel(root.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Slate);
            var heading = MakeLabel(bg, "PIMPMYBAKKIE", 42, FontStyle.Bold, TextAnchor.MiddleLeft);
            heading.rectTransform.anchorMin = new Vector2(.05f, .86f);
            heading.rectTransform.anchorMax = new Vector2(.72f, .98f);
            description = MakeLabel(bg, "REAL PHOTO IN. REALISTIC ACCESSORIES OUT.", 20, FontStyle.Normal, TextAnchor.MiddleLeft);
            description.color = new Color(.72f, .78f, .71f);
            description.rectTransform.anchorMin = new Vector2(.055f, .81f);
            description.rectTransform.anchorMax = new Vector2(.94f, .88f);
            settingsButton = MakeButton(bg, "API SETTINGS", new Vector2(.76f, .89f), new Vector2(.96f, .97f), 16);
            settingsButton.onClick.AddListener(ToggleSettings);

            var frame = MakePanel(bg.transform, "VehiclePhotoFrame", new Vector2(.04f, .36f), new Vector2(.96f, .79f), Vector2.zero, Vector2.zero, PanelFill);
            var photoObject = new GameObject("PhotoImage", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            photoObject.transform.SetParent(frame.transform, false);
            var photoRect = photoObject.GetComponent<RectTransform>();
            photoRect.anchorMin = Vector2.zero; photoRect.anchorMax = Vector2.one;
            photoRect.offsetMin = Vector2.zero; photoRect.offsetMax = Vector2.zero;
            photoSurface = photoObject.GetComponent<RawImage>();
            photoSurface.color = new Color(1, 1, 1, 0);
            photoSurface.raycastTarget = false;
            photoSurface.uvRect = new Rect(0, 0, 1, 1);
            photoAspect = photoObject.GetComponent<AspectRatioFitter>();
            photoAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            photoAspect.aspectRatio = 16f / 9f;
            photoInstructions = MakeLabel(frame, "YOUR ACTUAL BAKKIE\n\nImport a photo or take one with the camera.\nThe preview will preserve this photo as its reference.", 26, FontStyle.Normal, TextAnchor.MiddleCenter).gameObject;
            photoInstructions.name = "PhotoInstructions";
            photoInstructions.GetComponent<Text>().color = new Color(.77f, .82f, .76f);
            photoInstructions.GetComponent<Text>().raycastTarget = false;

            status = MakeLabel(bg, "", 18, FontStyle.Normal, TextAnchor.MiddleLeft);
            status.rectTransform.anchorMin = new Vector2(.05f, .265f);
            status.rectTransform.anchorMax = new Vector2(.95f, .305f);

            importButton = MakeButton(bg, IsWindows ? "IMPORT PHOTO" : "OPEN CAMERA", new Vector2(.05f, .205f), new Vector2(.34f, .255f), 20);
            importButton.onClick.AddListener(OnImportPhoto);
            cameraButton = MakeButton(bg, "TAKE PHOTO", new Vector2(.36f, .205f), new Vector2(.65f, .255f), 20);
            cameraButton.onClick.AddListener(OnCameraPressed);
            if (!IsWindows)
            {
                cameraButton.gameObject.SetActive(false);
                var importRect = importButton.GetComponent<RectTransform>();
                importRect.anchorMin = new Vector2(.20f, .205f);
                importRect.anchorMax = new Vector2(.80f, .255f);
            }
            retakeButton = MakeButton(bg, "RETAKE", new Vector2(.05f, .205f), new Vector2(.27f, .255f), 18);
            retakeButton.onClick.AddListener(OnRetake);
            identifyButton = MakeButton(bg, "IDENTIFY WITH AI", new Vector2(.29f, .205f), new Vector2(.61f, .255f), 18);
            identifyButton.onClick.AddListener(OnIdentify);
            manualButton = MakeButton(bg, "ENTER DETAILS", new Vector2(.63f, .205f), new Vector2(.95f, .255f), 18);
            manualButton.onClick.AddListener(() => OpenVehicleForm(null));
            previewButton = MakeButton(bg, "GENERATE AI PREVIEW  •  API COST MAY APPLY", new Vector2(.05f, .015f), new Vector2(.68f, .06f), 18);
            previewButton.onClick.AddListener(OnGeneratePreview);
            originalButton = MakeButton(bg, "SHOW ORIGINAL", new Vector2(.71f, .015f), new Vector2(.95f, .06f), 18);
            originalButton.onClick.AddListener(ToggleOriginalPreview);

            accessoryPanel = new GameObject("AccessoryOptions", typeof(RectTransform));
            accessoryPanel.transform.SetParent(bg.transform, false);
            var accessoryRect = accessoryPanel.GetComponent<RectTransform>();
            accessoryRect.anchorMin = new Vector2(.04f, .07f);
            accessoryRect.anchorMax = new Vector2(.96f, .195f);
            accessoryRect.offsetMin = Vector2.zero;
            accessoryRect.offsetMax = Vector2.zero;
            for (int i = 0; i < Accessories.Length; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float left = .005f + col * .335f;
                float right = left + .325f;
                float top = .98f - row * .25f;
                float bottom = top - .23f;
                var button = MakeButton(accessoryPanel, Accessories[i].ToUpperInvariant(), new Vector2(left, bottom), new Vector2(right, top), 15);
                string accessory = Accessories[i];
                button.onClick.AddListener(() => ToggleAccessory(accessory));
                accessoryButtons[accessory] = button;
            }

            vehiclePanel = MakePanel(bg.transform, "ConfirmVehiclePanel", new Vector2(.10f, .36f), new Vector2(.90f, .78f), Vector2.zero, Vector2.zero, new Color(.045f, .06f, .052f, .98f));
            var vehicleTitle = MakeLabel(vehiclePanel, "CONFIRM THE EXACT VEHICLE", 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            vehicleTitle.rectTransform.anchorMin = new Vector2(.04f, .85f);
            vehicleTitle.rectTransform.anchorMax = new Vector2(.96f, .99f);
            makeField = MakeInput(vehiclePanel, "Make (e.g. Toyota)", new Vector2(.05f, .68f), new Vector2(.95f, .82f));
            modelField = MakeInput(vehiclePanel, "Model (e.g. Hilux)", new Vector2(.05f, .52f), new Vector2(.95f, .66f));
            yearField = MakeInput(vehiclePanel, "Year (if known)", new Vector2(.05f, .36f), new Vector2(.95f, .50f));
            variantField = MakeInput(vehiclePanel, "Variant / trim (if known)", new Vector2(.05f, .20f), new Vector2(.95f, .34f));
            confirmButton = MakeButton(vehiclePanel, "CONFIRM VEHICLE", new Vector2(.05f, .035f), new Vector2(.55f, .16f), 18);
            confirmButton.onClick.AddListener(ConfirmVehicle);
            var cancel = MakeButton(vehiclePanel, "CANCEL", new Vector2(.59f, .035f), new Vector2(.95f, .16f), 18);
            cancel.onClick.AddListener(() => vehiclePanel.SetActive(false));

            settingsPanel = MakePanel(bg.transform, "ApiSettingsPanel", new Vector2(.08f, .32f), new Vector2(.92f, .75f), Vector2.zero, Vector2.zero, new Color(.045f, .06f, .052f, .99f));
            var settingsTitle = MakeLabel(settingsPanel, "PHOTO SERVICE SETTINGS", 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            settingsTitle.rectTransform.anchorMin = new Vector2(.04f, .84f);
            settingsTitle.rectTransform.anchorMax = new Vector2(.96f, .99f);
            apiUrlField = MakeInput(settingsPanel, "API base URL (e.g. http://127.0.0.1:5078)", new Vector2(.05f, .59f), new Vector2(.95f, .77f));
            apiTokenField = MakeInput(settingsPanel, "Optional app access token", new Vector2(.05f, .36f), new Vector2(.95f, .54f));
            apiTokenField.contentType = InputField.ContentType.Password;
            settingsStatus = MakeLabel(settingsPanel, "Check that the service is online and AI is configured.", 16, FontStyle.Normal, TextAnchor.MiddleCenter);
            settingsStatus.rectTransform.anchorMin = new Vector2(.04f, .20f);
            settingsStatus.rectTransform.anchorMax = new Vector2(.96f, .31f);
            testSettingsButton = MakeButton(settingsPanel, "TEST", new Vector2(.04f, .055f), new Vector2(.31f, .18f), 16);
            testSettingsButton.onClick.AddListener(() => StartCoroutine(TestPhotoService()));
            saveSettingsButton = MakeButton(settingsPanel, "SAVE", new Vector2(.35f, .055f), new Vector2(.64f, .18f), 16);
            saveSettingsButton.onClick.AddListener(SaveSettings);
            var closeSettings = MakeButton(settingsPanel, "CLOSE", new Vector2(.68f, .055f), new Vector2(.96f, .18f), 16);
            closeSettings.onClick.AddListener(() => settingsPanel.SetActive(false));

            retakeButton.gameObject.SetActive(false);
            identifyButton.gameObject.SetActive(false);
            manualButton.gameObject.SetActive(false);
            previewButton.gameObject.SetActive(false);
            originalButton.gameObject.SetActive(false);
            accessoryPanel.SetActive(false);
            vehiclePanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        static bool IsWindows
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return true;
#else
                return false;
#endif
            }
        }

        void OnImportPhoto()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var path = OpenImageDialog();
            if (string.IsNullOrWhiteSpace(path)) { SetStatus("No photo selected. Choose an image or use Take Photo."); return; }
            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!texture.LoadImage(File.ReadAllBytes(path), false))
                {
                    Destroy(texture);
                    SetStatus("Could not load that image. Use a JPEG or PNG photo.");
                    return;
                }
                SetPhoto(texture);
            }
            catch (Exception ex) { SetStatus("Could not open photo: " + ex.Message); }
#else
            OnCameraPressed();
#endif
        }

        void OnCameraPressed()
        {
            if (capture == null) return;
            cameraRequested = true;
            capture.StartCamera();
            if (!capture.IsRunning) { SetStatus("If a camera permission prompt appears, allow it and tap the camera button again. If denied, enable camera access in system settings."); return; }
            photoSurface.texture = capture.CurrentPreview;
            photoSurface.color = Color.white;
            photoInstructions.SetActive(false);
            cameraButton.GetComponentInChildren<Text>().text = "CAPTURE PHOTO";
            cameraButton.onClick.RemoveAllListeners();
            cameraButton.onClick.AddListener(OnCapture);
            SetStatus("CAMERA READY  •  FRAME THE WHOLE BAKKIE");
        }

        void OnCapture()
        {
            var photo = capture != null ? capture.Capture() : null;
            if (photo == null) { SetStatus("Camera is still starting. Wait a moment and tap Capture Photo again."); return; }
            capture.StopCamera();
            SetPhoto(photo);
        }

        void SetPhoto(Texture2D photo)
        {
            if (capturedPhoto != null) Destroy(capturedPhoto);
            if (previewPhoto != null) Destroy(previewPhoto);
            capturedPhoto = photo;
            previewPhoto = null;
            photoCaptured = true;
            vehicleConfirmed = false;
            showingPreview = false;
            confirmedVehicle = "";
            selectedAccessories.Clear();
            photoSurface.texture = capturedPhoto;
            photoSurface.color = Color.white;
            photoAspect.aspectRatio = (float)capturedPhoto.width / capturedPhoto.height;
            photoInstructions.SetActive(false);
            importButton.gameObject.SetActive(false);
            cameraButton.gameObject.SetActive(false);
            retakeButton.gameObject.SetActive(true);
            identifyButton.gameObject.SetActive(true);
            manualButton.gameObject.SetActive(true);
            previewButton.gameObject.SetActive(false);
            originalButton.gameObject.SetActive(false);
            accessoryPanel.SetActive(false);
            vehiclePanel.SetActive(false);
            foreach (var pair in accessoryButtons) SetAccessoryButton(pair.Key, false);
            description.text = "FIRST IDENTIFY THE VEHICLE. THEN SELECT THE ACCESSORIES YOU WANT TO VISUALISE.";
            SetStatus("PHOTO LOADED  •  IDENTIFY WITH AI OR ENTER THE VEHICLE DETAILS");
        }

        void OnRetake()
        {
            if (capture != null) capture.StopCamera();
            if (capturedPhoto != null) Destroy(capturedPhoto);
            if (previewPhoto != null) Destroy(previewPhoto);
            capturedPhoto = previewPhoto = null;
            photoCaptured = vehicleConfirmed = cameraRequested = showingPreview = false;
            confirmedVehicle = "";
            selectedAccessories.Clear();
            photoSurface.texture = null;
            photoSurface.color = new Color(1, 1, 1, 0);
            photoAspect.aspectRatio = 16f / 9f;
            photoInstructions.SetActive(true);
            importButton.gameObject.SetActive(true);
            cameraButton.gameObject.SetActive(IsWindows);
            importButton.GetComponentInChildren<Text>().text = IsWindows ? "IMPORT PHOTO" : "OPEN CAMERA";
            cameraButton.GetComponentInChildren<Text>().text = "TAKE PHOTO";
            cameraButton.onClick.RemoveAllListeners();
            cameraButton.onClick.AddListener(OnCameraPressed);
            retakeButton.gameObject.SetActive(false);
            identifyButton.gameObject.SetActive(false);
            manualButton.gameObject.SetActive(false);
            previewButton.gameObject.SetActive(false);
            originalButton.gameObject.SetActive(false);
            accessoryPanel.SetActive(false);
            vehiclePanel.SetActive(false);
            description.text = "REAL PHOTO IN. REALISTIC ACCESSORIES OUT.";
            SetStatus("START HERE  •  IMPORT A REAL PHOTO OF YOUR BAKKIE");
        }

        void OnIdentify()
        {
            if (busy || capturedPhoto == null) return;
            if (Application.isMobilePlatform && (apiUrl.Contains("127.0.0.1") || apiUrl.Contains("localhost")))
            {
                SetStatus("Set a reachable HTTPS photo-service URL in API Settings. On a phone, localhost means the phone itself.");
                return;
            }
            busy = true;
            SetStatus("IDENTIFYING THE VEHICLE FROM YOUR PHOTO...");
            StartCoroutine(PhotoConfiguratorApi.Identify(capturedPhoto, apiUrl, apiToken, (ok, result, error) =>
            {
                busy = false;
                if (!ok) { SetStatus(error); return; }
                makeField.text = result.make ?? "";
                modelField.text = result.model ?? "";
                yearField.text = result.year > 0 ? result.year.ToString() : "";
                variantField.text = result.variant ?? "";
                OpenVehicleForm(result);
                var confidence = Mathf.RoundToInt(Mathf.Clamp01(result.confidence) * 100f);
                SetStatus("AI SUGGESTION: " + result.make + " " + result.model + " (" + confidence + "% confidence). Verify it below.");
            }));
        }

        void OpenVehicleForm(VehicleIdentificationResult result)
        {
            if (result == null)
            {
                makeField.text = "";
                modelField.text = "";
                yearField.text = "";
                variantField.text = "";
            }
            vehiclePanel.SetActive(true);
        }

        void ConfirmVehicle()
        {
            var make = makeField.text.Trim();
            var model = modelField.text.Trim();
            if (string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model))
            {
                SetStatus("Enter at least the make and model before continuing.");
                return;
            }
            confirmedVehicle = yearField.text.Trim() + " " + make + " " + model + " " + variantField.text.Trim();
            confirmedVehicle = string.Join(" ", confirmedVehicle.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            vehicleConfirmed = true;
            vehiclePanel.SetActive(false);
            identifyButton.gameObject.SetActive(false);
            manualButton.gameObject.SetActive(false);
            accessoryPanel.SetActive(true);
            previewButton.gameObject.SetActive(true);
            originalButton.gameObject.SetActive(previewPhoto != null);
            description.text = confirmedVehicle.ToUpperInvariant() + "  •  SELECT ACCESSORIES TO VISUALISE";
            SetStatus("VEHICLE CONFIRMED. SELECT PARTS, THEN GENERATE A PHOTOREALISTIC CONCEPT.");
        }

        void ToggleAccessory(string accessory)
        {
            if (selectedAccessories.Contains(accessory)) selectedAccessories.Remove(accessory);
            else selectedAccessories.Add(accessory);
            SetAccessoryButton(accessory, selectedAccessories.Contains(accessory));
        }

        void SetAccessoryButton(string accessory, bool on)
        {
            if (accessoryButtons.TryGetValue(accessory, out var button))
            {
                var image = button.GetComponent<Image>();
                if (image != null) image.color = on ? Selected : Accent;
            }
        }

        void OnGeneratePreview()
        {
            if (busy || !vehicleConfirmed || capturedPhoto == null) return;
            if (Application.isMobilePlatform && (apiUrl.Contains("127.0.0.1") || apiUrl.Contains("localhost")))
            {
                SetStatus("Set a reachable HTTPS photo-service URL in API Settings. On a phone, localhost means the phone itself.");
                return;
            }
            if (selectedAccessories.Count == 0) { SetStatus("Select at least one accessory first."); return; }
            busy = true;
            previewButton.interactable = false;
            SetStatus("GENERATING A PHOTO-BASED PREVIEW. THIS CAN TAKE A MINUTE OR TWO...");
            var accessoryList = string.Join(",", selectedAccessories);
            StartCoroutine(PhotoConfiguratorApi.GeneratePreview(capturedPhoto, accessoryList, confirmedVehicle, apiUrl, apiToken, (ok, texture, error) =>
            {
                busy = false;
                previewButton.interactable = true;
                if (!ok) { SetStatus(error); return; }
                if (previewPhoto != null) Destroy(previewPhoto);
                previewPhoto = texture;
                showingPreview = true;
                photoSurface.texture = previewPhoto;
                photoAspect.aspectRatio = (float)previewPhoto.width / previewPhoto.height;
                originalButton.gameObject.SetActive(true);
                originalButton.GetComponentInChildren<Text>().text = "SHOW ORIGINAL";
                SetStatus("AI CONCEPT GENERATED  •  VISUAL PREVIEW ONLY; MECHANICAL FITMENT IS NOT VERIFIED.");
            }));
        }

        void ToggleOriginalPreview()
        {
            if (capturedPhoto == null) return;
            if (previewPhoto == null) { photoSurface.texture = capturedPhoto; return; }
            showingPreview = !showingPreview;
            photoSurface.texture = showingPreview ? previewPhoto : capturedPhoto;
            var shown = showingPreview ? previewPhoto : capturedPhoto;
            photoAspect.aspectRatio = (float)shown.width / shown.height;
            originalButton.GetComponentInChildren<Text>().text = showingPreview ? "SHOW ORIGINAL" : "SHOW PREVIEW";
        }

        IEnumerator TestPhotoService()
        {
            settingsStatus.text = "CHECKING PHOTO SERVICE...";
            var url = string.IsNullOrWhiteSpace(apiUrlField.text) ? apiUrl : apiUrlField.text.Trim().TrimEnd('/');
            yield return PhotoConfiguratorApi.CheckHealth(url, (ok, health, error) =>
            {
                if (!ok) { settingsStatus.text = string.IsNullOrWhiteSpace(error) ? "Service status could not be verified." : error; return; }
                settingsStatus.text = "SERVICE ONLINE  •  AI KEY: " + (health.imageEditingConfigured ? "CONFIGURED" : "MISSING") + "\n" + (health.imageEditingConfigured ? "Ready to test photo identification and previews." : "Add OPENAI_API_KEY on the server to enable AI.");
            });
        }

        void ToggleSettings()
        {
            if (settingsPanel.activeSelf) { settingsPanel.SetActive(false); return; }
            apiUrlField.text = apiUrl;
            apiTokenField.text = apiToken;
            settingsPanel.SetActive(true);
        }

        void SaveSettings()
        {
            apiUrl = string.IsNullOrWhiteSpace(apiUrlField.text) ? "http://127.0.0.1:5078" : apiUrlField.text.Trim().TrimEnd('/');
            apiToken = apiTokenField.text.Trim();
            PlayerPrefs.SetString("PimpMyBakkie.ApiUrl", apiUrl);
            PlayerPrefs.SetString("PimpMyBakkie.ApiToken", apiToken);
            PlayerPrefs.Save();
            settingsPanel.SetActive(false);
            SetStatus("PHOTO SERVICE URL SAVED: " + apiUrl);
        }

        void SetStatus(string message)
        {
            if (status != null) status.text = message;
        }

        GameObject MakePanel(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            obj.GetComponent<Image>().color = color;
            return obj;
        }

        Text MakeLabel(GameObject parent, string value, int size, FontStyle style, TextAnchor align)
        {
            var obj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12, 8); rect.offsetMax = new Vector2(-12, -8);
            var text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.fontStyle = style;
            text.alignment = align; text.color = Ink;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button MakeButton(GameObject parent, string caption, Vector2 min, Vector2 max, int size)
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
            colors.highlightedColor = new Color(.48f, .62f, .4f);
            colors.pressedColor = new Color(.27f, .38f, .24f);
            button.colors = colors;
            var label = MakeLabel(obj, caption, size, FontStyle.Bold, TextAnchor.MiddleCenter);
            label.raycastTarget = false;
            return button;
        }

        InputField MakeInput(GameObject parent, string placeholderText, Vector2 min, Vector2 max)
        {
            var obj = new GameObject("Input_" + placeholderText, typeof(RectTransform), typeof(Image), typeof(InputField));
            obj.transform.SetParent(parent.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            obj.GetComponent<Image>().color = new Color(.14f, .17f, .15f, 1f);
            var input = obj.GetComponent<InputField>();
            var text = MakeLabel(obj, "", 19, FontStyle.Normal, TextAnchor.MiddleLeft);
            text.color = Ink;
            text.supportRichText = false;
            input.textComponent = text;
            var placeholder = MakeLabel(obj, placeholderText, 17, FontStyle.Italic, TextAnchor.MiddleLeft);
            placeholder.color = new Color(.65f, .7f, .65f);
            placeholder.raycastTarget = false;
            input.placeholder = placeholder;
            input.targetGraphic = obj.GetComponent<Image>();
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        void OnDestroy()
        {
            if (capture != null) capture.StopCamera();
            if (capturedPhoto != null) Destroy(capturedPhoto);
            if (previewPhoto != null) Destroy(previewPhoto);
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct OPENFILENAME
        {
            public int lStructSize;
            public IntPtr hwndOwner, hInstance;
            public string lpstrFilter, lpstrCustomFilter;
            public int nMaxCustFilter, nFilterIndex;
            public StringBuilder lpstrFile;
            public int nMaxFile;
            public StringBuilder lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir, lpstrTitle;
            public int Flags;
            public short nFileOffset, nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData, lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved, FlagsEx;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetOpenFileName(ref OPENFILENAME ofn);

        static string OpenImageDialog()
        {
            var buffer = new StringBuilder(1024);
            var ofn = new OPENFILENAME
            {
                lStructSize = Marshal.SizeOf(typeof(OPENFILENAME)),
                lpstrFilter = "Vehicle photos\0*.jpg;*.jpeg;*.png;*.webp\0All files\0*.*\0\0",
                nFilterIndex = 1,
                lpstrFile = buffer,
                nMaxFile = buffer.Capacity,
                lpstrFileTitle = new StringBuilder(256),
                nMaxFileTitle = 256,
                lpstrTitle = "Choose a photo of your bakkie",
                Flags = 0x00001000 | 0x00000800 | 0x00000008,
                lpstrDefExt = "jpg"
            };
            return GetOpenFileName(ref ofn) ? buffer.ToString() : null;
        }
#endif
    }
}