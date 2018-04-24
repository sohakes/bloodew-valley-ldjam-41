using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public Vegetable carrot;
    public Vegetable tomato;
    public Dictionary<VegetablesEnum, Vegetable> enumToVeg;
    public Vegetable seed;
    public Text timeText;
    public Text goldText;
    public Text lifeText;
    public static InventoryEnum currentItem;
    public List<RawImage> orderedIcons;
    public Image advicePanel;
    public Image advicePanelImmediate;
    private List<string> messagesToSend;
    private bool runningText = false;
    private bool restartText = false;
    public static Manager instance;
    private bool stopMessages = false;
    public AudioSource audioSource;
    public AudioClip duskSong;
    public AudioClip battleSong;
    public AudioClip generalMusic;
    public Material skyBoxDay;
    public Material skyBoxNight;
    public Light theLight;
    public Transform cam1, cam2, view;
    private bool dead = false;
    private bool immediateRunning = false;
    public Button mainMenuBtn;
    public Button exitButton;
    public Image menuPanel;

    public void Awake()
    {
        DontDestroyOnLoad(this);

        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }

        if (Game.current == null) Game.newGame();
    }

    void exitGame()
    {
        SoundEffects.sf.playMenuClick();
        Application.Quit();
    }

    void loadMainMenu()
    {
        SoundEffects.sf.playMenuClick();
        SceneManager.LoadScene("mainmenu");
    }

    // Use this for initialization
    void Start () {
        //SaveLoad.Load();
        
        messagesToSend = new List<string>();
        currentItem = InventoryEnum.HAND;
        changeIcon(0);
        enumToVeg = new Dictionary<VegetablesEnum, Vegetable>();
        enumToVeg.Add(VegetablesEnum.CARROT, carrot);
        enumToVeg.Add(VegetablesEnum.TOMATO, tomato);
        instance = FindObjectsOfType<Manager>()[0];
        mainMenuBtn.onClick.AddListener(loadMainMenu);
        exitButton.onClick.AddListener(exitGame);

        //Start of the game messages
        if (Game.current.currentDay == 1 && Game.current.currentTime == 6 * 60)
        {
            setAdviceMessage("You need to gain money (2000 gold a day after the second)\n so your grandpa doesn't kill you");
            setAdviceMessage("Use your tools to plant either carrot or tomatoes");
            setAdviceMessage("Use the hoe (2) to tile the ground\nUse the seeds (4/5) to plant\nUse the watering can to water");
            setAdviceMessage("At night, the ghost of some of your plants (even seeds) will come to attack\nUse your gun to defend yourself.");
            setAdviceMessage("Tomatoes are worth 500g and take 2 days to grow\nCarrots are worth 200g and take a day to grow" +
                "\n40% of the carrots spawn a ghost, and 50% of tomatoes.");
            setAdviceMessage("Survive 5 days to find your grandpa\n(Don't forget to gain 2000 gold each day!)");
            setAdviceMessage("Enter your house and sleep in the bed\nYou can sleep only after the battle at night\n2000 gold will be retrieved from you.\nYou will die if you don't have it.");
            setAdviceMessage("You can spend leftover money at the shop. It will be important for the final battle.");
            setAdviceMessage("If you are done for the day, press N to advance to dusk, right before the battle.");
        }

        generalMusicStart();



    }

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Lol unity, really? https://answers.unity.com/questions/13840/how-to-detect-if-a-gameobject-has-been-destroyed.html
        if (scene.name == "mainmenu" || gameObject == null)
        {
            return;
        }
        if (scene.name == "scene1")
        {
            if(Game.current.currentTime >= 18 * 60) {            
                theLight = GameObject.Find("theLight").GetComponent<Light>();
                theLight.intensity = 0.2f;
                Camera.main.GetComponent<Skybox>().material = skyBoxNight; ;
            }
        }
    }


    IEnumerator LerpLight()
    {
        float from = 1f;
        float to = 0.2f;
        float elapsedTime = 0;
        float totalTime = 60f;
        float startTime = Game.current.currentTime;
        theLight = GameObject.Find("theLight").GetComponent<Light>();
        while(elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime - startTime;
            theLight.intensity = Mathf.Lerp(from, to, (elapsedTime / totalTime));
            yield return null;
        }
        Camera.main.GetComponent<Skybox>().material = skyBoxNight;
    }

    IEnumerator LerpColor()
    {
        if (messagesToSend.Count == 0) yield break;
        runningText = true;
        string text = messagesToSend[0];
        //MonoBehaviour.print("Got in with message" + text);
        messagesToSend.RemoveAt(0);
        float elapsedTime = 0.0f;
        float totalTime = 6.0f;
        Color from = advicePanel.color;
        from.a = 0.5f;
        advicePanel.color = from;
        Color to = advicePanel.color;
        to.a = 0;
        Text textComp = advicePanel.GetComponentInChildren<Text>();
        textComp.text = text;
        Color fromt = textComp.color;
        fromt.a = 1f;
        textComp.color = fromt;
        Color tot = textComp.color;
        tot.a = 0;
        yield return null;
        while (elapsedTime < totalTime)
        {
            if (stopMessages)
            {
                stopMessages = false;
                //MonoBehaviour.print("Stop message with text" + text);
                //runningText = false;
                advicePanel.color = to;
                textComp.color = tot;
                yield break;
            }
            elapsedTime += Time.deltaTime;
            advicePanel.color = Color.Lerp(from, to, (elapsedTime / totalTime));
            textComp.color = Color.Lerp(fromt, tot, (elapsedTime / totalTime));
            yield return null;
        }
        runningText = false;
        StartCoroutine(LerpColor());
    }

    public void setAdviceMessage(string text)
    {
        messagesToSend.Add(text);
        if(!runningText) StartCoroutine(LerpColor());
    }

    IEnumerator LerpColorImmediate()
    {
        immediateRunning = true;
        float elapsedTime = 0.0f;
        float totalTime = 3.0f;
        Color from = advicePanelImmediate.color;
        from.a = 0.5f;
        advicePanelImmediate.color = from;
        Color to = advicePanelImmediate.color;
        to.a = 0;
        Text textComp = advicePanelImmediate.GetComponentInChildren<Text>();
        Color fromt = textComp.color;
        fromt.a = 1f;
        textComp.color = fromt;
        Color tot = textComp.color;
        tot.a = 0;
        yield return null;
        while (elapsedTime < totalTime)
        {
            if(restartText)
            {
                restartText = false;
                elapsedTime = 0;
            }
            elapsedTime += Time.deltaTime;
            advicePanelImmediate.color = Color.Lerp(from, to, (elapsedTime / totalTime));
            textComp.color = Color.Lerp(fromt, tot, (elapsedTime / totalTime));
            yield return null;
        }
        immediateRunning = false;
    }

    public void setAdviceMessageImmediate(string text)
    {
        Text textComp = advicePanelImmediate.GetComponentInChildren<Text>();
        textComp.text = text;
        restartText = true;
        if(!immediateRunning)StartCoroutine(LerpColorImmediate());
    }

    void changeIcon(int index)
    {
        foreach(RawImage img in orderedIcons)
        {
            img.color = new Color(255, 255, 255);
        }
        orderedIcons[index].color = new Color(255, 0, 0);
    }

    IEnumerator LerpCam()
    {
        float elapsedTime = 0;
        float totalTime = 8f;
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(cam1.position, cam2.position, (elapsedTime / totalTime));
            Camera.main.transform.LookAt(view.transform);
            yield return null;
        }
        elapsedTime = 0;
        totalTime = 4f;
        while (elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene("mainmenu");
    }

    IEnumerator LerpCam2(Transform c1, Transform c2,  EvilGrandpa grandpa)
    {
        Game.stopped = true;
        Vector3 initialpos = Camera.main.transform.position;
        Quaternion initialrot = Camera.main.transform.rotation;
        float elapsedTime = 0;
        float totalTime = 8f;
        float startTime = Game.current.currentTime;
        while (elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime - startTime;
            Camera.main.transform.position = Vector3.Lerp(c1.position, c2.position, (elapsedTime / totalTime));
            Camera.main.transform.LookAt(grandpa.gameObject.transform);
            yield return null;
        }
        //Camera.main.GetComponent<Skybox>().material = skyBoxNight;
        Camera.main.transform.position = initialpos;
        Camera.main.transform.rotation = initialrot;
        grandpa.startZombie();
        Game.stopped = false;
    }

    public void die()
    {
        Game.current.currentLife = 0;
        GameObject.Find("Player").GetComponent<Animator>().SetTrigger("die");
        cam1 = GameObject.Find("cam1").GetComponent<Transform>();
        cam2 = GameObject.Find("cam2").GetComponent<Transform>();
        Transform cam3 = GameObject.Find("cam3").GetComponent<Transform>();
        Transform cam4 = GameObject.Find("cam3").GetComponent<Transform>();
        view = GameObject.Find("viewto").GetComponent<Transform>();
        if(SceneManager.GetActiveScene().name == "house")
        {
            cam1 = cam4;
            cam2 = cam3;
        }
        StartCoroutine(LerpCam());
        Camera.main.cullingMask = ~0;
        dead = true;
        clearMessages();
    }
	
	// Update is called once per frame
	void Update () {
        
        if (!SceneManager.GetActiveScene().name.Equals("house"))
        {
            Game.current.currentTime += Time.deltaTime * (Game.current.currentTime >= 17*60 ? 1.8f : 3);
        }
        if (Game.stopped) return;
        if (Game.current.currentTime < 6*60+1 && !SceneManager.GetActiveScene().name.Equals("house"))
        {
            theLight = GameObject.Find("theLight").GetComponent<Light>();
            Camera.main.GetComponent<Skybox>().material = skyBoxDay;
            theLight.intensity = 1f;
        }
        lifeText.text = "Life: " + (Game.current.currentLife < 0 ?  0 : Game.current.currentLife);
        if (dead) return;
        if(Game.current.currentLife <= 0)
        {
            die();
            return;
        }
        int totalsec = (int)Game.current.currentTime;
        int hours = totalsec / 60;
        string ampm = "AM";
        if(hours >= 12)
        {
            hours -= 12;
            ampm = "PM";
        }
        int minutes = totalsec % 60;
        timeText.text = "Day " + Game.current.currentDay + " - " 
            + (hours >= 10 ? "" : " ") + hours + ":" + (minutes < 10 ? "0" : "") + minutes + " " + ampm;

        if(Game.current.currentTime > 18 * 60)
        {
            timeText.text = "Day " + Game.current.currentDay + " - Night";
        }

        goldText.text = "Gold: " + Game.current.currentMoney;
        

        //There are other obvious better ways
        if (Input.GetKey(KeyCode.Alpha1))
        {
            currentItem = InventoryEnum.HAND;
            changeIcon(0);
            setAdviceMessageImmediate("Hand selected");
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            currentItem = InventoryEnum.HOE;
            changeIcon(1);
            setAdviceMessageImmediate("Hoe selected");

        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            currentItem = InventoryEnum.CAN;
            changeIcon(2);
            setAdviceMessageImmediate("Watering can selected");
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            currentItem = InventoryEnum.CARROTSEED;
            changeIcon(3);
            setAdviceMessageImmediate("Carrot seed selected");
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            currentItem = InventoryEnum.TOMATOSEED;
            changeIcon(4);
            setAdviceMessageImmediate("Tomato seed selected");
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            currentItem = InventoryEnum.GUN;
            changeIcon(5);
            setAdviceMessageImmediate("Gun selected");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundEffects.sf.playMenuClick();
            menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.N) && Game.current.currentTime < 18 * 60 
            && Game.current.currentTime >= 17 * 60 && SceneManager.GetActiveScene().name == "scene1")
        {
            Game.current.currentTime = 18 * 60;
            theLight = GameObject.Find("theLight").GetComponent<Light>();
            theLight.intensity = 0.2f;
            Camera.main.GetComponent<Skybox>().material = skyBoxNight;
        }
        if (Input.GetKeyDown(KeyCode.N) && Game.current.currentTime < 17*60 && SceneManager.GetActiveScene().name == "scene1")
        {
            Game.current.currentTime = 17 * 60;
            
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Game.current.currentLife = 20;
            Game.current.currentMoney = 50000;
            setAdviceMessage("20 life and 50k gold (you pressed C)");
        }

        /*
        if (Input.GetKey(KeyCode.L))
        {
            SaveLoad.Load();
        }*/

        if (Game.current.currentTime > 17 * 60 && Game.current.dawnMessage == false)
        {
            audioSource.clip = duskSong;
            audioSource.Play();
            if(Game.current.currentDay == 1)
            {
                clearMessages();
                setAdviceMessage("You can't enter your house after 6PM\nUntil the battle is over\nBut you can run (left shift).");
                setAdviceMessage("The crop ghosts appear on crops...\nStay away from them! (but not too far! Your weapon has a range)");
                setAdviceMessage("Aim and shoot the gun to kill them.\nThe night is coming!\nSurvive!");
                setAdviceMessageImmediate("Press N to battle");
            }

            if (Game.current.currentDay == 5)
            {
                clearMessages();
                setAdviceMessage("The final battle will begin soon...");
            }

            Game.current.dawnMessage = true;
            StartCoroutine(LerpLight());
        }

        if (Game.current.battleInProgress && Game.current.veggiesLeft == 0 && !Game.current.battleFinished)
        {
            audioSource.Stop();
            if (Game.current.currentDay == 1)
            {
                clearMessages();
                setAdviceMessage("You survived the first day..\nYou can go into the house and rest now.\nThe game will be saved.");
            }

            if (Game.current.currentDay == 5)
            {
                clearMessages();
                setAdviceMessage("Hey son, I'm sorry for everything that I did\nMy spirit was possesed by another one\nAn angry farmer");
                setAdviceMessage("Thank you\nI can rest now\nLive well");

                endGame();
            }

            Game.current.battleInProgress = false;
            Game.current.battleFinished = true;
            
        }
        checkGrandpa();

    }

    void checkGrandpa()
    {
        if (Game.current.battleInProgress == true || Game.current.currentTime < 18 * 60 
            || Game.current.battleFinished || Game.current.currentDay != 5) return;
        EvilGrandpa.instance.gameObject.SetActive(true);
        Game.current.battleInProgress = true;
        Manager.instance.battleStart();
        Game.current.veggiesLeft = 1;
        cam1 = GameObject.Find("cam1").GetComponent<Transform>();
        Transform cam3 = GameObject.Find("camgrandpa").GetComponent<Transform>();
        StartCoroutine(LerpCam2(cam1, cam3, EvilGrandpa.instance));
        clearMessages();
        Game.grandpaBattle = true;
    }

    IEnumerator FlyGrandpa()
    {
        CoolGrandpa thegran = CoolGrandpa.instance;
        thegran.gameObject.SetActive(true);
        Transform lookto = GameObject.Find("looktocool").transform;
        Transform campos = GameObject.Find("coolcampos").transform;
        Transform flyto = GameObject.Find("Flyto").transform;
        
        
        Camera.main.transform.position = campos.position;
        Camera.main.transform.LookAt(lookto);
        float elapsedTime = 0;
        float totalTime = 12f;
        float startTime = Game.current.currentTime;
        while (elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime - startTime;
            yield return null;
        }
        generalMusicStart();
        elapsedTime = 0f;

        totalTime = 8f;
        startTime = Game.current.currentTime;
        Vector3 initpos = thegran.transform.position;
        while (elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime - startTime;
            Camera.main.transform.LookAt(lookto);
            thegran.transform.position = Vector3.Lerp(initpos, flyto.position, (elapsedTime / totalTime));
            thegran.transform.Rotate(new Vector3(0, Time.deltaTime * 180, 0));
            yield return null;
        }
        Destroy(thegran.gameObject);

        elapsedTime = 0;
        totalTime = 2f;
        startTime = Game.current.currentTime;
        while (elapsedTime < totalTime)
        {
            elapsedTime = Game.current.currentTime - startTime;
            yield return null;
        }

        SceneManager.LoadScene("mainmenu");


    }

    public void endGame()
    {
        Game.grandpaBattle = false;
        Game.stopped = true;
        StartCoroutine(FlyGrandpa());
    }

    public void slept()
    {
        Game.current.battleFinished = false;
        Game.current.dawnMessage = false;
        generalMusicStart();
    }

    public void clearMessages()
    {
        if(messagesToSend.Count > 0)
        {
            stopMessages = true;
            runningText = false;
            messagesToSend.Clear();
        }
    }

    public void battleStart()
    {
        audioSource.clip = battleSong;        
        audioSource.Play();
        audioSource.loop = true;
        currentItem = InventoryEnum.GUN;
        changeIcon(5);
        setAdviceMessageImmediate("Gun selected");

    }

    public void generalMusicStart()
    {
        audioSource.clip = generalMusic;
        audioSource.Play();
        audioSource.loop = true;

    }
}
