//Microsoft
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using Org.BouncyCastle.Utilities;

//System
using System.ComponentModel;
using System.Data;
using System.Reflection.PortableExecutable;

//Terraria
using Terraria;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.Hooks;

namespace MKLP.Modules
{
    public static class SurvivalManager
    {
        public enum BossDType
        {
            NA,
            KingSlime,
            EyeOfCthulhu,
            EvilBoss,
            Skeletron,
            QueenBee,
            Deerclops,
            WallOfFlesh,
            QueenSlime,
            TheDestroyer,
            TheTwins,
            SkeletronPrime,
            Plantera,
            Golem,
            DukeFishron,
            EmpressOfLight,
            LunaticCultist,
            MoonLord
        }

        public static Dictionary<int, string> GetIllegalItem(BossDType currentbossdefeated = BossDType.NA)
        {
            #region [ get Illegal Items ]
            Dictionary<int, string> getillegalitems = new();
            
            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            bool allowvanity = (bool)MKLP.Config.Main.Progression.AllowVanityCloth;
            bool allowmusicbox = (bool)MKLP.Config.Main.Progression.AllowMusicBox;

            bool allowdungeonrush = (bool)MKLP.Config.Main.Progression.AllowDungeonRush;
            bool allowtemplerush = (bool)MKLP.Config.Main.Progression.AllowTempleRush;

            #region | unobtainable |

            getillegalitems.Add(58, "Unobtainable");//heart
            getillegalitems.Add(1734, "Unobtainable");
            getillegalitems.Add(1867, "Unobtainable");

            getillegalitems.Add(184, "Unobtainable");//star
            getillegalitems.Add(1735, "Unobtainable");
            getillegalitems.Add(1868, "Unobtainable");

            getillegalitems.Add(603, "Unobtainable");
            getillegalitems.Add(766, "Unobtainable");

            int[] unobtainableids = { 2772, 2773, 2775, 2777, 2778, 2780, 2782, 2783,
                2785, 2881, 3453, 3454, 3455, 3462, 3463, 3465, 3705, 3706, 3847, 3848,
                3849, 3850, 3851, 3853, 3861, 3862, 3978, 4010, 4058, 4143, 5013, 5437,
                5668, 6135, 6136, 6143 };

            foreach (int add in unobtainableids)
            {
                getillegalitems.Add(add, "Unobtainable");
            }

            if (!Main.drunkWorld && !Main.getGoodWorld && !Main.zenithWorld && !Main.tenthAnniversaryWorld)
            {
                getillegalitems.Add(678, "Unobtainable");
            }
            if (!Main.drunkWorld && !Main.remixWorld && !Main.zenithWorld)
            {
                getillegalitems.Add(5001, "Unobtainbale");
            }
            if (!Main.zenithWorld)
            {
                getillegalitems.Add(5334, "Unobtainable");
                getillegalitems.Add(5382, "Unobtainable");
            }

            if ((
                NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3 ||
                currentbossdefeated == BossDType.TheDestroyer || currentbossdefeated != BossDType.TheTwins || currentbossdefeated != BossDType.SkeletronPrime
                )
                && (Main.zenithWorld || Main.remixWorld))
            {
                getillegalitems.Add(779, "Unobtainable");
                getillegalitems.Add(780, "Unobtainable");
                getillegalitems.Add(781, "Unobtainable");
                getillegalitems.Add(782, "Unobtainable");
                getillegalitems.Add(783, "Unobtainable");
                getillegalitems.Add(784, "Unobtainable");
            }
            if ((
                NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3 ||
                currentbossdefeated == BossDType.TheDestroyer || currentbossdefeated != BossDType.TheTwins || currentbossdefeated != BossDType.SkeletronPrime
                )
                && (!Main.zenithWorld && !Main.remixWorld))
            {
                if (!NPC.downedMoonlord)
                {
                    if (WorldGen.crimson)
                    {
                        getillegalitems.Add(782, "Unobtainable");
                    }
                    else
                    {
                        getillegalitems.Add(784, "Unobtainable");
                    }
                }
            }

            #endregion

            #region ( Banners )

            bool allowbanners = false;

            if (!allowbanners)
            {
                if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
                {
                    int[] addin = { 1669 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    int[] addin = { 1689, 2908, 2976, 2910, 1651, 2938, 2966, 2962, 4977, 1699, 1695,
                        1700, 1620, 2923, 1630, 4965, 2969, 2973, 1615, 3448, 1623, 1650, 2898, 1662,
                        3452, 1663, 1674, 1696, 2897, 1640, 2925, 1688, 1666, 4976, 1619, 3418, 3417,
                        3412, 3415, 3416, 1671, 3419, 2905, 4973, 2909, 1637, 3449, 1697, 1624, 1625,
                        4966, 1636, 4974, 1645, 1660, 2936, 1629, 1642, 4975, 2937, 3450, 1677, 1691,
                        1616, 2934, 1631, 4545, 4544, 4546, 4602, 3390, 3780, 3789, 3792, 3790, 3791,
                        2932, 1673, 3441, 3443, 3444, 3442, 1676, 3840, 3842, 3841, 3843,
                        3394, 3399, 3395, 2920, 1692, 1648, 1687, 5597 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                    (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime)) // mech
                {
                    int[] addin = { 2943, 3446, 1670 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Mechanical Boss");
                    }
                    int[] templeids = { 3394, 3399, 3395, 2920, 1692, 1648, 1687, 5597 };
                    if (!allowtemplerush)
                    {
                        foreach (int add in templeids)
                        {
                            if (!getillegalitems.ContainsKey(add))
                            {
                                getillegalitems.Add(add, "Mechanical Boss");
                            }
                        }
                    }
                }

                if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                    (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                    (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)) // post mech
                {
                    int[] addin = { 1679 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Post Mechanical Boss");
                    }
                }

                if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
                {
                    int[] addin = { 2900, 2970, 2930, 2914, 2956, 2965, 2904, 2974, 2975, 2984, 2917,
                        2924, 2958, 3402, 3396, 3397, 3403, 3401, 3400, 2929, 2931, 2961, 2971, 2982,
                        2918, 2919, 2921, 2926, 2941, 2957, 2963, 2993, 2994 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Plantera");
                    }
                    if (!allowtemplerush)
                    {
                        int[] templeids = { 1647, 1667 };
                        foreach (int add in templeids)
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
                {
                    int[] addin = { 2945, 2946, 2947, 2948, 2949, 2950, 2951, 2952, 2953, 3445, 2972,
                        2901, 2902, 3846 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Golem");
                    }
                }

                if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
                {
                    int[] addin = { 3438, 3436, 3437, 3440, 3439, 3433, 3435, 3434, 3432, 3422, 3421,
                        3424, 3425, 3420, 3423, 3426, 3428, 3430, 3429, 3431, 3427 };
                    foreach (int add in addin)
                    {
                        getillegalitems.Add(add, "Lunatic Cultist");
                    }
                }

                getillegalitems.Add(2989, "Unobtainable");
                getillegalitems.Add(2990, "Unobtainable");
                getillegalitems.Add(2991, "Unobtainable");
                getillegalitems.Add(3404, "Unobtainable");
                getillegalitems.Add(3398, "Unobtainable");
            }

            #endregion

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing && currentbossdefeated != BossDType.KingSlime) //king slime
            {
                int[] addin = { 256, 257, 258, 762, 767, 769, 815, 998, 2430, 2489, 2493,
                    2585, 2610, 3090, 3318, 4120, 4797, 4929, 5131, 5495 };

                int[] solidifier = { 5687, 5688 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "King Slime");
                    }
                }
                if (true)
                {
                    foreach (int add in solidifier)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "King Slime");
                        }
                    }
                }
            }
            #endregion

            #region [ Eye of Cthulhu ]
            if (!NPC.downedBoss1 && currentbossdefeated != BossDType.EyeOfCthulhu)
            {
                for (int i = 4041; i <= 4048; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Eye of Cthulhu");
                    }
                }

                int[] addin = {
                    //planter boxes
                    3215, 3216, 3217, 3218, 3219, 3220, 3221, 3222, 5532, 5533,


                    114, 1299, 1360, 1853, 1854, 2112, 3097, 3215, 3216, 3262,
                    3319, 4241, 4347, 4798, 4924, 5004, 5276, 5323, 5454, 5455 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Eye of Cthulhu");
                    }
                }

                if (!allowvanity)
                {
                    if (!getillegalitems.ContainsKey(3763))
                    {
                        getillegalitems.Add(3763, "Eye of Cthulhu");
                    }
                }
            }
            #endregion

            #region [ Eye of Cthulhu OR King Slime ]
            if (!NPC.downedBoss1 && !NPC.downedSlimeKing && currentbossdefeated != BossDType.KingSlime && currentbossdefeated != BossDType.EyeOfCthulhu)
            {
                int[] addin = { 5542 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Eye of Cthulhu");
                    }
                }
            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
            {
                for (int i = 3797; i <= 3883; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Evil Boss");
                    }
                }

                int[] addin = {

                    //meteorite
                    //5476, 5535, 5536
                    
                    103, 104, 792, 793, 794, 797, 798, 994, 1361, 1362, 2104,
                    2111, 3060, 3100, 3101, 3126, 3129, 3132, 3135, 3138, 3141, 3144,
                    3147, 3150, 3153, 3156, 3159, 3162, 3165, 3168, 3171, 3174, 3177, 3180,
                    3223, 3224, 3266, 3267, 3268, 3320, 3321, 4076, 4131, 4141,
                    4796, 4799, 4800, 4925, 4926, 4946, 4947, 4948, 5325, 5456, 5467, 5468, 5469, 5470 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Evil Boss");
                    }
                }

                //meteorite bar glitch (unsure)
                if (false)
                {
                    int[] vanityblock = { 5486 };

                    foreach (int add in vanityblock)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Evil Boss");
                        }
                    }
                }

                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    int[] hellstoneid = { 119, 120, 121, 122, 174, 175,
                        217, 219, 221, 231, 232, 233, 273, 2365, 4533, 4534, 4535, 4536, 4821 };

                    foreach (int add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Evil Boss");
                        }
                    }
                }
                if (allowmusicbox)
                {
                    if (getillegalitems.ContainsKey(3869))
                    {
                        getillegalitems.Remove(3869);
                    }
                }
            }
            #endregion

            #region ( Evil Boss Enabled )

            if (((bool)MKLP.Config.BossManager.AllowEaterOfWorlds || (bool)MKLP.Config.BossManager.AllowBrainOfCthulhu) ||
                (currentbossdefeated == BossDType.EvilBoss))
            { //     true if its eow/boc enabled    |    false if its not enabled
                int[] hellstoneid = { 118, 119, 120, 121, 122, 174, 175,
                    217, 219, 221, 231, 232, 233, 273, 2365, 4533, 4534,
                    4535, 4536, 4821, 5456, 5467, 5468, 5469, 5470 };

                //meteorite can be obtain from chest pre boss | 116, 117, 123, 124, 125, 197, 198, 199, 200, 201, 202, 203, 204,

                foreach (int remove in hellstoneid) // hellstone are breakable from bombs on hm
                {
                    if (!getillegalitems.ContainsKey(remove))
                    {
                        getillegalitems.Remove(remove);
                    }
                }
            }
            else
            {
                if (!getillegalitems.ContainsKey(86))
                {
                    getillegalitems.Add(86, "Evil Boss");
                }
                if (!getillegalitems.ContainsKey(1329))
                {
                    getillegalitems.Add(1329, "Evil Boss");
                }
            }
            //86
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
            {
                for (int i = 3611; i <= 3616; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Skeletron");
                    }
                }

                int[] addin = { 346, 509, 510, 513, 541, 849, 850, 851, 852, 853, 1263, 1273,
                    1313, 1363, 2295, 2296, 2739, 2799, 3085, 3205, 3245, 3282,
                    3323, 3619, 3620, 3625, 3629, 3707, 3725, 4264, 4484, 4485, 4703, 4801,
                    4818, 4927, 4993, 5672, 5673, 5577, 5630 };

                int[] dungeonrushids = { 112, 113, 155, 156, 157, 163, 164, 218, 220, 273,
                    274, 328, 329, 1613, 3019, 3317, 5010, 5126, 5465, 5504, 5669 };

                int[] vanity_dungeonrush_ids = { 5515 };

                int[] vanityids = { 254, 260, 269, 270, 271, 322, 325, 326, 978, 979, 980,
                    981, 1275, 1276, 1281, 1288, 1289, 1429, 1740, 3242, 3243, 3244, 3246,
                    3247, 3362, 3363, 3627, 3730, 3731, 3733, 3734, 3735, 4128, 4129,
                    4130, 4132, 4133, 4134, 4685, 4686, 4704, 4705, 4706, 4707, 4708,
                    4709, 5624, 5625, 5626, 5628, 5737 };
                //981

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }
                    if (!allowdungeonrush)
                    {
                        foreach (int add in vanity_dungeonrush_ids)
                        {
                            if (!getillegalitems.ContainsKey(add))
                            {
                                getillegalitems.Add(add, "Skeletron");
                            }
                        }
                    }
                }


                if (!allowdungeonrush)
                {
                    foreach (int add in dungeonrushids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }

                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Skeletron");
                    }
                }

                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    int[] skeleids = { 151, 152, 153, 154, 327, 768, 808, 811, 820, 827, 890, 891, 904, 932, 959,
                        1307, 3095, 3122, 3124, 4076, 4131, 5074, 5325, 5328, 5358, 5359, 5360,
                        5361, 5438, 5518 };
                    foreach (int add in skeleids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Bee ]

            if (!NPC.downedQueenBee && currentbossdefeated != BossDType.QueenBee)
            {
                for (int i = 940; i <= 945; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Queen Bee");
                    }
                }

                int[] addin = { 842, 843, 844, 909, 910, 1123, 1129, 1130, 1132, 1167, 1170, 1249,
                    1339, 1340, 1341, 1342, 1353, 1354, 1355, 1356, 1357, 1359, 1364, 1430, 1578,
                    1791, 2108, 2361, 2362, 2363, 2364, 2431, 2502, 2888, 3251, 3322, 3333,
                    4417, 4802, 4922, 4928 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Queen Bee");
                    }
                }

                if (true)
                {
                    int[] vanityblock = { 5483 };
                    foreach (int add in vanityblock)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Queen Bee");
                        }
                    }
                }
            }

            #endregion

            #region [ Deerclops ]

            if (!NPC.downedDeerclops && currentbossdefeated != BossDType.Deerclops)
            {
                int[] addin = { 5090, 5095, 5098, 5100, 5108, 5110, 5111, 5117, 5118, 5119, 5385 };

                int[] vanityids = { 5101, 5109, 5113 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Deerclops");
                        }
                    }
                }
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Deerclops");
                    }
                }
            }

            #endregion


            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
            {
                for (int i = 364; i <= 391; i++)
                {
                    if (i == 370) continue;
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }

                for (int i = 487; i <= 497; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 514; i <= 528; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 1184; i <= 1224; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 1528; i <= 1537; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3006; i <= 3016; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3797; i <= 3808; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3884; i <= 3898; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3931; i <= 3950; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3955; i <= 3976; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 3979; i <= 3987; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 5898; i <= 5918; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (int i = 6067; i <= 6089; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "HardMode | Wall of Flesh");
                    }
                }

                if (!allowmusicbox)
                {
                    for (int i = 562; i <= 574; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    for (int i = 1596; i <= 1610; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    for (int i = 5014; i <= 5040; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "HardMode | Wall of Flesh");
                        }
                    }
                    int[] musicboxids = { 576, 2742, 3236, 3237, 3371, 3796, 4077, 4078,
                        4079, 4080, 4081, 4082, 4356, 4357, 4358, 4606, 4979, 4990,
                        4991, 4992, 5006, 5112, 5362, 5538, 5539, 5578, 5579, 5580, 5581,
                        5582, 5637, 5638, 5639, 6144 };

                    foreach (int add in musicboxids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }

                }

                int[] addin = { 400, 401, 402, 403, 404, 406, 408, 409, 412, 415, 416, 417, 420,
                    421, 422, 425, 426, 434, 435, 436, 437, 481, 482, 483, 484, 485, 497, 499, 500,
                    501, 502, 507, 508, 531, 532, 534, 535, 536, 537, 544, 545, 546, 556, 557, 575,
                    604, 605, 672, 682, 683, 684, 685, 686, 723, 725, 726, 761, 763, 770, 776, 777,
                    778, 783, 785, 822, 828, 854, 855, 856, 860, 861, 862, 886, 888, 889, 892, 893, 897,
                    900, 901, 903, 905, 970, 971, 972, 973, 991, 992, 993, 995, 996, 1104, 1105, 1106,
                    1165, 1171, 1181, 1244, 1247, 1253, 1264, 1265, 1306, 1308, 1311, 1312, 1314,
                    1315, 1321, 1324, 1326, 1328, 1332, 1333, 1334, 1335, 1336, 1347, 1348, 1351,
                    1352, 1353, 1355, 1356, 1365, 1519, 1520, 1589, 1590, 1591, 1592, 1593, 1594, 1612,
                    1613, 1704, 1705, 1710, 1716, 1720, 1967, 1968, 2105, 2133, 2137, 2143, 2147,
                    2151, 2155, 2161, 2169, 2170, 2177, 2209, 2238, 2270, 2331, 2370, 2371, 2372,
                    2379, 2389, 2405, 2429, 2454, 2462, 2463, 2465, 2468, 2471, 2473, 2480, 2483,
                    2484, 2494, 2551, 2584, 2607, 2673, 2693, 2694, 2695, 2696, 2697, 2698, 2701,
                    2750, 2751, 2752, 2753, 2754, 2755, 2787, 2788, 2801, 2802, 2998, 3020, 3022,
                    3023, 3029, 3030, 3033, 3034, 3035, 3046, 3049, 3051, 3052, 3053, 3054, 3064,
                    3091, 3092, 3103, 3104, 3182, 3184, 3185, 3186, 3209, 3210, 3211, 3214,
                    3234, 3238, 3258, 3260, 3269, 3283, 3289, 3290, 3315, 3316, 3324, 3334, 3335,
                    3338, 3339, 3343, 3346, 3351, 3359, 3366, 3385, 3386, 3387, 3388, 3752, 3753,
                    3754, 3755, 3764, 3765, 3766, 3767, 3768, 3769, 3770, 3771, 3776, 3777, 3778,
                    3779, 3781, 3782, 3783, 3784, 3785, 3786, 3787 ,3788, 3794, 3795, 3903, 3904,
                    3907, 3910, 3911, 3915, 3917, 3918, 3920, 3991, 3992, 4001, 4002, 4006, 4017,
                    4023, 4027, 4054, 4091, 4124, 4125, 4139, 4140, 4269, 4270, 4272, 4277, 4278,
                    4279, 4280, 4317, 4348, 4406, 4408, 4488, 4505, 4525, 4526, 4527, 4528, 4611,
                    4613, 4615, 4669, 4675, 4683, 4684, 4695, 4696, 4697, 4712, 4713, 4714, 4735,
                    4736, 4760, 4761, 4788, 4792, 4795, 4878, 4911, 4912, 4930, 4940, 4963, 4988,
                    5003, 5004, 5089, 5096, 5130, 5135, 5231, 5274, 5236, 5239, 5260, 5261, 5262,
                    5334, 5336, 5354, 5355, 5381, 5460, 5461, 5462, 5463, 5514, 5536, 5588, 5589,
                    5590, 5591, 5592, 5593, 5594, 5595, 5627, 5659, 5671 };

                int[] blockvanity = { 5488, 5492, 5493, 5494, 5926, 5927 };

                int[] vanityids = { 503, 504, 505, 754, 755, 870, 871, 872, 1277, 1278, 1279, 1280,
                    1739, 1983, 3025, 3026, 3027, 3028, 3038, 3039, 3040, 3041, 3042, 3190, 3242,
                    3243, 3244, 3259, 3263, 3264, 3265, 3533, 3534, 3535, 3553, 3554, 3555, 3560,
                    3561, 3562, 3597, 3598, 3600, 3773, 3774, 3775, 4994, 4995, 4996, 4997, 4998,
                    4999, 5457, 5458, 5459, 5596 };

                if (true)
                {
                    foreach (int add in blockvanity)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                }

                if (!allowvanity)
                {
                    if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
                    {
                        int[] skeletronvanityids = { 864, 865, 869, 873, 874, 875, 4994, 4995,
                            4996, 4997, 4998, 4999 };
                        foreach (int add in skeletronvanityids)
                        {
                            if (!getillegalitems.ContainsKey(add))
                            {
                                getillegalitems.Add(add, "Skeletron");
                            }
                        }
                    }
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    int[] hminprehm = { 621, 624, 627, 630, 633, 637, 640, 643, 646,
                        649, 652, 659, 660, 661, 736, 737, 738, 2004, 2005, 2027, 2039,
                        2051, 2061, 2078, 2088, 2099, 2212, 2310, 2317, 2352, 2400, 2602,
                        2830, 3207, 4098, 4297, 4288, 4623, 4693, 4916, 5310, 5571 };

                    foreach (int add in hminprehm)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {
                        if (!getillegalitems.ContainsKey(662))
                        {
                            getillegalitems.Add(662, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(663))
                        {
                            getillegalitems.Add(663, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(3045))
                        {
                            getillegalitems.Add(3045, "HardMode | Wall of Flesh");
                        }
                        if (!getillegalitems.ContainsKey(3050))
                        {
                            getillegalitems.Add(3050, "HardMode | Wall of Flesh");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Slime ]

            if (!NPC.downedQueenSlime && currentbossdefeated != BossDType.QueenSlime)
            {
                int[] addin = { 4758, 4950, 4957, 4958, 4959, 4960, 4980, 4981, 4982,
                    4983, 4984, 4986, 4987, 5131 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Queen Slime");
                    }
                }
            }

            #endregion

            #region ( Mechanical Bosses )

            #region [ Any Mechanical Bosses ]
            if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
            {
                for (int i = 3602; i <= 3608; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Mechanical Bosses");
                    }
                }
                for (int i = 5712; i <= 5734; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Mechanical Bosses");
                    }
                }

                int[] addin = { 550, 551, 552, 553, 558, 559, 578, 665, 748, 749, 756, 775, 779, 780, 781,
                    782, 784, 787, 809, 813, 817, 821, 1225, 1291, 1302, 1337, 1344, 1515,
                    1518, 1521, 1583, 1584, 1585, 1586, 1611, 1708, 1712, 1718, 1722, 2022, 2024,
                    2034, 2036, 2047, 2057, 2067, 2074, 2084, 2094, 2096, 2125, 2130, 2193, 2203, 2241,
                    2246, 2250, 2253, 2256, 2412, 2598, 2617, 2627, 2638, 2640, 2649, 2655, 2832, 2845,
                    3228, 3284, 3286, 3287, 3288, 3368, 3580, 3582, 3588, 3592, 3618, 3623,
                    3663, 3726, 3727, 3728, 3729, 3751, 3819, 3823, 3825, 3830, 3833, 3835, 3836, 3852,
                    3854, 3856, 3865, 3868, 3924, 3928, 4060, 4102, 4114, 4126, 4142, 4678, 4730, 4746,
                    4750, 4754, 4790, 4816, 4873, 4896, 4897, 4898, 4899, 4900, 4901, 4947, 5338, 5496,
                    5540, 5541, 5660, 5662, 5683, 5684, 5685, 5686, 5586, 6140 };

                int[] vanityids = { 666, 667, 668, 839, 840, 841, 1554, 1555, 1556, 1557, 1558, 1559, 1560,
                    1561, 1562, 1563, 1564, 1565, 1566, 1567, 1568, 1580, 1581, 1582, 1587, 1588,
                    1742, 1986, 2869, 2870, 2873, 2883, 3024, 3226, 3227, 3578, 3579, 3581, 3583, 3585,
                    3586, 3587, 3589, 3590, 3591, 3599, 3921, 3922, 3923, 3925, 3926, 3927,
                    3929, 4732, 4733, 4734, 4747, 4748, 4749, 4751, 4752, 4753, 4755, 4756,
                    4757, 5583, 5584, 5585, 5587, 6137, 6138, 6139, 6141 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Mechanical Bosses");
                        }
                    }
                }
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Mechanical Bosses");
                    }
                }

                int[] templeids = { 497, 900, 1165, 1311, 1520, 5236, 5239, 5260, 5261, 5262 };

                if (!allowtemplerush)
                {
                    foreach (int add in templeids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Mechanical Bosses");
                        }
                    }
                }
            }
            #endregion

            #region [ Post Mechanical Bosses ]

            if (((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)))
            {
                for (int i = 1226; i <= 1235; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Post Mechanical Bosses");
                    }
                }

                int[] addin = { 579, 674, 675, 935, 936, 947, 990, 1001, 1002, 1003, 1004, 1005, 1006,
                    1179, 1262, 1316, 1317, 1318, 1327, 1343, 2188, 2220, 2223, 2789, 2792, 3353, 5223, 5289,
                    5296, 5382, 5524 };


                if (!allowvanity)
                {
                    if (!getillegalitems.ContainsKey(1985))
                    {
                        getillegalitems.Add(1985, "Post Mechanical Bosses");
                    }
                }
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Post Mechanical Bosses");
                    }
                }
            }
            #endregion

            #region [ The Destroyer ]

            if (!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer)
            {
                int[] addin = { 533, 548, 561, 1366, 3325, 3355, 4699, 4803, 4932 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "The Destroyer");
                    }
                }
            }

            #endregion

            #region [ The Twins ]

            if (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins)
            {
                int[] addin = { 549, 1368, 1369, 2106, 2535, 3326, 3354,
                    4698, 4804, 4931 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "The Twins");
                    }
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.netID == 125 || npc.netID == 126)
                        {
                            getillegalitems.Remove(1368);
                            getillegalitems.Remove(1369);
                            break;
                        }
                    }
                }
            }

            #endregion

            #region [ Skeletron Prime ]

            if (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)
            {
                int[] addin = { 506, 547, 1367, 2107, 3327, 3356, 4700, 4805, 4933 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Skeletron Prime");
                    }
                }
            }

            #endregion

            //mech
            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
            {
                for (int i = 1155; i <= 1162; i++)
                {
                    if (i == 1158) continue;
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }
                for (int i = 1543; i <= 1552; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }
                for (int i = 1829; i <= 1837; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Plantera");
                    }
                }

                if (!allowmusicbox)
                {
                    for (int i = 1963; i <= 1965; i++)
                    {
                        if (!getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Add(i, "Plantera");
                        }
                    }
                }

                int[] addin = { 679, 757, 758, 759, 760, 771, 772, 773, 774, 786, 788, 823, 937, 938,
                    963, 977, 1137, 1141, 1167, 1178, 1182, 1183, 1254, 1255, 1259, 1260, 1266,
                    1300, 1305, 1339, 1340, 1341, 1342, 1346, 1350, 1357, 1370, 1444, 1445, 1446,
                    1503, 1504, 1505, 1506, 1507, 1508, 1513, 1517, 1569, 1570, 1571, 1572, 1573,
                    1729, 1730, 1782, 1783, 1784, 1785, 1797, 1798, 1801, 1802, 1803, 1804, 1805,
                    1806, 1807, 1811, 1814, 1815, 1816, 1817, 1818, 1826, 1844, 1845, 1855, 1856,
                    1858, 1864, 1866, 1871, 1914, 1916, 1928, 1929, 1930, 1931, 1946, 1947,
                    1958, 1959, 1960, 1961, 1962, 2028, 2043, 2064, 2071, 2081, 2091, 2103, 2109,
                    2176, 2189, 2383, 2393, 2409, 2605, 2620, 2650, 2770, 2791, 2794, 2847, 3018,
                    3021, 3098, 3105, 3106, 3107, 3108, 3249, 3261, 3291, 3292, 3328, 3336, 3997,
                    3998, 4005, 4013, 4116, 4409, 4444, 4445, 4446, 4447, 4448, 4449, 4457, 4458,
                    4459, 4607, 4679, 4680, 4701, 4731, 4789, 4793, 4794, 4806, 4812, 4813, 4814,
                    4934, 4941, 4942, 4943, 4944, 4945, 4961, 4964, 4985, 5065, 5227, 5237, 5288,
                    5291, 5292, 5344, 5345, 5451, 5452, 5477, 5738 };

                int[] blockvanity = { 5928, 5598, 5599 };

                int[] vanityids = { 1514, 1743, 1744, 1745, 1788, 1789, 1790, 1857, 1943, 1944,
                    1945, 2878, 2879, 2884, 2885, 4738, 4739, 4740, 4741, 4742, 5071, 5072, 5073,
                    5076, 5077, 5078, 5079, 5080 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Plantera");
                    }
                }

                if (true)
                {
                    foreach (int add in blockvanity)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (!allowtemplerush)
                {
                    int[] skipplantids = { 1142, 1143, 1144, 1445, 1150, 1151, 1152, 1153, 1154, 1172,
                        1293, 2195, 2595, 2766, 2767, 5230 };
                    foreach (int add in skipplantids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Plantera");
                        }
                    }
                }

                if (Main.tenthAnniversaryWorld)
                {
                    for (int i = 5076; i <= 5088; i++)
                    {
                        if (allowvanity)
                        {
                            if (vanityids.Contains(i))
                            {
                                continue;
                            }
                        }
                        if (getillegalitems.ContainsKey(i))
                        {
                            getillegalitems.Remove(i);
                        }
                    }

                    int[] princessids = { 5071, 5072, 5073 };
                    foreach (int add in princessids)
                    {
                        if (getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Remove(add);
                        }
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
            {
                for (int i = 2803; i <= 2826; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Golem");
                    }
                }
                for (int i = 3870; i <= 3883; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Golem");
                    }
                }
                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(4237))
                    {
                        getillegalitems.Add(4237, "Golem");
                    }
                }
                //
                int[] addin = { 899, 948, 1101, 1102, 1122, 1146, 1147, 1148, 1149,
                    1248, 1258, 1261, 1292, 1294, 1295, 1296, 1297, 1301, 1371, 1865, 2030,
                    2041, 2052, 2062, 2069, 2079, 2089, 2101, 2110, 2199, 2200, 2201, 2202,
                    2218, 2280, 2385, 2396, 2416, 2749, 2769, 2771, 2795, 2796, 2797, 2798,
                    2800, 2836, 2855, 2860, 2861, 2862, 2863, 2865, 2866, 2867, 2880,
                    2882, 3110, 3235, 3329, 3337, 3358, 3546, 3820, 3826, 3827, 3831, 3834,
                    3858, 3859, 3860, 3863, 3866, 3906, 4106, 4121, 4807, 4815, 4817, 4935,
                    4939, 4948 };

                int[] vanityids = { 3556, 2864 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Golem");
                    }
                }
                if (!allowvanity)
                {
                    foreach (int add in addin)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Golem");
                        }
                    }
                }
            }

            #endregion

            #region [ Duke Fishron ]

            if (!NPC.downedFishron && currentbossdefeated != BossDType.DukeFishron)
            {
                int[] addin = { 2588, 2589, 2609, 2611, 2621, 2622, 2623, 2624, 3330, 3367,
                    4808, 4936, 5478, 5526 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Duke Fishron");
                    }
                }
            }

            #endregion

            #region [ Empress of Light ]

            if (!NPC.downedEmpressOfLight && currentbossdefeated != BossDType.EmpressOfLight)
            {
                int[] addin = { 4715, 4778, 4782, 4783, 4784, 4811, 4823, 4914, 4923, 4949,
                    4952, 4953, 4989, 5005 };

                int[] vanityids = { 5075 };

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Empress of Light");
                        }
                    }
                }

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Empress of Light");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
            {
                for (int i = 4145; i <= 4236; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Lunatic Cultist");
                    }
                }

                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(3370))
                    {
                        getillegalitems.Add(3370, "Lunatic Cultist");
                    }
                }

                int[] addin = { 3331, 3357, 3372, 3456,
                    3457, 3458, 3459, 3473, 3474, 3475, 3476, 3536, 3537, 3538,
                    3539, 3540, 3542, 3543, 3544, 3549, 3572, 3573, 3574, 3575,
                    3576, 3601, 4809, 4937, 5479 };

                int[] vanityids = { 2856, 2857, 2858, 2859, 3526, 3527, 3528, 3529 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Lunatic Cultist");
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Lunatic Cultist");
                        }
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord && currentbossdefeated != BossDType.MoonLord)
            {
                for (int i = 2757; i <= 2765; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Moon Lord");
                    }
                }
                for (int i = 5401; i <= 5416; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Moon Lord");
                    }
                }

                int[] addin = { 1553, 2768, 2774, 2776, 2779, 2781, 2784, 2786, 3063,
                    3065, 3332, 3373, 3381, 3382, 3383, 3384, 3389, 3460, 3461, 3464,
                    3466, 3467, 3468, 3469, 3470, 3471, 3472, 3522, 3523, 3524, 3525,
                    3541, 3567, 3568, 3569, 3570, 3571, 3577, 3595, 3596, 3664, 3930,
                    4318, 4810, 4938, 4954, 4956, 5134, 5335, 5364, 5392, 5393, 5394,
                    5480, 5669 };

                int[] vanityids = { 3530 };

                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Moon Lord");
                    }
                }

                if (!allowvanity)
                {
                    foreach (int add in vanityids)
                    {
                        if (!getillegalitems.ContainsKey(add))
                        {
                            getillegalitems.Add(add, "Moon Lord");
                        }
                    }
                }

                if (!allowmusicbox)
                {
                    if (!getillegalitems.ContainsKey(3044))
                    {
                        getillegalitems.Add(3044, "Moon Lord");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    if (!getillegalitems.ContainsKey(5044))
                    {
                        getillegalitems.Add(5044, "Moon Lord");
                    }
                }
            }

            #endregion


            #region ( Difficulty )
            /*
            if (Main.expertMode)
            {
                if (Main.masterMode)
                {
                    int[] masterids = { };

                    foreach (int add in masterids)
                    {
                        getillegalitems.Remove(add);
                    }
                }
                int[] expertids = { 1131 };

                foreach (int add in expertids)
                {
                    getillegalitems.Remove(add);
                }
            }
            */
            #endregion

            #endregion

            #region ( Invasion )

            #region [ Goblin Army ]
            if (!NPC.downedGoblins) // Goblin Army
            {
                for (int i = 395; i <= 399; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Goblin Army");
                    }
                }
                for (int i = 3990; i <= 4008; i++)
                {
                    if (!getillegalitems.ContainsKey(i))
                    {
                        getillegalitems.Add(i, "Goblin Army");
                    }
                }

                int[] addin = { 128, 405, 407, 486, 898, 907, 908, 976, 982, 983, 1163,
                    1164, 860, 861, 862, 901, 902, 903, 904, 1247, 935, 936, 984, 1249,
                    1250, 1251, 1252, 1301, 1343, 1578, 1595, 1612, 1613, 1724, 1860,
                    1861, 1862, 1863, 1864, 1865, 2220, 2221, 3110, 3121, 3122, 3123,
                    3124, 3241, 3250, 3251, 3252, 3366, 3721, 4038, 4874, 5000, 5064,
                    5126, 5331, 5358, 5359, 5360, 5361 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Goblin Army");
                    }
                }
                if ((Main.hardMode || currentbossdefeated == BossDType.WallOfFlesh) && (Main.tenthAnniversaryWorld || Main.zenithWorld))
                {
                    if (!getillegalitems.ContainsKey(1724))
                    {
                        getillegalitems.Remove(1724);
                    }
                }
            }
            #endregion

            #region [ Pirate Invasion ]
            if (!NPC.downedPirates) // Pirate Invasion
            {
                int[] addin = { 876, 877, 878, 928, 929, 1180, 1337, 3369 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Frost Legion ]
            if (!NPC.downedFrost)
            {
                int[] addin = { 3055, 3056, 3057, 3058, 3059 };
                foreach (int add in addin)
                {
                    if (!getillegalitems.ContainsKey(add))
                    {
                        getillegalitems.Add(add, "Frost Legion");
                    }
                }
            }
            #endregion

            #region [ Solar Pillar ]
            if (!NPC.downedTowerSolar)
            {
                if (!getillegalitems.ContainsKey(4677))
                {
                    getillegalitems.Add(4677, "Solar Pillar");
                }
            }
            #endregion

            #endregion

            #region ( weapon switch )
            if (Main.remixWorld || Main.zenithWorld)
            {
                if (getillegalitems.ContainsKey(517))
                {
                    getillegalitems.Remove(517);
                }
                if (getillegalitems.ContainsKey(671))
                {
                    getillegalitems.Remove(671);
                }
                if (getillegalitems.ContainsKey(1314))
                {
                    getillegalitems.Remove(1314);
                }
                if (getillegalitems.ContainsKey(725))
                {
                    getillegalitems.Remove(725);
                }
                if (getillegalitems.ContainsKey(2623))
                {
                    if (allowdungeonrush || (NPC.downedBoss3 || currentbossdefeated == BossDType.Skeletron))
                    {
                        getillegalitems.Remove(2623);
                    }

                }
                if (getillegalitems.ContainsKey(683))
                {
                    if (allowdungeonrush || (NPC.downedBoss3 || currentbossdefeated == BossDType.Skeletron))
                    {
                        getillegalitems.Remove(683);
                    }

                }
                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    if (!getillegalitems.ContainsKey(3069))
                    {
                        getillegalitems.Add(3069, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(5147))
                    {
                        getillegalitems.Add(5147, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(1325))
                    {
                        getillegalitems.Add(1325, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalitems.ContainsKey(1319))
                    {
                        getillegalitems.Add(1319, "HardMode | Wall of Flesh");
                    }
                }
                if (!NPC.downedMechBossAny && (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
                {
                    if (!getillegalitems.ContainsKey(112))
                    {
                        getillegalitems.Add(112, "Mechanical Bosses");
                    }
                }
                if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
                {
                    if (!getillegalitems.ContainsKey(2273))
                    {
                        getillegalitems.Add(2273, "Plantera");
                    }

                }
                if (!NPC.downedFishron && currentbossdefeated != BossDType.DukeFishron)
                {
                    if (!getillegalitems.ContainsKey(157))
                    {
                        getillegalitems.Add(157, "Duke Fishron");
                    }
                }
            }
            #endregion

            #endregion

            foreach (int whitelist in MKLP.Config.Main.DisableNode.WhiteList_Survival_Code1)
            {
                if (getillegalitems.ContainsKey(whitelist))
                {
                    getillegalitems.Remove(whitelist);
                }
            }

            return getillegalitems;
        }

        public static Dictionary<short, string> GetIllegalProjectile(BossDType currentbossdefeated = BossDType.NA)
        {
            #region [ get Illegal Projectile ]

            Dictionary<short, string> getillegalprojectile = new();

            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            bool allowdungeonrush = (bool)MKLP.Config.Main.Progression.AllowDungeonRush;
            bool allowtemplerush = (bool)MKLP.Config.Main.Progression.AllowTempleRush;

            #region | unobtainable |

            short[] hostileProj = { 38, 39, 40, 44, 55, 56, 67, 71, 75, 81, 82, 83, 84,
                98, 99, 100, 101, 102, 109, 110, 115, 128, 129, 164, 174, 176, 177,
                179, 180, 184, 185, 186, 187, 188, 240, 241, 258, 259, 262, 264,
                275, 276, 277, 288, 290, 291, 292, 293, 299, 300, 302, 303, 325,
                326, 327, 328, 329, 345, 346, 347, 348, 349, 350, 351, 352, 384,
                385, 386, 435, 436, 437, 438, 447, 448, 449, 450, 452, 453, 454,
                455, 456, 462, 464, 465, 466, 467, 468, 472, 490, 498, 501, 526,
                537, 538, 539, 540, 572, 573, 574, 575, 576, 577, 578, 579, 580,
                581, 592, 593, 594, 596, 605, 607, 608, 622, 654, 657, 658, 670,
                671, 672, 673, 674, 675, 676, 682, 683, 686, 687, 719, 727, 811,
                813, 814, 836, 871, 872, 873, 874, 909, 919, 920, 921, 922, 923,
                926, 961, 962, 965, 980, 1001, 1002, 1005, 1007, 1013, 1014, 1021,
                1049, 1073, 1091, 1092, 1099 };

            short[] unobtainableids = { 39, 111, 132, 156, 157, 427, 428, 429, 430, 431,
                432, 457, 458, 582, 583, 584, 585, 586, 589, 590, 609, 610, 624, 857,
                880, 924, 925, 929 };

            foreach (short add in unobtainableids)
            {
                if (!getillegalprojectile.ContainsKey(add))
                {
                    getillegalprojectile.Add(add, "Unobtainable");
                }
            }

            foreach (short add in unobtainableids)
            {
                if (!getillegalprojectile.ContainsKey(add))
                {
                    getillegalprojectile.Add(add, "Hostile");
                }
            }

            if (!Main.zenithWorld)
            {
                if (!getillegalprojectile.ContainsKey(1012))
                {
                    getillegalprojectile.Add(1012, "Unobtainable");
                }
            }

            #endregion

            //357 

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing && currentbossdefeated != BossDType.KingSlime) //king slime
            {
                short[] addin = { 406, 881, 1042 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "King Slime");
                    }
                }

                short[] solidifier = { 1103, 1104 };
                if (true)
                {
                    foreach (short add in solidifier)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "King Slime");
                        }
                    }

                }
            }
            #endregion

            #region [ Eye of Cthulhu ]
            if (!NPC.downedBoss1 && currentbossdefeated != BossDType.EyeOfCthulhu)
            {
                short[] addin = { 17, 882, 994, 995 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Eye of Cthulhu");
                    }
                }

            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
            {
                for (short i = 663; i <= 669; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Evil Boss");
                    }
                }
                for (short i = 688; i <= 712; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Evil Boss");
                    }
                }

                short[] addin = { 20, 175, 534, 564, 677, 678, 679, 680, 883, 884, 955, 1022, 1031, 1037 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode)
                {
                    short[] hellstoneid = { 19, 36, 41, 154, 375, 376, 734, 972, 973, 978 };

                    foreach (short add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Evil Boss");
                        }
                    }
                }
            }
            #endregion

            #region ( Evil Boss Enabled )

            if ((bool)MKLP.Config.BossManager.AllowEaterOfWorlds || (bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
            { //     true if its eow/boc enabled    |    false if its not enabled
                short[] hellstoneid = { 19, 36, 41, 154, 375, 376, 734, 972, 973, 978, 1022,
                    1037 };

                foreach (short remove in hellstoneid) // hellstone are breakable from bombs on hm
                {
                    if (!getillegalprojectile.ContainsKey(remove))
                    {
                        getillegalprojectile.Remove(remove);
                    }
                }
            }
            else
            {

            }
            //86
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
            {
                short[] addin = { 256, 270, 532, 545, 837, 885, 902, 1085 };

                short[] dungeonrushids = { 15, 16, 22, 26, 34, 35, 46, 485, 565, 972, 973, 977 };



                foreach (short add in dungeonrushids)
                {
                    if (!allowdungeonrush)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Skeletron");
                        }
                    }

                }

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Skeletron");
                    }
                }

                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    short[] skeleids = { 734, 1019 };
                    foreach (short add in skeleids)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Bee ]

            if (!NPC.downedQueenBee && currentbossdefeated != BossDType.QueenBee)
            {
                short[] addin = { 181, 183, 198, 373, 374, 469, 566, 886, 999 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Queen Bee");
                    }
                }
            }

            #endregion

            #region [ Deerclops ]

            if (!NPC.downedDeerclops && currentbossdefeated != BossDType.Deerclops)
            {
                short[] addin = { 958, 960, 964, 966, 967, 968, 969 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Deerclops");
                    }
                }
            }

            #endregion


            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
            {
                for (short i = 57; i <= 64; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 88; i <= 97; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 212; i <= 221; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }
                for (short i = 476; i <= 482; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "HardMode | Wall of Flesh");
                    }
                }

                short[] addin = { 66, 68, 69, 73, 74, 76, 79, 80, 103, 104, 116, 117, 120, 130, 131,
                    158, 159, 160, 161, 208, 209, 237, 238, 239, 242, 253, 263, 265, 269, 271, 272,
                    278, 279, 280, 286, 287, 359, 367, 377, 378, 379, 382, 393, 390, 391, 392, 393,
                    394, 395, 424, 425, 426, 486, 488, 489, 491, 493, 494, 495, 496, 497, 521, 522,
                    523, 524, 535, 536, 546, 552, 553, 562, 563, 656, 659, 660, 661, 723, 724, 725,
                    726, 755, 756, 757, 823, 826, 838, 852, 853, 858, 859, 866, 877, 908, 912, 913,
                    917, 918, 928, 957, 1023, 5461, 1025, 1026, 1040, 1041, 1043, 1059, 1060, 1061,
                    1062, 1063, 1064, 1071, 1076, 1086, 1087, 1102, 1107, 1108, 1109 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    short[] hminprehm = { 370 };

                    foreach (short add in hminprehm)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {

                    }
                }
            }
            #endregion

            #region [ Queen Slime ]

            if (!NPC.downedQueenSlime && currentbossdefeated != BossDType.QueenSlime)
            {
                short[] addin = { 864, 934, 935, 936, 937 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Queen Slime");
                    }
                }
            }

            #endregion

            #region ( Mechanical Bosses )

            #region [ Any Mechanical Bosses ]
            if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
            {
                short[] addin = { 105, 114, 268, 274, 547, 549, 550, 551, 595, 652,
                    665, 666, 678, 689, 692, 695, 697, 698, 699, 700, 702, 704,
                    705, 706, 712, 728, 729, 847, 879, 900, 982 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Mechanical Bosses");
                    }
                }
            }
            #endregion

            #region [ Post Mechanical Bosses ]
            if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime))
            {
                for (short i = 222; i <= 229; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Post Mechanical Bosses");
                    }
                }

                short[] addin = { 107, 207, 252, 355, 357, 591, 973, 983 };


                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Post Mechanical Bosses");
                    }
                }
            }
            #endregion

            #region [ The Destroyer ]

            if (!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer)
            {
                short[] addin = { 106, 887 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "The Destroyer");
                    }
                }
            }

            #endregion

            #region [ The Twins ]

            if (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins)
            {
                short[] addin = { 72, 76, 77, 78, 86, 87, 387, 388, 389, 888 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "The Twins");
                    }
                }
            }

            #endregion

            #region [ Skeletron Prime ]

            if (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)
            {
                short[] addin = { 85, 889 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Skeletron Prime");
                    }
                }
            }

            #endregion

            //mechs
            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
            {
                for (short i = 133; i <= 144; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 335; i <= 344; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 776; i <= 801; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }
                for (short i = 803; i <= 810; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Plantera");
                    }
                }

                short[] addin = { 150, 151, 152, 189, 190, 191, 192, 193, 194, 195, 199, 206, 210,
                    211, 247, 248, 250, 251, 254, 255, 282, 283, 285, 294, 295, 296, 297, 298,
                    301, 304, 305, 306, 307, 308, 309, 311, 312, 313, 314, 316, 317, 321, 322,
                    323, 324, 353, 356, 483, 484, 487, 504, 509, 510, 511, 512, 513, 514, 533,
                    554, 555, 567, 568, 569, 570, 571, 831, 832, 833, 834, 848, 849, 854, 862,
                    863, 878, 890, 896, 897, 898, 907, 916, 930, 984, 985, 997, 1020, 1032, 1038,
                    1074, 1105 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Plantera");
                    }
                }

                if (!allowtemplerush)
                {
                    short[] templeids = { 200 };

                    foreach (short add in templeids)
                    {
                        if (!getillegalprojectile.ContainsKey(add))
                        {
                            getillegalprojectile.Add(add, "Plantera");
                        }
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    if (!getillegalprojectile.ContainsKey(950))
                    {
                        getillegalprojectile.Add(950, "Plantera");
                    }
                    if (!getillegalprojectile.ContainsKey(956))
                    {
                        getillegalprojectile.Add(956, "Plantera");
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
            {
                for (short i = 439; i <= 446; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Golem");
                    }
                }

                short[] addin = { 182, 246, 249, 260, 261, 423, 433, 434, 451, 459,
                    460, 461, 606, 667, 668, 679, 684, 690, 693, 696, 707, 708,
                    709, 710, 711, 891, 899, 901, 1110 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Golem");
                    }
                }
            }

            #endregion

            #region [ Duke Fishron ]

            if (!NPC.downedFishron && currentbossdefeated != BossDType.DukeFishron)
            {
                short[] addin = { 404, 405, 407, 408, 409, 410, 892, 1033, 1058 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Duke Fishron");
                    }
                }
            }

            #endregion

            #region [ Empress of Light ]

            if (!NPC.downedEmpressOfLight && currentbossdefeated != BossDType.EmpressOfLight)
            {
                short[] addin = { 856, 895, 915, 927, 931, 932, 946 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Empress of Light");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
            {
                short[] addin = { 625, 626, 627, 630, 631, 634, 635, 636, 893, 953, 1034, 1039 };

                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Lunatic Cultist");
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord && currentbossdefeated != BossDType.MoonLord)
            {
                for (short i = 638; i <= 650; i++)
                {
                    if (!getillegalprojectile.ContainsKey(i))
                    {
                        getillegalprojectile.Add(i, "Moon Lord");
                    }
                }
                short[] addin = { 502, 503, 600, 601, 602, 603, 604, 611, 612, 613,
                    614, 615, 616, 617, 618, 619, 620, 623, 632, 633, 714, 715,
                    716, 717, 718, 894, 933, 1015, 1016, 1017, 1035, 1045, 1100 };


                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Moon Lord");
                    }
                }
            }

            #endregion

            #endregion

            #region ( Invasion )

            #region [ Goblin Army ]
            if (!NPC.downedGoblins) // Goblin Army
            {
                /*
                int[] addin = { };

                foreach (int add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Goblin Army");
                    }
                }
                */
            }
            #endregion

            #region [ Pirate Invasion ]
            if (!NPC.downedPirates) // Pirate Invasion
            {
                short[] addin = { 162, 281 };
                foreach (short add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Frost Legion ]
            if (!NPC.downedFrost)
            {
                /*
                int[] addin = { };
                foreach (int add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Frost Legion");
                    }
                }
                */
            }
            #endregion

            #region [ Solar Pillar ]
            if (!NPC.downedTowerSolar)
            {
                if (!getillegalprojectile.ContainsKey(846))
                {
                    getillegalprojectile.Add(846, "Solar Pillar");
                }
            }
            #endregion

            #endregion

            #region ( weapon switch )
            if (Main.remixWorld || Main.zenithWorld)
            {
                if (getillegalprojectile.ContainsKey(93))
                {
                    getillegalprojectile.Remove(93);
                }
                if (getillegalprojectile.ContainsKey(271))
                {
                    getillegalprojectile.Remove(271);
                }
                if (getillegalprojectile.ContainsKey(120))
                {
                    getillegalprojectile.Remove(120);
                }
                if (getillegalprojectile.ContainsKey(410))
                {
                    if (allowdungeonrush || (NPC.downedBoss3 || currentbossdefeated == BossDType.Skeletron))
                    {
                        getillegalprojectile.Remove(410);
                    }

                }
                if (getillegalprojectile.ContainsKey(114))
                {
                    if (allowdungeonrush || (NPC.downedBoss3 || currentbossdefeated == BossDType.Skeletron))
                    {
                        getillegalprojectile.Remove(114);
                    }

                }
                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    if (!getillegalprojectile.ContainsKey(954))
                    {
                        getillegalprojectile.Add(954, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalprojectile.ContainsKey(979))
                    {
                        getillegalprojectile.Add(979, "HardMode | Wall of Flesh");
                    }
                    if (!getillegalprojectile.ContainsKey(273))
                    {
                        getillegalprojectile.Add(273, "HardMode | Wall of Flesh");
                    }
                }
                if (!NPC.downedMechBossAny && (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
                {
                    if (!getillegalprojectile.ContainsKey(15))
                    {
                        getillegalprojectile.Add(15, "Mechanical Bosses");
                    }
                }
                if (!NPC.downedFishron && currentbossdefeated != BossDType.DukeFishron)
                {
                    if (!getillegalprojectile.ContainsKey(22))
                    {
                        getillegalprojectile.Add(22, "Duke Fishron");
                    }
                }
            }
            #endregion

            #endregion

            foreach (short whitelist in MKLP.Config.Main.DisableNode.WhiteList_Survival_Code2)
            {
                if (getillegalprojectile.ContainsKey(whitelist))
                {
                    getillegalprojectile.Remove(whitelist);
                }
            }

            return getillegalprojectile;
        }

        public struct MKLP_Tile
        {
            public ushort ID;
            public short SubID;
            bool AllSubID;
            public MKLP_Tile(ushort ID, short SubID, bool AllSubID = false)
            {
                this.ID = ID;
                this.SubID = SubID;
                if (AllSubID == true) this.SubID = 0;
                this.AllSubID = AllSubID;
            }

            public static bool[] ObjectIDs = new bool[692];
        }
        private static void INIT_ObjectIDs()
        {
            #region INIT_ObjectIDs
            for (int i = 0; i < MKLP_Tile.ObjectIDs.Length; i++)
            {
                MKLP_Tile.ObjectIDs[i] = false;
            }

            MKLP_Tile.ObjectIDs[10] = true;
            MKLP_Tile.ObjectIDs[11] = true;
            MKLP_Tile.ObjectIDs[12] = true;
            MKLP_Tile.ObjectIDs[14] = true;
            MKLP_Tile.ObjectIDs[15] = true;
            MKLP_Tile.ObjectIDs[16] = true;
            MKLP_Tile.ObjectIDs[17] = true;
            MKLP_Tile.ObjectIDs[18] = true;
            MKLP_Tile.ObjectIDs[20] = true;
            MKLP_Tile.ObjectIDs[21] = true;
            MKLP_Tile.ObjectIDs[26] = true;
            MKLP_Tile.ObjectIDs[27] = true;
            MKLP_Tile.ObjectIDs[28] = true;
            MKLP_Tile.ObjectIDs[29] = true;

            MKLP_Tile.ObjectIDs[31] = true;
            MKLP_Tile.ObjectIDs[34] = true;
            MKLP_Tile.ObjectIDs[35] = true;
            MKLP_Tile.ObjectIDs[42] = true;
            MKLP_Tile.ObjectIDs[55] = true;
            MKLP_Tile.ObjectIDs[77] = true;
            MKLP_Tile.ObjectIDs[79] = true;
            MKLP_Tile.ObjectIDs[85] = true;
            MKLP_Tile.ObjectIDs[86] = true;
            MKLP_Tile.ObjectIDs[87] = true;
            MKLP_Tile.ObjectIDs[88] = true;
            MKLP_Tile.ObjectIDs[89] = true;
            MKLP_Tile.ObjectIDs[90] = true;

            MKLP_Tile.ObjectIDs[91] = true;
            MKLP_Tile.ObjectIDs[92] = true;
            MKLP_Tile.ObjectIDs[93] = true;
            MKLP_Tile.ObjectIDs[94] = true;
            MKLP_Tile.ObjectIDs[95] = true;
            MKLP_Tile.ObjectIDs[96] = true;
            MKLP_Tile.ObjectIDs[97] = true;
            MKLP_Tile.ObjectIDs[99] = true;
            MKLP_Tile.ObjectIDs[100] = true;

            MKLP_Tile.ObjectIDs[101] = true;
            MKLP_Tile.ObjectIDs[102] = true;
            MKLP_Tile.ObjectIDs[103] = true;
            MKLP_Tile.ObjectIDs[104] = true;
            MKLP_Tile.ObjectIDs[105] = true;
            MKLP_Tile.ObjectIDs[106] = true;
            MKLP_Tile.ObjectIDs[125] = true;
            MKLP_Tile.ObjectIDs[126] = true;
            MKLP_Tile.ObjectIDs[128] = true;
            MKLP_Tile.ObjectIDs[133] = true;
            MKLP_Tile.ObjectIDs[134] = true;
            MKLP_Tile.ObjectIDs[138] = true;
            MKLP_Tile.ObjectIDs[139] = true;
            MKLP_Tile.ObjectIDs[142] = true;
            MKLP_Tile.ObjectIDs[143] = true;
            MKLP_Tile.ObjectIDs[165] = true;
            MKLP_Tile.ObjectIDs[171] = true;
            MKLP_Tile.ObjectIDs[172] = true;
            MKLP_Tile.ObjectIDs[173] = true;
            MKLP_Tile.ObjectIDs[185] = true;
            MKLP_Tile.ObjectIDs[186] = true;
            MKLP_Tile.ObjectIDs[187] = true;

            MKLP_Tile.ObjectIDs[209] = true;
            MKLP_Tile.ObjectIDs[212] = true;
            MKLP_Tile.ObjectIDs[215] = true;
            MKLP_Tile.ObjectIDs[216] = true;
            MKLP_Tile.ObjectIDs[217] = true;
            MKLP_Tile.ObjectIDs[218] = true;
            MKLP_Tile.ObjectIDs[219] = true;
            MKLP_Tile.ObjectIDs[220] = true;
            MKLP_Tile.ObjectIDs[228] = true;
            MKLP_Tile.ObjectIDs[231] = true;
            MKLP_Tile.ObjectIDs[233] = true;
            MKLP_Tile.ObjectIDs[235] = true;
            MKLP_Tile.ObjectIDs[236] = true;
            MKLP_Tile.ObjectIDs[237] = true;
            MKLP_Tile.ObjectIDs[238] = true;
            MKLP_Tile.ObjectIDs[240] = true;
            MKLP_Tile.ObjectIDs[241] = true;
            MKLP_Tile.ObjectIDs[242] = true;
            MKLP_Tile.ObjectIDs[243] = true;
            MKLP_Tile.ObjectIDs[244] = true;
            MKLP_Tile.ObjectIDs[245] = true;
            MKLP_Tile.ObjectIDs[246] = true;
            MKLP_Tile.ObjectIDs[247] = true;
            MKLP_Tile.ObjectIDs[254] = true;
            MKLP_Tile.ObjectIDs[269] = true;
            MKLP_Tile.ObjectIDs[270] = true;
            MKLP_Tile.ObjectIDs[271] = true;
            MKLP_Tile.ObjectIDs[275] = true;
            MKLP_Tile.ObjectIDs[276] = true;
            MKLP_Tile.ObjectIDs[277] = true;
            MKLP_Tile.ObjectIDs[278] = true;
            MKLP_Tile.ObjectIDs[279] = true;
            MKLP_Tile.ObjectIDs[280] = true;
            MKLP_Tile.ObjectIDs[281] = true;
            MKLP_Tile.ObjectIDs[282] = true;
            MKLP_Tile.ObjectIDs[283] = true;
            MKLP_Tile.ObjectIDs[285] = true;
            MKLP_Tile.ObjectIDs[286] = true;
            MKLP_Tile.ObjectIDs[287] = true;
            MKLP_Tile.ObjectIDs[288] = true;
            MKLP_Tile.ObjectIDs[289] = true;
            MKLP_Tile.ObjectIDs[290] = true;
            MKLP_Tile.ObjectIDs[291] = true;
            MKLP_Tile.ObjectIDs[292] = true;
            MKLP_Tile.ObjectIDs[293] = true;
            MKLP_Tile.ObjectIDs[294] = true;
            MKLP_Tile.ObjectIDs[295] = true;
            MKLP_Tile.ObjectIDs[296] = true;
            MKLP_Tile.ObjectIDs[297] = true;
            MKLP_Tile.ObjectIDs[298] = true;
            MKLP_Tile.ObjectIDs[299] = true;

            MKLP_Tile.ObjectIDs[300] = true;
            MKLP_Tile.ObjectIDs[301] = true;
            MKLP_Tile.ObjectIDs[302] = true;
            MKLP_Tile.ObjectIDs[303] = true;
            MKLP_Tile.ObjectIDs[304] = true;
            MKLP_Tile.ObjectIDs[305] = true;
            MKLP_Tile.ObjectIDs[306] = true;
            MKLP_Tile.ObjectIDs[307] = true;
            MKLP_Tile.ObjectIDs[308] = true;
            MKLP_Tile.ObjectIDs[309] = true;
            MKLP_Tile.ObjectIDs[310] = true;
            MKLP_Tile.ObjectIDs[316] = true;
            MKLP_Tile.ObjectIDs[317] = true;
            MKLP_Tile.ObjectIDs[318] = true;
            MKLP_Tile.ObjectIDs[319] = true;
            MKLP_Tile.ObjectIDs[320] = true;
            MKLP_Tile.ObjectIDs[334] = true;
            MKLP_Tile.ObjectIDs[335] = true;
            MKLP_Tile.ObjectIDs[337] = true;
            MKLP_Tile.ObjectIDs[338] = true;
            MKLP_Tile.ObjectIDs[339] = true;
            MKLP_Tile.ObjectIDs[349] = true;
            MKLP_Tile.ObjectIDs[354] = true;
            MKLP_Tile.ObjectIDs[355] = true;
            MKLP_Tile.ObjectIDs[356] = true;
            MKLP_Tile.ObjectIDs[358] = true;
            MKLP_Tile.ObjectIDs[359] = true;
            MKLP_Tile.ObjectIDs[360] = true;
            MKLP_Tile.ObjectIDs[361] = true;
            MKLP_Tile.ObjectIDs[362] = true;
            MKLP_Tile.ObjectIDs[363] = true;
            MKLP_Tile.ObjectIDs[364] = true;
            MKLP_Tile.ObjectIDs[376] = true;
            MKLP_Tile.ObjectIDs[377] = true;
            MKLP_Tile.ObjectIDs[378] = true;
            MKLP_Tile.ObjectIDs[386] = true;
            MKLP_Tile.ObjectIDs[387] = true;
            MKLP_Tile.ObjectIDs[388] = true;
            MKLP_Tile.ObjectIDs[389] = true;
            MKLP_Tile.ObjectIDs[390] = true;
            MKLP_Tile.ObjectIDs[391] = true;
            MKLP_Tile.ObjectIDs[392] = true;
            MKLP_Tile.ObjectIDs[393] = true;
            MKLP_Tile.ObjectIDs[394] = true;
            MKLP_Tile.ObjectIDs[395] = true;
            MKLP_Tile.ObjectIDs[405] = true;
            MKLP_Tile.ObjectIDs[406] = true;
            MKLP_Tile.ObjectIDs[410] = true;
            MKLP_Tile.ObjectIDs[411] = true;
            MKLP_Tile.ObjectIDs[412] = true;
            MKLP_Tile.ObjectIDs[413] = true;
            MKLP_Tile.ObjectIDs[414] = true;
            MKLP_Tile.ObjectIDs[425] = true;
            MKLP_Tile.ObjectIDs[440] = true;
            MKLP_Tile.ObjectIDs[441] = true;
            MKLP_Tile.ObjectIDs[443] = true;
            MKLP_Tile.ObjectIDs[444] = true;
            MKLP_Tile.ObjectIDs[452] = true;
            MKLP_Tile.ObjectIDs[453] = true;
            MKLP_Tile.ObjectIDs[454] = true;
            MKLP_Tile.ObjectIDs[455] = true;
            MKLP_Tile.ObjectIDs[456] = true;
            MKLP_Tile.ObjectIDs[457] = true;
            MKLP_Tile.ObjectIDs[462] = true;
            MKLP_Tile.ObjectIDs[463] = true;
            MKLP_Tile.ObjectIDs[464] = true;
            MKLP_Tile.ObjectIDs[465] = true;
            MKLP_Tile.ObjectIDs[466] = true;
            MKLP_Tile.ObjectIDs[467] = true;
            MKLP_Tile.ObjectIDs[468] = true;
            MKLP_Tile.ObjectIDs[469] = true;

            MKLP_Tile.ObjectIDs[470] = true;
            MKLP_Tile.ObjectIDs[471] = true;
            MKLP_Tile.ObjectIDs[475] = true;
            MKLP_Tile.ObjectIDs[480] = true;
            MKLP_Tile.ObjectIDs[484] = true;
            MKLP_Tile.ObjectIDs[485] = true;
            MKLP_Tile.ObjectIDs[486] = true;
            MKLP_Tile.ObjectIDs[487] = true;
            MKLP_Tile.ObjectIDs[488] = true;
            MKLP_Tile.ObjectIDs[489] = true;
            MKLP_Tile.ObjectIDs[490] = true;
            MKLP_Tile.ObjectIDs[491] = true;
            MKLP_Tile.ObjectIDs[493] = true;
            MKLP_Tile.ObjectIDs[497] = true;
            MKLP_Tile.ObjectIDs[499] = true;
            MKLP_Tile.ObjectIDs[505] = true;
            MKLP_Tile.ObjectIDs[506] = true;
            MKLP_Tile.ObjectIDs[507] = true;
            MKLP_Tile.ObjectIDs[508] = true;
            MKLP_Tile.ObjectIDs[509] = true;
            MKLP_Tile.ObjectIDs[510] = true;
            MKLP_Tile.ObjectIDs[511] = true;
            MKLP_Tile.ObjectIDs[520] = true;
            MKLP_Tile.ObjectIDs[521] = true;
            MKLP_Tile.ObjectIDs[522] = true;
            MKLP_Tile.ObjectIDs[523] = true;
            MKLP_Tile.ObjectIDs[524] = true;
            MKLP_Tile.ObjectIDs[525] = true;
            MKLP_Tile.ObjectIDs[526] = true;
            MKLP_Tile.ObjectIDs[527] = true;
            MKLP_Tile.ObjectIDs[530] = true;
            MKLP_Tile.ObjectIDs[531] = true;
            MKLP_Tile.ObjectIDs[532] = true;
            MKLP_Tile.ObjectIDs[533] = true;
            MKLP_Tile.ObjectIDs[538] = true;
            MKLP_Tile.ObjectIDs[542] = true;
            MKLP_Tile.ObjectIDs[543] = true;
            MKLP_Tile.ObjectIDs[544] = true;
            MKLP_Tile.ObjectIDs[545] = true;
            MKLP_Tile.ObjectIDs[547] = true;
            MKLP_Tile.ObjectIDs[548] = true;
            MKLP_Tile.ObjectIDs[550] = true;
            MKLP_Tile.ObjectIDs[551] = true;
            MKLP_Tile.ObjectIDs[552] = true;
            MKLP_Tile.ObjectIDs[553] = true;
            MKLP_Tile.ObjectIDs[554] = true;
            MKLP_Tile.ObjectIDs[555] = true;
            MKLP_Tile.ObjectIDs[556] = true;
            MKLP_Tile.ObjectIDs[558] = true;
            MKLP_Tile.ObjectIDs[559] = true;
            MKLP_Tile.ObjectIDs[560] = true;
            MKLP_Tile.ObjectIDs[564] = true;
            MKLP_Tile.ObjectIDs[565] = true;
            MKLP_Tile.ObjectIDs[567] = true;
            MKLP_Tile.ObjectIDs[568] = true;
            MKLP_Tile.ObjectIDs[569] = true;
            MKLP_Tile.ObjectIDs[570] = true;
            MKLP_Tile.ObjectIDs[572] = true;
            MKLP_Tile.ObjectIDs[573] = true;
            MKLP_Tile.ObjectIDs[580] = true;
            MKLP_Tile.ObjectIDs[581] = true;
            MKLP_Tile.ObjectIDs[582] = true;
            MKLP_Tile.ObjectIDs[590] = true;
            MKLP_Tile.ObjectIDs[591] = true;
            MKLP_Tile.ObjectIDs[592] = true;
            MKLP_Tile.ObjectIDs[594] = true;
            MKLP_Tile.ObjectIDs[595] = true;
            MKLP_Tile.ObjectIDs[597] = true;
            MKLP_Tile.ObjectIDs[598] = true;
            MKLP_Tile.ObjectIDs[599] = true;
            MKLP_Tile.ObjectIDs[600] = true;
            MKLP_Tile.ObjectIDs[601] = true;
            MKLP_Tile.ObjectIDs[602] = true;
            MKLP_Tile.ObjectIDs[603] = true;
            MKLP_Tile.ObjectIDs[604] = true;
            MKLP_Tile.ObjectIDs[605] = true;
            MKLP_Tile.ObjectIDs[606] = true;
            MKLP_Tile.ObjectIDs[607] = true;
            MKLP_Tile.ObjectIDs[608] = true;
            MKLP_Tile.ObjectIDs[609] = true;
            MKLP_Tile.ObjectIDs[610] = true;
            MKLP_Tile.ObjectIDs[611] = true;
            MKLP_Tile.ObjectIDs[612] = true;
            MKLP_Tile.ObjectIDs[613] = true;
            MKLP_Tile.ObjectIDs[614] = true;
            MKLP_Tile.ObjectIDs[615] = true;
            MKLP_Tile.ObjectIDs[617] = true;
            MKLP_Tile.ObjectIDs[619] = true;
            MKLP_Tile.ObjectIDs[620] = true;
            MKLP_Tile.ObjectIDs[621] = true;
            MKLP_Tile.ObjectIDs[622] = true;
            MKLP_Tile.ObjectIDs[623] = true;
            MKLP_Tile.ObjectIDs[629] = true;
            MKLP_Tile.ObjectIDs[632] = true;
            MKLP_Tile.ObjectIDs[639] = true;
            MKLP_Tile.ObjectIDs[640] = true;
            MKLP_Tile.ObjectIDs[642] = true;
            MKLP_Tile.ObjectIDs[643] = true;
            MKLP_Tile.ObjectIDs[644] = true;
            MKLP_Tile.ObjectIDs[645] = true;
            MKLP_Tile.ObjectIDs[647] = true;
            MKLP_Tile.ObjectIDs[648] = true;
            MKLP_Tile.ObjectIDs[649] = true;
            MKLP_Tile.ObjectIDs[651] = true;
            MKLP_Tile.ObjectIDs[652] = true;
            MKLP_Tile.ObjectIDs[653] = true;
            MKLP_Tile.ObjectIDs[654] = true;
            MKLP_Tile.ObjectIDs[656] = true;
            MKLP_Tile.ObjectIDs[657] = true;
            MKLP_Tile.ObjectIDs[658] = true;
            MKLP_Tile.ObjectIDs[660] = true;
            MKLP_Tile.ObjectIDs[663] = true;
            MKLP_Tile.ObjectIDs[664] = true;
            MKLP_Tile.ObjectIDs[665] = true;

            #endregion
        }
        public static Dictionary<MKLP_Tile, string> GetIllegalTile(BossDType currentbossdefeated = BossDType.NA)
        {
            #region [ get Illegal Tile ]

            Dictionary<MKLP_Tile, string> getillegaltile = new();

            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            bool allowdungeonrush = (bool)MKLP.Config.Main.Progression.AllowDungeonRush;
            bool allowtemplerush = (bool)MKLP.Config.Main.Progression.AllowTempleRush;

            bool allowmusicbox = (bool)MKLP.Config.Main.Progression.AllowMusicBox;

            //current: 735

            #region | unobtainable |

            MKLP_Tile[] unobtainableids = { new(5, 0, true), new(10, 11), new(21, 52), new(21, 53), new(26, 0, true), new(28, 0, true),
                new(31, 0, true), new(32, 0, true), new(441, 52), new(441, 53), new(441, 2), new(441, 4), new(441, 5), new(441, 6),
                new(441, 23), new(441, 24), new(441, 25), new(441, 26), new(441, 27), new(441, 36), new(441, 38), new(441, 40),
                new(468, 13) };

            foreach (MKLP_Tile add in unobtainableids)
            {
                if (!getillegaltile.ContainsKey(add))
                {
                    getillegaltile.Add(add, "Illegal");
                }
            }

            /*
            MKLP_Tile[] cannotplace = { new(21, 3), new(21, 23), new(21, 24), new(21, 25), new(21, 26), new(21, 27), new(21, 36),
                new(21, 38), new(21, 40), new(80, 0, true), new(165, 0, true), new(185, 0, true), new(186, 0, true), new(187, 0, true),
                new(227, 6), new(227, 12), new(227, 13), new(227, 14), new(231, 0), new(233, 0, true), new(236, 0, true),
                new(238, 0, true), new(323, 0), new(485, 0), new(518, 0), new(519, 0), new(529, 0), new(530, 0), new(549, 0),
                new(624, 0), new(668, 0) };

            foreach (MKLP_Tile add in cannotplace)
            {
                if (!getillegaltile.ContainsKey(add))
                {
                    getillegaltile.Add(add, "Unplacable");
                }
            }
            */
            #endregion

            #region ( Banners )

            bool allowbanners = (bool)MKLP.Config.Main.Progression.AllowBanners;

            if (!allowbanners)
            {
                if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
                {
                    MKLP_Tile[] addin = { new(91, 76) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Evil Boss");
                    }
                }

                if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
                {
                    if (!Main.drunkWorld && !Main.remixWorld)
                    {
                        MKLP_Tile[] addin = { new(91, 89), new(91, 123), new(91, 128), new(91, 268) };
                        foreach (MKLP_Tile add in addin)
                        {
                            getillegaltile.Add(add, "Skeletron");
                        }
                    }
                }

                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    MKLP_Tile[] addin = { new(91, 22), new(91, 23), new(91, 26), new(91, 27), new(91, 30), new(91, 32),
                       new(91, 33), new(91, 36), new(91, 37), new(91, 38), new(91, 43), new(91, 44), new(91, 47),
                        new(91, 49), new(91, 52), new(91, 57), new(91, 58), new(91, 67), new(91, 69), new(91, 70),
                        new(91, 73), new(91, 78), new(91, 80), new(91, 83), new(91, 84), new(91, 95), new(91, 96),
                        new(91, 102), new(91, 103), new(91, 104), new(91, 107), new(91, 109), new(91, 110), new(91, 117),
                        new(91, 120), new(91, 121), new(91, 122), new(91, 135), new(91, 137), new(91, 144), new(91, 146),
                        new(91, 148), new(91, 149), new(91, 150), new(91, 174), new(91, 178), new(91, 181), new(91, 185),
                        new(91, 188), new(91, 207), new(91, 229), new(91, 232), new(91, 233), new(91, 234), new(91, 235),
                        new(91, 236), new(91, 258), new(91, 259), new(91, 260), new(91, 261), new(91, 265), new(91, 266),
                        new(91, 267), new(91, 269), new(91, 272), new(91, 273), new(91, 274), new(91, 275), new(91, 276),
                        new(91, 281), new(91, 282), new(91, 283), new(91, 284), new(91, 291), new(91, 292), new(91, 293),
                        new(91, 294), new(91, 297), new(91, 298), new(91, 305), new(91, 306), new(91, 307), new(91, 308),
                        new(91, 309) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                    (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime)) // mech
                {
                    MKLP_Tile[] addin = { new(91, 55), new(91, 77), new(91, 94), new(91, 99), new(91, 132), new(91, 155),
                        new(91, 211), new(91, 212), new(91, 216), new(91, 263) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Mechanical Boss");
                    }
                }

                if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                    (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                    (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)) // post mech
                {
                    MKLP_Tile[] addin = { new(91, 86) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Post Mechanical Boss");
                    }
                }

                if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
                {
                    MKLP_Tile[] addin = { new(91, 112), new(91, 116), new(91, 126), new(91, 129), new(91, 130), new(91, 131),
                        new(91, 133), new(91, 136), new(91, 138), new(91, 141), new(91, 142), new(91, 143), new(91, 153),
                        new(91, 157), new(91, 158), new(91, 159), new(91, 160), new(91, 161), new(91, 162), new(91, 163),
                        new(91, 164), new(91, 165), new(91, 168), new(91, 169), new(91, 170), new(91, 173), new(91, 175),
                        new(91, 177), new(91, 182), new(91, 183), new(91, 186), new(91, 187), new(91, 194), new(91, 196),
                        new(91, 205), new(91, 206), new(91, 0), new(91, 213), new(91, 214), new(91, 217), new(91, 218),
                        new(91, 219), new(91, 220), new(91, 287) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Plantera");
                    }
                    if (!allowtemplerush)
                    {
                        MKLP_Tile[] templeids = { new(91, 54), new(91, 74) };
                        foreach (MKLP_Tile add in templeids)
                        {
                            getillegaltile.Add(add, "Plantera");
                        }
                    }
                }

                if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
                {
                    MKLP_Tile[] addin = { new(91, 113), new(91, 114), new(91, 184), new(91, 262) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Golem");
                    }
                }

                if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
                {
                    MKLP_Tile[] addin = { new(91, 237), new(91, 238), new(91, 239), new(91, 240), new(91, 241), new(91, 242),
                        new(91, 243), new(91, 244), new(91, 245), new(91, 246), new(91, 247), new(91, 248), new(91, 249),
                        new(91, 250), new(91, 251), new(91, 252), new(91, 253), new(91, 254), new(91, 255), new(91, 256),
                        new(91, 257) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Lunatic Cultist");
                    }
                }

                getillegaltile.Add(new(91, 115), "Unobtainable");
                getillegaltile.Add(new(91, 201), "Unobtainable");
                getillegaltile.Add(new(91, 202), "Unobtainable");
                getillegaltile.Add(new(91, 203), "Unobtainable");
                getillegaltile.Add(new(91, 215), "Unobtainable");
                getillegaltile.Add(new(91, 221), "Unobtainable");
            }

            #endregion

            #region ( Music Box )

            if (!allowmusicbox)
            {
                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    getillegaltile.Add(new(139, 0, true), "HardMode | Wall of Flesh");
                }

                if (!NPC.downedMechBossAny && (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
                {
                    MKLP_Tile[] addin = { new(139, 26) };


                    foreach (MKLP_Tile add in addin)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Mechanical Bosses");
                        }
                    }
                }

                if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                    (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                    (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime))
                {
                    MKLP_Tile[] addin = { new(139, 23), new(139, 81) };

                    foreach (MKLP_Tile add in addin)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Post Mechanical Bosses");
                        }
                    }
                }

                if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
                {
                    MKLP_Tile[] addin = { new(139, 16), new(139, 28), new(139, 30), new(139, 53) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Plantera");
                    }
                }

                if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
                {
                    MKLP_Tile[] addin = { new(139, 33), new(139, 46) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Golem");
                    }
                }

                if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
                {
                    MKLP_Tile[] addin = { new(139, 36), new(139, 79), new(139, 32), new(139, 80), new(139, 85) };
                    foreach (MKLP_Tile add in addin)
                    {
                        getillegaltile.Add(add, "Lunatic Cultist");
                    }
                }
            }

            #endregion

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing && currentbossdefeated != BossDType.KingSlime) //king slime
            {
                MKLP_Tile[] addin = { new(10, 31), new(11, 32), new(14, 29), new(15, 31), new(18, 8), new(19, 20), new(34, 26),
                    new(42, 30), new(79, 25), new(87, 24), new(88, 19), new(89, 25), new(90, 20), new(93, 21), new(100, 21),
                    new(101, 26), new(104, 7), new(172, 26), new(193, 0), new(197, 0), new(220, 0), new(240, 54), new(371, 0),
                    new(380, 0), new(441, 34), new(497, 24) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "King Slime");
                    }
                }
            }
            #endregion

            #region [ Eye of Cthulhu ]
            if (!NPC.downedBoss1 && currentbossdefeated != BossDType.EyeOfCthulhu)
            {
                MKLP_Tile[] addin = { new(240, 0), new(380, 0, true) , new(695, 0, true) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Eye of Cthulhu");
                    }
                }

            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
            {
                MKLP_Tile[] addin = { new(10, 33), new(11, 33), new(14, 32), new(15, 33), new(18, 28), new(19, 27), new(21, 49),
                    new(33, 27), new(34, 34), new(37, 0), new(42, 34), new(79, 28), new(87, 27), new(88, 25), new(89, 30),
                    new(90, 28), new(93, 28), new(100, 28), new(101, 29), new(104, 25), new(172, 29), new(239, 9), new(240, 1),
                    new(240, 2), new(240, 60), new(240, 61), new(240, 62), new(370, 0), new(380, 2), new(380, 3), new(441, 49),
                    new(463, 0), new(464, 0), new(465, 0), new(466, 0), new(497, 32), new(696, 0, true) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    MKLP_Tile[] hellstoneid = { new(58, 0), new(77, 0), new(239, 10), new(491, 0), new(648, 6), new(648, 7),
                        new(648, 8) };

                    foreach (MKLP_Tile add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Evil Boss");
                        }
                    }
                }
            }
            #endregion

            #region ( Evil Boss Enabled )

            if ((bool)MKLP.Config.BossManager.AllowEaterOfWorlds || (bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
            { //     true if its eow/boc enabled    |    false if its not enabled
                MKLP_Tile[] hellstoneid = { new(58, 0), new(77, 0), new(239, 10), new(491, 0), new(696, 0, true) };

                foreach (MKLP_Tile remove in hellstoneid) // hellstone are breakable from bombs on hm
                {
                    if (!getillegaltile.ContainsKey(remove))
                    {
                        getillegaltile.Remove(remove);
                    }
                }
            }
            else
            {

            }
            //86
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
            {
                MKLP_Tile[] addin = { new(10, 8), new(11, 8), new(14, 4), new(15, 7), new(18, 4), new(21, 41), new(34, 21),
                    new(42, 25), new(79, 20), new(87, 16), new(88, 10), new(89, 11), new(89, 43), new(93, 16), new(97, 0),
                    new(100, 16), new(101, 21), new(104, 8), new(135, 1), new(135, 4), new(135, 5), new(144, 3), new(144, 4),
                    new(172, 4), new(194, 0), new(235, 0), new(240, 3), new(300, 0), new(314, 2), new(380, 5), new(380, 6),
                    new(424, 0), new(425, 0), new(429, 0), new(441, 41), new(442, 0), new(445, 0), new(491, 0), new(497, 5) };

                MKLP_Tile[] dungeonrushids = { new(21, 3), new(21, 4) };



                foreach (MKLP_Tile add in dungeonrushids)
                {
                    if (!allowdungeonrush)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Skeletron");
                        }
                    }

                }

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Skeletron");
                    }
                }

                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    MKLP_Tile[] skeleids = { new(19, 4), new(647, 0), new(647, 1), new(647, 2), new(647, 3), new(647, 4),
                        new(647, 5), new(647, 6), new(648, 52), new(648, 53), new(648, 54), new(649, 6), new(649, 7),
                        new(649, 8), new(649, 9), new(649, 10), new(649, 11), new(649, 12), new(649, 13), new(649, 14),
                        new(649, 15), new(650, 12), new(650, 13), new(650, 14), new(650, 15), new(650, 16), new(650, 17),
                        new(650, 18), new(650, 19), new(650, 20), new(650, 21), new(650, 22), new(650, 23), new(650, 24),
                        new(650, 25), new(650, 26), new(650, 27) };

                    foreach (MKLP_Tile add in skeleids)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Bee ]

            if (!NPC.downedQueenBee && currentbossdefeated != BossDType.QueenBee)
            {
                MKLP_Tile[] addin = { new(96, 1), new(207, 0, true), new(225, 0), new(240, 4), new(380, 1) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Queen Bee");
                    }
                }
            }

            #endregion

            #region [ Deerclops ]

            if (!NPC.downedDeerclops && currentbossdefeated != BossDType.Deerclops)
            {
                MKLP_Tile[] addin = { new(240, 75), new(733, 75) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Deerclops");
                    }
                }
            }

            #endregion


            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
            {

                MKLP_Tile[] addin = { new(4, 8), new(4, 11), new(4, 14), new(5, 3), new(10, 21), new(10, 36), new(10, 37),
                    new(10, 38),new(11, 21), new(11, 36), new(11, 37), new(11, 38), new(14, 18), new(14, 35), new(15, 4),
                    new(15, 19), new(15, 20), new(15, 36), new(15, 37), new(15, 38), new(18, 31), new(18, 32), new(18, 33),
                    new(19, 30), new(19, 31), new(19, 34), new(19, 36), new(19, 37), new(33, 16), new(33, 30), new(33, 31),
                    new(34, 20), new(34, 37), new(34, 38), new(42, 24), new(42, 37), new(42, 38), new(79, 10), new(79, 31),
                    new(79, 32), new(87, 14), new(87, 30), new(87, 32), new(88, 8), new(88, 28), new(88, 32), new(89, 9),
                    new(89, 33), new(89, 35), new(90, 26), new(90, 31), new(90, 32), new(93, 15), new(93, 31), new(93, 32),
                    new(100, 31), new(100, 32), new(101, 20), new(101, 32), new(101, 33), new(104, 2), new(104, 28), new(104, 33),
                    new(107, 0), new(108, 0), new(111, 0), new(116, 0), new(117, 0), new(118, 0), new(121, 0), new(122, 0),
                    new(125, 0), new(127, 0), new(129, 0, true), new(133, 0, true), new(134, 0, true), new(137, 5), new(150, 0),
                    new(164, 0), new(172, 3), new(172, 16), new(172, 32), new(172, 33), new(195, 0), new(215, 1),
                    new(215, 4), new(215, 5), new(218, 0), new(221, 0), new(222, 0), new(223, 0), new(227, 8), new(227, 9),
                    new(227, 10), new(227, 11), new(239, 11), new(239, 12), new(239, 13), new(239, 14), new(239, 15), new(239, 16),
                    new(240, 5), new(240, 58), new(240, 89), new(242, 52), new(242, 55), new(246, 15), new(246, 23), new(246, 24),
                    new(246, 25), new(248, 0), new(249, 0), new(250, 0), new(326, 0), new(327, 0), new(328, 0), new(329, 0),
                    new(336, 0), new(340, 0), new(341, 0), new(342, 0), new(343, 0), new(344, 0), new(345, 0), new(351, 0),
                    new(356, 0), new(367, 9), new(367, 10), new(367, 11), new(367, 12),
                    new(367, 13), new(367, 14), new(367, 15), new(367, 16), new(367, 17), new(367, 19), new(367, 21), new(367, 23),
                    new(367, 25), new(379, 0), new(380, 7), new(385, 0), new(402, 0), new(403, 0), new(423, 0, true), new(458, 0),
                    new(459, 0), new(461, 0), new(462, 0), new(474, 0), new(480, 0), new(496, 0), new(497, 28), new(497, 29),
                    new(498, 0), new(507, 0), new(508, 0), new(663, 0) , new(708, 0), new(709, 0), new(711, 0), new(726, 0) };

                // new(373, 0), new(374, 0), new(375, 0) [ water drop bugged ]

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    MKLP_Tile[] hminprehm = { new(4, 20), new(10, 3), new(14, 3), new(18, 3), new(19, 3), new(20, 5), new(20, 7),
                        new(21, 9), new(33, 10), new(34, 13), new(42, 17), new(79, 3), new(87, 3), new(88, 3), new(89, 4),
                        new(90, 7), new(93, 7), new(100, 8), new(101, 13), new(104, 19), new(109, 0), new(159, 0), new(172, 23),
                        new(242, 62), new(441, 9), new(492, 0), new(497, 2), new(597, 2) };

                    foreach (MKLP_Tile add in hminprehm)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {
                        MKLP_Tile[] hminprehm2 = { new(160, 0) };

                        foreach (MKLP_Tile add in hminprehm2)
                        {
                            if (!getillegaltile.ContainsKey(add))
                            {
                                getillegaltile.Add(add, "HardMode | Wall of Flesh");
                            }
                        }
                    }
                }
            }
            #endregion

            #region [ Queen Slime ]

            if (!NPC.downedQueenSlime && currentbossdefeated != BossDType.QueenSlime)
            {
                MKLP_Tile[] addin = { new(240, 73) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Queen Slime");
                    }
                }
            }

            #endregion

            #region ( Mechanical Bosses )

            #region [ Any Mechanical Bosses ]
            if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
            {
                MKLP_Tile[] addin = { new(10, 5), new(10, 23), new(11, 5), new(11, 23), new(14, 5), new(14, 20), new(15, 8),
                    new(15, 23), new(18, 6), new(18, 21), new(18, 34), new(19, 21), new(21, 43), new(33, 6), new(33, 23),
                    new(33, 32), new(34, 30), new(34, 39), new(42, 12), new(42, 14), new(42, 39), new(79, 12), new(79, 14),
                    new(87, 6), new(87, 10), new(79, 33), new(87, 6), new(87, 33), new(88, 23), new(88, 33), new(89, 16),
                    new(89, 26), new(89, 36), new(90, 12), new(90, 33), new(93, 3), new(93, 12), new(93, 33), new(100, 3),
                    new(100, 5), new(100, 33), new(101, 8), new(101, 34), new(104, 5), new(104, 15), new(104, 34), new(172, 5),
                    new(172, 18), new(172, 34), new(198, 0), new(217, 0), new(239, 18), new(240, 62), new(240, 81), new(272, 0),
                    new(301, 0), new(307, 0), new(419, 0, true), new(420, 0, true), new(421, 0), new(422, 0), new(441, 43),
                    new(467, 3), new(468, 3), new(469, 2), new(497, 6), new(497, 18), new(497, 30), new(499, 0), new(642, 0),
                    new(702, 0), new(735, 0) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Mechanical Bosses");
                    }
                }
            }
            #endregion

            #region [ Post Mechanical Bosses ]
            if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime))
            {
                MKLP_Tile[] addin = { new(211, 0), new(239, 17), new(346, 0) };


                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Post Mechanical Bosses");
                    }
                }
            }
            #endregion

            #region [ The Destroyer ]

            if (!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer)
            {
                MKLP_Tile[] addin = { new(240, 6) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "The Destroyer");
                    }
                }
            }

            #endregion

            #region [ The Twins ]

            if (!(bool)MKLP.Config.BossManager.AllowTheTwins && !NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins)
            {
                MKLP_Tile[] addin = { new(240, 8), new(240, 9), new(413, 2) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "The Twins");
                    }
                }
            }

            if (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins)
            {
                MKLP_Tile[] addin = { };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "The Twins");
                    }
                }
            }

            #endregion

            #region [ Skeletron Prime ]

            if (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime)
            {
                MKLP_Tile[] addin = { new(240, 7) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Skeletron Prime");
                    }
                }
            }

            #endregion

            //mechs
            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
            {
                MKLP_Tile[] addin = { new(10, 25), new(11, 25), new(14, 22), new(15, 25), new(18, 17), new(19, 16), new(19, 48), new(21, 18),
                    new(21, 19), new(21, 20), new(21, 21), new(21, 22), new(21, 23), new(21, 24), new(21, 25), new(21, 26), new(21, 27),
                    new(21, 46), new(33, 24), new(34, 16), new(42, 21), new(79, 18), new(87, 18), new(88, 12), new(89, 13), new(90, 10),
                    new(93, 10), new(100, 12), new(101, 14), new(104, 22), new(172, 20), new(210, 0), new(239, 20), new(239, 21),
                    new(240, 36), new(240, 37), new(240, 38), new(240, 39), new(240, 40), new(240, 85), new(242, 53), new(247, 0),
                    new(253, 0), new(348, 0), new(441, 11), new(441, 18), new(441, 19), new(441, 20), new(441, 21), new(441, 46),
                    new(467, 12), new(467, 13), new(468, 12), new(497, 20), new(497, 39), new(541, 0), new(620, 0), new(657, 0),
                    new(720, 0), new(721, 0) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Plantera");
                    }
                }

                if (!allowtemplerush)
                {
                    MKLP_Tile[] templeids = { new(14, 9), new(15, 12), new(18, 10), new(21, 16), new(105, 43), new(105, 44), new(105, 45),
                        new(135, 6), new(240, 88), new(303, 0), new(441, 16) };

                    foreach (MKLP_Tile add in templeids)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Plantera (temple rush)");
                        }
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    MKLP_Tile[] princessids = { new(240, 74), new(240, 80), new(240, 86), new(242, 45), new(242, 64),
                        new(245, 13), new(246, 29), new(623, 0, true) };

                    foreach (MKLP_Tile add in princessids)
                    {
                        if (!getillegaltile.ContainsKey(add))
                        {
                            getillegaltile.Add(add, "Plantera");
                        }
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
            {
                MKLP_Tile[] addin = { new(10, 12), new(10, 32), new(11, 12), new(11, 32), new(14, 31), new(15, 32), new(18, 27),
                    new(19, 26), new(19, 33), new(21, 48), new(33, 11), new(33, 26), new(34, 14), new(34, 33), new(42, 19),
                    new(42, 33), new(79, 16), new(79, 27), new(87, 20), new(87, 26), new(88, 15), new(88, 24), new(89, 20),
                    new(89, 29), new(90, 8), new(90, 27), new(93, 8), new(93, 27), new(100, 10), new(100, 27), new(101, 16),
                    new(101, 28), new(104, 12), new(104, 24), new(137, 1), new(137, 2), new(137, 3), new(137, 4), new(172, 9),
                    new(172, 28), new(226, 0), new(238, 0, true), new(240, 57), new(240, 61), new(350, 0), new(441, 48),
                    new(497, 10), new(497, 25), new(648, 18), new(648, 19), new(648, 20) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Golem");
                    }
                }
            }

            #endregion

            #region [ Duke Fishron ]

            if (!NPC.downedFishron && currentbossdefeated != BossDType.DukeFishron)
            {
                MKLP_Tile[] addin = { new(240, 55) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Duke Fishron");
                    }
                }
            }

            #endregion

            #region [ Empress of Light ]

            if (!NPC.downedEmpressOfLight && currentbossdefeated != BossDType.EmpressOfLight)
            {
                MKLP_Tile[] addin = { new(240, 72) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Empress of Light");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
            {
                MKLP_Tile[] addin = { new(10, 39), new(10, 40), new(10, 41), new(10, 42), new(11, 39), new(11, 40), new(11, 41),
                    new(11, 42), new(15, 39), new(15, 40), new(15, 41), new(15, 42), new(18, 35), new(18, 36), new(18, 37),
                    new(18, 38), new(19, 38), new(19, 39), new(19, 40), new(19, 41), new(33, 33), new(33, 34), new(33, 35),
                    new(33, 36), new(34, 40), new(34, 41), new(34, 42), new(34, 43), new(42, 40), new(42, 41), new(42, 42),
                    new(42, 43), new(79, 34), new(79, 35), new(79, 36), new(79, 37), new(87, 34), new(87, 35), new(87, 36),
                    new(87, 37), new(88, 34), new(88, 35), new(88, 36), new(88, 37), new(89, 37), new(89, 38), new(89, 39),
                    new(89, 40), new(90, 34), new(90, 35), new(90, 36), new(90, 37), new(93, 34), new(93, 35), new(93, 36),
                    new(93, 37), new(100, 34), new(100, 35), new(100, 36), new(100, 37), new(101, 35), new(101, 36),
                    new(101, 37), new(101, 38), new(104, 35), new(104, 36), new(104, 37), new(104, 38), new(172, 35),
                    new(172, 36), new(172, 37), new(172, 38), new(240, 56), new(410, 0, true), new(412, 0), new(415, 0),
                    new(416, 0), new(417, 0), new(418, 0), new(467, 5), new(467, 6), new(467, 7), new(467, 8), new(468, 5),
                    new(468, 6), new(468, 7), new(468, 8), new(469, 3), new(469, 4), new(469, 5), new(469, 6), new(497, 33),
                    new(497, 34), new(497, 35), new(497, 36), new(500, 0), new(501, 0), new(502, 0), new(503, 0) };

                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Lunatic Cultist");
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord && currentbossdefeated != BossDType.MoonLord)
            {
                MKLP_Tile[] addin = { new(209, 3), new(209, 4), new(239, 22), new(240, 59), new(242, 36), new(242, 59),
                    new(408, 0), new(409, 0), new(509, 0), new(597, 8), new(669, 0), new(670, 0), new(671, 0), new(672, 0),
                    new(673, 0), new(674, 0), new(675, 0), new(676, 0) };


                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Moon Lord");
                    }
                }
            }

            #endregion

            #endregion

            #region ( Invasion )

            #region [ Goblin Army ]
            if (!NPC.downedGoblins) // Goblin Army
            {
                /*
                int[] addin = { };

                foreach (int add in addin)
                {
                    if (!getillegalprojectile.ContainsKey(add))
                    {
                        getillegalprojectile.Add(add, "Goblin Army");
                    }
                }
                */
            }
            #endregion

            #region [ Pirate Invasion ]
            if (!NPC.downedPirates) // Pirate Invasion
            {
                MKLP_Tile[] addin = { new(209, 0, true) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Frost Legion ]
            if (!NPC.downedFrost)
            {
                MKLP_Tile[] addin = { new(242, 31), new(242, 32), new(242, 33), new(242, 34), new(242, 35) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #region [ Martians ]
            if (!NPC.downedMartians)
            {
                MKLP_Tile[] addin = { new(242, 27), new(242, 28), new(242, 29) };
                foreach (MKLP_Tile add in addin)
                {
                    if (!getillegaltile.ContainsKey(add))
                    {
                        getillegaltile.Add(add, "Pirate Invasion");
                    }
                }
            }
            #endregion

            #endregion

            #endregion

            INIT_ObjectIDs();

            return getillegaltile;
        }

        public static Dictionary<ushort, string> GetIllegalWall(BossDType currentbossdefeated = BossDType.NA)
        {
            #region [ get Illegal Walls ]
            Dictionary<ushort, string> getillegalwalls = new();

            //bool bothevilworld = (Main.drunkWorld || Main.remixWorld || Main.zenithWorld);

            #region | unobtainable |

            ushort[] unobtainableids = { 2, 3, 13, 14, 15, 28, 61, 63, 64, 65, 69, 70, 71, 79, 80, 81, 83,
                86, 170, 171, 178, 180, 185, 244 };

            foreach (ushort add in unobtainableids)
            {
                getillegalwalls.Add(add, "Unobtainable");
            }

            for (ushort i = 48; i <= 59; i++)
            {
                if (!getillegalwalls.ContainsKey(i))
                {
                    getillegalwalls.Add(i, "Unobtainable");
                }
            }

            for (ushort i = 188; i <= 215; i++)
            {
                if (!getillegalwalls.ContainsKey(i))
                {
                    getillegalwalls.Add(i, "Unobtainable");
                }
            }

            for (ushort i = 217; i <= 222; i++)
            {
                if (!getillegalwalls.ContainsKey(i))
                {
                    getillegalwalls.Add(i, "Unobtainable");
                }
            }

            #endregion

            //2223 

            //100

            #region ( Boss )

            #region [ king Slime ]
            if (!NPC.downedSlimeKing && currentbossdefeated != BossDType.KingSlime) //king slime
            {
                ushort[] addin = { 76 };
                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "King Slime");
                    }
                }
            }
            #endregion

            #region [ Evil Boss ]
            if (!NPC.downedBoss2 && currentbossdefeated != BossDType.EvilBoss)
            {
                ushort[] addin = { 182 };
                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Evil Boss");
                    }
                }

                if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
                {
                    ushort[] hellstoneid = { 116, 117, 119, 120, 121, 122, 174, 175,
                        217, 219, 221, 231, 232, 233, 273, 2365, 4533, 4534, 4535, 4536, 4821 };

                    foreach (ushort add in hellstoneid) // hellstone are breakable from bombs on hm
                    {
                        if (!getillegalwalls.ContainsKey(add))
                        {
                            getillegalwalls.Add(add, "Evil Boss");
                        }
                    }
                }
            }
            #endregion

            #region [ Skeletron ]
            if (!NPC.downedBoss3 && currentbossdefeated != BossDType.Skeletron)
            {

                int[] addin = { 346, 509, 510, 513, 541, 849, 850, 851, 852, 853, 1263, 1273,
                    1313, 1363, 2295, 2296, 2739, 2799, 3085, 3205, 3220, 3221, 3245, 3282,
                    3323, 3619, 3620, 3625, 3629, 3707, 3725, 4264, 4484, 4485, 4703, 4801,
                    4818, 4927, 4993 };

                int[] dungeonrushids = { 112, 113, 151, 152, 153, 155, 156, 157, 163, 164,
                    218, 220, 273, 274, 328, 329, 1613, 3019, 3317, 5010, 5126 };

                int[] vanityids = { 254, 260, 269, 270, 271, 322, 325, 326, 978, 979, 980,
                    981, 1275, 1276, 1281, 1288, 1289, 1429, 1740, 3242, 3243, 3244, 3246,
                    3247, 3362, 3363, 3627, 3730, 3731, 3733, 3734, 3735, 4128, 4129,
                    4130, 4132, 4133, 4134, 4685, 4686, 4704, 4705, 4706, 4707, 4708,
                    4709 };


                if (!Main.drunkWorld && !Main.remixWorld)
                {
                    ushort[] skeleids = { 75 };
                    foreach (ushort add in skeleids)
                    {
                        if (!getillegalwalls.ContainsKey(add))
                        {
                            getillegalwalls.Add(add, "Skeletron");
                        }
                    }
                }
            }
            #endregion

            #region [ HardMode | Wall of Flesh ]
            if (!Main.hardMode && currentbossdefeated != BossDType.WallOfFlesh)
            {
                ushort[] addin = { 22, 25, 26, 32, 109, 110, 111, 136, 137, 168, 169, 172,
                    186, 226, 227, 236, 242, 243, 248, 265, 288, 289, 290, 291, 310, 339,
                    340 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "HardMode | Wall of Flesh");
                    }
                }

                if (!Main.tenthAnniversaryWorld)
                {
                    ushort[] hminprehm = { 43, 140 };

                    foreach (ushort add in hminprehm)
                    {
                        if (!getillegalwalls.ContainsKey(add))
                        {
                            getillegalwalls.Add(add, "HardMode | Wall of Flesh");
                        }
                    }
                    if (!Main.zenithWorld)
                    {
                        if (!getillegalwalls.ContainsKey(44))
                        {
                            getillegalwalls.Add(44, "HardMode | Wall of Flesh");
                        }
                    }
                }
            }
            #endregion

            #region ( Mechanical Bosses )

            if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3) &&
                (currentbossdefeated != BossDType.TheDestroyer && currentbossdefeated != BossDType.TheTwins && currentbossdefeated != BossDType.SkeletronPrime))
            {
                ushort[] addin = { 77, 225, 233 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Mechanical Bosses");
                    }
                }
            }


            if ((!NPC.downedMechBoss1 && currentbossdefeated != BossDType.TheDestroyer) ||
                (!NPC.downedMechBoss2 && currentbossdefeated != BossDType.TheTwins) ||
                (!NPC.downedMechBoss3 && currentbossdefeated != BossDType.SkeletronPrime))
            {
                ushort[] addin = { 173 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Post Mechanical Bosses");
                    }
                }
            }

            #endregion

            #region [ Plantera ]

            if (!NPC.downedPlantBoss && currentbossdefeated != BossDType.Plantera)
            {
                ushort[] addin = { 115, 175, 318 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Plantera");
                    }
                }
            }

            #endregion

            #region [ Golem ]

            if (!NPC.downedGolemBoss && currentbossdefeated != BossDType.Golem)
            {
                ushort[] addin = { 112, 176 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Golem");
                    }
                }
            }

            #endregion

            #region [ Lunatic Cultist ]

            if (!NPC.downedAncientCultist && currentbossdefeated != BossDType.LunaticCultist)
            {
                ushort[] addin = { 237, 238, 239, 240 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Lunatic Cultist");
                    }
                }
            }

            #endregion

            #region [ Moon Lord ]

            if (!NPC.downedMoonlord && currentbossdefeated != BossDType.MoonLord)
            {
                ushort[] addin = { 224, 323, 324, 325, 326, 327, 328, 329, 330 };

                foreach (ushort add in addin)
                {
                    if (!getillegalwalls.ContainsKey(add))
                    {
                        getillegalwalls.Add(add, "Moon Lord");
                    }
                }
            }

            #endregion

            #endregion


            #endregion

            foreach (ushort whitelist in MKLP.Config.Main.DisableNode.WhiteList_Survival_Code4)
            {
                if (getillegalwalls.ContainsKey(whitelist))
                {
                    getillegalwalls.Remove(whitelist);
                }
            }

            return getillegalwalls;
        }

        public static bool ItemSuspicion(Item item)
        {
            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            if ((item.value * item.stack) / 5000000 >= maxvalue && item.type != 74) return true;

            if (MKLP.IllegalItemProgression.ContainsKey(item.type)) return true;

            return false;
        }

        public static void BossManager()
        {

        }
    }


}
