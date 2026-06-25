using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using TerrariaApi.Server;

namespace MKLP.Modules
{
    public static class BossEventManager
    {

        #region Hooks

        public static async void Hooks_OnNPCSpawn(NpcSpawnEventArgs args)
        {
            #region code

            if (!(bool)MKLP.Config.BossManager.UsingBossManager) return;

            int[] BossIDs =
            {
                50, // King Slime
			    4, // Eye of Cthulu			
			    222, // Queen Bee
			    13, // Eater of Worlds	
			    266, // Brain of Cthulu
			    35, // Skeletron
			    668, // Deerclops
			    113, // Wall of Flesh
			    657, // Queen Slime
			    125, // Retinazer
			    126, // Spazmatism
			    127, // Skeletron Prime	
			    134, // The Destroyer
			    262, // Plantera
			    245, // Golem
			    636, // Empress Of Light
			    370, // Duke Fishron
			    439, // Lunatic Cultist
			    396, // Moon Lord
            };

            if (!BossIDs.Contains(Main.npc[args.NpcId].type)) return;

            int[] MechdusaIDs = { NPCID.Retinazer, NPCID.Spazmatism, NPCID.Probe, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice };

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                int requireplayers;

                switch (BossEventManager.BossIsAllowed(npc.type, out requireplayers))
                {
                    case BossEventManager.BossAllowType.Allowed:
                        break;
                    case BossEventManager.BossAllowType.NotAllowed:
                        {
                            TShock.Utils.Broadcast(MKLP.GetText($"{npc.FullName} isn't allowed yet!"), Color.MediumPurple);
                            DespawnNPC();
                            break;
                        }
                    case BossEventManager.BossAllowType.LackOfPlayers:
                        {
                            TShock.Utils.Broadcast(MKLP.GetText($"{npc.FullName} required {requireplayers} Players to be summoned"), Color.MediumPurple);
                            DespawnNPC();
                            break;
                        }
                    case BossEventManager.BossAllowType.Illegal:
                        {
                            DespawnNPC();
                            break;
                        }

                    case BossEventManager.BossAllowType.MechDusaNotAllowed1:
                        {
                            await Task.Run(async () => {
                                if (!NPC_Is_Active(MechdusaIDs))
                                {
                                    await Task.Delay(800);
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                }
                                else
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                    TShock.Utils.Broadcast(MKLP.GetText("Mechdusa isn't allowed yet!"), Color.MediumPurple);
                                }
                            });
                            break;
                        }
                    case BossEventManager.BossAllowType.MechDusaNotAllowed2:
                        {
                            await Task.Run(async () => {
                                if (!NPC_Is_Active(MechdusaIDs))
                                {
                                    await Task.Delay(800);
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                }
                                else
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                    TShock.Utils.Broadcast(MKLP.GetText($"Mechdusa required {requireplayers} Players to be summoned"), Color.MediumPurple);
                                }
                            });
                            break;
                        }
                }

                void DespawnNPC()
                {
                    args.Handled = true;
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }



            bool DespawnNPCs(int[] npcIDs)
            {
                int NPCDel = 0;

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    if (Main.npc[i] == null) continue;
                    if (!Main.npc[i].active) continue;
                    if (npcIDs.Contains(Main.npc[i].type))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                        NPCDel++;
                    }
                }

                return NPCDel > 0;
            }

            bool NPC_Is_Active(int[] npcIDs)
            {
                List<int> result = new();
                foreach (int get in npcIDs)
                {
                    result.Add(get);
                }

                foreach (var gnpc in Main.npc)
                {
                    if (gnpc == null) continue;
                    if (!gnpc.active) continue;
                    if (result.Contains(gnpc.type))
                    {
                        result.Remove(gnpc.type);
                    }
                }

                return result.Count > 0;
            }
            #endregion
        }
        public static void Hooks_OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            if (!(bool)MKLP.Config.BossManager.UsingBossManager) return;

            TSPlayer plr = args.Player;
            if (args.Control.IsUsingItem && IsSpawnerItem(plr.SelectedItem))
            {
                int requireplayer = -1;
                switch (BossIsAllowed(GetBossNetIDFromSpawner(plr.SelectedItem.type), out requireplayer))
                {
                    case BossAllowType.NotAllowed:
                    case BossAllowType.MechDusaNotAllowed1:
                    case BossAllowType.Illegal:
                        {
                            plr.SendErrorMessage("This boss isn't allowed yet!");
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(plr.SelectedItem.Name), plr.Index, plr.TPlayer.selectedItem);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, plr.Index, -1, NetworkText.FromLiteral(plr.SelectedItem.Name), plr.Index, plr.TPlayer.selectedItem);
                            break;
                        }
                    case BossAllowType.LackOfPlayers:
                    case BossAllowType.MechDusaNotAllowed2:
                        {
                            plr.SendErrorMessage("you have lack of players to summon this boss!");
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(plr.SelectedItem.Name), plr.Index, plr.TPlayer.selectedItem);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, plr.Index, -1, NetworkText.FromLiteral(plr.SelectedItem.Name), plr.Index, plr.TPlayer.selectedItem);
                            break;
                        }
                }
            }
        }

        public static void Hooks_OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            if (!(bool)MKLP.Config.BossManager.UsingBossManager) return;

            string text = args.Message.ToString();

            if (text.EndsWith(" has awoken!"))
            {
                args.Message._mode = NetworkText.Mode.LocalizationKey;
                text = args.Message.ToString();

                string bossName = text[..text.IndexOf(" has awoken!")];

                foreach (NPC npc in Main.npc)
                {
                    if (npc.FullName.StartsWith(bossName) && npc.type == 0 && !npc.active)
                    {
                        args.Handled = true;
                    }
                }
            }
        }

        #endregion

        #region Var Func


        public static bool IsSpawnerItem(Item item)
        {
            int[] spawnerIds = {
                ItemID.SlimeCrown,
                ItemID.SuspiciousLookingEye,
                ItemID.WormFood,
                ItemID.BloodySpine,
                ItemID.Abeemination,
                ItemID.DeerThing,
                ItemID.QueenSlimeCrystal,
                ItemID.MechanicalWorm,
                ItemID.MechanicalEye,
                ItemID.MechanicalSkull,
                ItemID.MechdusaSummon,
                ItemID.LihzahrdPowerCell,
                ItemID.TruffleWorm,
                ItemID.CelestialSigil
            };

            return spawnerIds.Contains(item.type);
        }
        public static int GetBossNetIDFromSpawner(int itemID)
        {
            return itemID switch
            {
                ItemID.SlimeCrown => NPCID.KingSlime,
                ItemID.SuspiciousLookingEye => NPCID.EyeofCthulhu,
                ItemID.WormFood => NPCID.EaterofWorldsHead,
                ItemID.BloodySpine => NPCID.BrainofCthulhu,
                ItemID.Abeemination => NPCID.QueenBee,
                ItemID.DeerThing => NPCID.Deerclops,
                ItemID.QueenSlimeCrystal => NPCID.QueenSlimeBoss,
                ItemID.MechanicalWorm => NPCID.TheDestroyer,
                ItemID.MechanicalEye => NPCID.Retinazer,
                ItemID.MechanicalSkull => NPCID.SkeletronPrime,
                ItemID.MechdusaSummon => NPCID.SkeletronPrime,
                ItemID.LihzahrdPowerCell => NPCID.Golem,
                ItemID.TruffleWorm => NPCID.DukeFishron,
                ItemID.CelestialSigil => NPCID.MoonLordCore,
                _ => 0,
            };
        }
        #endregion

        public enum BossAllowType
        {
            Allowed,

            NotAllowed,
            LackOfPlayers,

            Illegal,

            MechDusaNotAllowed1,
            MechDusaNotAllowed2,

        }

        public static BossAllowType BossIsAllowed(int npcID, out int PlayerCountRequire)
        {
            #region code
            PlayerCountRequire = -1;

            if ((bool)MKLP.Config.BossManager.PreventIllegalBoss)
            {
                if (!Main.hardMode && (
                    npcID == NPCID.QueenSlimeBoss ||
                    npcID == NPCID.TheDestroyer ||
                    npcID == NPCID.Retinazer ||
                    npcID == NPCID.Spazmatism ||
                    npcID == NPCID.SkeletronPrime ||
                    npcID == NPCID.DukeFishron))
                {
                    return BossAllowType.Illegal;
                }

                if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && (npcID == NPCID.Plantera))
                {
                    return BossAllowType.Illegal;
                }
                if (!NPC.downedPlantBoss && (npcID == NPCID.HallowBoss || npcID == NPCID.EmpressButterfly || npcID == NPCID.Golem))
                {
                    return BossAllowType.Illegal;
                }
                if (!NPC.downedGolemBoss && (npcID == NPCID.CultistBoss || npcID == NPCID.MoonLordCore))
                {
                    return BossAllowType.Illegal;
                }
            }


            if (!NPC.downedSlimeKing && npcID == NPCID.KingSlime) // King Slime
            {
                if (!(bool)MKLP.Config.BossManager.AllowKingSlime)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.KingSlime_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.KingSlime_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedBoss1 && npcID == NPCID.EyeofCthulhu) // Eye of Cthulhu
            {
                if (!(bool)MKLP.Config.BossManager.AllowEyeOfCthulhu)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedBoss2 && (npcID is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail)) // Eater of Worlds
            {
                if (!(bool)MKLP.Config.BossManager.AllowEaterOfWorlds)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedBoss2 && npcID == NPCID.BrainofCthulhu) // Brain of Cthulhu
            {
                if (!(bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedQueenBee && npcID == NPCID.QueenBee) // Queen Bee
            {
                if (!(bool)MKLP.Config.BossManager.AllowQueenBee)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.QueenBee_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.QueenBee_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedBoss3 && npcID == NPCID.SkeletronHead) // Skeletron
            {
                if (!(bool)MKLP.Config.BossManager.AllowSkeletron)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.Skeletron_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.Skeletron_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedDeerclops && npcID == NPCID.Deerclops) // Deerclops
            {
                if (!(bool)MKLP.Config.BossManager.AllowDeerclops)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.Deerclops_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.Deerclops_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!Main.hardMode && npcID == NPCID.WallofFlesh) // Wall of Flesh
            {
                if (!(bool)MKLP.Config.BossManager.AllowWallOfFlesh)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.WallOfFlesh_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.WallOfFlesh_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedQueenSlime && npcID == NPCID.QueenSlimeBoss) // Queen Slime
            {
                if (!(bool)MKLP.Config.BossManager.AllowQueenSlime)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.QueenSlime_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.QueenSlime_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (Main.zenithWorld)
            {
                int[] MechdusaIDs = { NPCID.Retinazer, NPCID.Spazmatism, NPCID.Probe, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice };
                if ((!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss1) &&
                    (npcID == NPCID.Retinazer || npcID == NPCID.Spazmatism || (npcID == NPCID.TheDestroyer || npcID == NPCID.TheDestroyerBody || npcID == NPCID.TheDestroyerTail) || npcID == NPCID.SkeletronPrime)
                     //&& NPC_Is_Active(new int[] { NPCID.Retinazer, NPCID.Spazmatism, NPCID.Probe, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice })
                     )
                {
                    if (!(bool)MKLP.Config.BossManager.AllowMechdusa)
                    {
                        return BossAllowType.MechDusaNotAllowed1;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                    {
                        PlayerCountRequire = (int)MKLP.Config.BossManager.Mechdusa_RequiredPlayersforBoss;
                        return BossAllowType.MechDusaNotAllowed2;
                    }
                }
            }
            else
            {
                if (!NPC.downedMechBoss2 && (npcID == NPCID.Retinazer || npcID == NPCID.Spazmatism)) // The Twins
                {
                    if (!(bool)MKLP.Config.BossManager.AllowTheTwins)
                    {
                        return BossAllowType.NotAllowed;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.TheTwins_RequiredPlayersforBoss)
                    {
                        PlayerCountRequire = (int)MKLP.Config.BossManager.TheTwins_RequiredPlayersforBoss;
                        return BossAllowType.LackOfPlayers;
                    }
                }

                if (!NPC.downedMechBoss1 && (npcID == NPCID.TheDestroyer || npcID == NPCID.TheDestroyerBody || npcID == NPCID.TheDestroyerTail)) // The Destroyer
                {
                    if (!(bool)MKLP.Config.BossManager.AllowTheDestroyer)
                    {
                        return BossAllowType.NotAllowed;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.TheDestroyer_RequiredPlayersforBoss)
                    {
                        PlayerCountRequire = (int)MKLP.Config.BossManager.TheDestroyer_RequiredPlayersforBoss;
                        return BossAllowType.LackOfPlayers;
                    }
                }

                if (!NPC.downedMechBoss3 && npcID == NPCID.SkeletronPrime) // Skeletron Prime
                {
                    if (!(bool)MKLP.Config.BossManager.AllowSkeletronPrime)
                    {
                        return BossAllowType.NotAllowed;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.SkeletronPrime_RequiredPlayersforBoss)
                    {
                        PlayerCountRequire = (int)MKLP.Config.BossManager.SkeletronPrime_RequiredPlayersforBoss;
                        return BossAllowType.LackOfPlayers;
                    }
                }
            }



            if (!NPC.downedPlantBoss && npcID == NPCID.Plantera) // Plantera
            {
                if (!(bool)MKLP.Config.BossManager.AllowPlantera)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.Plantera_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.Plantera_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedGolemBoss && npcID == NPCID.Golem) // Golem
            {
                if (!(bool)MKLP.Config.BossManager.AllowGolem)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.Golem_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.Golem_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedFishron && npcID == NPCID.DukeFishron) // Duke Fishron
            {
                if (!(bool)MKLP.Config.BossManager.AllowDukeFishron)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.DukeFishron_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.DukeFishron_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedEmpressOfLight && npcID == NPCID.HallowBoss) // Empress of Light
            {
                if (!(bool)MKLP.Config.BossManager.AllowEmpressOfLight)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.EmpressOfLight_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.EmpressOfLight_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedAncientCultist && npcID == NPCID.CultistBoss) // Lunatic Cultist
            {
                if (!(bool)MKLP.Config.BossManager.AllowLunaticCultist)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.LunaticCultist_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.LunaticCultist_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            if (!NPC.downedMoonlord && npcID == NPCID.MoonLordCore) // Moon Lord
            {
                if (!(bool)MKLP.Config.BossManager.AllowMoonLord)
                {
                    return BossAllowType.NotAllowed;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)MKLP.Config.BossManager.MoonLord_RequiredPlayersforBoss)
                {
                    PlayerCountRequire = (int)MKLP.Config.BossManager.MoonLord_RequiredPlayersforBoss;
                    return BossAllowType.LackOfPlayers;
                }
            }

            return BossAllowType.Allowed;
            #endregion
        }


        public static string GetBossIconFromName(string Name)
        {
            return Name switch
            {
                "King Slime" => "[i:2493]",
                "Eye Of Cthulhu" => "[i:2112]",

                "Eater Of Worlds" => "[i:2111]",
                "Brain Of Cthulhu" => "[i:2104]",
                "Evil Boss" => WorldGen.crimson ? "[i:2104]" : "[i:2111]",

                "Deerclops" => "[i:5109]",
                "Queen Bee" => "[i:2108]",
                "Skeletron" => "[i:1281]",
                "Wall Of Flesh" => "[i:2105]",
                "Queen Slime" => "[i:4959]",
                "The Destroyer" => "[i:2113]",
                "The Twins" => "[i:2106]",
                "Skeletron Prime" => "[i:2107]",
                "Mechdusa" => "[i:2113][i:2106][i:2107]",
                "Duke Fishron" => "[i:2588]",
                "Plantera" => "[i:2109]",
                "Empress Of Light" => "[i:4784]",
                "Golem" => "[i:2110]",
                "Lunatic Cultist" => "[i:3372]",
                "MoonLord" => "[i:3373]",
                _ => ""
            };
        }

        public static Dictionary<string, bool> GetDefeatedBoss()
        {
            #region code
            Config.CONFIG_BOSSES getenabledboss = MKLP.Config.BossManager;

            Dictionary<string, bool> defeatedbosses = new();

            if ((bool)getenabledboss.AllowKingSlime)
            {
                if (NPC.downedSlimeKing)
                {
                    defeatedbosses.Add("King Slime", true);
                }
                else
                {
                    defeatedbosses.Add("King Slime", false);
                }
            }
            else if (NPC.downedSlimeKing)
            {
                defeatedbosses.Add("King Slime", true);
            }
            if ((bool)getenabledboss.AllowEyeOfCthulhu)
            {
                if (NPC.downedBoss1)
                {
                    defeatedbosses.Add("Eye Of Cthulhu", true);
                }
                else
                {
                    defeatedbosses.Add("Eye Of Cthulhu", false);
                }
            }
            else if (NPC.downedBoss1)
            {
                defeatedbosses.Add("Eye Of Cthulhu", true);
            }
            if ((bool)getenabledboss.AllowEaterOfWorlds || (bool)getenabledboss.AllowBrainOfCthulhu)
            {
                if (NPC.downedBoss2)
                {
                    defeatedbosses.Add($"Evil Boss", true);
                }
                else
                {
                    defeatedbosses.Add($"Evil Boss", false);
                }
            }
            else if (NPC.downedBoss2)
            {
                defeatedbosses.Add($"Evil Boss", true);
            }
            if ((bool)getenabledboss.AllowDeerclops)
            {
                if (NPC.downedDeerclops)
                {
                    defeatedbosses.Add("Deerclops", true);
                }
                else
                {
                    defeatedbosses.Add("Deerclops", false);
                }
            }
            else if (NPC.downedDeerclops)
            {
                defeatedbosses.Add("Deerclops", true);
            }
            if ((bool)getenabledboss.AllowQueenBee)
            {
                if (NPC.downedQueenBee)
                {
                    defeatedbosses.Add("Queen Bee", true);
                }
                else
                {
                    defeatedbosses.Add("Queen Bee", false);
                }
            }
            else if (NPC.downedQueenBee)
            {
                defeatedbosses.Add("Queen Bee", true);
            }
            if ((bool)getenabledboss.AllowSkeletron)
            {
                if (NPC.downedBoss3)
                {
                    defeatedbosses.Add("Skeletron", true);
                }
                else
                {
                    defeatedbosses.Add("Skeletron", false);
                }
            }
            else if (NPC.downedBoss3)
            {
                defeatedbosses.Add("Skeletron", true);
            }
            if ((bool)getenabledboss.AllowWallOfFlesh)
            {
                if (Main.hardMode)
                {
                    defeatedbosses.Add("Wall Of Flesh", true);
                }
                else
                {
                    defeatedbosses.Add("Wall Of Flesh", false);
                }
            }
            else if (Main.hardMode)
            {
                defeatedbosses.Add("Wall Of Flesh", true);
            }
            if ((bool)getenabledboss.AllowQueenSlime)
            {
                if (NPC.downedQueenSlime)
                {
                    defeatedbosses.Add("Queen Slime", true);
                }
                else
                {
                    defeatedbosses.Add("Queen Slime", false);
                }
            }
            else if (NPC.downedQueenSlime)
            {
                defeatedbosses.Add("Queen Slime", true);
            }
            if (Main.zenithWorld)
            {
                if ((bool)getenabledboss.AllowTheDestroyer && (bool)getenabledboss.AllowTheTwins && (bool)getenabledboss.AllowSkeletronPrime)
                {
                    if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("Mechdusa", true);
                    }
                    else
                    {
                        defeatedbosses.Add("Mechdusa", false);
                    }
                }
                else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                {
                    defeatedbosses.Add("Mechdusa", true);
                }
            }
            else
            {
                if ((bool)getenabledboss.AllowTheDestroyer)
                {
                    if (NPC.downedMechBoss1)
                    {
                        defeatedbosses.Add("The Destroyer", true);
                    }
                    else
                    {
                        defeatedbosses.Add("The Destroyer", false);
                    }
                }
                else if (NPC.downedMechBoss1)
                {
                    defeatedbosses.Add("The Destroyer", true);
                }
                if ((bool)getenabledboss.AllowTheTwins)
                {
                    if (NPC.downedMechBoss2)
                    {
                        defeatedbosses.Add("The Twins", true);
                    }
                    else
                    {
                        defeatedbosses.Add("The Twins", false);
                    }
                }
                else if (NPC.downedMechBoss2)
                {
                    defeatedbosses.Add("The Twins", true);
                }
                if ((bool)getenabledboss.AllowSkeletronPrime)
                {
                    if (NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("Skeletron prime", true);
                    }
                    else
                    {
                        defeatedbosses.Add("Skeletron prime", false);
                    }
                }
                else if (NPC.downedMechBoss3)
                {
                    defeatedbosses.Add("Skeletron prime", true);
                }
            }

            if ((bool)getenabledboss.AllowDukeFishron)
            {
                if (NPC.downedFishron)
                {
                    defeatedbosses.Add("Duke Fishron", true);
                }
                else
                {
                    defeatedbosses.Add("Duke Fishron", false);
                }
            }
            else if (NPC.downedFishron)
            {
                defeatedbosses.Add("Duke Fishron", true);
            }
            if ((bool)getenabledboss.AllowPlantera)
            {
                if (NPC.downedPlantBoss)
                {
                    defeatedbosses.Add("Plantera", true);
                }
                else
                {
                    defeatedbosses.Add("Plantera", false);
                }
            }
            else if (NPC.downedPlantBoss)
            {
                defeatedbosses.Add("Plantera", true);
            }
            if ((bool)getenabledboss.AllowEmpressOfLight)
            {
                if (NPC.downedEmpressOfLight)
                {
                    defeatedbosses.Add("Empress Of Light", true);
                }
                else
                {
                    defeatedbosses.Add("Empress Of Light", false);
                }
            }
            else if (NPC.downedEmpressOfLight)
            {
                defeatedbosses.Add("Empress Of Light", true);
            }
            if ((bool)getenabledboss.AllowGolem)
            {
                if (NPC.downedGolemBoss)
                {
                    defeatedbosses.Add("Golem", true);
                }
                else
                {
                    defeatedbosses.Add("Golem", false);
                }
            }
            else if (NPC.downedGolemBoss)
            {
                defeatedbosses.Add("Golem", true);
            }
            if ((bool)getenabledboss.AllowLunaticCultist)
            {
                if (NPC.downedAncientCultist)
                {
                    defeatedbosses.Add("Lunatic Cultist", true);
                }
                else
                {
                    defeatedbosses.Add("Lunatic Cultist", false);
                }
            }
            else if (NPC.downedAncientCultist)
            {
                defeatedbosses.Add("Lunatic Cultist", true);
            }
            if ((bool)getenabledboss.AllowMoonLord)
            {
                if (NPC.downedMoonlord)
                {
                    defeatedbosses.Add("MoonLord", true);
                }
                else
                {
                    defeatedbosses.Add("MoonLord", false);
                }
            }
            else if (NPC.downedMoonlord)
            {
                defeatedbosses.Add("MoonLord", true);
            }
            return defeatedbosses;
            #endregion
        }

        public static string GetEventsIconFromName(string Name)
        {
            return Name switch
            {
                "Goblin Army" => "[i:361]",
                "Frost Legion" => "[i:602]",
                "Pirates" => "[i:1315]",
                "Pirate Invasion" => "[i:1315]",
                "Pumpkin Moon" => "[i:1844]",
                "Frost Moon" => "[i:1958]",
                "Martians" => "[i:2769]",
                "The Martians" => "[i:2769]",
                "Martian Invasion" => "[i:2769]",
                "Celestial Pillar" => "[i:3601]",
                "Celestial Pillars" => "[i:3601]",
                "Lunar Event" => "[i:3601]",
                "Lunar Events" => "[i:3601]",
                _ => ""
            };
        }
        public static Dictionary<string, bool> GetDefeatedEvents()
        {
            #region code
            Dictionary<string, bool> defeatedinvasion = new();
            if (true)
            {
                if (NPC.downedGoblins)
                {
                    defeatedinvasion.Add("Goblin Army", true);
                }
                else
                {
                    //defeatedinvasion.Add("Goblin Army", false);
                }
            }
            if (true)
            {
                if (NPC.downedFrost)
                {
                    defeatedinvasion.Add("Frost Legion", true);
                }
                else
                {
                    //defeatedinvasion.Add("Frost Legion", false);
                }
            }
            if (true)
            {
                if (NPC.downedPirates)
                {
                    defeatedinvasion.Add("Pirates", true);
                }
                else
                {
                    //defeatedinvasion.Add("Pirates", false);
                }
            }
            if (true)
            {
                if (NPC.downedChristmasTree && NPC.downedChristmasSantank && NPC.downedChristmasIceQueen)
                {
                    defeatedinvasion.Add("Frost Moon", true);
                }
                else
                {
                    //defeatedinvasion.Add("Frost Moon", false);
                }
            }
            if (true)
            {
                if (NPC.downedHalloweenTree && NPC.downedHalloweenKing)
                {
                    defeatedinvasion.Add("Pumpkin Moon", true);
                }
                else
                {
                    //defeatedinvasion.Add("Pumpkin Moon", false);
                }
            }
            if (true)
            {
                if (NPC.downedMartians)
                {
                    defeatedinvasion.Add("The Martians", true);
                }
                else
                {
                    //defeatedinvasion.Add("The Martians", false);
                }
            }
            if (true)
            {
                if (NPC.downedTowers)
                {
                    defeatedinvasion.Add("Celestial Pillars", true);
                }
                else
                {
                    //defeatedinvasion.Add("Celestial Pillars", false);
                }
            }
            return defeatedinvasion;
            #endregion
        }

        public static (string, DateTime) GetNextBossSchedule()
        {
            #region code
            Config.CONFIG_BOSSES getbosssched = MKLP.Config.BossManager;
            string result = "";
            DateTime nextsched = DateTime.MaxValue;
            if (!(bool)getbosssched.AllowKingSlime && !NPC.downedSlimeKing)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowKingSlime)
                {
                    result = "\n\nNext Boss is King Slime in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowKingSlime;
                }
            }
            if (!(bool)getbosssched.AllowEyeOfCthulhu && !NPC.downedBoss1)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowEyeOfCthulhu)
                {
                    result = "\n\nNext Boss is Eye Of Cthulhu in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowEyeOfCthulhu;
                }
            }
            if (!(bool)getbosssched.AllowEaterOfWorlds && !NPC.downedBoss2)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowEaterOfWorlds)
                {
                    result = "\n\nNext Boss is Eater Of Worlds in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowEaterOfWorlds;
                }
            }
            if (!(bool)getbosssched.AllowDeerclops && !NPC.downedDeerclops)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowDeerclops)
                {
                    result = "\n\nNext Boss is Deerclops in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowDeerclops;
                }
            }
            if (!(bool)getbosssched.AllowQueenBee && !NPC.downedQueenBee)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowQueenBee)
                {
                    result = "\n\nNext Boss is Queen Bee in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowQueenBee;
                }
            }
            if (!(bool)getbosssched.AllowSkeletron && !NPC.downedBoss3)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowSkeletron)
                {
                    result = "\n\nNext Boss is Skeletron in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowSkeletron;
                }
            }
            if (!(bool)getbosssched.AllowWallOfFlesh && !Main.hardMode)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowWallOfFlesh)
                {
                    result = "\n\nNext Boss is Wall of Flesh in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowWallOfFlesh;
                }
            }
            if (!(bool)getbosssched.AllowQueenSlime && !NPC.downedQueenSlime)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowQueenSlime)
                {
                    result = "\n\nNext Boss is Queen Slime in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowQueenSlime;
                }
            }
            if (!(bool)getbosssched.AllowTheDestroyer && !NPC.downedMechBoss1)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowTheDestroyer)
                {
                    result = "\n\nNext Boss is The Destroyer in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowTheDestroyer;
                }
            }
            if (!(bool)getbosssched.AllowTheTwins && !NPC.downedMechBoss2)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowTheTwins)
                {
                    result = "\n\nNext Boss is The Twins in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowTheTwins;
                }
            }
            if (!(bool)getbosssched.AllowSkeletronPrime && !NPC.downedMechBoss3)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowSkeletronPrime)
                {
                    result = "\n\nNext Boss is Skeletron Prime in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowSkeletronPrime;
                }
            }
            if (!(bool)getbosssched.AllowDukeFishron && !NPC.downedFishron)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowDukeFishron)
                {
                    result = "\n\nNext Boss is Duke Fishron in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowDukeFishron;
                }
            }
            if (!(bool)getbosssched.AllowPlantera && !NPC.downedPlantBoss)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowPlantera)
                {
                    result = "\n\nNext Boss is Plantera in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowPlantera;
                }
            }
            if (!(bool)getbosssched.AllowEmpressOfLight && !NPC.downedEmpressOfLight)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowEmpressOfLight)
                {
                    result = "\n\nNext Boss is Empress Of Light in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowEmpressOfLight;
                }
            }
            if (!(bool)getbosssched.AllowGolem && !NPC.downedGolemBoss)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowGolem)
                {
                    result = "\n\nNext Boss is Golem in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowGolem;
                }
            }
            if (!(bool) getbosssched.AllowLunaticCultist && !NPC.downedAncientCultist)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowLunaticCultist)
                {
                    result = "\n\nNext Boss is Lunatic Cultist in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowLunaticCultist;
                }
            }

            if (!(bool)getbosssched.AllowMoonLord && !NPC.downedMoonlord)
            {
                if (nextsched > (DateTime)getbosssched.ScheduleAllowMoonLord)
                {
                    result = "\n\nNext Boss is Moon Lord in ";
                    nextsched = (DateTime)getbosssched.ScheduleAllowMoonLord;
                }
            }

            return (result, nextsched);
            #endregion
        }
    }
}
