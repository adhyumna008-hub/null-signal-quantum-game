using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Story;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NullSignal.Presentation
{
    [DefaultExecutionOrder(-100)]
    public sealed class FinalGameFlow : MonoBehaviour
    {
        private enum Mode { Menu, Opening, Playing, Pause, Ending, Complete }
        [SerializeField] private MainGameDirector game;
        [SerializeField] private StationCinematicDirector cinematics;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private StationFeedbackAudio sound;
        [SerializeField] private SubtitleController subtitles;
        [SerializeField] private CanvasGroup gameplayHud, menuAlpha;
        [SerializeField] private GameObject menu, controls, credits, pause, completion, skip;
        [SerializeField] private TMP_Text playLabel, muteLabel, shakeLabel;
        [SerializeField] private RectTransform subtitleRect;
        private PlayerController movement;
        private PlayerInteraction interaction;
        private PlayerCombat combat;
        private AnveshController anvesh;
        private Mode mode;
        private bool muted, openingSeen;
        private bool movementWasEnabled, interactionWasEnabled, combatWasEnabled, anveshWasEnabled;
        private static bool returnedToMenu;
        private float panelAge;
        public void Configure(MainGameDirector director, StationCinematicDirector scenes, StationCamera camera, StationFeedbackAudio audio,
            SubtitleController dialogue, CanvasGroup hud, GameObject menuRoot, CanvasGroup fade, GameObject controlPanel, GameObject creditPanel,
            GameObject pausePanel, GameObject completePanel, GameObject skipPanel, TMP_Text startText, TMP_Text audioText, TMP_Text shakeText, RectTransform subtitlePanel)
        {
            game = director; cinematics = scenes; cameraRig = camera; sound = audio; subtitles = dialogue; gameplayHud = hud;
            menu = menuRoot; menuAlpha = fade; controls = controlPanel; credits = creditPanel; pause = pausePanel;
            completion = completePanel; skip = skipPanel; playLabel = startText; muteLabel = audioText; shakeLabel = shakeText; subtitleRect = subtitlePanel;
        }
        private void Awake()
        {
            movement = game.Player.GetComponent<PlayerController>(); interaction = game.Player.GetComponent<PlayerInteraction>();
            combat = game.Player.GetComponent<PlayerCombat>(); anvesh = game.Anvesh;
            SetControls(false); cameraRig.enabled = false; Time.timeScale = 0;
        }
        private void Start()
        {
            openingSeen = returnedToMenu; returnedToMenu = false;
            muted = PlayerPrefs.GetInt("NullSignal.AudioMuted", 0) != 0;
            AudioListener.volume = muted ? 0 : 1;
            sound.SetMuted(muted); subtitles.SetVoiceMuted(muted);
            AudioListener.pause = false; ShowMenu();
        }
        private void Update()
        {
            panelAge += Time.unscaledDeltaTime;
            if (menu.activeSelf) menuAlpha.alpha = Mathf.SmoothStep(0, 1, Mathf.Clamp01(panelAge / .35f));
#if ENABLE_INPUT_SYSTEM
            bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            if ((mode == Mode.Opening || mode == Mode.Ending) && (escape || space))
            {
                bool wasOpening = mode == Mode.Opening; cinematics.Stop(); subtitles.Clear();
                if (wasOpening) OpeningFinished(); else EndingFinished();
                return;
            }
            if (escape && mode == Mode.Playing) Pause();
            else if (escape && mode == Mode.Pause) Resume();
            else if (escape && mode == Mode.Menu) { controls.SetActive(false); credits.SetActive(false); menu.SetActive(true); }
#endif
            if (mode == Mode.Playing && game.CurrentRoom != null && game.CurrentRoom.Finale != null && game.CurrentRoom.Finale.Stage == FinaleStage.Ending)
                BeginEnding();
        }
        private void OnApplicationFocus(bool focused) { if (!focused && mode == Mode.Playing) Pause(); }
        private void OnDestroy() { Time.timeScale = 1; AudioListener.pause = false; }
        private void SetControls(bool value)
        {
            movement.enabled = value; interaction.enabled = value; combat.enabled = value; anvesh.enabled = value;
        }
        private void ShowHud(bool visible)
        { gameplayHud.alpha = visible ? 1 : 0; gameplayHud.blocksRaycasts = visible; gameplayHud.interactable = visible; }
        private void HidePanels()
        { menu.SetActive(false); controls.SetActive(false); credits.SetActive(false); pause.SetActive(false); completion.SetActive(false); skip.SetActive(false); }
        private void ShowMenu()
        {
            mode = Mode.Menu; HidePanels(); ShowHud(false); SetControls(false); cameraRig.enabled = false;
            Time.timeScale = 0; subtitles.Clear(); cinematics.ShowBackdrop();
            menu.SetActive(true); menuAlpha.alpha = 0; panelAge = 0;
            playLabel.text = openingSeen ? "PLAY" : "WATCH BACKSTORY";
            muteLabel.text = muted ? "AUDIO: OFF" : "AUDIO: ON";
            shakeLabel.text = StationCamera.ReducedShake ? "CAMERA SHAKE: REDUCED" : "CAMERA SHAKE: NORMAL";
        }
        public void StartOrPlay()
        {
            if (mode != Mode.Menu) return;
            HidePanels(); subtitles.Clear(); Time.timeScale = 1;
            if (!openingSeen)
            {
                mode = Mode.Opening; skip.SetActive(true); subtitleRect.anchoredPosition = new Vector2(0, 88);
                cinematics.PlayOpening(OpeningFinished);
            }
            else
            {
                cinematics.Stop(); mode = Mode.Playing; cameraRig.GetComponent<Camera>().enabled = true; cameraRig.enabled = true;
                subtitleRect.anchoredPosition = new Vector2(0, 207); SetControls(true); ShowHud(true);
                game.BeginGameplay(); cameraRig.SnapToPlayer();
            }
        }
        private void OpeningFinished() { openingSeen = true; ShowMenu(); }
        private void BeginEnding()
        {
            mode = Mode.Ending; SetControls(false); cameraRig.enabled = false; ShowHud(false); HidePanels(); skip.SetActive(true);
            subtitleRect.anchoredPosition = new Vector2(0, 88); subtitles.Clear(); cinematics.PlayEnding(EndingFinished);
        }
        private void EndingFinished()
        {
            mode = Mode.Complete; HidePanels(); ShowHud(false); Time.timeScale = 0;
            cinematics.ShowBackdrop(); completion.SetActive(true); subtitles.Clear();
        }
        public void Pause()
        {
            if (mode != Mode.Playing) return;
            mode = Mode.Pause;
            movementWasEnabled = movement.enabled; interactionWasEnabled = interaction.enabled; combatWasEnabled = combat.enabled; anveshWasEnabled = anvesh.enabled;
            SetControls(false); Time.timeScale = 0; sound.SetPaused(true); subtitles.SetPaused(true); ShowHud(false); pause.SetActive(true);
        }
        public void Resume()
        {
            if (mode != Mode.Pause) return;
            mode = Mode.Playing; Time.timeScale = 1; sound.SetPaused(false); subtitles.SetPaused(false); pause.SetActive(false); controls.SetActive(false);
            movement.enabled = movementWasEnabled; interaction.enabled = interactionWasEnabled; combat.enabled = combatWasEnabled; anvesh.enabled = anveshWasEnabled; ShowHud(true);
        }
        public void RestartCheckpoint() { Resume(); game.RestartCheckpoint(); }
        public void ReturnToMenu()
        {
            cinematics.Stop(); sound.SetPaused(false); subtitles.SetPaused(false); Time.timeScale = 1;
            returnedToMenu = true; SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void ShowControls() { menu.SetActive(false); credits.SetActive(false); controls.SetActive(true); }
        public void ShowCredits() { menu.SetActive(false); controls.SetActive(false); credits.SetActive(true); }
        public void Back() { controls.SetActive(false); credits.SetActive(false); if (mode == Mode.Menu) { menu.SetActive(true); panelAge = 0; } }
        public void ToggleMute()
        {
            muted = !muted; AudioListener.volume = muted ? 0 : 1; sound.SetMuted(muted); subtitles.SetVoiceMuted(muted);
            muteLabel.text = muted ? "AUDIO: OFF" : "AUDIO: ON";
        }
        public void ToggleShake() { StationCamera.ReducedShake = !StationCamera.ReducedShake; shakeLabel.text = StationCamera.ReducedShake ? "CAMERA SHAKE: REDUCED" : "CAMERA SHAKE: NORMAL"; }
    }
}
