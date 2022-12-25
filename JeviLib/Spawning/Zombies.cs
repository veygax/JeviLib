using MelonLoader.Assertions;
using PuppetMasta;
using BoneLib.Nullables;
using SLZ.AI;
using SLZ.Marrow.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SLZ.Marrow.Data;
using SLZ.Marrow.Warehouse;

namespace Jevil.Spawning;

/// <summary>
/// Class responsible for spawning EarlyExits.
/// <para>NPCs spawned through this class will have an infinite aggro distance and thus will never break aggro due to distance from their starting location.</para>
/// </summary>
public static class Zombies
{
    static AssetPool zombiePool;
    static TriggerRefProxy playerProxy;
    static readonly Dictionary<ZombieType, BaseEnemyConfig> zombieConfigs = new(Enum.GetValues(typeof(ZombieType)).Length);

    internal static void Init()
    {
        bool isAutoCaching = Instances<AIBrain>.TryAutoCache();
#if DEBUG
        JeviLib.Log($"{(isAutoCaching ? "Succeeded" : "Failed")} in autocaching {nameof(AIBrain)}'s");
#endif
        byte[] bundleRaw = JeviLib.instance.Assembly.GetEmbeddedResource("Jevil.Resources.EnemyConfigs.bundle");
        AssetBundle bundle = AssetBundle.LoadFromMemory(bundleRaw);
#if DEBUG
        JeviLib.Log("Loaded NPC configs bundle, here's everything we have");
        foreach (string path in bundle.GetAllAssetNames()) JeviLib.Log(" - " + path);
#endif

        zombieConfigs.Add(ZombieType.FAST_NOTHROW,      bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_fast_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.FAST_NOTHROW_TANK, bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_fast_nothrow_Tank.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.FAST_THROW,        bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_fast_throw.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.MED_NOTHROW,       bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_fast_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.MED_THROW,         bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_med_nothrow.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.SLOW_THROW,        bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_slow_throw.asset").Cast<BaseEnemyConfig>());
        zombieConfigs.Add(ZombieType.SLOW_THROW_TANK,   bundle.LoadAsset("assets/export/enemyconfigs/earlyexit_slow_throw_tank.asset").Cast<BaseEnemyConfig>());

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
        if (zombiePool == null)
            zombiePool = Barcodes.ToAssetPool(JevilBarcode.EARLY_EXIT_ZOMBIE);
        if (playerProxy.INOC()) 
            playerProxy = GameObject.FindObjectsOfType<TriggerRefProxy>().FirstOrDefault(t => t.transform.IsChildOfRigManager());
#if DEBUG
        LemonAssert.IsFalse(zombieConfigs[0] == null, "Zombie configs should not be null! The first one was found to be null! Get the lib developer to fix this!");
#endif
        BaseEnemyConfig conf = zombieConfigs[zt];

        //GameObject spawned = zombiePool.InstantiatePoolee(pos, rot).gameObject; changed b/c instantiates new gameobject (no way fr?)
        
        GameObject spawned = zombiePool.Spawn(pos, rot, null, false).GetAwaiter().GetResult().gameObject;
        AIBrain braiiinnnsssss = Instances<AIBrain>.Get(spawned);
        
        // do 2 of em because im not sure what the difference is lol
        braiiinnnsssss.SetBaseConfig(conf);
        conf.ApplyTo(braiiinnnsssss.behaviour);

        if (aggro)
        {
            braiiinnnsssss.behaviour.SetAgro(playerProxy);
        }

        return spawned;
    }

    /// <summary>
    /// Spawn a zombie using the given one as a base, with the given health/recovery values. Any unset values will use the ones from <paramref name="baseType"/>.
    /// </summary>
    /// <param name="pos">The position to spawn the zombie at.</param>
    /// <param name="rot">The rotation the zombie will spawn with.</param>
    /// <param name="baseType">The <see cref="ZombieType"/> to use as a base. The thrower and speed values will be set off this.</param>
    /// <param name="maxHp">The max HP that the zombies is going to have. I think this just means the HP it's going to have.</param>
    /// <param name="maxLimbHp">The max HP that any given limb can have. I'm not sure how this scales between say, a foot vs the torso.</param>
    /// <param name="stunRecovery">How recoverable it is from a stun? Shit idk what the fuck this is.</param>
    /// <param name="maxStunSeconds">The max time, in seconds, the zombie can be stunned for. I'm guessing at least.</param>
    /// <returns></returns>
    public static GameObject Spawn(Vector3 pos, Quaternion rot, ZombieType baseType, float? maxHp = null, float? maxLimbHp = null, float? stunRecovery = null, float? maxStunSeconds = null)
    {
        BaseEnemyConfig _config = zombieConfigs[baseType];
        BaseEnemyConfig clonefig = UnityEngine.Object.Instantiate(_config);
        BaseEnemyConfig.HealthSettings hs = Utilities.CloneHealthSettings(_config.healthSettings);
        hs.maxHitPoints = maxHp ?? clonefig.healthSettings.maxHitPoints;
        hs.maxAppendageHp = maxLimbHp ?? hs.maxAppendageHp;
        hs.stunRecovery = stunRecovery ?? hs.stunRecovery;
        hs.maxStunSeconds = maxStunSeconds ?? hs.maxStunSeconds;
        clonefig.healthSettings = hs;

        GameObject spawned = zombiePool.Spawn(pos, rot, null, null).GetAwaiter().GetResult().gameObject; // uhhhh.... ?? i dont know man. unitask moment i guess
        AIBrain brainiac = Instances<AIBrain>.Get(spawned);

        brainiac.SetBaseConfig(clonefig);
        clonefig.ApplyTo(brainiac.behaviour);
        return spawned;
    }

    /// <summary>
    /// Aggros an AIBrain (NPC) onto the player.
    /// </summary>
    /// <param name="braniac">The AIBrain to aggro on the player.</param>
    public static void AggroOnPlayer(AIBrain braniac) // braniac maniac from the pvz soundtrac. i mean soundtrack.
    {
        if (playerProxy.INOC())
            playerProxy = GameObject.FindObjectsOfType<TriggerRefProxy>().FirstOrDefault(t => t.transform.IsChildOfRigManager());
        braniac.behaviour.SetAgro(playerProxy);
    }

    /// <summary>
    /// Returns the config corresponding to 
    /// </summary>
    /// <param name="zt">The type of zombie to get the config for.</param>
    /// <returns></returns>
    public static BaseEnemyConfig GetConfigOfType(ZombieType zt) => zombieConfigs[zt];
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