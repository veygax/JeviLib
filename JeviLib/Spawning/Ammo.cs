using Jevil;
using BoneLib.Nullables;
using System.Collections.Generic;
using UnityEngine;
using SLZ.Marrow.Data;
using SLZ.Bonelab;
using SLZ.Marrow.Pool;
using System.Threading.Tasks;

namespace Jevil.Spawning;

/// <summary>
/// Spawning ammo boxes from the pools
/// </summary>
public static class Ammo
{
    static readonly Dictionary<Weight, Spawnable> spawnableWeights = new(3);

    internal static void Init()
    {
        spawnableWeights[Weight.LIGHT] = Barcodes.ToSpawnable(JevilBarcode.AMMO_BOX_LIGHT);
        spawnableWeights[Weight.MEDIUM] = Barcodes.ToSpawnable(JevilBarcode.AMMO_BOX_MEDIUM);
        spawnableWeights[Weight.HEAVY] = Barcodes.ToSpawnable(JevilBarcode.AMMO_BOX_HEAVY);
    }

    /// <summary>
    /// Redirect to <see cref="Spawn(Weight, int, Vector3, Quaternion)"/>, using <see cref="Quaternion.identity"/>.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static Task<GameObject> Spawn(Weight ammoWgt, int ammoCount, Vector3 pos) => Spawn(ammoWgt, ammoCount, pos, Quaternion.identity);

    /// <summary>
    /// Spawns an ammo box of weight <paramref name="ammoWgt"/> that gives the player <paramref name="ammoCount"/> ammo of that weight.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <param name="rot"> The rotation to assign the ammo box.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static async Task<GameObject> Spawn(Weight ammoWgt, int ammoCount, Vector3 pos, Quaternion rot)
    {
        if (!spawnableWeights.TryGetValue(ammoWgt, out var spawnable) || spawnable.WasCollected || spawnable == null) Init(); // yep


        AssetPoolee spawnedAmmo = await NullableMethodExtensions.PoolManager_SpawnAsync(spawnableWeights[ammoWgt], pos, rot);
        AmmoPickup pickup = spawnedAmmo.GetComponentInChildren<AmmoPickup>();
        pickup.ammoCount = ammoCount;
        return spawnedAmmo.gameObject;
    }
}
