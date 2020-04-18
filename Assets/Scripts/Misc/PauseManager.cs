using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour {

    public string pausedTitle = "PAUSED";
    public string resumeButtonText = "RESUME";

    public Text titleText;
    public Text buttonText;

    Canvas menuCanvas;
    Canvas hudCanvas;

    bool gameHasStarted = false;

    void Start() {
        menuCanvas = GetComponent<Canvas>();
        hudCanvas = GameObject.Find("HUDCanvas").GetComponent<Canvas>();
        hudCanvas.enabled = false;
        Time.timeScale = 0;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            gameHasStarted = true;
            Pause();
        }
    }

    public void Pause() {
        menuCanvas.enabled = !menuCanvas.enabled;
        hudCanvas.enabled = !hudCanvas.enabled;

        Time.timeScale = Time.timeScale == 0 ? 1 : 0;

        if (gameHasStarted) {
            titleText.text = pausedTitle;
            buttonText.text = resumeButtonText;
        }
    }
}
