/*
 * Doesn't need to use Collections
 * Does need UnityEditor, which isn't in by default
 */

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class fs_SquadEditor : EditorWindow
{
    #region Squad Size and Array
    // Starts with the List, makes sure it works
    List<char_Masterscript> newSquad_Units = new List<char_Masterscript>();

    // Small getter/setter included with the value to add some degree of error-checking
    int _newSquad_Size;
    public int newSquad_Size
    {
        get { return _newSquad_Size; }
        set
        {
            // If the new value does not equal the original...
            if (value != _newSquad_Size)
            {
                // If the value is 1 or more, sets value and resets array
                if (value >= 1)
                {
                    _newSquad_Size = value;

                    // Clears List, sets its Capacity to new value, then fills with nulls
                    newSquad_Units.Clear();
                    newSquad_Units.Capacity = value;

                    for (int i = 0; i < newSquad_Units.Capacity; i++)
                    {
                        newSquad_Units.Add(null);
                    }
                }
            }
        }
    }
    #endregion
    private int newSquad_Faction = 0;
    private string newSquad_ID = "";
    private int newSquad_PatrolSize = 0;
    private bool newSquad_PatrolLoop = false;


    bool bGenerationSucces = true;
    string sGenerationMessage = "Generate Squad";

    // MenuItem adds this to the toolbar at the top, under the specified path
    // "Window" already exists, "Firestick" is the extension name, final piece is 
    [MenuItem("Window/Firestick/Squad Editor")]
    public static void ShowWindow()
    {
        GetWindow<fs_SquadEditor>("Squad Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Squad", EditorStyles.boldLabel);

        // Squad Name
        newSquad_ID = EditorGUILayout.TextField("ID", newSquad_ID);
        var tt_ID = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_ID, new GUIContent("", "The name that will appear in the hierarchy"));

        // Size of the Squad
        newSquad_Size = int.Parse(EditorGUILayout.TextField("Squad Size", newSquad_Size.ToString()));
        var tt_SquadSize = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_SquadSize, new GUIContent("", "Number of Characters in Squad"));

        // For loop that goes through all elements
        for (int i = 0; i < newSquad_Units.Capacity; i++)
        {
            newSquad_Units[i] = (char_Masterscript)EditorGUILayout.ObjectField(("Unit: " + i), newSquad_Units[i], typeof(char_Masterscript), true);
            var tt_SquadMember = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_SquadMember, new GUIContent("", "Squad member " + i));
        }

        newSquad_Faction = int.Parse(EditorGUILayout.TextField("Faction", newSquad_Faction.ToString()));
        var tt_SquadFaction = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_SquadFaction, new GUIContent("", "Overrides faction in each member"));


        // Locations to move to
        newSquad_PatrolSize = int.Parse(EditorGUILayout.TextField("Number of Patrol Points", newSquad_PatrolSize.ToString()));
        var tt_CommandSize = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_CommandSize, new GUIContent("", "How many patrol points will be created for this squad"));

        newSquad_PatrolLoop = EditorGUILayout.Toggle("Looping Patrol", newSquad_PatrolLoop);
        var tt_PatrolLoop = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_PatrolLoop, new GUIContent("", "Will Squad move to first patrol point after reaching the last?"));

        // Generate Squad button
        if (GUILayout.Button(sGenerationMessage))
        {
            // Same as Weapon/Unit version, runs through a list of checks before allowing creation
            for (int i = 0; i < newSquad_Size; i++)
            {
                if (newSquad_Units[i])
                {
                    bGenerationSucces = false;
                    sGenerationMessage = ("Unit " + i + " is empty");
                }
            }
            if (newSquad_Size <= 0)
            {
                bGenerationSucces = false;
                sGenerationMessage = "Squad Size is 0 or negative";
            }
            else if ((newSquad_ID == "") || (newSquad_ID == null))
            {
                bGenerationSucces = false;
                sGenerationMessage = "ID is empty or null";
            }

            // All fields are passed, therefore gun can be generated properly
            else
            {
                bGenerationSucces = true;
                sGenerationMessage = "Success!";
            }

            GenerateSquad();
        }
    }

    private void GenerateSquad()
    {
        // Small catch for errors, should never trigger however
        if (!bGenerationSucces)
        {
            Debug.Log("Error reached while generating squad");
            return;
        }

        Debug.Log("No error found, have a nice day");

        // Creates initial game object to house everything, as well as script
        GameObject NewSquad = new GameObject(newSquad_ID);
        squad_Masterscript newScript = NewSquad.AddComponent<squad_Masterscript>();
        newScript.InitSquad(newSquad_ID, newSquad_PatrolLoop);
        GameObject squadChars = new GameObject("Squad Characters");
        squadChars.transform.SetParent(NewSquad.transform);
        GameObject squadLocs = new GameObject("Patrol Points");
        squadLocs.transform.SetParent(NewSquad.transform);

        // Loop for creaitng each character
        for (int i = 0; i < newSquad_Size; i++)
        {
            // Instantiates characters
            GameObject newChar;
            newChar = GameObject.Instantiate(newSquad_Units[i].gameObject);
            newChar.transform.SetParent(squadChars.transform);
            // Sets faction to match the squad
            newChar.GetComponent<char_Masterscript>().ChangeFaction(newSquad_Faction);
            newChar.GetComponent<char_Masterscript>().characterSquad = newScript;
            // Adds character to the Squad's roster
            newScript.AddSquadMember(newChar.GetComponent<char_Masterscript>());
        }

        // Loop for creating each Patrol Point
        for (int i = 0; i < newSquad_PatrolSize; i++)
        {
            // Creates empty objects
            GameObject newPatrol = new GameObject(i.ToString());
            newPatrol.transform.SetParent(squadLocs.transform);
            newScript.AddPatrolPoint(newPatrol.transform);
        }

    }
}