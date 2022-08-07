using ModThatIsNotMod.Nullables;
using StressLevelZero;
using StressLevelZero.Combat;
using StressLevelZero.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Spawning;

/// <summary>
/// Spawning ammo boxes from the pools
/// </summary>
public static class Ammo
{
    static Dictionary<Weight, Pool> ammoPools = new Dictionary<Weight, Pool>();

    internal static void Init()
    {
        ammoPools.Clear();
        ammoPools[Weight.LIGHT] = PoolManager.GetPool("Ammo Box Small 2500");
        ammoPools[Weight.MEDIUM] = PoolManager.GetPool("Ammo Box Medium 2500");
    }

    /// <summary>
    /// Redirect to <see cref="Spawn(Weight, int, Vector3, Quaternion)"/>, using <see cref="Quaternion.identity"/>.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static GameObject Spawn(Weight ammoWgt, int ammoCount, Vector3 pos) => Spawn(ammoWgt, ammoCount, pos, Quaternion.identity);

    /// <summary>
    /// Spawns an ammo box of weight <paramref name="ammoWgt"/> that gives the player <paramref name="ammoCount"/> ammo of that weight.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <param name="rot"> The rotation to assign the ammo box.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static GameObject Spawn(Weight ammoWgt, int ammoCount, Vector3 pos, Quaternion rot)
    {
#if DEBUG
        if (ammoWgt == Weight.HEAVY) 
            JeviLib.Warn("Cannot spawn Heavy ammo, JeviLib does not grab the pool for it! This operation will throw a KeyNotFoundException!");
#endif
        Pool pool = ammoPools[ammoWgt];
        
        //GameObject spawnedAmmo = pool.InstantiatePoolee(pos, rot).gameObject; changed b/c instantiates new gameobject (no way fr?)
        GameObject spawnedAmmo = pool.Spawn(pos, rot, null, false);
        AmmoPickup pickup = spawnedAmmo.GetComponentInChildren<AmmoPickup>();
        pickup.ammoCount = ammoCount;
        return spawnedAmmo;
    }
}
