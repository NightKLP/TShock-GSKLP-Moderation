using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using TShockAPI;
using TShockAPI.DB;
using Org.BouncyCastle.Asn1.X509;

namespace MKLP.Modules
{
    public static class InventoryManager
    {

        public static List<string> InventoryLogs = new();
        static int InvLog_index = 0;

        public static void TryAddInvLog(TSPlayer tsplayer, Item prevplayerinv, Item playerinv, int slot, string Type)
        {
            if (!(bool)MKLP.Config.Main.Logging.Save_Inventory_Log) return;

            InvLog_index++;

            if (InventoryLogs.Count >= (int)MKLP.Config.Main.Logging.Save_InvLog_Max)
            {
                InventoryLogs.RemoveRange(0, (int)MKLP.Config.Main.Logging.Remove_InvLog_IfMax);
            }
            //OutOfMemoryException

            string log = $"{tsplayer.Name}{DiscordKLP.S_}{Type}{DiscordKLP.S_}{slot}{DiscordKLP.S_}" +
                $"{prevplayerinv.netID},{prevplayerinv.stack},{prevplayerinv.prefix}" +
                DiscordKLP.S_ +
                $"{playerinv.netID},{playerinv.stack},{playerinv.prefix}|{InvLog_index}";

            SendLog_ToInvTrackPlayers(tsplayer,
                $"[c/FF8E59:{tsplayer.Name}] [c/FFFC59:{Type} Slot {slot}] has change from {TShock.Utils.ItemTag(prevplayerinv)} to {TShock.Utils.ItemTag(playerinv)}"
                );

            if (!InventoryLogs.Contains(log))
            {
                InventoryLogs.Add(log);
            }
        }

        public static void SendLog_ToInvTrackPlayers(TSPlayer target, string message)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;

                if (player.ContainsData("MKLP_TrackInv"))
                {
                    if (player.GetData<int>("MKLP_TrackInv") == target.Index)
                    {
                        player.SendWarningMessage(
                            "[MKLP] " + message
                            );
                    }
                }
            }
        } 

        public static void UnTrack_WhoTracksThePlayer(int TargetIndex)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;

                if (player.ContainsData("MKLP_TrackInv"))
                {
                    if (player.GetData<int>("MKLP_TrackInv") == TargetIndex)
                    {
                        player.SetData("MKLP_TrackInv", -1);
                    }
                }
            }
        }

        public static void TryAddInvLog(TSPlayer tsplayer, NetItem prevplayerinv, Item playerinv, int slot, string Type)
        {
            if (!(bool)MKLP.Config.Main.Logging.Save_Inventory_Log) return;

            InvLog_index++;

            if (InventoryLogs.Count >= (int)MKLP.Config.Main.Logging.Save_InvLog_Max)
            {
                InventoryLogs.RemoveRange(0, (int)MKLP.Config.Main.Logging.Remove_InvLog_IfMax);
            }
            //OutOfMemoryException

            string log = $"{tsplayer.Account.Name}{DiscordKLP.S_}{Type}{DiscordKLP.S_}{slot}{DiscordKLP.S_}" +
                $"{prevplayerinv.NetId},{prevplayerinv.Stack},{prevplayerinv.PrefixId}" +
                DiscordKLP.S_ +
                $"{playerinv.netID},{playerinv.stack},{playerinv.prefix}|{InvLog_index}";

            if (!InventoryLogs.Contains(log))
            {
                InventoryLogs.Add(log);
            }
        }

        public static void InventoryView(CommandArgs args)
        {
            TSPlayer Player = args.Player;
            if (args.Parameters.Count == 0)
            {
                Player.SendErrorMessage(MKLP.GetText($"Invalid syntax. Proper syntax: {Commands.Specifier}inventoryview <player> <type>" +
                    $"\nDo '{Commands.Specifier}inventoryview help' for more info"));
                return;
            }

            //help text
            string helptext = MKLP.GetText(
                "[i:3619] [c/00f412:Inventory Viewer Info] [i:3619]" +
                $"\nProper syntax: {Commands.Specifier}inventoryview <target> <type>" +
                $"\nyou can use [c/519688:-account] to specify player account basically track offline players" +
                $"\nor use [c/519688:-accountid] if you wanna specify accountid on [c/899651:<target>]" +
                "\n[c/a4ff4e:[List of Types][c/a4ff4e:]]" +
                "\n[c/ffffff:'inventory/inv'] [c/71b45a:'equipment/equip'] [c/f268ff:'piggy/pig'] [c/6f6f6f:'safe'] [c/e3fa00:'defenderforge/forge'] [c/c600fa:'voidvault/vault'] [c/fa2b00:'all']" +
                "\n------------------------------" +
                "\n[c/fab200:about 'track' type]\ninfo: get logged when a player inventory changes... \nturnoff: to turn it off repeat the command again\n[c/f40000:warning: this can flood your chat message]" +
                "\n------------------------------" +
                "\nExample Commands:" +
                $"\n'{Commands.Specifier}inventoryview [c/abff96:\"{Player.Name}\"] [c/96ffdc:inv]' : View inventory contents" +
                $"\n'{Commands.Specifier}inventoryview [c/abff96:\"{Player.Name}\"] [c/96ffdc:inv] -account' : View inventory contents (offline player)");

            if (args.Parameters[0] == "help")
            {
                Player.SendMessage(helptext, Color.WhiteSmoke);
                return;
            }

            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Specify a type!\nDo [ {Commands.Specifier}inventoryview help ] for more info"));
                return;
            }

            bool selectaccount = args.Parameters.Any(p => p == "-account");
            bool selectaccountid = args.Parameters.Any(p => p == "-accountid");

            string targetarg = args.Parameters[0];

            string targetplayerlogin = "";
            string targetplayername;

            InvData_PLR getplrdata = new();
            if (selectaccountid)
            {
                if (!int.TryParse(targetarg, out int targetid))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Account ID!"));
                    return;
                }
                UserAccount getacc = TShock.UserAccounts.GetUserAccountByID(targetid);
                if (getacc == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account ID : {0} doesn't exist!", targetid));
                    return;
                }

                if (!getplrdata.Init(getacc.ID))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Unable to get inventory data from accountid {0}", targetid));
                    return;
                }
                targetplayername = getacc.Name;
            } else if (selectaccount)
            {
                UserAccount getacc = TShock.UserAccounts.GetUserAccountByName(targetarg);
                if (getacc == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account by the name {0} doesn't exist!", targetarg));
                    return;
                }

                if (!getplrdata.Init(getacc.ID))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Unable to get inventory data from account {0}", targetarg));
                    return;
                }
                targetplayername = getacc.Name;
            } else
            {
                var foundPlr = TSPlayer.FindByNameOrID(targetarg);
                if (foundPlr.Count == 0)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid player!"));
                    return;
                }
                if (foundPlr.Count > 1)
                {
                    args.Player.SendMultipleMatchError(foundPlr.Select(plr => plr.Name));
                    return;
                }

                var targetplayer = foundPlr[0];

                getplrdata.Init(targetplayer);
                targetplayername = targetplayer.Name;
                //makes a variable to check if this player is logged in or not ( usefull to avoid false ban )
                targetplayerlogin = (targetplayer.IsLoggedIn ? MKLP.GetText("[c/5c5c5c:status: ][c/05f400:this player is logged in.]") : MKLP.GetText("[c/5c5c5c:status: ][c/f40000:This player hasn't been logged in!]"));
            }

                

            
            #region Types
            switch (args.Parameters[1])
            {
                case "inventory":
                case "inv":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) inventory:\n{getplrdata.GetInventoryText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.WhiteSmoke);
                        return;
                    }
                case "equipment":
                case "equip":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Equipment:\n{getplrdata.GetArmorEquipText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.Green);
                        return;
                    }
                case "piggybank":
                case "piggy":
                case "pig":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Piggy Bank:\n{getplrdata.GetPiggyBankText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.Pink);
                        return;
                    }
                case "safe":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Safe:\n{getplrdata.GetSafeText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.Gray);
                        return;
                    }
                case "defenderforge":
                case "forge":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) defender's forge:\n{getplrdata.GetDefenderForgeText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.Yellow);
                        return;
                    }
                case "voidvault":
                case "void":
                case "vault":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Void vault\n{getplrdata.GetVoidVaultText(args.Player.RealPlayer)}\n\n{targetplayerlogin}", Color.Purple);
                        return;
                    }
                case "all":
                    {
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Inventory:\n{getplrdata.GetInventoryText(args.Player.RealPlayer)}\n\n" +
                            $"Equipment:\n{getplrdata.GetArmorEquipText(args.Player.RealPlayer)}\n" +
                            $"piggy bank:\n{getplrdata.GetPiggyBankText(args.Player.RealPlayer)}\n" +
                            $"safe:\n{getplrdata.GetSafeText(args.Player.RealPlayer)}\n" +
                            $"defender's forge:\n{getplrdata.GetDefenderForgeText(args.Player.RealPlayer)}\n" +
                            $"void vault:\n{getplrdata.GetVoidVaultText(args.Player.RealPlayer)}\n" +
                            $"{targetplayerlogin}", Color.Gray);
                        return;
                    }
                case "track":
                    {
                        if (selectaccount || selectaccountid)
                        {
                            args.Player.SendErrorMessage(
                                MKLP.GetText(
                                    "you can only use this to track online players!"
                                    ));
                            return;
                        }
                        if (!args.Player.RealPlayer)
                        {
                            args.Player.SendErrorMessage(
                                MKLP.GetText(
                                    "you can only use this in-game."
                                    ));
                            return;
                        }

                        var foundPlr = TSPlayer.FindByNameOrID(targetarg);
                        if (foundPlr.Count == 0)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText("Invalid player!"));
                            return;
                        }
                        if (foundPlr.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(foundPlr);
                            return;
                        }

                        TSPlayer targetplayer = foundPlr[0];

                        if (Player.ContainsData("MKLP_TrackInv"))
                        {
                            if (Player.GetData<int>("MKLP_TrackInv") == -1)
                            {
                                args.Player.SendErrorMessage("Invalid Player.");
                            }

                            args.Player.SendSuccessMessage("Your no longer Tracking someones inventory");
                            Player.SetData("MKLP_TrackInv", -1);
                        }

                        args.Player.SendSuccessMessage($"You're now tracking {targetplayer.Name} Inventory...");
                        Player.SetData("MKLP_TrackInv", targetplayer.Index);
                        return;
                    }
                default:
                    {
                        Player.SendErrorMessage($"Invalid type!\nDo [ {Commands.Specifier}inventoryview help ] for more info");
                        return;
                    }

            }
            #endregion
        }

    }

    #region OBJECTS

    public class InvData_PLR
    {
        public int AccountID;

        public NetItem[] Inventory = new NetItem[NetItem.InventoryIndex.Item2];
        public int HeldItem = -1;

        public NetItem TrashSlot;

        public int Loadout_Using;

        //armor
        public NetItem[] Armor = new NetItem[NetItem.ArmorIndex.Item2];
        public NetItem[] ArmorDye = new NetItem[NetItem.DyeIndex.Item2];

        //misc
        public NetItem[] Equipment = new NetItem[NetItem.MiscEquipIndex.Item2];
        public NetItem[] EquipmentDye = new NetItem[NetItem.MiscDyeIndex.Item2];

        //extra inv ( safe chest )
        public NetItem[] PiggyBank = new NetItem[NetItem.PiggyIndex.Item2];
        public NetItem[] Safe = new NetItem[NetItem.SafeIndex.Item2];
        public NetItem[] DefenderForge = new NetItem[NetItem.ForgeIndex.Item2];
        public NetItem[] VoidVault = new NetItem[NetItem.VoidIndex.Item2];

        //armor
        public NetItem[] Loadout1 = new NetItem[NetItem.Loadout1Armor.Item2];
        public NetItem[] LoadoutDye1 = new NetItem[NetItem.Loadout1Dye.Item2];
        public NetItem[] Loadout2 = new NetItem[NetItem.Loadout2Armor.Item2];
        public NetItem[] LoadoutDye2 = new NetItem[NetItem.Loadout2Dye.Item2];
        public NetItem[] Loadout3 = new NetItem[NetItem.Loadout3Armor.Item2];
        public NetItem[] LoadoutDye3 = new NetItem[NetItem.Loadout3Dye.Item2];

        #region [ Prefix List ]
        public static string[] prefixlist = { "" ,"Large", "Massive", "Dangerous", "Savage", "Sharp", "Pointy", "Tiny",
            "Terrible", "Small", "Dull", "Unhappy", "Bulky", "Shameful", "Heavy", "Light", "Sighted", "Sighted",
            "Sighted", "Intimidating", "Deadly", "Staunch", "Awful", "Lethargic", "Awkward", "Powerful", "Mystic",
            "Adept", "Masterful", "Inept", "Ignorant", "Deranged", "Intense", "Taboo", "Celestial", "Furious", "Keen",
            "Superior", "Forceful", "Broken", "Damaged", "Shoddy", "Quick", "Deadly", "Agile", "Nimble", "Murderous",
            "Slow", "Sluggish", "Lazy", "Annoying", "Nasty", "Manic", "Hurtful", "Strong", "Unpleasant", "Weak",
            "Ruthless", "Frenzying", "Godly", "Demonic", "Zealous", "Hard", "Guarding", "Armored", "Warding",
            "Arcane", "Precise", "Lucky", "Jagged", "Spiked", "Angry", "Menacing", "Brisk", "Fleeting", "Hasty",
            "Quick", "Wild", "Rash", "Intrepid", "Violent", "Legendary", "Unreal", "Mythical", "Legendary", "Piercing" };
        #endregion

        public bool Init(TSPlayer player)
        {
            Inventory = ConvertToNet(player.TPlayer.inventory);
            HeldItem = player.TPlayer.selectedItem;
            TrashSlot = new(player.TPlayer.trashItem);
            Armor = ConvertToNet(player.TPlayer.armor);
            ArmorDye = ConvertToNet(player.TPlayer.dye);
            Equipment = ConvertToNet(player.TPlayer.miscEquips);
            EquipmentDye = ConvertToNet(player.TPlayer.miscDyes);
            PiggyBank = ConvertToNet(player.TPlayer.bank.item);
            Safe = ConvertToNet(player.TPlayer.bank2.item);
            DefenderForge = ConvertToNet(player.TPlayer.bank3.item);
            VoidVault = ConvertToNet(player.TPlayer.bank4.item);
            Loadout_Using = player.TPlayer.CurrentLoadoutIndex;
            Loadout1 = ConvertToNet(player.TPlayer.Loadouts[0].Armor);
            LoadoutDye1 = ConvertToNet(player.TPlayer.Loadouts[0].Dye);
            Loadout2 = ConvertToNet(player.TPlayer.Loadouts[1].Armor);
            LoadoutDye2 = ConvertToNet(player.TPlayer.Loadouts[1].Dye);
            Loadout3 = ConvertToNet(player.TPlayer.Loadouts[2].Armor);
            LoadoutDye3 = ConvertToNet(player.TPlayer.Loadouts[2].Dye);

            return true;
        }
        public bool Init(int AccountID)
        {
            try
            {
                this.AccountID = AccountID;

                if (TryGetPLR(out TSPlayer player))
                {
                    Inventory = ConvertToNet(player.TPlayer.inventory);
                    HeldItem = player.TPlayer.selectedItem;
                    TrashSlot = new(player.TPlayer.trashItem);
                    Armor = ConvertToNet(player.TPlayer.armor);
                    ArmorDye = ConvertToNet(player.TPlayer.dye);
                    Equipment = ConvertToNet(player.TPlayer.miscEquips);
                    EquipmentDye = ConvertToNet(player.TPlayer.miscDyes);
                    PiggyBank = ConvertToNet(player.TPlayer.bank.item);
                    Safe = ConvertToNet(player.TPlayer.bank2.item);
                    DefenderForge = ConvertToNet(player.TPlayer.bank3.item);
                    VoidVault = ConvertToNet(player.TPlayer.bank4.item);
                    Loadout1 = ConvertToNet(player.TPlayer.Loadouts[0].Armor);
                    LoadoutDye1 = ConvertToNet(player.TPlayer.Loadouts[0].Dye);
                    Loadout2 = ConvertToNet(player.TPlayer.Loadouts[1].Armor);
                    LoadoutDye2 = ConvertToNet(player.TPlayer.Loadouts[1].Dye);
                    Loadout3 = ConvertToNet(player.TPlayer.Loadouts[2].Armor);
                    LoadoutDye3 = ConvertToNet(player.TPlayer.Loadouts[2].Dye);

                    return true;
                }
                else
                {
                    if (GetPlayerData(AccountID, out PlayerData plrdata))
                    {
                        NetItem[] inventory = plrdata.inventory;
                        Loadout_Using = plrdata.currentLoadoutIndex;

                        for (int i = 0; i < inventory.Length; i++)
                        {
                            if (i < NetItem.InventoryIndex.Item2)
                            {
                                Inventory[i] = inventory[i];
                            }
                            else if (i < NetItem.ArmorIndex.Item2)
                            {
                                int num = i - NetItem.ArmorIndex.Item1;
                                Armor[num] = inventory[i];
                            }
                            else if (i < NetItem.DyeIndex.Item2)
                            {
                                int num2 = i - NetItem.DyeIndex.Item1;
                                ArmorDye[num2] = inventory[i];
                            }
                            else if (i < NetItem.MiscEquipIndex.Item2)
                            {
                                int num3 = i - NetItem.MiscEquipIndex.Item1;
                                Equipment[num3] = inventory[i];
                            }
                            else if (i < NetItem.MiscDyeIndex.Item2)
                            {
                                int num4 = i - NetItem.MiscDyeIndex.Item1;
                                EquipmentDye[num4] = inventory[i];
                            }
                            else if (i < NetItem.PiggyIndex.Item2)
                            {
                                int num5 = i - NetItem.PiggyIndex.Item1;
                                PiggyBank[num5] = inventory[i];
                            }
                            else if (i < NetItem.SafeIndex.Item2)
                            {
                                int num6 = i - NetItem.SafeIndex.Item1;
                                Safe[num6] = inventory[i];
                            }
                            else if (i < NetItem.TrashIndex.Item2)
                            {
                                TrashSlot = inventory[i];
                            }
                            else if (i < NetItem.ForgeIndex.Item2)
                            {
                                int num7 = i - NetItem.ForgeIndex.Item1;
                                DefenderForge[num7] = inventory[i];
                            }
                            else if (i < NetItem.VoidIndex.Item2)
                            {
                                int num8 = i - NetItem.VoidIndex.Item1;
                                VoidVault[num8] = inventory[i];
                            }
                            else if (i < NetItem.Loadout1Armor.Item2)
                            {
                                int num9 = i - NetItem.Loadout1Armor.Item1;
                                Loadout1[num9] = inventory[i];
                            }
                            else if (i < NetItem.Loadout1Dye.Item2)
                            {
                                int num10 = i - NetItem.Loadout1Dye.Item1;
                                LoadoutDye1[num10] = inventory[i];
                            }
                            else if (i < NetItem.Loadout2Armor.Item2)
                            {
                                int num11 = i - NetItem.Loadout2Armor.Item1;
                                Loadout2[num11] = inventory[i];
                            }
                            else if (i < NetItem.Loadout2Dye.Item2)
                            {
                                int num12 = i - NetItem.Loadout2Dye.Item1;
                                LoadoutDye2[num12] = inventory[i];
                            }
                            else if (i < NetItem.Loadout3Armor.Item2)
                            {
                                int num13 = i - NetItem.Loadout3Armor.Item1;
                                Loadout3[num13] = inventory[i];
                            }
                            else if (i < NetItem.Loadout3Dye.Item2)
                            {
                                int num14 = i - NetItem.Loadout3Dye.Item1;
                                LoadoutDye3[num14] = inventory[i];
                            }
                        }
                        return true;
                    }
                    return false;
                }
            } catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                return false;
            }

            

            bool TryGetPLR(out TSPlayer getplr)
            {
                foreach (TSPlayer plr in TShock.Players)
                {
                    if (plr == null) continue;
                    if (plr.Account == null) continue;
                    if (plr.Account.ID != AccountID) continue;

                    getplr = plr;
                    return true;
                }
                getplr = null;
                return false;
            }
        }

        private NetItem[] ConvertToNet(Item[] item)
        {
            List<NetItem> result = new();

            foreach (Item get in item)
            {
                result.Add(new NetItem(get));
            }

            return result.ToArray();
        }
        public static bool GetPlayerData(int accountid, out PlayerData playerData)
        {
            playerData = new PlayerData(includingStarterInventory: false);
            try
            {
                using QueryResult queryResult = TShock.CharacterDB.database.QueryReader("SELECT * FROM tsCharacter WHERE Account=@0", accountid);
                if (queryResult.Read())
                {
                    playerData.exists = true;
                    playerData.health = queryResult.Get<int>("Health");
                    playerData.maxHealth = queryResult.Get<int>("MaxHealth");
                    playerData.mana = queryResult.Get<int>("Mana");
                    playerData.maxMana = queryResult.Get<int>("MaxMana");
                    List<NetItem> list = queryResult.Get<string>("Inventory").Split('~').Select(NetItem.Parse)
                        .ToList();
                    if (list.Count < NetItem.MaxInventory)
                    {
                        list.InsertRange(67, new NetItem[2]);
                        list.InsertRange(77, new NetItem[2]);
                        list.InsertRange(87, new NetItem[2]);
                        list.AddRange(new NetItem[NetItem.MaxInventory - list.Count]);
                    }
                    playerData.inventory = list.ToArray();
                    playerData.extraSlot = queryResult.Get<int>("extraSlot");
                    playerData.spawnX = queryResult.Get<int>("spawnX");
                    playerData.spawnY = queryResult.Get<int>("spawnY");
                    playerData.skinVariant = queryResult.Get<int?>("skinVariant");
                    playerData.hair = queryResult.Get<int?>("hair");
                    playerData.hairDye = (byte)queryResult.Get<int>("hairDye");
                    playerData.hairColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("hairColor"));
                    playerData.pantsColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("pantsColor"));
                    playerData.shirtColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("shirtColor"));
                    playerData.underShirtColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("underShirtColor"));
                    playerData.shoeColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("shoeColor"));
                    playerData.hideVisuals = TShock.Utils.DecodeBoolArray(queryResult.Get<int?>("hideVisuals"));
                    playerData.skinColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("skinColor"));
                    playerData.eyeColor = TShock.Utils.DecodeColor(queryResult.Get<int?>("eyeColor"));
                    playerData.questsCompleted = queryResult.Get<int>("questsCompleted");
                    playerData.usingBiomeTorches = queryResult.Get<int>("usingBiomeTorches");
                    playerData.happyFunTorchTime = queryResult.Get<int>("happyFunTorchTime");
                    playerData.unlockedBiomeTorches = queryResult.Get<int>("unlockedBiomeTorches");
                    playerData.currentLoadoutIndex = queryResult.Get<int>("currentLoadoutIndex");
                    playerData.ateArtisanBread = queryResult.Get<int>("ateArtisanBread");
                    playerData.usedAegisCrystal = queryResult.Get<int>("usedAegisCrystal");
                    playerData.usedAegisFruit = queryResult.Get<int>("usedAegisFruit");
                    playerData.usedArcaneCrystal = queryResult.Get<int>("usedArcaneCrystal");
                    playerData.usedGalaxyPearl = queryResult.Get<int>("usedGalaxyPearl");
                    playerData.usedGummyWorm = queryResult.Get<int>("usedGummyWorm");
                    playerData.usedAmbrosia = queryResult.Get<int>("usedAmbrosia");
                    playerData.unlockedSuperCart = queryResult.Get<int>("unlockedSuperCart");
                    playerData.enabledSuperCart = queryResult.Get<int>("enabledSuperCart");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region [ String Text ]
        public string GetInventoryText(bool ingametxt)
        {
            return InvData_PLR.InventoryString(Inventory, HeldItem, ingametxt) + $"\n|= {ItemTag(TrashSlot, ingametxt)} =|";
        }
        public string GetArmorEquipText(bool ingametxt)
        {
            return 
                (Loadout_Using != 0 ? ($"Loadout 1\n" + InvData_PLR.ArmorString(Loadout1, LoadoutDye1, ingametxt)) : ($"Loadout 1 ( In Use )\n" + InvData_PLR.ArmorString(Armor, ArmorDye, ingametxt))) + "\n\n" +
                (Loadout_Using != 1 ? ($"Loadout 2\n" + InvData_PLR.ArmorString(Loadout2, LoadoutDye2, ingametxt)) : ($"Loadout 2 ( In Use )\n" + InvData_PLR.ArmorString(Armor, ArmorDye, ingametxt))) + "\n\n" +
                (Loadout_Using != 2 ? ($"Loadout 3\n" + InvData_PLR.ArmorString(Loadout3, LoadoutDye3, ingametxt)) : ($"Loadout 3 ( In Use )\n" + InvData_PLR.ArmorString(Armor, ArmorDye, ingametxt))) + "\n\n" +
                InvData_PLR.EquipmentString(Equipment, EquipmentDye, ingametxt);
        }
        public string GetPiggyBankText(bool ingametxt)
        {
            return InvData_PLR.ChestString(PiggyBank, ingametxt);
        }
        public string GetSafeText(bool ingametxt)
        {
            return InvData_PLR.ChestString(Safe, ingametxt);
        }
        public string GetDefenderForgeText(bool ingametxt)
        {
            return InvData_PLR.ChestString(DefenderForge, ingametxt);
        }
        public string GetVoidVaultText(bool ingametxt)
        {
            return InvData_PLR.ChestString(VoidVault, ingametxt);
        }

        #endregion

        #region [ String Text (Static) ]

        public static string InventoryString(NetItem[] item, int HeldItem, bool ingametxt)
        {
            int gi = 0;

            return string.Join("", item.Select(get =>
            {
                string r = (gi == HeldItem ? $"| -{ItemTag(get, ingametxt)}- |" : $"| {ItemTag(get, ingametxt)} |") +
                    ((gi + 1) % 10 == 0 && gi != 0 ? "\n" : "");
                gi++;

                return r;
            })).Replace("||", "|");
        }
        public static string ChestString(NetItem[] item, bool ingametxt)
        {
            int gi = 0;

            return string.Join("", item.Select(get =>
            {
                string r = $"| {ItemTag(get, ingametxt)} |" +
                    ((gi + 1) % 10 == 0 && gi != 0 ? "\n" : "");
                gi++;

                return r;
            })).Replace("||", "|");
        }
        public static string ArmorString(NetItem[] armor, NetItem[] dye, bool ingametxt)
        {
            string result = "";
            for (int i = 0; i < 10; i++)
            {
                int ii = i + 10;
                if (i < 3)
                {
                    result += $"|{ItemTag(dye[i + 3], ingametxt)}|{ItemTag(armor[ii + 3], ingametxt)}|{ItemTag(armor[i + 3], ingametxt)}|====|{ItemTag(dye[i], ingametxt)}|{ItemTag(armor[ii], ingametxt)}|{ItemTag(armor[i], ingametxt)}|\n";
                }
                if (i >= 6 && i <= 9)
                {
                    result += $"|{ItemTag(dye[i], ingametxt)}|{ItemTag(armor[ii], ingametxt)}|{ItemTag(armor[i], ingametxt)}|\n";
                }
            }
            return result;
        }
        public static string EquipmentString(NetItem[] equip, NetItem[] dye, bool ingametxt)
        {
            string result = "";
            for (int i = 0; i < 5; i++)
            {
                result += $"|{ItemTag(dye[i], ingametxt)}|{ItemTag(equip[i], ingametxt)}|\n";
            }
            return result;
        }

        private static string ItemTag(NetItem item, bool ingametxt)
        {
            int netID = item.NetId;
            int stack = item.Stack;
            int prefix = item.PrefixId;
            if (ingametxt)
            {
                string arg = ((stack > 1) ? ("/s" + stack) : ((prefix != 0) ? ("/p" + prefix) : ""));
                return $"[i{arg}:{netID}]";
            } else
            {
                return $"( {prefixlist[prefix]}{(prefix != 0 ? " " : "")}{Lang.GetItemName(netID)}{(stack > 1 ? $" {stack}": "")} )";
            }
        }
        #endregion
    }

    #endregion
}
