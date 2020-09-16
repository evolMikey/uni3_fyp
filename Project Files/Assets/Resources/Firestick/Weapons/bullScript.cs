using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullScript : MonoBehaviour
{

    // Basic values for the bullet
    public float bulletDmg;
    public float bulletLife;
    public float bulletSpeed;

    // Initialises bullet, called when spawned
    public void InitValues(float dmg, float life, float speed)
    {
        bulletDmg = dmg;
        bulletLife = life;
        bulletSpeed = speed;
        gameObject.GetComponent<ConstantForce>().relativeForce = new Vector3(0, 0, speed);
    }

    private void Start()
    {
        if (bulletLife > 0)
        {
            // Immediately begins countdown of life
            Destroy(gameObject, bulletLife);
        }
        else
        {
            // Failsafe, if life is 0 or negative (it shouldnt be) then kill after 1 second
            Destroy(gameObject, 1f);
        }
    }

    // On collision with anything, sends out "do damage" message then deletes self
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<bullScript>() == null)
        {
            if (other.gameObject.GetComponent<char_Masterscript>() != null)
            {
                Debug.Log("Hit!");
                other.transform.SendMessage("applyDamage", bulletDmg, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
            }
        }
    }
}