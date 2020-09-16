using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunMasterScript : MonoBehaviour {
    
    // Variables not taken from the Generator
    // Affected by player/NPC Controller, or set by Designer
    public GameObject goGunOwner;
    public GameObject goMuzzlePoint;
    public GameObject goBullet;
    public bool bGunCooldown = false;

    // Used to control burst and fully-automatic firing
    private int burstCounter = 0;
    private bool fullyAutoOn = false;

    #region Tied to the Editor
    // These are taken from the Generator so should not need to be edited
    public string sGunID = "id";
    public string sGunName = "Gun";
    public int iClipCur = 1;
    public int iClipMax = 1;
    public float fFireSpread = 1;
    public float fFireRange = 1;
    public float fProjSpeed = 1;
    public float fProjLife = 1;
    public float fFireRate = 1;
    public int iBurstAmount = 1;
    public int iPelletAmount = 1;
    public enumProjectileType eProjType = enumProjectileType.Raycast;
    public enumFiringMode eFireMode = enumFiringMode.Singleshot;
    public float fDamage = 1;
    public float fReloadTime = 1;

    // CONSTRUCTOR
    public void SetInitValues(string newID, string newName, float newDamage, int newClip, float newSpread, float newRange, float newSpeed, float newLife, float newRate, int newPellet, float newReloadTime, int newBurst, enumProjectileType newProjType, enumFiringMode newFiringMode)
    {
        sGunID = newID;
        sGunName = newName;
        iClipMax = newClip;
        fFireSpread = newSpread;
        fFireRange = newRange;
        fProjSpeed = newSpeed;
        fProjLife = newLife;
        fFireRate = newRate;
        iPelletAmount = newPellet;
        iClipCur = iClipMax;
        eProjType = newProjType;
        eFireMode = newFiringMode;
        fDamage = newDamage;
        fReloadTime = newReloadTime;
        iBurstAmount = newBurst;
    }
    #endregion

    // Use this for initialization
    void Start ()
    {
        goBullet = (GameObject)Resources.Load("Firestick/Weapons/bullet");
	}

    // Begin/End Firing and Reloading are public so that they can be called by the NPC holding the gun or by the player controller
    public void BeginFiring()
    {
        if (!bGunCooldown)
        {
            fullyAutoOn = true; // Used to enable continuous fire until EndFiring is triggered
            StartCoroutine(gunFiringSequence());
        }
    }
    public void EndFiring()
    {
        // Resets cooldown so weapon can fire again/reload
        bGunCooldown = false;

        // Resets burst counter and fully-auto
        burstCounter = 0;
        fullyAutoOn = false;
    }


    public void Reloading()
    {
        if (!bGunCooldown)
        {
            StartCoroutine(gunReloading());
        }
    }

    protected IEnumerator gunFiringSequence()
    {
        //Debug.Log("In enumerator");
        // If weapon is out of ammo, reload
        if (iClipCur <= 0)
        {
            //Debug.Log("Out of ammo");
            EndFiring();
            StartCoroutine(gunReloading());
        } // Else fire projectile/ray and count burst
        else
        {
            iClipCur--;
            //Debug.Log("Has ammo");
            bGunCooldown = true;

            // If the gun is the projectile firing type, spawn bullet
            if (eProjType == enumProjectileType.Projectile)
            {
                // Repeats for every "pellet" (shotgun/scatter behaviour)
                for (int i = 0; i < iPelletAmount; i++)
                {
                    // Aim offset based on fireSpread
                    // Calculation gives a random value between -fFireSpread and fFireSpread
                    Vector3 aimOffset = goMuzzlePoint.transform.rotation.eulerAngles;
                    aimOffset.x += (((Random.value - 0.5f) * 2) * fFireSpread);
                    aimOffset.y += (((Random.value - 0.5f) * 2) * fFireSpread);
                    aimOffset.z += (((Random.value - 0.5f) * 2) * fFireSpread);

                    // Spawns the bullet, sets its position and rotation to the muzzle, then applies new random offset on top
                    GameObject projBullet = GameObject.Instantiate(goBullet);
                    projBullet.GetComponent<bullScript>().InitValues(fDamage, fProjLife, fProjSpeed);
                    projBullet.transform.position = goMuzzlePoint.transform.position;
                    projBullet.transform.rotation = Quaternion.Euler(aimOffset);

                    // Testing for collision
                    {
                        Physics.IgnoreCollision(projBullet.GetComponent<Collider>(), goGunOwner.GetComponent<Collider>());
                    }
                }
            }
            // If the gun is the hitscan type, do raycast
            else
            {
                for (int i = 0; i < iPelletAmount; i++)
                {
                    // Aim offset based on fireSpread
                    // Calculation gives a random value between -fFireSpread and fFireSpread
                    Vector3 aimOffset = goMuzzlePoint.transform.rotation.eulerAngles;
                    aimOffset.x += (((Random.value - 0.5f) * 2) * fFireSpread);
                    aimOffset.y += (((Random.value - 0.5f) * 2) * fFireSpread);
                    aimOffset.z += (((Random.value - 0.5f) * 2) * fFireSpread);

                    // Casts ray from gun muzzle, adjusts rotation for offset, sends message to first hit
                    RaycastHit hit;
                    if (Physics.Raycast(goMuzzlePoint.transform.position, aimOffset, out hit, fFireRange))
                    {
                        //Debug.Log("Hit a thing");
                        Debug.DrawLine(goMuzzlePoint.transform.position, hit.point, Color.red, 1/fFireRate);

                        // Same line used by projectile when it collides with an object, function name is identical
                        hit.collider.transform.SendMessage("applyDamage", fDamage, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            yield return new WaitForSeconds(1 / fFireRate);

            // If the gun is a burst weapon, it will repeat this coroutine until the counter maxes out
            // If it is fully-automatic, it will continue until the EndFiring function is called
            if ((eFireMode == enumFiringMode.BurstShot) && (burstCounter < iBurstAmount))
            {
                burstCounter++;
                StartCoroutine(gunFiringSequence());
            }
            else if ((eFireMode == enumFiringMode.AutomaticFire) && (fullyAutoOn))
            {
                StartCoroutine(gunFiringSequence());
            }
            else
            {
                EndFiring();
            }
        }
    }

    protected IEnumerator gunReloading()
    {
        iClipCur = 0;
        bGunCooldown = true;
        yield return new WaitForSeconds(fReloadTime);
        iClipCur = iClipMax;
        bGunCooldown = false;
    }
}
