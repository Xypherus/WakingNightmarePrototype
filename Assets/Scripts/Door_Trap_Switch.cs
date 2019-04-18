using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using soundTool.soundManager;

public class Door_Trap_Switch : MonoBehaviour{
    //Definition of Audio Sources used
    public AudioClip Door;
    public AudioClip Switch;
    public AudioClip Trap;
    public float DoorDelay = 1.5f;

    public int playerTrapCount = 2;
    public List<PlayerFearController> playersInTrigger;

    Triggerscript trigger;
    public bool captured;
    public bool canremove;
    private Animator open;
    private Rigidbody2D trap;
    public Rigidbody2D[] snare;
    public string switch_key = "v";

    private void Start()
    {
        trigger = GetComponentInParent<Triggerscript>();
        trap = gameObject.GetComponent<Rigidbody2D>();
         
        if (gameObject.GetComponent<Animator>() != null)
        {
            Debug.Log("Door animation found.");
            open = gameObject.GetComponent<Animator>();
        }
    }

    private void InitSnare() { 
        snare = gameObject.GetComponentsInChildren<Rigidbody2D>();
        
        Debug.LogWarning(snare.Length - 1 + " is the length of the snare", this);
        HingeJoint2D hinge = snare[snare.Length - 1].GetComponent<HingeJoint2D>();

        hinge.enabled = false;
    }

    IEnumerator Delay(UnityEngine.Events.UnityAction DelayFunct)
    {
        yield return new WaitForSeconds(DoorDelay);
        DelayFunct();
    }

    private void Update()
    {
        if(trigger.triggered == true && gameObject.CompareTag("Door"))
        {
            open.SetBool("isopen", true);
            SoundManager.PlaySound(Door, 1f);
        }

        if(trigger.triggered == true && gameObject.CompareTag("Trap"))
        {
            if (trap != null)
            {
                Debug.Log("Trap has been triggered");
                SoundManager.PlaySound(Trap);
                trap.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        if(trigger.triggered == true && gameObject.CompareTag("Snare"))
        {
            InitSnare();
            if(snare != null)
            {
                Debug.Log("Trap has been triggered");
                SoundManager.PlaySound(Trap);

                foreach (Rigidbody2D body in snare)
                {
                    body.bodyType = RigidbodyType2D.Dynamic;
                }

                if(canremove == true && captured == true && Input.GetButtonDown("Action") && playersInTrigger.Count >= playerTrapCount)
                {
                    gameObject.SetActive(false);
                    captured = false;
               }
            }
        }
        if (trigger.triggered == true && gameObject.CompareTag("Switch"))
        {
            if (Input.GetKeyDown(switch_key))
            {
                open.SetBool("isopen", true);
                SoundManager.PlaySound(Switch);

                //Delays door sound to be one second after switch sound
                StartCoroutine(Delay( () => {
                    SoundManager.PlaySound(Door);
                }));
            }
        }
        if (trigger.triggered == false)
        {
            open.SetBool("isopen", false);
        }
    }

}