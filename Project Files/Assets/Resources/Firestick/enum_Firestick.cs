using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script is used to house all the Enums used within the generators
 * Each generator's enums are split into different regions for readability
 */

#region Weapon Editor
public enum enumProjectileType
{
    Projectile,
    Raycast
};

public enum enumFiringMode
{
    Singleshot,
    BurstShot,
    AutomaticFire
};
#endregion
