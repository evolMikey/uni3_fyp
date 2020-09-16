/*
 * Doesn't need to use Collections
 * Does need UnityEditor, which isn't in by default
 */

using UnityEngine;
using UnityEditor;
using System.IO;

// Public class needs to be derived from EditorWindow, not Monobehaviour
public class fs_WeaponEditor : EditorWindow
{
    /*
     * Code snippet for adding tooltips to things
        var typeRect = GUILayoutUtility.GetLastRect();
        GUI.Label(typeRect, new GUIContent("", "tooltip text"));
     */

    #region WeaponGen Variables
    // Name and identifiers for weapon
    string newWeapon_ID; // Unique ID, not shown ingame
    string newWeapon_Name; // Name, shown ingame
    GameObject newWeapon_Model = null; // Model shown ingame
    float newWeapon_Damage = 0;

    // Controls the projectile movements
    float newWeapon_Spread; // Weapon spread in degrees
    float newWeapon_Range = 0; // Weapon max range
    float newWeapon_ProjLife = 0; // Projectile lifetime
    float newWeapon_ProjVelocity = 0; // Projectile speed

    // How the gun will behave ingame
    enumFiringMode newWeapon_FiringMode = enumFiringMode.Singleshot; // How the weapon fires; single, burst, auto
    float newWeapon_FireRate; // Rate at which weapon fires, not always used
    int newWeapon_MaxClipSize; // Maximum shots before reloading, zero for no reloading
    float newWeapon_ReloadTime; // How long it takes for the gun to reload in seconds

    // What is actually fired
    enumProjectileType newWeapon_ProjectileType = enumProjectileType.Raycast;
    int newWeapon_PelletAmount = 1;
    int newWeapon_BurstAmount = 1;

    // Testing boolean
    bool bGenerationSuccess = true;
    string sGenerationMessage = "Generate Gun";
    #endregion

    // MenuItem adds this to the toolbar at the top, under the specified path
    // "Window" already exists, "Firestick" is the extension name, final piece is 
    [MenuItem("Window/Firestick/Weapon Editor")]
    public static void ShowWindow()
    {
        GetWindow<fs_WeaponEditor>("Weapon Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Weapon Editor", EditorStyles.boldLabel);


        // Mostly relating to identification
        newWeapon_ID = EditorGUILayout.TextField("Weapon ID", newWeapon_ID);
        var tt_ID = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_ID, new GUIContent("", "The name that will appear in the hierarchy"));

        newWeapon_Name = EditorGUILayout.TextField("Weapon Name", newWeapon_Name);
        var tt_Name = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Name, new GUIContent("", "Proper name in script, this can be used for ingame purposes"));

        newWeapon_Model = (GameObject)EditorGUILayout.ObjectField("Mesh", newWeapon_Model, typeof(GameObject), false);
        var tt_Model = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Model, new GUIContent("", "Pre-existing model of the weapon goes here"));
        
        newWeapon_Damage = float.Parse(EditorGUILayout.TextField("Damage", newWeapon_Damage.ToString()));
        var tt_Damage = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Damage, new GUIContent("", "How much damage the weapon does"));

        // Weapons ammunition info
        newWeapon_MaxClipSize = int.Parse(EditorGUILayout.TextField("Clip Size", newWeapon_MaxClipSize.ToString()));
        var tt_Clip = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Clip, new GUIContent("", "Amount of shots before reloading, leave 0 for infinite"));

        newWeapon_FireRate = float.Parse(EditorGUILayout.TextField("Rate of Fire", newWeapon_FireRate.ToString()));
        var tt_FireRate = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_FireRate, new GUIContent("", "Cooldown between each shot"));


        // Weapons range/spread and what type of projectile it uses
        newWeapon_Spread = float.Parse(EditorGUILayout.TextField("Fire Spread", newWeapon_Spread.ToString()));
        var tt_Spread = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Spread, new GUIContent("", "Firing arc of the weapon, higher value = less accurate"));

        newWeapon_ProjectileType = (enumProjectileType)EditorGUILayout.EnumPopup("Projectile Style", newWeapon_ProjectileType);
        var tt_ProjectileType = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_ProjectileType, new GUIContent("", "Invisible Hitscan vs Physical Projectile"));

        if (newWeapon_ProjectileType == enumProjectileType.Raycast)
        {
            newWeapon_Range = float.Parse(EditorGUILayout.TextField("Weapon Range", newWeapon_Range.ToString()));
            var tt_Range = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_Range, new GUIContent("", "If hitscan, how far away can it hit?"));
        }
        else
        {
            newWeapon_ProjLife = float.Parse(EditorGUILayout.TextField("Projectile Life", newWeapon_ProjLife.ToString()));
            var tt_Life = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_Life, new GUIContent("", "If Projectile, how long does the bullet live?"));

            newWeapon_ProjVelocity = float.Parse(EditorGUILayout.TextField("Projectile Speed", newWeapon_ProjVelocity.ToString()));
            var tt_Velocity = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_Velocity, new GUIContent("", "If Projectile, how fast does the bullet move?"));
        }

        newWeapon_PelletAmount = int.Parse(EditorGUILayout.TextField("Pellet Amount", newWeapon_PelletAmount.ToString()));
        var tt_Pellet = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Pellet, new GUIContent("", "For shotgun-like guns, how many pellets are fired per shot"));

        newWeapon_ReloadTime = float.Parse(EditorGUILayout.TextField("Reload Time", newWeapon_ReloadTime.ToString()));
        var tt_Reload = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Reload, new GUIContent("", "How long it takes for the gun to reload"));


        // The way the gun fires (affects what happens when the "fire" signal is given)
        newWeapon_FiringMode = (enumFiringMode)EditorGUILayout.EnumPopup("Firing Style", newWeapon_FiringMode);
        var tt_FiringMode = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_FiringMode, new GUIContent("", "How the weapon acts when fired, can it continue firing if the trigger is held?"));

        if (newWeapon_FiringMode == enumFiringMode.BurstShot)
        {
            newWeapon_BurstAmount = int.Parse(EditorGUILayout.TextField("Burst-fire Amount", newWeapon_BurstAmount.ToString()));
            var tt_BurstAmount = GUILayoutUtility.GetLastRect();
            GUI.Label(tt_BurstAmount, new GUIContent("", "How many shots are fired from a single click"));

        }

        if (GUILayout.Button(sGenerationMessage))
        {
            // Runs a series of tests to ensure all data needed for generation is there
            // Each field is tested for success and an error message is written over the button if failure is found
            // Some fields don't need to be tested however
            if ((newWeapon_ID == null) || (newWeapon_ID == ""))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Weapon ID";
            }
            else if ((newWeapon_Name == null) || (newWeapon_Name == ""))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Weapon Name";
            }
            else if ((newWeapon_Model == null))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Weapon Model";
            }
            else if (newWeapon_Damage < 0)
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Damage is negative";
            }
            else if ((newWeapon_FireRate <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Firerate is 0 or negative";
            }
            else if ((newWeapon_MaxClipSize < 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Clip size is negative";
            }
            else if ((newWeapon_Spread < 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Spread is negative";
            }
            else if ((newWeapon_ProjectileType == enumProjectileType.Raycast) && (newWeapon_Range <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Range is 0 or negative";
            }
            else if ((newWeapon_ProjectileType == enumProjectileType.Projectile) && (newWeapon_ProjVelocity <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Projectile Speed is 0 or negative";
            }
            else if ((newWeapon_ProjectileType == enumProjectileType.Projectile) && (newWeapon_ProjLife <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Projectile Life is 0 or negative";
            }
            else if (newWeapon_PelletAmount <= 0)
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Pellets are either 0 or negative";
            }
            else if (newWeapon_ReloadTime <= 0)
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Reload Time is either 0 or negative";
            }
            else if ((newWeapon_FiringMode == enumFiringMode.BurstShot) && (newWeapon_BurstAmount <= 0))
            {
                bGenerationSuccess = false;
                sGenerationMessage = "Error: Burst amount is either 0 or negative";
            }

            // All fields are passed, therefore gun can be generated properly
            else
            {
                bGenerationSuccess = true;
                sGenerationMessage = "Success!";
            }

            GenerateWeapon();
        }
        var tt_Generate = GUILayoutUtility.GetLastRect();
        GUI.Label(tt_Generate, new GUIContent("", "Create weapon using these parameters"));

    }

    private void GenerateWeapon()
    {
        // Small catch for errors, should never reach this however
        if (!bGenerationSuccess)
        {
            Debug.Log("Error reached while generating gun!");
            return;
        }
        else
        {
            Debug.Log("No error found, have a nice day");
            // Creates new gameobject in scene
            GameObject NewWeapon = new GameObject(newWeapon_ID);
            // Creates child object for nonfunctional art
            GameObject BaseModel = GameObject.Instantiate(newWeapon_Model);
            BaseModel.transform.SetParent(NewWeapon.transform);
            BaseModel.name = "Weapon Model";
            // Creates child object for muzzle-point
            GameObject MuzzlePoint = new GameObject("GunMuzzle");
            MuzzlePoint.transform.SetParent(NewWeapon.transform);
            gunMasterScript NewScript = NewWeapon.AddComponent<gunMasterScript>();
            NewScript.SetInitValues(newWeapon_ID, newWeapon_Name, newWeapon_Damage, newWeapon_MaxClipSize, newWeapon_Spread, newWeapon_Range, newWeapon_ProjVelocity, newWeapon_ProjLife, newWeapon_FireRate, newWeapon_PelletAmount, newWeapon_ReloadTime, newWeapon_BurstAmount, newWeapon_ProjectileType, newWeapon_FiringMode);
            NewScript.goMuzzlePoint = MuzzlePoint;
        }
    }
}