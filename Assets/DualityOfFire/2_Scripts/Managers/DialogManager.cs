using UnityEngine;
using UnityEngine.UI;

namespace TheSortingCafe
{
    public class DialogManager : MonoBehaviour
    {
        [Header("Setting Dialog")]

        [SerializeField] private Image musicONIMG;
        [SerializeField] private Image musicOFFIMG; 
        [SerializeField] private Image soundONIMG;
        [SerializeField] private Image soundOFFIMG;
        [SerializeField] private Image vibrationONIMG;
        [SerializeField] private Image vibrationOFFIMG;
        [SerializeField] private Button privacyBtn;

        private const string MUSIC_KEY = "MUSIC_STATE";
        private const string SOUND_KEY = "SOUND_STATE";
        private const string VIBRATION_KEY = "VIBRATION_STATE";

        private const string PRIVACY_LINK = "http://www.thegamewise.com/privacy-policy/";

        private void Start()
        {
            LoadSettings();

            if (privacyBtn != null)
                privacyBtn.onClick.AddListener(OpenPrivacyPolicy);
        }

        // ====================== LOAD SETTINGS ======================
        private void LoadSettings()
        {
            int musicState = PlayerPrefs.GetInt(MUSIC_KEY, 1);
            int soundState = PlayerPrefs.GetInt(SOUND_KEY, 1);
            int vibrationState = PlayerPrefs.GetInt(VIBRATION_KEY, 1);

            Debug.Log($"LoadSettings -> Music: {musicState}, Sound: {soundState}, Vibration: {vibrationState}");

            if (musicState == 1) MusicON();
            else MusicOFF();

            if (soundState == 1) SoundON();
            else SoundOFF();

            if (vibrationState == 1) VibrationON();
            else VibrationOFF();
        }


        // ====================== MUSIC ======================
        public void MusicON()
        {
            musicONIMG.gameObject.SetActive(true);
            musicOFFIMG.gameObject.SetActive(false);

            PlayerPrefs.SetInt(MUSIC_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("Music ON");
        }

        public void MusicOFF()
        {
            musicONIMG.gameObject.SetActive(false);
            musicOFFIMG.gameObject.SetActive(true);

            PlayerPrefs.SetInt(MUSIC_KEY, 0);
            PlayerPrefs.Save();

            Debug.Log("Music OFF");
        }


        // ====================== SOUND ======================
        public void SoundON()
        {
            soundONIMG.gameObject.SetActive(true);
            soundOFFIMG.gameObject.SetActive(false);

            PlayerPrefs.SetInt(SOUND_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("Sound ON");
        }

        public void SoundOFF()
        {
            soundONIMG.gameObject.SetActive(false);
            soundOFFIMG.gameObject.SetActive(true);

            PlayerPrefs.SetInt(SOUND_KEY, 0);
            PlayerPrefs.Save();

            Debug.Log("Sound OFF");
        }

        // ====================== VIBRATION ======================
        public void VibrationON()
        {
            vibrationONIMG.gameObject.SetActive(true);
            vibrationOFFIMG.gameObject.SetActive(false);

            PlayerPrefs.SetInt(VIBRATION_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log("Vibration ON");
        }

        public void VibrationOFF()
        {
            vibrationONIMG.gameObject.SetActive(false);
            vibrationOFFIMG.gameObject.SetActive(true);

            PlayerPrefs.SetInt(VIBRATION_KEY, 0);
            PlayerPrefs.Save();

            Debug.Log("Vibration OFF");
        }

        // ====================== PRIVACY POLICY ======================
        public void OpenPrivacyPolicy()
        {
            Application.OpenURL(PRIVACY_LINK);
        }
    }
}
