//Microsoft
using Discord;
using Discord.Rest;
using IL.Terraria.Graphics;
using IL.Terraria.ID;
using IL.Terraria.UI;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;



//System
using System.ComponentModel;
using System.Data;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;


//Terraria
using Terraria;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using static System.Net.Mime.MediaTypeNames;

namespace MKLP.Modules
{
    public static class ManagePlayer
    {
        public static Dictionary<TSPlayer, NetItem[]> PreviousChestP = new();
        public static void CheckPlayerInventory(TSPlayer tsplayer,
             Item[] previnv = null,
             Item[] prevpig = null,
             Item[] prevsafe = null,
             Item[] prevforge = null,
             Item[] prevvault = null)
        {
            if (!tsplayer.IsLoggedIn) return;

            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            //Item[] prevchestopen = tsplayer.GetData<Item[]>("MKLP_PrevChestOpen");

            //Item[] previnv = tsplayer.GetData<Item[]>("MKLP_PrevInventory");

            //Item[] prevpig = tsplayer.GetData<Item[]>("MKLP_PrevPiggyBank");
            //Item[] prevsafe = tsplayer.GetData<Item[]>("MKLP_PrevsSafe");
            //Item[] prevforge = tsplayer.GetData<Item[]>("MKLP_PrevDefenderForge");
            //Item[] prevvault = tsplayer.GetData<Item[]>("MKLP_PrevVoidVault");
            if (!(bool)MKLP.Config.Main.Allow_Players_StackSameAccessory)
            {
                for (int armori = 0; armori < 10; armori++)
                {
                    for (int armorii = 0; armorii < 10; armorii++)
                    {
                        if (armorii == armori) continue;
                        if (tsplayer.TPlayer.armor[armorii].IsAir || tsplayer.TPlayer.armor[armorii].netID == 0) continue;

                        if (tsplayer.TPlayer.armor[armorii].netID == tsplayer.TPlayer.armor[armori].netID)
                        {

                            tsplayer.GiveItem(tsplayer.TPlayer.armor[armori].netID, tsplayer.TPlayer.armor[armori].stack, tsplayer.TPlayer.armor[armori].prefix);
                            tsplayer.TPlayer.armor[armori].SetDefaults();
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Terraria.Localization.NetworkText.Empty, tsplayer.Index, NetItem.InventorySlots + armori, 0f, 0f, 0);

                        }
                    }
                }
            }

            Dictionary<int, string> illegalitems = MKLP.IllegalItemProgression;

            #region test
            for (int i = 0; i < tsplayer.TPlayer.inventory.Count(); i++)
            {
                checkingifsus1("Inventory", i, tsplayer.TPlayer.inventory[i]);

                if (tsplayer.TPlayer.inventory[i].netID != previnv[i].netID ||
                    tsplayer.TPlayer.inventory[i].stack != previnv[i].stack ||
                    tsplayer.TPlayer.inventory[i].prefix != previnv[i].prefix)
                {
                    InventoryManager.TryAddInvLog(tsplayer, previnv[i], tsplayer.TPlayer.inventory[i], i, "Inv");

                    checkingifsus2("Inventory", i, previnv[i], tsplayer.TPlayer.inventory[i]);
                }
            }


            if ((bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 && !tsplayer.HasPermission(MKLP.Config.Permissions.IgnoreMainCode_1))
            {
                for (int i = 0; i < tsplayer.TPlayer.armor.Count(); i++)
                {
                    if ((tsplayer.TPlayer.armor[i].value * tsplayer.TPlayer.armor[i].stack) / 5000000 >= maxvalue && tsplayer.TPlayer.armor[i].netID != 74) MKLP.PunishPlayer(MKLP_CodeType.Main, 1, tsplayer, $"Abnormal Item [i/s{tsplayer.TPlayer.armor[i].stack}:{tsplayer.TPlayer.armor[i].netID}]", $"Player **{tsplayer.Name}** has High Value Item Stack `({tsplayer.TPlayer.armor[i].stack}) {tsplayer.TPlayer.armor[i].Name}`", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.dye.Count(); i++)
                {
                    if ((tsplayer.TPlayer.dye[i].value * tsplayer.TPlayer.dye[i].stack) / 5000000 >= maxvalue && tsplayer.TPlayer.dye[i].netID != 74) MKLP.PunishPlayer(MKLP_CodeType.Main, 1, tsplayer, $"Abnormal Item [i/s{tsplayer.TPlayer.dye[i].stack}:{tsplayer.TPlayer.dye[i].netID}]", $"Player **{tsplayer.Name}** has High Value Item Stack `({tsplayer.TPlayer.dye[i].stack}) {tsplayer.TPlayer.dye[i].Name}`", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.miscEquips.Count(); i++)
                {
                    if ((tsplayer.TPlayer.miscEquips[i].value * tsplayer.TPlayer.miscEquips[i].stack) / 5000000 >= maxvalue && tsplayer.TPlayer.miscEquips[i].netID != 74) MKLP.PunishPlayer(MKLP_CodeType.Main, 1, tsplayer, $"Abnormal Item [i/s{tsplayer.TPlayer.miscEquips[i].stack}:{tsplayer.TPlayer.miscEquips[i].netID}]", $"Player **{tsplayer.Name}** has High Value Item Stack `({tsplayer.TPlayer.miscEquips[i].stack}) {tsplayer.TPlayer.miscEquips[i].Name}`", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.miscDyes.Count(); i++)
                {
                    if ((tsplayer.TPlayer.miscDyes[i].value * tsplayer.TPlayer.miscDyes[i].stack) / 5000000 >= maxvalue && tsplayer.TPlayer.miscDyes[i].netID != 74) MKLP.PunishPlayer(MKLP_CodeType.Main, 1, tsplayer, $"Abnormal Item [i/s{tsplayer.TPlayer.miscDyes[i].stack}:{tsplayer.TPlayer.miscDyes[i].netID}]", $"Player **{tsplayer.Name}** has High Value Item Stack `({tsplayer.TPlayer.miscDyes[i].stack}) {tsplayer.TPlayer.miscDyes[i].Name}`", true);
                }
            }

            if ((bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 && !tsplayer.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_1))
            {
                for (int i = 0; i < tsplayer.TPlayer.armor.Count(); i++)
                {
                    if (illegalitems.ContainsKey(tsplayer.TPlayer.armor[i].netID)) MKLP.PunishPlayer(MKLP_CodeType.Survival, 1, tsplayer, $"{illegalitems[tsplayer.TPlayer.armor[i].netID]} Item Progression", $"Player **{tsplayer.Name}** has a item that is illegal on this progression `Item: {tsplayer.TPlayer.armor[i].Name}` **{illegalitems[tsplayer.TPlayer.armor[i].netID]}**", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.dye.Count(); i++)
                {
                    if (illegalitems.ContainsKey(tsplayer.TPlayer.dye[i].netID)) MKLP.PunishPlayer(MKLP_CodeType.Survival, 1, tsplayer, $"{illegalitems[tsplayer.TPlayer.dye[i].netID]} Item Progression", $"Player **{tsplayer.Name}** has a item that is illegal on this progression `Item: {tsplayer.TPlayer.dye[i].Name}` **{illegalitems[tsplayer.TPlayer.dye[i].netID]}**", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.miscEquips.Count(); i++)
                {
                    if (illegalitems.ContainsKey(tsplayer.TPlayer.miscEquips[i].netID)) MKLP.PunishPlayer(MKLP_CodeType.Survival, 1, tsplayer, $"{illegalitems[tsplayer.TPlayer.miscEquips[i].netID]} Item Progression", $"Player **{tsplayer.Name}** has a item that is illegal on this progression `Item: {tsplayer.TPlayer.miscEquips[i].Name}` **{illegalitems[tsplayer.TPlayer.miscEquips[i].netID]}**", true);
                }
                for (int i = 0; i < tsplayer.TPlayer.miscDyes.Count(); i++)
                {
                    if (illegalitems.ContainsKey(tsplayer.TPlayer.miscDyes[i].netID)) MKLP.PunishPlayer(MKLP_CodeType.Survival, 1, tsplayer, $"{illegalitems[tsplayer.TPlayer.miscDyes[i].netID]} Item Progression", $"Player **{tsplayer.Name}** has a item that is illegal on this progression `Item: {tsplayer.TPlayer.miscDyes[i].Name}` **{illegalitems[tsplayer.TPlayer.miscDyes[i].netID]}**", true);
                }
            }

            #region Chest
            bool dontrevertchest = (PunishmentType)MKLP.Config.Main.DisableNode.SuspiciousDupe_PunishmentType != PunishmentType.RevertAndLog &&
                        (PunishmentType)MKLP.Config.Main.DisableNode.SuspiciousDupe_PunishmentType != PunishmentType.Revert;
            if (tsplayer.ActiveChest != -1)
            {
                try
                {

                    //NetItem[] prevchestopen = tsplayer.GetData<NetItem[]>("MKLP_PrevChestOpen");
                    NetItem[] prevchestopencheck = PreviousChestP[tsplayer];

                    int indexchestcheck = 0;

                    foreach (var prevchestopen in PreviousChestP[tsplayer])
                    {
                        //checkingifsus1("Chest", i, Main.chest[tsplayer.ActiveChest].item[i]);
                        if (checkingifsus1e1("Chest", indexchestcheck, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]) || checkingifsus1e2("Chest", indexchestcheck, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]))
                        {
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].SetDefaults();
                            tsplayer.SendData(PacketTypes.ChestItem, "", tsplayer.ActiveChest, indexchestcheck, 0, 0, 0);
                        }

                        if (Main.chest[tsplayer.ActiveChest].item[indexchestcheck].netID != prevchestopen.NetId ||
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].stack != prevchestopen.Stack ||
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].prefix != prevchestopen.PrefixId)
                        {
                            InventoryManager.TryAddInvLog(tsplayer, prevchestopen, Main.chest[tsplayer.ActiveChest].item[indexchestcheck], indexchestcheck, "Chest");

                            checkingifsus3("Chest", indexchestcheck, prevchestopen, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]);
                        }
                        indexchestcheck++;
                    }

                    //NetItem[] prevchestopen = tsplayer.GetData<Item>("MKLP_PrevChestOpen");

                    /*
                    int indexchestcheck = 0;

                    foreach (var prevchestopen in tsplayer.GetData<Item[]>("MKLP_PrevChestOpen"))
                    {
                        //checkingifsus1("Chest", i, Main.chest[tsplayer.ActiveChest].item[i]);
                        if (checkingifsus1e1("Chest", indexchestcheck, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]) || checkingifsus1e2("Chest", indexchestcheck, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]))
                        {
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].SetDefaults();
                            tsplayer.SendData(PacketTypes.ChestItem, "", tsplayer.ActiveChest, indexchestcheck, 0, 0, 0);
                        }

                        if (Main.chest[tsplayer.ActiveChest].item[indexchestcheck].netID != prevchestopen.netID ||
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].stack != prevchestopen.stack ||
                            Main.chest[tsplayer.ActiveChest].item[indexchestcheck].prefix != prevchestopen.prefix)
                        {
                            InventoryManager.TryAddInvLog(tsplayer, prevchestopen, Main.chest[tsplayer.ActiveChest].item[indexchestcheck], indexchestcheck, "Chest");

                            checkingifsus4("Chest", indexchestcheck, prevchestopen, Main.chest[tsplayer.ActiveChest].item[indexchestcheck]);
                        }
                        Console.WriteLine(prevchestopen.netID + "|" + prevchestopen.stack + "---" + Main.chest[tsplayer.ActiveChest].item[indexchestcheck].netID + "|" + Main.chest[tsplayer.ActiveChest].item[indexchestcheck].stack);
                        indexchestcheck++;

                    }
                    */
                }
                catch (Exception e)
                {
                    //Console.WriteLine("\n\n" + e);
                }
            }
            try
            {

                List<NetItem> chestitem = new();
                foreach (Item item in Main.chest[tsplayer.ActiveChest].item)
                {
                    if (item == null)
                    {
                        chestitem.Add(new NetItem(0, 0, 0));
                        //Console.Write("0-");
                        continue;
                    }
                    if (item.IsAir)
                    {
                        chestitem.Add(new NetItem(0, 0, 0));
                        //Console.Write("0-");
                        continue;
                    }

                    chestitem.Add(new NetItem(item.netID, item.stack, item.prefix));
                    //Console.Write(item.netID + "-");

                }
                var result = chestitem.ToArray();
                //Console.WriteLine("\n");
                foreach (var e in result)
                {
                    //Console.Write(e.NetId + "-");

                }
                if (PreviousChestP.ContainsKey(tsplayer))
                {
                    PreviousChestP[tsplayer] = result;
                }
                else
                {
                    PreviousChestP.Add(tsplayer, (NetItem[])result);
                }

                //Item[] chestitem = new Item[40];
                //chestitem = (Item[])Main.chest[tsplayer.ActiveChest].item.Clone();
                //tsplayer.SetData("MKLP_PrevChestOpen", Main.chest[tsplayer.ActiveChest].item.Clone());
                //tsplayer.SetData("MKLP_PrevChestOpen", result);
            }
            catch (Exception e)
            {
                //Console.WriteLine("\n\n" + e);
            }

            #endregion

            if ((bool)MKLP.Config.Main.DetectAllPlayerInv)
            {
                for (int i = 0; i < tsplayer.TPlayer.bank.item.Count(); i++)
                {
                    checkingifsus1("PiggyBank", i, tsplayer.TPlayer.bank.item[i]);

                    if (tsplayer.TPlayer.bank.item[i].netID != prevpig[i].netID ||
                        tsplayer.TPlayer.bank.item[i].stack != prevpig[i].stack ||
                        tsplayer.TPlayer.bank.item[i].prefix != prevpig[i].prefix)
                    {
                        InventoryManager.TryAddInvLog(tsplayer, prevpig[i], tsplayer.TPlayer.bank.item[i], i, "Pig");

                        checkingifsus2("PiggyBank", i, prevpig[i], tsplayer.TPlayer.bank.item[i]);
                    }
                }

                for (int i = 0; i < tsplayer.TPlayer.bank2.item.Count(); i++)
                {
                    checkingifsus1("Safe", i, tsplayer.TPlayer.bank2.item[i]);

                    if (tsplayer.TPlayer.bank2.item[i].netID != prevsafe[i].netID ||
                        tsplayer.TPlayer.bank2.item[i].stack != prevsafe[i].stack ||
                        tsplayer.TPlayer.bank2.item[i].prefix != prevsafe[i].prefix)
                    {
                        InventoryManager.TryAddInvLog(tsplayer, prevsafe[i], tsplayer.TPlayer.bank2.item[i], i, "Safe");

                        checkingifsus2("Safe", i, prevsafe[i], tsplayer.TPlayer.bank2.item[i]);
                    }
                }

                for (int i = 0; i < tsplayer.TPlayer.bank3.item.Count(); i++)
                {
                    checkingifsus1("DefenderForge", i, tsplayer.TPlayer.bank3.item[i]);

                    if (tsplayer.TPlayer.bank3.item[i].netID != prevforge[i].netID ||
                        tsplayer.TPlayer.bank3.item[i].stack != prevforge[i].stack ||
                        tsplayer.TPlayer.bank3.item[i].prefix != prevforge[i].prefix)
                    {
                        InventoryManager.TryAddInvLog(tsplayer, prevforge[i], tsplayer.TPlayer.bank3.item[i], i, "Forge");

                        checkingifsus2("DefenderForge", i, prevforge[i], tsplayer.TPlayer.bank3.item[i]);
                    }
                }

                for (int i = 0; i < tsplayer.TPlayer.bank4.item.Count(); i++)
                {
                    checkingifsus1("VoidVault", i, tsplayer.TPlayer.bank4.item[i]);

                    if (tsplayer.TPlayer.bank4.item[i].netID != prevvault[i].netID ||
                        tsplayer.TPlayer.bank4.item[i].stack != prevvault[i].stack ||
                        tsplayer.TPlayer.bank4.item[i].prefix != prevvault[i].prefix)
                    {
                        InventoryManager.TryAddInvLog(tsplayer, prevvault[i], tsplayer.TPlayer.bank4.item[i], i, "Vault");

                        checkingifsus2("VoidVault", i, prevvault[i], tsplayer.TPlayer.bank4.item[i]);
                    }
                }
            }

            bool checkingifsus1(string type, int slot, Item now)
            {

                if (illegalitems.ContainsKey(now.netID) &&
                    (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 &&
                    !tsplayer.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_1))
                {
                    MKLP.PunishPlayer(MKLP_CodeType.Survival, 1, tsplayer, $"{illegalitems[now.netID]} Item Progression", $"Player **{tsplayer.Name}** has a item that is illegal on this progression `Item: {now.Name}` **{illegalitems[now.netID]}**", true);
                    return true;
                }

                if ((now.value * now.stack) / 5000000 >= maxvalue &&
                    now.netID != 74
                    && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 &&
                    !tsplayer.HasPermission(MKLP.Config.Permissions.IgnoreMainCode_1))
                {
                    MKLP.PunishPlayer(MKLP_CodeType.Main, 1, tsplayer, $"Abnormal Item [i/s{now.stack}:{now.netID}]", $"Player **{tsplayer.Name}** has High Value Item Stack `({now.stack}) {now.Name}`", true);
                    return true;
                }

                return false;
            }
            bool checkingifsus1e1(string type, int slot, Item now)
            {

                if (illegalitems.ContainsKey(now.netID) &&
                    (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1)
                {
                    return true;
                }

                return false;
            }
            bool checkingifsus1e2(string type, int slot, Item now)
            {

                if ((now.value * now.stack) / 5000000 >= maxvalue &&
                    now.netID != 74
                    && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1)
                {
                    return true;
                }

                return false;
            }

            void checkingifsus2(string type, int slot, Item prev, Item now)
            {
                if (type == "Inventory" && slot == 58) return;

                if (!(bool)MKLP.Config.Main.DisableNode.Use_SuspiciousDupe) return;

                if (now.stack == 255)
                {
                    if (prev.stack > 240) return;
                    if (prev.netID != now.netID) return;
                    if (prev.netID == 0 || prev.stack == 0) return;

                    if (confirmedREV()) return;

                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Split Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prev.stack} ) {prev.Name}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.stack}:{prev.netID}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 500)
                {
                    if (prev.stack > 485) return;
                    if (prev.netID != now.netID) return;
                    if (prev.netID == 0 || prev.stack == 0) return;

                    if (confirmedREV()) return;

                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.stack} ) {prev.Name}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.stack}:{prev.netID}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 1000)
                {
                    if (prev.stack > 950) return;
                    if (prev.netID != now.netID) return;
                    if (prev.netID == 0 || prev.stack == 0) return;

                    if (confirmedREV()) return;

                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.stack} ) {prev.Name}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.stack}:{prev.netID}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 5000)
                {
                    if (prev.stack > 4905) return;
                    if (prev.netID != now.netID) return;
                    if (prev.netID == 0 || prev.stack == 0) return;

                    if (confirmedREV()) return;

                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.stack} ) {prev.Name}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.stack}:{prev.netID}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 9999)
                {
                    if (prev.stack > 9905) return;
                    if (prev.netID != now.netID) return;
                    if (prev.netID == 0 || prev.stack == 0) return;

                    if (confirmedREV()) return;

                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.stack} ) {prev.Name}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.stack}:{prev.netID}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
            }

            void checkingifsus3(string type, int slot, NetItem prev, Item now)
            {
                if (type == "Inventory" && slot == 58) return;

                if (!(bool)MKLP.Config.Main.DisableNode.Use_SuspiciousDupe) return;

                if (now.stack == 255)
                {
                    if (prev.Stack > 240) return;
                    if (prev.NetId != now.netID) return;
                    if (prev.NetId == 0 || prev.Stack == 0) return;

                    if (confirmedREV()) return;
                    revertchest2();
                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Split Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prev.Stack} ) {Lang.GetItemName(prev.NetId)}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.Stack}:{prev.NetId}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 500)
                {
                    if (prev.Stack > 485) return;
                    if (prev.NetId != now.netID) return;
                    if (prev.NetId == 0 || prev.Stack == 0) return;

                    if (confirmedREV()) return;
                    revertchest2();
                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.Stack} ) {Lang.GetItemName(prev.NetId)}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.Stack}:{prev.NetId}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 1000)
                {
                    if (prev.Stack > 950) return;
                    if (prev.NetId != now.netID) return;
                    if (prev.NetId == 0 || prev.Stack == 0) return;

                    if (confirmedREV()) return;
                    revertchest2();
                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.Stack} ) {Lang.GetItemName(prev.NetId)}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.Stack}:{prev.NetId}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 5000)
                {
                    if (prev.Stack > 4905) return;
                    if (prev.NetId != now.netID) return;
                    if (prev.NetId == 0 || prev.Stack == 0) return;

                    if (confirmedREV()) return;
                    revertchest2();
                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.Stack} ) {Lang.GetItemName(prev.NetId)}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.Stack}:{prev.NetId}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                if (now.stack == 9999)
                {
                    if (prev.Stack > 9905) return;
                    if (prev.NetId != now.netID) return;
                    if (prev.NetId == 0 || prev.Stack == 0) return;

                    if (confirmedREV()) return;
                    revertchest2();
                    MKLP.PunishPlayer(MKLP_CodeType.Dupe, 0, tsplayer, "Duplicating", $"Player **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prev.Stack} ) {Lang.GetItemName(prev.NetId)}` to `( {now.stack} ) {now.Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                        $"\n- Please Check this player if they are duping", true);
                    MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prev.Stack}:{prev.NetId}] to [i/s{now.stack}:{now.netID}]" +
                        $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                }
                void revertchest2()
                {
                    if (dontrevertchest) return;
                    int checkindex = 0;
                    foreach (var revertitem in PreviousChestP[tsplayer])
                    {
                        /*
                        Main.chest[tsplayer.ActiveChest].item[i] = tsplayer.GetData<Item[]>("MKLP_PrevChestOpen")[i];
                        tsplayer.SendData(PacketTypes.ChestItem, "", tsplayer.ActiveChest, i,
                            tsplayer.GetData<Item[]>("MKLP_PrevChestOpen")[i].stack,
                            tsplayer.GetData<Item[]>("MKLP_PrevChestOpen")[i].prefix,
                            tsplayer.GetData<Item[]>("MKLP_PrevChestOpen")[i].netID);
                        */
                        Item RevertingItem = new Item();

                        RevertingItem.netDefaults(revertitem.NetId);
                        RevertingItem.stack = revertitem.Stack;
                        RevertingItem.prefix = revertitem.PrefixId;

                        Main.chest[tsplayer.ActiveChest].item[checkindex] = RevertingItem;
                        tsplayer.SendData(PacketTypes.ChestItem, "", tsplayer.ActiveChest, checkindex,
                            revertitem.Stack,
                            revertitem.PrefixId,
                            revertitem.NetId);
                        checkindex++;
                    }
                }
            }

            bool confirmedREV()
            {
                if (tsplayer.ContainsData("MKLP_Confirmed_InvRev"))
                {
                    /*
                    switch (tsplayer.GetData<int>("MKLP_Confirmed_InvRev"))
                    {
                        case 4:
                            tsplayer.SetData("MKLP_Confirmed_InvRev", 3);
                            return true;
                        case 3:
                            tsplayer.SetData("MKLP_Confirmed_InvRev", 2);
                            return true;
                        case 2:
                            tsplayer.SetData("MKLP_Confirmed_InvRev", 1);
                            return true;
                        case 1:
                            tsplayer.SetData("MKLP_Confirmed_InvRev", 0);
                            return true;
                    }
                    */
                    if (tsplayer.GetData<int>("MKLP_Confirmed_InvRev") > 0)
                    {
                        tsplayer.SetData("MKLP_Confirmed_InvRev", tsplayer.GetData<int>("MKLP_Confirmed_InvRev") - 1);
                        return true;
                    }
                }
                return false;
            }
            #endregion
            
        }



        #region [ Disable Player ]

        public static bool PlayerIsDisable(string name, string ip, string uuid)
        {
            string dummy1;
            return PlayerIsDisable(name, ip, uuid, out dummy1);
        }
        public static bool PlayerIsDisable(string name, string ip, string uuid, out string GetReason)
        {
            foreach (var get in MKLP.DisabledKey)
            {
                if (get.Key == name ||
                    get.Value.Item1 == ip ||
                    get.Value.Item2 == uuid)
                {
                    GetReason = get.Value.Item3;
                    return true;
                }
            }
            GetReason = "N/A";
            return false;
        }

        public static bool DisablePlayer(TSPlayer player, string Reason = "No Reason Specified", string executername = "Unknown", string ServerReason = "")
        {
            if (PlayerIsDisable(player.Name, player.IP, player.UUID))
            {
                return false;
            }
            else
            {
                MKLP.DisabledKey.Add(player.Name, (player.IP, player.UUID, Reason));

                player.SetData("MKLP_IsDisabled", true);

                if (player.ActiveChest != -1)
                {
                    player.ActiveChest = -1;

                    player.SendData(PacketTypes.ChestOpen, "", -1);
                }



                if (ServerReason != "")
                {
                    MKLP.Discordklp.KLPBotSendMessage_Disabled(ServerReason, player.Name, Reason);
                }

                player.SendMessage(MKLP.GetText("You have been Disable reason : ") + Reason, Microsoft.Xna.Framework.Color.Red);
                if (ServerReason == "")
                {
                    MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("Player **{0}** was Disabled by **{1}**", player.Name, executername));
                    MKLP.SendStaffMessage(MKLP.GetText("{0} disabled {1} for: {2}", executername, player.Name, Reason), Microsoft.Xna.Framework.Color.DarkRed);
                }
                else
                {
                    MKLP.SendStaffMessage(MKLP.GetText("{0} was disabled for: {1}", player.Name, Reason), Microsoft.Xna.Framework.Color.DarkRed);
                }


                return true;
            }


        }

        public enum DisableResult
        {
            Success,
            SuccessOffline,
            AlreadyDisabled,
            AlreadyEnabled,
            OfflinePermission,
            MultiplePlayerMatch,
            NotFoundOffline
        }
        public static DisableResult UnDisablePlayer(string playername, bool UsingOffline, bool specificName, out string TargetPlayerName, out IEnumerable<string> Mplayermatch, string executername = "Unknown")
        {
            TargetPlayerName = "";
            Mplayermatch = new string[] { };

            var getplayers = TSPlayer.FindByNameOrID(playername);

            if (specificName)
            {
                foreach (var player in TShock.Players)
                {
                    if (player == null) continue;
                    if (player.Name != playername) continue;
                    if (!PlayerIsDisable(player.Name, player.IP, player.UUID)) continue;

                    player.SetData("MKLP_IsDisabled", false);

                    player.SendMessage(MKLP.GetText("You're now enabled"), Microsoft.Xna.Framework.Color.Lime);

                    MKLP.SendStaffMessage(MKLP.GetText("{0} was enable by {1}", player.Name, executername), Microsoft.Xna.Framework.Color.DarkRed);

                    MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("Player **{0}** was Enabled by **{1}**", player.Name, executername));

                    MKLP.DisabledKey.Remove(player.Name);

                    TargetPlayerName = player.Name;
                    return DisableResult.Success;
                }
            } else if (getplayers.Count != 0)
            {
                if (getplayers.Count != 1)
                {
                    Mplayermatch = getplayers.Select(p => p.Name);
                    return DisableResult.MultiplePlayerMatch;
                }

                TSPlayer player = getplayers[0];

                if (!PlayerIsDisable(player.Name, player.IP, player.UUID)) return DisableResult.AlreadyEnabled;

                player.SetData("MKLP_IsDisabled", false);

                player.SendMessage(MKLP.GetText("You're now enabled"), Microsoft.Xna.Framework.Color.Lime);

                MKLP.SendStaffMessage(MKLP.GetText("{0} was enable by {1}", player.Name, executername), Microsoft.Xna.Framework.Color.DarkRed);

                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("Player **{0}** was Enabled by **{1}**", player.Name, executername));

                MKLP.DisabledKey.Remove(player.Name);

                TargetPlayerName = player.Name;
                return DisableResult.Success;
            }

            if (!PlayerIsDisable(playername, "", "")) return DisableResult.NotFoundOffline;

            if (!UsingOffline) return DisableResult.OfflinePermission;

            MKLP.DisabledKey.Remove(playername);

            MKLP.SendStaffMessage(MKLP.GetText("[Offline] {0} was enable by {1}", playername, executername), Microsoft.Xna.Framework.Color.DarkRed);

            MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("[Offline] Player **{0}** was Enabled by **{1}**", playername, executername));

            TargetPlayerName = playername;
            return DisableResult.SuccessOffline;
        }

        #endregion

        #region [ Ban ]

        public static bool OnlineBan(bool Silent, TSPlayer Player, string Reason, string Executer, DateTime Duration, bool IP = false, bool UUID = false, string banguardtype = "N/A")
        {
            var getban = TShock.Bans.RetrieveBansByIdentifier(Identifier.Account + Player.Name);

            foreach (Ban ban in getban)
            {
                if (ban.Identifier == Identifier.Name + Player.Name)
                {
                    return false;
                }
            }
            if (Player.Account != null)
            {
                foreach (Ban ban in getban)
                {
                    if (ban.Identifier == Identifier.Account + Player.Account.Name)
                    {
                        return false;
                    }
                }
            }

            string Tickets = "";

            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Name + Player.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : PlayerName\n";
            if (Player.Account != null) Tickets += $"- {TShock.Bans.InsertBan(Identifier.Account + Player.Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : Account\n";
            if (IP)
            {
                Tickets += $"- {TShock.Bans.InsertBan(Identifier.IP + Player.IP, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : IP\n";
            }
            if (UUID) Tickets += $"- {TShock.Bans.InsertBan(Identifier.UUID + Player.UUID, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : UUID\n";

            if (MKLP.DisabledKey.ContainsKey(Identifier.Name + Player.Name)) { MKLP.DisabledKey.Remove(Identifier.Name + Player.Name); }
            if (MKLP.DisabledKey.ContainsKey(Identifier.IP + Player.IP)) { MKLP.DisabledKey.Remove(Identifier.IP + Player.IP); }
            if (MKLP.DisabledKey.ContainsKey(Identifier.UUID + Player.UUID)) { MKLP.DisabledKey.Remove(Identifier.UUID + Player.UUID); }

            bool banguardused = false;
            if (banguardtype != "N/A" && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                _ = BanGuardAPI.BanPlayer(Player.UUID, banguardtype, Player.IP);
                banguardused = true;
            }

            MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔨Banned **{1}** for `{2}`" +
                (banguardused ? $"\n-# 🛡️BanGuard has been used on this one! ( category: {banguardtype} )" : "") +
                $"\n### Ban Tickets Numbers:\n" +
                Tickets +
                $"-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}", Executer, Player.Name, Reason));

            if (!Silent) TShock.Utils.Broadcast(MKLP.GetText("Player [c/3378f0:{0}] was banned!", Player.Name), Microsoft.Xna.Framework.Color.Cyan);

            MKLP.SendStaffMessage(MKLP.GetText("[MKLP] [c/008ecf:{0}] was banned by [c/008ecf:{1}]", Player.Name, Executer), Microsoft.Xna.Framework.Color.DarkCyan);

            Player.Disconnect(MKLP.GetText("You were Banned By ") + Executer +
                MKLP.GetText("\nReason: ") + Reason);

            return true;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OfflineBan(UserAccount Account, string Reason, string Executer, DateTime Duration, bool IP = false, bool UUID = false, string banguardtype = "N/A")
        {
            var getban = TShock.Bans.RetrieveBansByIdentifier(Identifier.Account + Account.Name);


            foreach (Ban ban in getban)
            {
                if (ban.Identifier == Identifier.Account + Account.Name)
                {
                    return false;
                }
            }

            string Tickets = "";

            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Name + Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : PlayerName\n";
            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Account + Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : Account\n";

            var GetIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);
            if (IP)
            {
                Tickets += $"- {TShock.Bans.InsertBan(Identifier.IP + GetIPs[GetIPs.Count() - 1], Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : IP\n";
            }
            if (UUID) Tickets += $"- {TShock.Bans.InsertBan(Identifier.UUID + Account.UUID, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : UUID\n";

            if (MKLP.DisabledKey.ContainsKey(Identifier.Name + Account.Name)) { MKLP.DisabledKey.Remove(Identifier.Name + Account.Name); }
            if (MKLP.DisabledKey.ContainsKey(Identifier.IP + GetIPs[GetIPs.Count() - 1])) { MKLP.DisabledKey.Remove(Identifier.IP + GetIPs[GetIPs.Count() - 1]); }
            if (MKLP.DisabledKey.ContainsKey(Identifier.UUID + Account.UUID)) { MKLP.DisabledKey.Remove(Identifier.UUID + Account.UUID); }

            bool banguardused = false;
            if (banguardtype != "N/A" && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                _ = BanGuardAPI.BanPlayer(Account.UUID, banguardtype, GetIPs[GetIPs.Count() - 1]);
                banguardused = true;
            }

            MKLP.SendStaffMessage(MKLP.GetText("[MKLP] Account [c/008ecf:{0}] was banned by [c/008ecf:{1}]", Account.Name, Executer), Microsoft.Xna.Framework.Color.DarkCyan);

            MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔨Banned **{1}** for `{2}`" +
                (banguardused ? $"\n-# 🛡️BanGuard has been used on this one! ( category: {banguardtype} )" : "") +
                $"\n### Ban Tickets Numbers:\n" +
                Tickets +
                $"-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}", Executer, Account.Name, Reason));

            return true;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool UnBanAccount(UserAccount Account, string Executer)
        {
            bool unbanned = false;

            string Tickets = "";

            var getIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);


            int? getban_Name = getticket(Identifier.Name + Account.Name);

            if (getban_Name != null)
            {
                if (TShock.Bans.RemoveBan((int)getban_Name, true))
                {
                    Tickets += $"- {(int)getban_Name} : PlayerName\n";
                    unbanned = true;
                }
            }


            int? getban_Account = getticket(Identifier.Account + Account.Name);

            if (getban_Account != null)
            {
                if (TShock.Bans.RemoveBan((int)getban_Account, true))
                {
                    Tickets += $"- {(int)getban_Account} : Account\n";
                    unbanned = true;
                }
            }

            int? getban_IP = getticket(Identifier.IP + getIPs[getIPs.Count() - 1]);

            if (getban_IP != null)
            {
                if (TShock.Bans.RemoveBan((int)getban_IP, true))
                {
                    Tickets += $"- {(int)getban_IP} : IP\n";
                    unbanned = true;
                }
            }


            int? getban_UUID = getticket(Identifier.UUID + Account.UUID);

            if (getban_UUID != null)
            {
                if (TShock.Bans.RemoveBan((int)getban_UUID, true))
                {
                    Tickets += $"- {(int)getban_UUID} : UUID\n";
                    unbanned = true;
                }
            }

            MKLP.SendStaffMessage(MKLP.GetText("[MKLP] Account: [c/008ecf:{1}] was unbanned by [c/008ecf:{0}]", Account.Name, Executer), Microsoft.Xna.Framework.Color.DarkCyan);

            MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText($"**{Executer}** ✅UnBan **{Account.Name}**" +
                $"\n### Ban Tickets Removed:\n" +
                Tickets));
            return unbanned;

            int? getticket(string identifier)
            {
                using var reader = TShock.DB.QueryReader($"SELECT * FROM PlayerBans WHERE Identifier=@0 AND Expiration > {DateTime.UtcNow.Ticks}", identifier);
                while (reader.Read())
                {
                    return reader.Get<int>("TicketNumber");
                }
                return null;
            }
        }

        public static bool UnBanTicketNumber(int TicketNumber, string Executer)
        {

            if (TShock.Bans.RemoveBan(TicketNumber, true))
            {
                MKLP.SendStaffMessage(MKLP.GetText("[MKLP] BanTicket: [c/008ecf:{0}] was removed by [c/008ecf:{1}]", TicketNumber, Executer), Microsoft.Xna.Framework.Color.DarkCyan);

                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** ✅Remove Ticket Ban No. **{1}**", Executer, TicketNumber));
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region [ Mute ]
        public static bool OnlineMute(bool Silent, TSPlayer Player, string Reason, string Executer, DateTime Duration)
        {
            bool MuteSuccess = false;

            if (MKLP.DBManager.AddMute(Identifier.Name + Player.Name, Duration, Reason)) MuteSuccess = true;
            if (Player.Account != null)
            {
                if (MKLP.DBManager.AddMute(Identifier.Account + Player.Account.Name, Duration, Reason)) MuteSuccess = true;
            }
            if (MKLP.DBManager.AddMute(Identifier.IP + Player.IP, Duration, Reason)) MuteSuccess = true;
            if (MKLP.DBManager.AddMute(Identifier.UUID + Player.UUID, Duration, Reason)) MuteSuccess = true;

            if (MuteSuccess)
            {
                Player.mute = true;
                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔇Muted **{1}**" + (Reason == "" ? "" : $" for ") + "{2}" +
                    $"\n-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}", Executer, Player.Name, (Reason == "" ? "" : $" for `{Reason}`")));

                if (!Silent)
                {
                    TShock.Utils.Broadcast(MKLP.GetText("[c/228f25:{0}] Muted [c/228f25:{1}]{2}", Executer, Player.Name, (Reason == "" ? "" : $" for {Reason}")), Microsoft.Xna.Framework.Color.Lime);
                }
                else
                {
                    MKLP.SendStaffMessage(MKLP.GetText("[MKLP] [c/09c100:{0}] was muted by [c/09c100:{1}]{2}", Player.Name, Executer, (Reason == "" ? "" : $" for {Reason}")), Microsoft.Xna.Framework.Color.DarkOliveGreen);
                }

                Player.SendMessage(MKLP.GetText("you have been muted for {0}" +
                    $"\nDuration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}", Reason), Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return MuteSuccess;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OnlineUnMute(bool Silent, TSPlayer Player, string Executer)
        {
            bool UnMuteSuccess = false;

            if (MKLP.DBManager.DeleteMute(Identifier.Name + Player.Name)) UnMuteSuccess = true;
            if (Player.Account != null)
            {
                if (MKLP.DBManager.DeleteMute(Identifier.Account + Player.Account.Name)) UnMuteSuccess = true;
            }
            if (MKLP.DBManager.DeleteMute(Identifier.IP + Player.IP)) UnMuteSuccess = true;
            if (MKLP.DBManager.DeleteMute(Identifier.UUID + Player.UUID)) UnMuteSuccess = true;

            if (UnMuteSuccess)
            {
                Player.mute = false;

                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔊Unmuted **{1}**", Executer, Player.Name));
                if (!Silent)
                {
                    TShock.Utils.Broadcast(MKLP.GetText("[c/228f25:{0}] Unmuted [c/228f25:{1}]", Executer, Player.Name), Microsoft.Xna.Framework.Color.Lime);
                }
                else
                {
                    MKLP.SendStaffMessage(MKLP.GetText("[MKLP] [c/09c100:{0}] was unmuted by [c/09c100:{1}]", Player.Name, Executer), Microsoft.Xna.Framework.Color.DarkOliveGreen);
                }
            }

            return UnMuteSuccess;
        }

        public static bool OfflineMute(UserAccount Account, string Reason, string Executer, DateTime Duration)
        {

            bool MuteSuccess = false;

            if (MKLP.DBManager.AddMute(Identifier.Account + Account.Name, Duration, Reason)) MuteSuccess = true;
            var GetIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);
            if (MKLP.DBManager.AddMute(Identifier.IP + GetIPs[GetIPs.Count() - 1], Duration, Reason)) MuteSuccess = true;
            if (MKLP.DBManager.AddMute(Identifier.UUID + Account.UUID, Duration, Reason)) MuteSuccess = true;

            if (MuteSuccess)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔇Muted **{1}**" + (Reason == "" ? "" : $" for ") + "{2}" +
                    $"\n-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}", Executer, Account.Name, (Reason == "" ? "" : $" for `{Reason}`")));

                MKLP.SendStaffMessage(MKLP.GetText("[MKLP] Account: [c/09c100:{0}] was muted by [c/09c100:{1}]{2}", Account.Name, Executer, (Reason == "" ? "" : $" for {Reason}")), Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return MuteSuccess;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OfflineUnMute(UserAccount Account, string Executer)
        {
            bool UnMuteSuccess = false;

            if (MKLP.DBManager.DeleteMute(Identifier.Name + Account.Name)) UnMuteSuccess = true;
            if (MKLP.DBManager.DeleteMute(Identifier.Account + Account.Name)) UnMuteSuccess = true;
            var GetIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);
            if (MKLP.DBManager.DeleteMute(Identifier.IP + GetIPs[GetIPs.Count() - 1])) UnMuteSuccess = true;
            if (MKLP.DBManager.DeleteMute(Identifier.UUID + Account.UUID)) UnMuteSuccess = true;

            if (UnMuteSuccess)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔊Unmuted **{1}**", Executer, Account.Name));

                MKLP.SendStaffMessage(MKLP.GetText("[MKLP] Account: [c/09c100:{0}] was Unmuted by [c/09c100:{1}]", Account.Name, Executer), Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return UnMuteSuccess;
        }

        #endregion

    }
}
