/*
 * Doesn't need to use Collections
 * Does need UnityEditor, which isn't in by default
 */

using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.IO;
using System.Collections.Generic;

// Public class needs to be derived from EditorWindow, not Monobehaviour
public class fs_UnitEditor : EditorWindow
{
    #region UnitGenerator Variables
    string newUnit_ID;
    string newUnit_Name;
    float newUnit_HP = 1;
    bool newUnit_HasShields = false;
    float newUnit_ShieldNum = 1;
    float newUnit_ShieldRechargeRate = 0;
    float newUnit_ShieldWaitTime = 1;
    int newUnit_Faction;
    bool newUnit_HasWeapon;
    gunMasterScript newUnit_Weapon;
    GameObject newUnit_Model;


    // Testing boolean
    bool bGenerationSuccess = true;
    string sGenerationMessage = "Generate Gun";
    #endregion

    // MenuItem adds this to the toolbar at the top, under the specified path
    // "Window" already exists, "Firestick" is the extension name, final piece is 
    [MenuItem("Window/Firestick/Character Editor")]
    public static void ShowWindow()
    {
        GetWindow<fs_UnitEditor>("Character Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Character Editor", EditorStyles.boldLabel);

        newUnit_ID = EditorGUILayout.TextField("ID", newUnit_ID);
        var tt_ID = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_ID, new GUIContent("", "The name that will appear in the hierarchy"));

        newUnit_Name = EditorGUILayout.TextField("Display Name", newUnit_Name);
        var tt_Name = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Name, new GUIContent("", "Proper name in script, can be accessed by things such as UI that don't depend on uniqueness"));

        newUnit_HP = float.Parse(EditorGUILayout.TextField("Health", newUnit_HP.ToString()));
        var tt_HP = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_HP, new GUIContent("", "Health of this character, tune it to match the weapons"));

        newUnit_HasShields = EditorGUILayout.Toggle("Has Shields", newUnit_HasShields);
        var tt_HasShields = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_HasShields, new GUIContent("", "Shields take damage first and can regenerate"));
        if (newUnit_HasShields)
        {
            newUnit_ShieldNum = float.Parse(EditorGUILayout.TextField("Shield Hitpoints", newUnit_ShieldNum.ToString()));
            var tt_ShieldNum = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_ShieldNum, new GUIContent("", "Shield hitpoints, takes damage before Health but otherwise identical purpose"));

            newUnit_ShieldRechargeRate = float.Parse(EditorGUILayout.TextField("Recharge Per Second", newUnit_ShieldRechargeRate.ToString()));
            var tt_ShieldRecharge = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_ShieldRecharge, new GUIContent("", "Number of Shield hitpoints regenerated per second"));

            newUnit_ShieldWaitTime = float.Parse(EditorGUILayout.TextField("Charge Delay", newUnit_ShieldWaitTime.ToString()));
            var tt_ShieldWaitTime = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_ShieldWaitTime, new GUIContent("", "Time after being attacked before shields start charging"));
        }

        newUnit_HasWeapon = EditorGUILayout.Toggle("Has Weapon", newUnit_HasWeapon);
        var tt_HasWeapon = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_HasWeapon, new GUIContent("", "Determines if character has a weapon or not"));
        if (newUnit_HasWeapon)
        {
            newUnit_Weapon = (gunMasterScript)EditorGUILayout.ObjectField(("Weapon Prefab"), newUnit_Weapon, typeof(gunMasterScript), true);
            var tt_Weapon = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_Weapon, new GUIContent("", "Weapon Asset used by this character, dependency injection"));
        }

        newUnit_Faction = int.Parse(EditorGUILayout.TextField("Team", newUnit_Faction.ToString()));
        var tt_Faction = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Faction, new GUIContent("", "Allegiance of this character, determines Friend or Foe"));

        newUnit_Model = (GameObject)EditorGUILayout.ObjectField("Base Object", newUnit_Model, typeof(GameObject), false);
        var tt_Model = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Model, new GUIContent("", "Model for this character, should be an existing prefab"));

        if (GUILayout.Button(sGenerationMessage))
        {
            // Runs a series of tests to ensure all data needed for generation is there
            // Each field is tested for success and an error message is written over the button if failure is found
            // Some fields don't need to be tested however
            if ((newUnit_ID == null) || (newUnit_ID == ""))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Character ID";
            }
            else if (newUnit_HP <= 0)
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Health is 0 or negative";
            }
            else if ((newUnit_HasShields) && (newUnit_ShieldNum <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Shield is 0 or negative";
            }
            else if (newUnit_Model == null)
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Base Object is null";
            }
            else if ((newUnit_HasWeapon)&&(newUnit_Weapon == null))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Weapon slot is empty";
            }

            // All fields are passed, therefore gun can be generated properly
            else
            {
                bGenerationSuccess = true;
                sGenerationMessage = "Success!";
            }

            GenerateCharacter();
        }
    }

    private void GenerateCharacter()
    {
        // Small catch for errors, should never reach this however
        if (!bGenerationSuccess)
        {
            Debug.Log("Error reached while generating character!");
            return;
        }
        Debug.Log("No error found, have a nice day");
        // Creates new gameobject in scene
        GameObject NewCharacter = new GameObject(newUnit_ID);
        // Creates a child object to house the model and its animations, but otherwise nonfunctional
        GameObject BaseModel = GameObject.Instantiate(newUnit_Model);
        BaseModel.transform.SetParent(NewCharacter.transform);

        // Creates NavAgent
        NewCharacter.AddComponent<NavMeshAgent>();

        // Creates a weapon object
        GameObject WeaponBase;
        if (newUnit_HasWeapon)
        {
            // Once the weapon is created, set its parenting, position, etc
            WeaponBase = GameObject.Instantiate(newUnit_Weapon.gameObject);
            WeaponBase.transform.SetParent(NewCharacter.transform);
            WeaponBase.GetComponent<gunMasterScript>().goGunOwner = NewCharacter;
            WeaponBase.transform.position = NewCharacter.transform.position;
            WeaponBase.transform.rotation = NewCharacter.transform.rotation;
        }
        else
            WeaponBase = null;
        // Creates script to control character, then edits its inputs
        char_Masterscript newCharScript = NewCharacter.AddComponent<char_Masterscript>();
        newCharScript.SetInitValues(newUnit_ID, newUnit_Name, newUnit_Faction, newUnit_HP, newUnit_HasShields, newUnit_ShieldNum, newUnit_HasWeapon, WeaponBase);
    }
}