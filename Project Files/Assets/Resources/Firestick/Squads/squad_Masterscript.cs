using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class squad_Masterscript : MonoBehaviour {

    #region Squad membership
    // List used to house an undetermined number of members
    public List<char_Masterscript> squad_Members = new List<char_Masterscript>();
    public List<Transform> squad_PatrolPoints = new List<Transform>();
    private int patrolCounter = 0;
    public bool bPatrolLoops;
    int squad_Size = 0;
    string squadID = "";
    #endregion

    public void InitSquad(string newID, bool newLoop)
    {
        squadID = newID;
        bPatrolLoops = newLoop;
    }
    public void AddSquadMember(char_Masterscript newChara)
    {
        // Will be called by each generated character in the Squad
        squad_Members.Add(newChara);
        squad_Size = squad_Members.Count;
    }
    public void AddPatrolPoint(Transform newPoint)
    {
        // Same as Squad characters, will be called by each generated patrol point
        squad_PatrolPoints.Add(newPoint);
    }

    private void Start()
    {
        NextWaypoint();
    }

    public void NextWaypoint()
    {
        // If there are multiple patrol points
        if (squad_PatrolPoints.Count > 0)
        {

            // Send to every member of Squad
            for (int i = 0; i < squad_Members.Count; i++)
            {
                squad_Members[i].MoveToLocation(squad_PatrolPoints[patrolCounter].position);
            }

            // Increases counter
            patrolCounter++;

            // If maxed out, either resets to 0 (for loop) or stays at max (nonloop)
            if (patrolCounter >= squad_PatrolPoints.Count)
            {
                if (bPatrolLoops)
                    patrolCounter = 0;
                else
                {
                    patrolCounter = squad_PatrolPoints.Count;
                    for (int i = 0; i < squad_Members.Count; i++)
                    {
                        squad_Members[i].SetMoveStatus(false);
                    }
                }
            }
            Debug.Log(patrolCounter);
        }
    }

    // Called when a character is killed
    public void MemberKilled()
    {
        foreach (char_Masterscript characterMember in squad_Members)
        {
            // If character is dead, remove from the squad
            if (characterMember.characterCurHit <= 0)
            {
                squad_Members.Remove(characterMember);
                squad_Size = squad_Members.Count;
            }
            // If everyone is dead, delete the squad gameObject
            if (squad_Members.Count <= 0)
            {
                Debug.Log("Squad " + squadID + " has been destroyed!");
                Destroy(gameObject);
            }
        }
    }
}
