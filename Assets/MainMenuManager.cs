using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    public Button newGameButton;
    public Button exitGameButton;
    public Button loadGameButton;
    public Button creditsButton;
    public Button creditsPanelButton;
    public Button instButton;
    public Button instPanelButton;

    // Use this for initialization
    void Start () {
        if (!SaveLoad.canLoad())
        {
            loadGameButton.interactable = false;
        }
        loadGameButton.onClick.AddListener(loadGame);
        newGameButton.onClick.AddListener(newGame);
        exitGameButton.onClick.AddListener(exit);
        creditsButton.onClick.AddListener(openCredits);
        creditsPanelButton.onClick.AddListener(closeCredits);
        instButton.onClick.AddListener(openInstructions);
        instPanelButton.onClick.AddListener(closeInstructions);

        Manager theManager = GameObject.FindObjectOfType<Manager>();
        if (theManager != null)
        {
            theManager.gameObject.SetActive(false);
            DestroyImmediate(theManager.gameObject);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void loadGame()
    {
        SoundEffects.sf.playMenuClick();
        SaveLoad.Load();
        SceneManager.LoadScene("house");
    }

    void newGame()
    {
        SoundEffects.sf.playMenuClick();
        Game.newGame();
        SceneManager.LoadScene("house");
    }

    void exit()
    {
        SoundEffects.sf.playMenuClick();
        Application.Quit();
    }

    void openCredits()
    {
        SoundEffects.sf.playMenuClick();
        creditsPanelButton.gameObject.SetActive(true);
    }

    void closeCredits()
    {
        SoundEffects.sf.playMenuClick();
        creditsPanelButton.gameObject.SetActive(false);
    }

    void openInstructions()
    {
        SoundEffects.sf.playMenuClick();
        instPanelButton.gameObject.SetActive(true);
    }

    void closeInstructions()
    {
        SoundEffects.sf.playMenuClick();
        instPanelButton.gameObject.SetActive(false);
    }
}
