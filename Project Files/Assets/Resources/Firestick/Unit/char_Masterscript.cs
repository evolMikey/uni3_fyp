using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class char_Masterscript : MonoBehaviour {

    #region Character Stats
    public string characterID;
    public string characterName;
    public int characterFaction;
    public float characterCurHit;
    public float characterMaxHit;
    public bool characterHasShield;
    public float characterCurShield;
    public float characterMaxShield;
    public bool characterHasWeapon;
    public float characterTargetRange = 1;
    public GameObject characterWeapon;
    public squad_Masterscript characterSquad;
    public NavMeshAgent navAgent;

    private bool willFire = true;
    private bool willMove = true;
    public SphereCollider targetRadius;
    public List<GameObject> targetList;

    private void Start()
    {
        // Sets reference for NavMeshAgent
        if (gameObject.GetComponent<NavMeshAgent>() != null)
            navAgent = gameObject.GetComponent<NavMeshAgent>();

        // Creates trigger sphere around character for detecting other units
        targetRadius = gameObject.AddComponent<SphereCollider>();
        targetRadius.isTrigger = true;
        targetRadius.radius = characterTargetRange;

        Rigidbody rigBod = gameObject.AddComponent<Rigidbody>();
        rigBod.isKinematic = true;
        CapsuleCollider capCol = gameObject.AddComponent<CapsuleCollider>();
        capCol.radius = 0.5f;
        capCol.height = 2;
        capCol.center = new Vector3(0, 1, 0);

        // Inits the List
        targetList.Clear();
    }

    private void Update()
    {
        if (willMove)
        {
            if ((characterSquad != null) && (navAgent.remainingDistance <= 0.5f))
            {
                characterSquad.NextWaypoint();
            }
        }
    }

    public void SetMoveStatus(bool canMove)
    {
        willMove = canMove;
    }

    #region Colliders adding/removing to target list
    private void OnTriggerEnter(Collider other)
    {
        // If the other object is a character and not on same team, add to list
        if (other.gameObject.GetComponent<char_Masterscript>() != null)
        {
            if (other.gameObject.GetComponent<char_Masterscript>().characterFaction != characterFaction)
            {
                targetList.Capacity = targetList.Count + 1;
                targetList.Add(other.gameObject);

                // If not currently firing on something, fire on this new target
                if (willFire)
                {
                    AcquireTarget();
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<char_Masterscript>() != null)
        {
            if (targetList.Contains(other.gameObject))
            {
                targetList.Remove(other.gameObject);
            }
        }
    }
    #endregion

    public void SetInitValues(string newID, string newName, int newFaction, float newHP, bool hasShield, float newShield, bool hasWeapon, GameObject newWeapon)
    {
        characterID = newID;
        characterName = newName;
        characterFaction = newFaction;
        characterCurHit = newHP;
        characterMaxHit = newHP;

        characterHasShield = hasShield;
        characterHasWeapon = hasWeapon;

        if (characterHasShield)
        {
            characterCurShield = newShield;
            characterMaxShield = newShield;
        }
        else
        {
            characterCurShield = 0;
            characterMaxShield = 0;
        }

        if ((characterHasWeapon) && (newWeapon.GetComponent<gunMasterScript>() != null))
        {
            characterWeapon = newWeapon;
            // Calculates distance of the weapon, either by projectile speed*life, or the length of the raycast
            if (newWeapon.GetComponent<gunMasterScript>().eProjType == enumProjectileType.Projectile)
            {
                characterTargetRange = newWeapon.GetComponent<gunMasterScript>().fProjLife * newWeapon.GetComponent<gunMasterScript>().fProjSpeed / 20;
                targetRadius.radius = characterTargetRange;
            }
            else if (newWeapon.GetComponent<gunMasterScript>().fFireRange > 0)
            {
                characterTargetRange = newWeapon.GetComponent<gunMasterScript>().fFireRange / 20;
                targetRadius.radius = characterTargetRange;
            }
            else
            {
                characterTargetRange = 10;
                targetRadius.radius = characterTargetRange;
            }
        }
        else
        {
            characterWeapon = null;
            characterTargetRange = 0;
            targetRadius.radius = 1;
        }
    }
    #endregion

    #region Setters and Utilities
    public void ChangeName(string newName)
    {
        characterName = newName;
    }
    public void ChangeFaction(int newFaction)
    {
        characterFaction = newFaction;
    }
    public void SetMaxHitPoints(float newHitPoints)
    {
        characterMaxHit = newHitPoints;
    }
    public void SetCurHitPoints(float newHit)
    {
        characterCurHit = newHit;
    }
    public void RemoveWeapon()
    {
        characterHasWeapon = false;
        if (characterWeapon != null)
        {
            Destroy(characterWeapon);
        }
    }
    public void DropWeapon()
    {
        characterHasWeapon = false;
        if (characterWeapon != null)
        {
            characterWeapon.transform.SetParent(null, true);
        }
    }
    public void SetNewWeapon(GameObject newWeapon)
    {
        DropWeapon();
        characterHasWeapon = true;
        characterWeapon = newWeapon;
        characterWeapon.transform.SetParent(gameObject.transform);
    }
    public void SetSquad(squad_Masterscript newSquad)
    {
        characterSquad = newSquad;
    }
    #endregion


    public void applyDamage(float dmgAmount)
    {
        // Temporary value, in case it needs to be split over shields and health
        float tempDmg = dmgAmount;

        // If shields can take all the damage
        if ((characterHasShield) && (characterCurShield > dmgAmount))
        {
            characterCurShield -= dmgAmount;
        }
        // Shields can't take all damage
        else if (characterHasShield)
        {
            // Remove all shields, then apply rest of damage to health
            tempDmg = dmgAmount - characterCurShield;
            characterCurShield = 0;
            characterCurHit -= tempDmg;
            // If character is dead, run death script
            if (characterCurHit <= 0)
                deathFunc();
        }
        else
        {
            characterCurHit -= dmgAmount;
            if (characterCurHit <= 0)
                deathFunc();

        }
    }
    public void deathFunc()
    {
        // Sets health to 0, in case this was called outside of damaging scripts
        characterCurHit = 0;
        // Finds squad and updates it before deleting self
        if (characterSquad != null)
        {
            characterSquad.MemberKilled();
        }
        Destroy(gameObject, 0.3f);
    }

    // Checks distnace, points gun at target, fires
    public void AcquireTarget()
    {
        if (characterWeapon != null)
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                // Removes destroyed targets that can't be used
                if (targetList[i] == null)
                    targetList.RemoveAt(i);
                else
                {
                    navAgent.updateRotation = false;
                    gameObject.transform.LookAt(targetList[i].transform.position);
                    characterWeapon.transform.LookAt(targetList[i].transform.position);
                    StartCoroutine(FireWeaponLoop());
                }
            }
        }
    }
    // Tells gun to fire, behaviour varies based on firingmode
    private IEnumerator FireWeaponLoop()
    {
        // Conditions: character must have a gun, gun must have the right script, character must be willing to fire
        if ((characterHasWeapon) && (characterWeapon.GetComponent<gunMasterScript>() != null) && (willFire))
        {
            navAgent.isStopped = true;

            gunMasterScript gunScript = characterWeapon.GetComponent<gunMasterScript>(); // Will work due to middle condition above
            enumFiringMode gunMode = gunScript.eFireMode;
            float fireRate = (1 / gunScript.fFireRate);
            willFire = false; // Prevents this function being called repeatedly
            switch (gunMode) // Behaviour is different depending on firing style
            {
                case (enumFiringMode.Singleshot): // Fires single shots repeatedly for a while, then wait
                    {
                        // Fires a random number of times, waiting 1 second between each shot
                        int fireAmount = Random.Range(1, gunScript.iClipCur);
                        for (int i = 0; i < fireAmount; i++)
                        {
                            gunScript.BeginFiring();
                            yield return new WaitForSeconds(fireRate + 1f);
                        }
                        // Makes character reload before firing again
                        gunScript.Reloading();
                        yield return new WaitForSeconds(gunScript.fReloadTime + 1f);
                        break;
                    }

                case (enumFiringMode.BurstShot): // Similar to single, except it considers number of rounds used in burst
                    {
                        // Fires a random number of times, waiting 1 second between each shot
                        int fireAmount = Random.Range(1, (gunScript.iClipCur / gunScript.iBurstAmount));
                        for (int i = 0; i < fireAmount; i++)
                        {
                            gunScript.BeginFiring();
                            yield return new WaitForSeconds(fireRate + 1f);
                        }
                        // Makes character reload before firing again
                        gunScript.Reloading();
                        yield return new WaitForSeconds(gunScript.fReloadTime + 1f);
                        break;
                    }

                case (enumFiringMode.AutomaticFire): // Fires long burst then reloads
                    {
                        // Begins firing, waits random time between 2 and 10 seconds, then stops
                        gunScript.BeginFiring();
                        yield return new WaitForSeconds(Random.Range(2, 10));
                        gunScript.EndFiring();

                        // Makes character reload before firing again
                        gunScript.Reloading();
                        yield return new WaitForSeconds(gunScript.fReloadTime + 1f);
                        break;
                    }
            }
            yield return new WaitForSeconds(0.5f);
            willFire = true;
            // Resets moving/rotation behaviour
            navAgent.updateRotation = true;
            navAgent.isStopped = false;
            // Checks if there are any new targets in the area
            AcquireTarget();
        }
    }

    public void MoveToLocation(Vector3 gotoPos)
    {
        navAgent.ResetPath();
        navAgent.SetDestination(gotoPos);
    }

}
