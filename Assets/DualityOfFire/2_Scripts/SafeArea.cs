using UnityEngine;

namespace ChessClash.Prototype
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        public static SafeArea Instance { get; private set; }
        private RectTransform _panel;
        private Rect _lastSafeArea = new(0, 0, 0, 0);
        private Vector2Int _lastScreenSize = new(0, 0);
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        public bool NotchCheck =>
            Screen.safeArea.height < Screen.height || Screen.safeArea.width < Screen.width;


        [Header("Optional Top Black Bar for Notch")]
        //[SerializeField] private int notchOffeset = 25;
         public GameObject NotchPopUp;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            _panel = GetComponent<RectTransform>();
            ApplySafeArea();
          //  NotchPopUp.SetActive(false);
        }

        void Update()
        {
            if (_lastSafeArea != Screen.safeArea ||
            _lastScreenSize.x != Screen.width ||
            _lastScreenSize.y != Screen.height ||
            _lastOrientation != Screen.orientation) ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // Save state
            _lastSafeArea = safeArea;
            _lastScreenSize.x = Screen.width;
            _lastScreenSize.y = Screen.height;
            _lastOrientation = Screen.orientation;

            // Existing panel behavior (Safe Area) 
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _panel.anchorMin = anchorMin;
            _panel.anchorMax = anchorMax;

        }
    }
}
