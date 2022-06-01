using MelonLoader.Assertions;
using PuppetMasta;
using StressLevelZero.AI;
using StressLevelZero.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Spawning;

/// <summary>
/// Class responsible for spawning EarlyExits.
/// <para>NPCs spawned through this class will have an infinite aggro distance and thus will never break aggro due to distance from their starting location.</para>
/// </summary>
public static class Zombies
{
    private static Pool zombiePool;

    static readonly Dictionary<ZombieType, BaseEnemyConfig> zombieConfigs = new(Enum.GetValues(typeof(ZombieType)).Length);

    internal static void Init()
    {
        byte[] bundleRaw = JeviLib.instance.Assembly.GetEmbeddedResource("Jevil.Resources.EnemyConfigs.bundle");
        AssetBundle bundle = AssetBundle.LoadFromMemory(bundleRaw);
#if DEBUG
        JeviLib.Log("Loaded NPC configs bundle, here's everything we have");
        foreach (string path in bundle.GetAllAssetNames()) JeviLib.Log(" - " + path);
#endif

        zombieConfigs.Add(ZombieType.FAST_NOTHROW,      bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_fast_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.FAST_NOTHROW_TANK, bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_fast_nothrow_Tank.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.FAST_THROW,        bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_fast_throw.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.MED_NOTHROW,       bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_fast_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.MED_THROW,         bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_med_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.SLOW_THROW,        bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_slow_throw.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.SLOW_THROW_TANK,   bundle.LoadAsset<BaseEnemyConfig>("assets/export/enemyconfigs/earlyexit_slow_throw_tank.asset").Cast<BaseEnemyConfig>());

        foreach (BaseEnemyConfig conf in zombieConfigs.Values)
        {
#if DEBUG
            JeviLib.Log("Changing hideflags of " + (conf != null ? conf.name : "<null>"));
#endif
            GameObject.DontDestroyOnLoad(conf);
            conf.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        }

#if DEBUG
        JeviLib.Log("Loaded " + zombieConfigs.Count + " configs and set their hideflags");
#endif
    }

    /// <summary>
    /// Spawns a zombie of type <paramref name="zt"/> zombie at <paramref name="pos"/>.
    /// </summary>
    /// <param name="pos">The position to spawn the zombie at.</param>
    /// <param name="autoAggro">Whether to have the zombie aggroed at the player when its spawned. Defaults to true.</param>
    /// <param name="zt">The zombie type to spawn. Defaults to <see cref="ZombieType.MED_NOTHROW"/>.</param>
    /// <returns>The inactive zombie's root gameobject.</returns>
    public static GameObject Spawn(Vector3 pos, bool autoAggro = true, ZombieType zt = ZombieType.MED_NOTHROW) => Spawn(pos, Quaternion.identity, zt, autoAggro);

    /// <summary>
    /// Spawns a zombie of type <paramref name="zt"/> zombie at <paramref name="pos"/>.
    /// </summary>
    /// <param name="pos">The position to spawn the zombie at.</param>
    /// <param name="rot">The rotation to assign to the spawned zombie.</param>
    /// <param name="zt">The zombie type to spawn.</param>
    /// <param name="aggro">Whether to have the zombie aggroed at the player when its spawned. Defaults to true.</param>
    /// <returns>The inactive zombie's root gameobject.</returns>
    public static GameObject Spawn(Vector3 pos, Quaternion rot, ZombieType zt, bool aggro)
    {
        if (zombiePool == null) zombiePool = GameObject.FindObjectsOfType<Pool>().FirstOrDefault(p => p.name == "pool - Ford Early Exit Headset");
#if DEBUG
        LemonAssert.IsFalse(zombieConfigs[0] == null, "Zombie configs should not be null! The first one was found to be null! Get the lib developer to fix this!");
#endif
        BaseEnemyConfig conf = zombieConfigs[zt];

        GameObject spawned = zombiePool.InstantiatePoolee(pos, rot).gameObject;
        AIBrain braiiinnnsssss = spawned.GetComponentInChildren<AIBrain>();
        
        // do 2 of em because im not sure what the difference is lol
        braiiinnnsssss.SetBaseConfig(conf);
        conf.ApplyTo(braiiinnnsssss.behaviour);

        return spawned;
    } 
}
/*
- Assets/Export/EnemyConfigs/EarlyExit_Slow_Throw_Tank.asset
- Assets/Export/EnemyConfigs/Ford_CourtsFords_1.asset
- Assets/Export/EnemyConfigs/Ford_KingArena_1.asset
- Assets/Export/EnemyConfigs/OmniHazmat_ChargeInaccurate_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Med_Throw.asset
- Assets/Export/EnemyConfigs/NullBody_5mVision_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Falling_Med_3mVision.asset
- Assets/Export/EnemyConfigs/EarlyExit_Med_NoThrow.asset
- Assets/Export/EnemyConfigs/EarlyExit_TowerBoss_NoThrow.asset
- Assets/Export/EnemyConfigs/Dead.asset
- Assets/Export/EnemyConfigs/EarlyExit_Fast_Throw.asset
- Assets/Export/EnemyConfigs/NullBody_TowerBoss.asset
- Assets/Export/EnemyConfigs/Ford_DungeonFords_1.asset
- Assets/Export/EnemyConfigs/NullBody_NoVision_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Slow_Throw.asset
- Assets/Export/EnemyConfigs/NullBody_3mVision_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Falling_3mVision.asset
- Assets/Export/EnemyConfigs/OmniHazmat_ShootingRange.asset
- Assets/Export/EnemyConfigs/OmniHazmat_PerchedGunner_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_TowerBoss_Throw.asset
- Assets/Export/EnemyConfigs/Ford_VrJunkie_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Fast_NoThrow.asset
- Assets/Export/EnemyConfigs/Ford_KingThrone_1.asset
- Assets/Export/EnemyConfigs/EarlyExit_Fast_NoThrow_Tank.asset
- Assets/Export/EnemyConfigs/OmniHazmat_Default_1.asset
*/