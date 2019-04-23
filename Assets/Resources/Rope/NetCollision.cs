using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetCollision : MonoBehaviour {
    PlayerFearController FearControl;
    Door_Trap_Switch snarecap;
    private void Start()
    {
        snarecap = GetComponentInParent<Door_Trap_Switch>();
    }
    IEnumerator Delay(UnityEngine.Events.UnityAction action)
    {
        yield return new WaitForSeconds(2f);
        action();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FearControl = collision.gameObject.GetComponent<PlayerFearController>();
            if (gameObject.CompareTag("Trap"))
            {
                FearControl.ChangeFear(FearControl.trapdamage, true);
                Destroy(gameObject);
            }
            if (gameObject.CompareTag("Snare") && snarecap.captured == false)
            {
                FearControl.ChangeFear(FearControl.trapdamage, true);
                snarecap.captured = true;
                StartCoroutine(Delay(() => {
                    snarecap.canremove = true;
                }));
            }
        }
    }
}
