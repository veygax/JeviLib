using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Spawning;

/// <summary>
/// JeviLib's type-safe way of identifying pools. To be used in <see cref="Barcodes"/>
/// <br>Being consistently 4 bytes long, this can easily be sent "over the wire", or serialized as an int.</br>
/// </summary>
public enum JevilBarcode
{
    /// <summary>
    /// Name: Ammo Box Heavy
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_BOX_HEAVY,
    /// <summary>
    /// Name: Ammo Box Light
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_BOX_LIGHT,
    /// <summary>
    /// Name: Ammo Box Medium
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_BOX_MEDIUM,
    /// <summary>
    /// Name: Ammo Dest Box Heavy
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_DEST_BOX_HEAVY,
    /// <summary>
    /// Name: Ammo Dest Box Light
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_DEST_BOX_LIGHT,
    /// <summary>
    /// Name: Ammo Dest Box Medium
    /// <br>Tags: Ammo</br>
    /// </summary>
    AMMO_DEST_BOX_MEDIUM,
    /// <summary>
    /// Name: Avatar Dice
    /// <br>Tags: Gadget</br>
    /// </summary>
    AVATAR_DICE,
    /// <summary>
    /// Name: Battery A
    /// <br>Tags: Gadget, Powered</br>
    /// </summary>
    BATTERY_A,
    /// <summary>
    /// Name: Boardgun
    /// <br>Tags: Gadget, DevTool</br>
    /// </summary>
    BOARDGUN,
    /// <summary>
    /// Name: Constrainer
    /// <br>Tags: Gadget, DevTool</br>
    /// </summary>
    CONSTRAINER,
    /// <summary>
    /// Name: Dev Manipulator
    /// <br>Tags: Gadget, DevTool</br>
    /// </summary>
    DEV_MANIPULATOR,
    /// <summary>
    /// Name: Gravity Cup
    /// <br>Tags: Gadget</br>
    /// </summary>
    GRAVITY_CUP,
    /// <summary>
    /// Name: Gravity Plate
    /// <br>Tags: Gadget</br>
    /// </summary>
    GRAVITY_PLATE,
    /// <summary>
    /// Name: Nimbus Gun
    /// <br>Tags: Gadget, DevTool</br>
    /// </summary>
    NIMBUS_GUN,
    /// <summary>
    /// Name: Omni Way
    /// <br>Tags: Gadget</br>
    /// </summary>
    OMNI_WAY,
    /// <summary>
    /// Name: Pallet Jack
    /// <br>Tags: Gadget</br>
    /// </summary>
    PALLET_JACK,
    /// <summary>
    /// Name: Power Punhcer
    /// <br>Tags: Gadget</br>
    /// </summary>
    POWER_PUNHCER,
    /// <summary>
    /// Name: Spawn Gun
    /// <br>Tags: Gadget, DevTool</br>
    /// </summary>
    SPAWN_GUN,
    /// <summary>
    /// Name: Toy Balloon Gun
    /// <br>Tags: Gadget</br>
    /// </summary>
    TOY_BALLOON_GUN,
    /// <summary>
    /// Name: ZipStick
    /// <br>Tags: Gadget</br>
    /// </summary>
    ZIPSTICK,
    /// <summary>
    /// Name: 1911
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    GUN_1911,
    /// <summary>
    /// Name: 590A1
    /// <br>Tags: Weapon, Gun, Shotgun</br>
    /// </summary>
    GUN_590A1,
    /// <summary>
    /// Name: AKM
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    AKM,
    /// <summary>
    /// Name: Eder22 Training
    /// <br>Tags: </br>
    /// </summary>
    EDER22_TRAINING,
    /// <summary>
    /// Name: Eder22
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    EDER22,
    /// <summary>
    /// Name: FAB
    /// <br>Tags: Weapon, Gun, Shotgun</br>
    /// </summary>
    FAB,
    /// <summary>
    /// Name: Gruber
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    GRUBER,
    /// <summary>
    /// Name: M16 ACOG
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    M16_ACOG,
    /// <summary>
    /// Name: M16 Holosight
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    M16_HOLOSIGHT,
    /// <summary>
    /// Name: M16 Ironsights
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    M16_IRONSIGHTS,
    /// <summary>
    /// Name: M16 Laser Foregrip
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    M16_LASER_FOREGRIP,
    /// <summary>
    /// Name: M4
    /// <br>Tags: Weapon, Gun, Shotgun</br>
    /// </summary>
    M4,
    /// <summary>
    /// Name: M9
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    M9,
    /// <summary>
    /// Name: MK18 Holosight
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    MK18_HOLOSIGHT,
    /// <summary>
    /// Name: MK18 Ironsights
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    MK18_IRONSIGHTS,
    /// <summary>
    /// Name: MK18 Laser Foregrip
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    MK18_LASER_FOREGRIP,
    /// <summary>
    /// Name: MK18 Sabrelake
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    MK18_SABRELAKE,
    /// <summary>
    /// Name: MK18 Naked
    /// <br>Tags: Weapon, Gun, Rifle</br>
    /// </summary>
    MK18_NAKED,
    /// <summary>
    /// Name: MP5
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5,
    /// <summary>
    /// Name: MP5K Flashlight
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5K_FLASHLIGHT,
    /// <summary>
    /// Name: MP5K Ironsights
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5K_IRONSIGHTS,
    /// <summary>
    /// Name: MP5K Laser
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5K_LASER,
    /// <summary>
    /// Name: MP5K Holosight
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5K_HOLOSIGHT,
    /// <summary>
    /// Name: MP5K Sabrelake
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    MP5K_SABRELAKE,
    /// <summary>
    /// Name: P350
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    P350,
    /// <summary>
    /// Name: PDRC
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    PDRC,
    /// <summary>
    /// Name: PT8 Alaris
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    PT8_ALARIS,
    /// <summary>
    /// Name: M870
    /// <br>Tags: Weapon, Gun, Shotgun</br>
    /// </summary>
    M870,
    /// <summary>
    /// Name: Stapler
    /// <br>Tags: Weapon, Gun, Pistol</br>
    /// </summary>
    STAPLER,
    /// <summary>
    /// Name: UMP
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    UMP,
    /// <summary>
    /// Name: UZI
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    UZI,
    /// <summary>
    /// Name: Vector
    /// <br>Tags: Weapon, Gun, SMG</br>
    /// </summary>
    VECTOR,
    /// <summary>
    /// Name: 12G Slug Mag
    /// <br>Tags: </br>
    /// </summary>
    MAG_12G_SLUG,
    /// <summary>
    /// Name: 12G Small Mag
    /// <br>Tags: </br>
    /// </summary>
    MAG_12G_SMALL,
    /// <summary>
    /// Name: 12G Shell Mag
    /// <br>Tags: </br>
    /// </summary>
    MAG_12G_SHELL,
    /// <summary>
    /// Name: 1911 Mag
    /// <br>Tags: </br>
    /// </summary>
    MAG_1911,
    /// <summary>
    /// Name: AKM Mag
    /// <br>Tags: </br>
    /// </summary>
    AKM_MAG,
    /// <summary>
    /// Name: Eder Mag
    /// <br>Tags: </br>
    /// </summary>
    EDER_MAG,
    /// <summary>
    /// Name: Gruber Mag
    /// <br>Tags: </br>
    /// </summary>
    GRUBER_MAG,
    /// <summary>
    /// Name: M16 Mag
    /// <br>Tags: </br>
    /// </summary>
    M16_MAG,
    /// <summary>
    /// Name: M9 Mag
    /// <br>Tags: </br>
    /// </summary>
    M9_MAG,
    /// <summary>
    /// Name: MP5 Mag
    /// <br>Tags: </br>
    /// </summary>
    MP5_MAG,
    /// <summary>
    /// Name: P350 Mag
    /// <br>Tags: </br>
    /// </summary>
    P350_MAG,
    /// <summary>
    /// Name: PDRC Mag
    /// <br>Tags: </br>
    /// </summary>
    PDRC_MAG,
    /// <summary>
    /// Name: PT8 Alaris Mag
    /// <br>Tags: </br>
    /// </summary>
    PT8_ALARIS_MAG,
    /// <summary>
    /// Name: Staplegun
    /// <br>Tags: </br>
    /// </summary>
    STAPLEGUN,
    /// <summary>
    /// Name: UMP Mag
    /// <br>Tags: </br>
    /// </summary>
    UMP_MAG,
    /// <summary>
    /// Name: UZI Mag
    /// <br>Tags: </br>
    /// </summary>
    UZI_MAG,
    /// <summary>
    /// Name: Vector Mag
    /// <br>Tags: </br>
    /// </summary>
    VECTOR_MAG,
    /// <summary>
    /// Name: Axe Double
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    AXE_DOUBLE,
    /// <summary>
    /// Name: Axe Firefighter
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    AXE_FIREFIGHTER,
    /// <summary>
    /// Name: Axe Horror
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    AXE_HORROR,
    /// <summary>
    /// Name: Barbed Bat
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    BARBED_BAT,
    /// <summary>
    /// Name: Baseball Bat
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    BASEBALL_BAT,
    /// <summary>
    /// Name: Bastard Sword
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    BASTARD_SWORD,
    /// <summary>
    /// Name: Baton
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    BATON,
    /// <summary>
    /// Name: Chef Knife
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    CHEF_KNIFE,
    /// <summary>
    /// Name: Cleaver
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    CLEAVER,
    /// <summary>
    /// Name: Combat Knife
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    COMBAT_KNIFE,
    /// <summary>
    /// Name: Crowbar
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    CROWBAR,
    /// <summary>
    /// Name: Kunai
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    KUNAI,
    /// <summary>
    /// Name: Dagger
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    DAGGER,
    /// <summary>
    /// Name: Electric Guitar
    /// <br>Tags: Weapon, Blunt, Prop</br>
    /// </summary>
    ELECTRIC_GUITAR,
    /// <summary>
    /// Name: Frying Pan
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    FRYING_PAN,
    /// <summary>
    /// Name: Golf Club
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    GOLF_CLUB,
    /// <summary>
    /// Name: Half Sword
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    HALF_SWORD,
    /// <summary>
    /// Name: Hammer
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    HAMMER,
    /// <summary>
    /// Name: Hand Hammer
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    HAND_HAMMER,
    /// <summary>
    /// Name: Hatchet
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    HATCHET,
    /// <summary>
    /// Name: Melee Ice Axe
    /// <br>Tags: </br>
    /// </summary>
    MELEE_ICE_AXE,
    /// <summary>
    /// Name: Katana
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    KATANA,
    /// <summary>
    /// Name: Katar
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    KATAR,
    /// <summary>
    /// Name: Lead Pipe
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    LEAD_PIPE,
    /// <summary>
    /// Name: Machete
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    MACHETE,
    /// <summary>
    /// Name: Morningstar
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    MORNINGSTAR,
    /// <summary>
    /// Name: Norse Axe
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    NORSE_AXE,
    /// <summary>
    /// Name: Pick Axe
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    PICK_AXE,
    /// <summary>
    /// Name: Shovel
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    SHOVEL,
    /// <summary>
    /// Name: Sledgehammer
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    SLEDGEHAMMER,
    /// <summary>
    /// Name: Spear
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    SPEAR,
    /// <summary>
    /// Name: Spiked Club
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    SPIKED_CLUB,
    /// <summary>
    /// Name: Sword Claymore
    /// <br>Tags: Weapon, Blade</br>
    /// </summary>
    SWORD_CLAYMORE,
    /// <summary>
    /// Name: Trashcan Lid
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    TRASHCAN_LID,
    /// <summary>
    /// Name: Viking Shield
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    VIKING_SHIELD,
    /// <summary>
    /// Name: Warhammer
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    WARHAMMER,
    /// <summary>
    /// Name: Wrench
    /// <br>Tags: Weapon, Blunt</br>
    /// </summary>
    WRENCH,
    /// <summary>
    /// Name: Crablet Plus
    /// <br>Tags: NPC</br>
    /// </summary>
    CRABLET_PLUS,
    /// <summary>
    /// Name: Crablet
    /// <br>Tags: NPC</br>
    /// </summary>
    CRABLET,
    /// <summary>
    /// Name: Cultist
    /// <br>Tags: NPC</br>
    /// </summary>
    CULTIST,
    /// <summary>
    /// Name: Early Exit Zombie
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    EARLY_EXIT_ZOMBIE,
    /// <summary>
    /// Name: Ford VR Junkie
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    FORD_VR_JUNKIE,
    /// <summary>
    /// Name: Ford
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    FORD,
    /// <summary>
    /// Name: Null Body Corrupted
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    NULL_BODY_CORRUPTED,
    /// <summary>
    /// Name: Null Body Agent
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    NULL_BODY_AGENT,
    /// <summary>
    /// Name: Null Body
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    NULL_BODY,
    /// <summary>
    /// Name: Null Rat
    /// <br>Tags: NPC</br>
    /// </summary>
    NULL_RAT,
    /// <summary>
    /// Name: Omni Projector
    /// <br>Tags: NPC</br>
    /// </summary>
    OMNI_PROJECTOR,
    /// <summary>
    /// Name: Omni Projector Hazmat
    /// <br>Tags: NPC</br>
    /// </summary>
    OMNI_PROJECTOR_HAZMAT,
    /// <summary>
    /// Name: Omni Turret
    /// <br>Tags: NPC</br>
    /// </summary>
    OMNI_TURRET,
    /// <summary>
    /// Name: Peasant Female A
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_FEMALE_A,
    /// <summary>
    /// Name: Peasant Female B
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_FEMALE_B,
    /// <summary>
    /// Name: Peasant Female C
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_FEMALE_C,
    /// <summary>
    /// Name: Peasant Male A
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_MALE_A,
    /// <summary>
    /// Name: Peasant Male B
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_MALE_B,
    /// <summary>
    /// Name: Peasant Male C
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_MALE_C,
    /// <summary>
    /// Name: Peasant Null
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    PEASANT_NULL,
    /// <summary>
    /// Name: Security Guard
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    SECURITY_GUARD,
    /// <summary>
    /// Name: Skeleton Steel
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    SKELETON_STEEL,
    /// <summary>
    /// Name: Skeleton Fire Mage
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    SKELETON_FIRE_MAGE,
    /// <summary>
    /// Name: Skeleton
    /// <br>Tags: NPC, Humanoid</br>
    /// </summary>
    SKELETON,
    /// <summary>
    /// Name: Void Turret
    /// <br>Tags: NPC</br>
    /// </summary>
    VOID_TURRET,
    /// <summary>
    /// Name: Void Wrecker
    /// <br>Tags: NPC, Redacted</br>
    /// </summary>
    VOID_WRECKER,
    /// <summary>
    /// Name: Projectile Gacha Spawn 
    /// <br>Tags: </br>
    /// </summary>
    PROJECTILE_GACHA_SPAWN_,
    /// <summary>
    /// Name: Projectile Void Energy
    /// <br>Tags: </br>
    /// </summary>
    PROJECTILE_VOID_ENERGY,
    /// <summary>
    /// Name: Projectile VoidBall 
    /// <br>Tags: </br>
    /// </summary>
    PROJECTILE_VOIDBALL_,
    /// <summary>
    /// Name: Aluminum Table B
    /// <br>Tags: </br>
    /// </summary>
    ALUMINUM_TABLE_B,
    /// <summary>
    /// Name: Aluminum Table
    /// <br>Tags: </br>
    /// </summary>
    ALUMINUM_TABLE,
    /// <summary>
    /// Name: Angry Void Radiation
    /// <br>Tags: </br>
    /// </summary>
    ANGRY_VOID_RADIATION,
    /// <summary>
    /// Name: Apollo
    /// <br>Tags: Prop, Toy</br>
    /// </summary>
    APOLLO,
    /// <summary>
    /// Name: Barrel 2
    /// <br>Tags: </br>
    /// </summary>
    BARREL_2,
    /// <summary>
    /// Name: Baseball
    /// <br>Tags: Prop, Toy</br>
    /// </summary>
    BASEBALL,
    /// <summary>
    /// Name: Basketball
    /// <br>Tags: Prop, Toy</br>
    /// </summary>
    BASKETBALL,
    /// <summary>
    /// Name: Bowling Ball Big
    /// <br>Tags: Prop</br>
    /// </summary>
    BOWLING_BALL_BIG,
    /// <summary>
    /// Name: Brick
    /// <br>Tags: Prop</br>
    /// </summary>
    BRICK,
    /// <summary>
    /// Name: Bucket
    /// <br>Tags: </br>
    /// </summary>
    BUCKET,
    /// <summary>
    /// Name: Cafe Tray
    /// <br>Tags: </br>
    /// </summary>
    CAFE_TRAY,
    /// <summary>
    /// Name: Cardboard Box Monogon
    /// <br>Tags: Prop</br>
    /// </summary>
    CARDBOARD_BOX_MONOGON,
    /// <summary>
    /// Name: Cardboard Box
    /// <br>Tags: Prop</br>
    /// </summary>
    CARDBOARD_BOX,
    /// <summary>
    /// Name: Cave Small Rock
    /// <br>Tags: </br>
    /// </summary>
    CAVE_SMALL_ROCK,
    /// <summary>
    /// Name: Cinder Block
    /// <br>Tags: </br>
    /// </summary>
    CINDER_BLOCK,
    /// <summary>
    /// Name: Clipboard Lore
    /// <br>Tags: </br>
    /// </summary>
    CLIPBOARD_LORE,
    /// <summary>
    /// Name: Coffee Cup
    /// <br>Tags: Prop</br>
    /// </summary>
    COFFEE_CUP,
    /// <summary>
    /// Name: Concrete Barrier
    /// <br>Tags: Prop</br>
    /// </summary>
    CONCRETE_BARRIER,
    /// <summary>
    /// Name: Core Key
    /// <br>Tags: Prop</br>
    /// </summary>
    CORE_KEY,
    /// <summary>
    /// Name: Couch 2 Seat
    /// <br>Tags: </br>
    /// </summary>
    COUCH_2_SEAT,
    /// <summary>
    /// Name: Couch 3 Seat
    /// <br>Tags: </br>
    /// </summary>
    COUCH_3_SEAT,
    /// <summary>
    /// Name: Crate 1m Indestructable
    /// <br>Tags: Prop</br>
    /// </summary>
    CRATE_1M_INDESTRUCTABLE,
    /// <summary>
    /// Name: Crate 2m Indestructable
    /// <br>Tags: Prop</br>
    /// </summary>
    CRATE_2M_INDESTRUCTABLE,
    /// <summary>
    /// Name: Crate Wooden Destructable
    /// <br>Tags: Prop</br>
    /// </summary>
    CRATE_WOODEN_DESTRUCTABLE,
    /// <summary>
    /// Name: Crown
    /// <br>Tags: Prop</br>
    /// </summary>
    CROWN,
    /// <summary>
    /// Name: Destructable Wood Plank
    /// <br>Tags: Prop</br>
    /// </summary>
    DESTRUCTABLE_WOOD_PLANK,
    /// <summary>
    /// Name: Prop Destructible Barrel
    /// <br>Tags: </br>
    /// </summary>
    PROP_DESTRUCTIBLE_BARREL,
    /// <summary>
    /// Name: Die D20
    /// <br>Tags: </br>
    /// </summary>
    DIE_D20,
    /// <summary>
    /// Name: Dungeon Large Brick
    /// <br>Tags: </br>
    /// </summary>
    DUNGEON_LARGE_BRICK,
    /// <summary>
    /// Name: Dungeon Small Brick
    /// <br>Tags: </br>
    /// </summary>
    DUNGEON_SMALL_BRICK,
    /// <summary>
    /// Name: Flashlight
    /// <br>Tags: Prop</br>
    /// </summary>
    FLASHLIGHT,
    /// <summary>
    /// Name: Glowstick
    /// <br>Tags: Prop</br>
    /// </summary>
    GLOWSTICK,
    /// <summary>
    /// Name: Gym Arch
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_ARCH,
    /// <summary>
    /// Name: Gym Beam
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BEAM,
    /// <summary>
    /// Name: Gym Block A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BLOCK_A,
    /// <summary>
    /// Name: Gym Block B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BLOCK_B,
    /// <summary>
    /// Name: Gym Block C
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BLOCK_C,
    /// <summary>
    /// Name: Gym Block D
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BLOCK_D,
    /// <summary>
    /// Name: Gym Block E
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_BLOCK_E,
    /// <summary>
    /// Name: Gym Cone A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CONE_A,
    /// <summary>
    /// Name: Gym Cone B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CONE_B,
    /// <summary>
    /// Name: Gym Cube 1x1
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CUBE_1X1,
    /// <summary>
    /// Name: Gym Cube 2x2
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CUBE_2X2,
    /// <summary>
    /// Name: Gym Cube 3x3
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CUBE_3X3,
    /// <summary>
    /// Name: Gym Cube Small
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CUBE_SMALL,
    /// <summary>
    /// Name: Gym Cylinder Half A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CYLINDER_HALF_A,
    /// <summary>
    /// Name: Gym Cylinder Half B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CYLINDER_HALF_B,
    /// <summary>
    /// Name: Gym Cylinder Large A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CYLINDER_LARGE_A,
    /// <summary>
    /// Name: Gym Cylinder Large B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CYLINDER_LARGE_B,
    /// <summary>
    /// Name: Gym Cylinder Small
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_CYLINDER_SMALL,
    /// <summary>
    /// Name: Gym D10
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D10,
    /// <summary>
    /// Name: Gym D12
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D12,
    /// <summary>
    /// Name: Gym D20
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D20,
    /// <summary>
    /// Name: Gym D4
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D4,
    /// <summary>
    /// Name: Gym D6
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D6,
    /// <summary>
    /// Name: Gym D8
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_D8,
    /// <summary>
    /// Name: Gym Disc A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_DISC_A,
    /// <summary>
    /// Name: Gym Disc B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_DISC_B,
    /// <summary>
    /// Name: Gym Medicine Ball
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_MEDICINE_BALL,
    /// <summary>
    /// Name: Gym Octogon
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_OCTOGON,
    /// <summary>
    /// Name: Gym Prism
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_PRISM,
    /// <summary>
    /// Name: Gym Shallow Ramp
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_SHALLOW_RAMP,
    /// <summary>
    /// Name: Gym Soccer Ball
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_SOCCER_BALL,
    /// <summary>
    /// Name: Gym Tall Ramp
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TALL_RAMP,
    /// <summary>
    /// Name: Gym Torus
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TORUS,
    /// <summary>
    /// Name: Gym Trapazoid A
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TRAPAZOID_A,
    /// <summary>
    /// Name: Gym Trapazoid B
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TRAPAZOID_B,
    /// <summary>
    /// Name: Gym Trapazoid C
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TRAPAZOID_C,
    /// <summary>
    /// Name: Gym Trapazoid D
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TRAPAZOID_D,
    /// <summary>
    /// Name: Gym Trapazoid E
    /// <br>Tags: Prop, Gym</br>
    /// </summary>
    GYM_TRAPAZOID_E,
    /// <summary>
    /// Name: Hexagonal Container
    /// <br>Tags: Prop</br>
    /// </summary>
    HEXAGONAL_CONTAINER,
    /// <summary>
    /// Name: Prop KeyBall Amber
    /// <br>Tags: Redacted</br>
    /// </summary>
    PROP_KEYBALL_AMBER,
    /// <summary>
    /// Name: KeyBall Blue
    /// <br>Tags: Redacted</br>
    /// </summary>
    KEYBALL_BLUE,
    /// <summary>
    /// Name: Prop KeyBall Green
    /// <br>Tags: Redacted</br>
    /// </summary>
    PROP_KEYBALL_GREEN,
    /// <summary>
    /// Name: Prop KeyBall Purple
    /// <br>Tags: Redacted</br>
    /// </summary>
    PROP_KEYBALL_PURPLE,
    /// <summary>
    /// Name: KeyBall White
    /// <br>Tags: Redacted</br>
    /// </summary>
    KEYBALL_WHITE,
    /// <summary>
    /// Name: KeyBall
    /// <br>Tags: Redacted</br>
    /// </summary>
    KEYBALL,
    /// <summary>
    /// Name: Keyboard
    /// <br>Tags: </br>
    /// </summary>
    KEYBOARD,
    /// <summary>
    /// Name: Meeting Room Table
    /// <br>Tags: </br>
    /// </summary>
    MEETING_ROOM_TABLE,
    /// <summary>
    /// Name: Mirror
    /// <br>Tags: Prop</br>
    /// </summary>
    MIRROR,
    /// <summary>
    /// Name: Monitor
    /// <br>Tags: Prop</br>
    /// </summary>
    MONITOR,
    /// <summary>
    /// Name: Monkey
    /// <br>Tags: Prop</br>
    /// </summary>
    MONKEY,
    /// <summary>
    /// Name: ComputerMouse
    /// <br>Tags: </br>
    /// </summary>
    COMPUTERMOUSE,
    /// <summary>
    /// Name: Paper Folder
    /// <br>Tags: </br>
    /// </summary>
    PAPER_FOLDER,
    /// <summary>
    /// Name: PC
    /// <br>Tags: </br>
    /// </summary>
    PC,
    /// <summary>
    /// Name: Pizzabox
    /// <br>Tags: </br>
    /// </summary>
    PIZZABOX,
    /// <summary>
    /// Name: Plant
    /// <br>Tags: Prop</br>
    /// </summary>
    PLANT,
    /// <summary>
    /// Name: Plunger
    /// <br>Tags: Prop</br>
    /// </summary>
    PLUNGER,
    /// <summary>
    /// Name: Printer
    /// <br>Tags: </br>
    /// </summary>
    PRINTER,
    /// <summary>
    /// Name: Radio
    /// <br>Tags: Prop</br>
    /// </summary>
    RADIO,
    /// <summary>
    /// Name: Roll Ball 100kg
    /// <br>Tags: </br>
    /// </summary>
    ROLL_BALL_100KG,
    /// <summary>
    /// Name: Prop Scaffolding
    /// <br>Tags: </br>
    /// </summary>
    PROP_SCAFFOLDING,
    /// <summary>
    /// Name: Shopping Cart
    /// <br>Tags: Prop</br>
    /// </summary>
    SHOPPING_CART,
    /// <summary>
    /// Name: Small Chair 2
    /// <br>Tags: </br>
    /// </summary>
    SMALL_CHAIR_2,
    /// <summary>
    /// Name: Soup Can
    /// <br>Tags: Prop</br>
    /// </summary>
    SOUP_CAN,
    /// <summary>
    /// Name: Stationary Turret
    /// <br>Tags: </br>
    /// </summary>
    STATIONARY_TURRET,
    /// <summary>
    /// Name: Sword Noodledog
    /// <br>Tags: Prop, Toy</br>
    /// </summary>
    SWORD_NOODLEDOG,
    /// <summary>
    /// Name: Table 01
    /// <br>Tags: </br>
    /// </summary>
    TABLE_01,
    /// <summary>
    /// Name: Trashbag 01
    /// <br>Tags: </br>
    /// </summary>
    TRASHBAG_01,
    /// <summary>
    /// Name: Trashbag 02
    /// <br>Tags: </br>
    /// </summary>
    TRASHBAG_02,
    /// <summary>
    /// Name: Trashcan 01
    /// <br>Tags: </br>
    /// </summary>
    TRASHCAN_01,
    /// <summary>
    /// Name: Trashcan A
    /// <br>Tags: </br>
    /// </summary>
    TRASHCAN_A,
    /// <summary>
    /// Name: Trashcan Metal
    /// <br>Tags: </br>
    /// </summary>
    TRASHCAN_METAL,
    /// <summary>
    /// Name: Trashcan
    /// <br>Tags: </br>
    /// </summary>
    TRASHCAN,
    /// <summary>
    /// Name: Vending Machine UM
    /// <br>Tags: </br>
    /// </summary>
    VENDING_MACHINE_UM,
    /// <summary>
    /// Name: Watermelon
    /// <br>Tags: Prop</br>
    /// </summary>
    WATERMELON,
    /// <summary>
    /// Name: Gokart
    /// <br>Tags: Vehicle</br>
    /// </summary>
    GOKART,
    /// <summary>
    /// Invalid Barcode that doesn't correspond to anything
    /// </summary>
    INVALID = -1,
}
