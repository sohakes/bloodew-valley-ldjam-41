using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonControllerMine : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_WalkSpeedUpgraded;
    [SerializeField] private float m_RunSpeedUpgraded;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    public float maxDistance = 1;

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;
    private float gunCd = -1;
    public float maxGunCd = 3f;
    public float maxGunCdUpgraded = 2f;
    public static FirstPersonControllerMine instance = null;

    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            return;
        }

        instance.transform.position = this.transform.position;

        Destroy(gameObject);
        
    }

    public void unlockMouse()
    {
        m_MouseLook.lockCursor = false;
    }

    public void lockMouse()
    {
        m_MouseLook.lockCursor = true;
    }

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);
    }


    // Update is called once per frame
    private void Update()
    {
        if (Game.stopped) return;
        if (Game.current.currentLife <=0)
        {
            this.enabled = false;
        }
        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        if(gunCd >= 0) {
            //MonoBehaviour.print("Guncd = " + gunCd);
            if(Mathf.Floor(gunCd - Time.deltaTime) < Mathf.Floor(gunCd))
            {
                if (Mathf.Ceil(gunCd - Time.deltaTime) == 0f) Manager.instance.setAdviceMessageImmediate("Gun ready!");
                else Manager.instance.setAdviceMessageImmediate("Gun will cooldown in " + (int)Mathf.Ceil(gunCd - Time.deltaTime));
            }
            gunCd -= Time.deltaTime;
        }
        checkFire();
        checkShop();
        
        if(Game.current.hasHermesBoot)
        {
            m_WalkSpeed = m_WalkSpeedUpgraded;
            m_RunSpeed = m_RunSpeedUpgraded;
        }
        if (Game.current.hasLightGloves)
        {
            maxGunCd = maxGunCdUpgraded;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //MonoBehaviour.print("Colliding");
        if (col.gameObject.tag == "EvilVeggie")
        {
            Destroy(col.gameObject);
            Game.current.currentLife--;
            Game.current.veggiesLeft--;
            SoundEffects.sf.playMeHurt();
        }
        if (col.gameObject.tag == "EvilGrandpa")
        {
            SoundEffects.sf.playMeHurt();
            Game.current.currentLife--;
            col.gameObject.GetComponent<EvilGrandpa>().resetPosition();
        }
    }

    private void checkShop()
    {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        Physics.Raycast(ray, out hitInfo, maxDistance, LayerMask.GetMask("ShopItem"));
        if (hitInfo.collider == null) return;
        GameObject shopItem = hitInfo.collider.gameObject;
        if(shopItem.name == "greenjunimo")
        {
            Manager.instance.setAdviceMessageImmediate("Spawn junimos from the hut: 3000g");
        }
        else if(shopItem.name == "fastgloves")
        {
            Manager.instance.setAdviceMessageImmediate("Faster gun cooldown: 2000g");
        }
        else if (shopItem.name == "hermesboot")
        {
            Manager.instance.setAdviceMessageImmediate("Walk and run faster: 1000g");
        }
    }

    private void checkFire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            Physics.Raycast(ray, out hitInfo, Mathf.Infinity);
            if (hitInfo.collider == null) return;

            if (hitInfo.distance > 2*maxDistance) return;

            if (Manager.currentItem == InventoryEnum.GUN && gunCd <= 0)
            {
                gunCd = maxGunCd;
                if (hitInfo.collider.CompareTag("EvilVeggie"))
                {
                    Game.current.veggiesLeft--;
                    SoundEffects.sf.playTheyHurt();
                    Destroy(hitInfo.collider.gameObject);
                }
                if (hitInfo.collider.CompareTag("EvilGrandpa"))
                {
                    hitInfo.collider.gameObject.GetComponent<EvilGrandpa>().damage();
                }
            }
            if (hitInfo.distance > maxDistance) return;

            GameObject shopItem = hitInfo.collider.gameObject;
            if(shopItem.layer == LayerMask.NameToLayer("ShopItem"))
            {
                if (shopItem.name == "greenjunimo")
                {
                    if(Game.current.currentMoney < 3000)
                    {
                        Manager.instance.setAdviceMessageImmediate("You don't have enough money");
                        return;
                    }
                    SoundEffects.sf.playPowerUp();
                    Game.current.currentMoney -= 3000;
                    Game.current.hasJunimoHut = true;
                    Destroy(shopItem);
                }
                else if (shopItem.name == "fastgloves")
                {
                    if (Game.current.currentMoney < 2000)
                    {
                        Manager.instance.setAdviceMessageImmediate("You don't have enough money");
                        return;
                    }
                    SoundEffects.sf.playPowerUp();
                    Game.current.currentMoney -= 2000;
                    Game.current.hasLightGloves = true;
                    Destroy(shopItem);
                }
                else if (shopItem.name == "hermesboot")
                {
                    if (Game.current.currentMoney < 1000)
                    {
                        Manager.instance.setAdviceMessageImmediate("You don't have enough money");
                        return;
                    }
                    SoundEffects.sf.playPowerUp();
                    Game.current.currentMoney -= 1000;
                    Game.current.hasHermesBoot = true;
                    Destroy(shopItem);
                }
            }
            

            if (hitInfo.collider.CompareTag("Door") && !Game.current.battleInProgress)
            {
                SceneManager.LoadScene("house");
            }
            if (hitInfo.collider.CompareTag("Door2"))
            {
                SceneManager.LoadScene("scene1");
            }
            if (hitInfo.collider.CompareTag("Bed"))
            {
                if (!Game.current.battleFinished)
                {
                    Manager.instance.setAdviceMessageImmediate("You can't sleep before the battle");
                    return;
                }
                if (!Game.current.sleep()) return;
                Manager.instance.slept();
                StartCoroutine(GoToSleep());
            }
        }
    }

    IEnumerator GoToSleep()
    {
        Game.stopped = true;

        float elapsedTime = 0.0f;
        float totalTime = 3.0f;
        Image fadePanel = GameObject.Find("FadeToBlack").GetComponent<Image>();
        Color from = fadePanel.color;
        from.a = 0f;
        fadePanel.color = from;
        Color to = fadePanel.color;
        to.a = 1;
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.color = Color.Lerp(from, to, (elapsedTime / totalTime));
            yield return null;
        }

        Game.stopped = false;
        fadePanel.color = from;
        SceneManager.LoadScene("house");

        
    }

    void OnEnable()
    {
        Debug.Log("OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Lol unity, really? https://answers.unity.com/questions/13840/how-to-detect-if-a-gameobject-has-been-destroyed.html
        if (scene.name == "mainmenu" || gameObject == null)
        {
            return;
        }

        if (scene.name == "house")
        {
            if(Game.current.currentTime == 6*60)
            {
                this.transform.localPosition = new Vector3(-47.71f, -0.029f, 63.43575f);
                this.transform.localEulerAngles = new Vector3(0f, -268.507f, 0f);
            } else
            {
                this.transform.localPosition = new Vector3(-46.005f, -0.029f, 62.551f);
                this.transform.localEulerAngles = new Vector3(0f, -367.148f, 0f);
            }
            

        } else if(scene.name == "scene1")
        {
            this.transform.localPosition = new Vector3(0.008735657f, -0.037f, 2.914749f);
            this.transform.localEulerAngles = new Vector3(0f, 134.822f, 0f);
        }
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }


    private void FixedUpdate()
    {
        if (Game.stopped) return;
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();
    }


    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                            Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                    (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }


    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
