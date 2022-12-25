using BoneLib.Nullables;
using SLZ.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZ.Marrow.Data;
using Harmony;
using SLZ;
using SLZ.Marrow.Pool;
using SLZ.Bonelab;

namespace Jevil.Spawning;

/// <summary>
/// Spawning ammo boxes from the pools
/// </summary>
public static class Ammo
{
    static readonly Dictionary<Weight, AssetPool> weightPools = new(3);

    internal static void Init()
    {
        weightPools[Weight.LIGHT] = Barcodes.ToAssetPool(JevilBarcode.AMMO_BOX_LIGHT);
        weightPools[Weight.MEDIUM] = Barcodes.ToAssetPool(JevilBarcode.AMMO_BOX_MEDIUM);
        weightPools[Weight.HEAVY] = Barcodes.ToAssetPool(JevilBarcode.AMMO_BOX_HEAVY);
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
        if (!weightPools.TryGetValue(ammoWgt, out var pool) || pool == null) Init(); // yep
        
        GameObject spawnedAmmo = weightPools[ammoWgt].Spawn(pos, rot, null, false).GetAwaiter().GetResult().gameObject;
        AmmoPickup pickup = spawnedAmmo.GetComponentInChildren<AmmoPickup>();
        pickup.ammoCount = ammoCount;
        return spawnedAmmo;
    }
}
