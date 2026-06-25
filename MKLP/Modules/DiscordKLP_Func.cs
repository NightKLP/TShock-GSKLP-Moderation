using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Interactions;
using Discord.WebSocket;
using TShockAPI;
using Terraria;
using TShockAPI.DB;
using Newtonsoft.Json;
using Terraria.ID;
using static MonoMod.InlineRT.MonoModRule;
using Microsoft.Xna.Framework;
using MKLP.Functions;

namespace MKLP.Modules
{
    public static class DiscordKLP_Func
    {
        public static readonly char S_ = DiscordKLP.S_;

        public static bool ServerModView(out string message, out Embed embed, out MessageComponent components)
        {
            #region [ ServerModView ]
            embed = null;
            components = null;
            try
            {
                string stringplayers = "";

                foreach (TSPlayer ply in TShock.Players)
                {
                    if (ply != null && ply.Active)
                    {
                        string stplayer = $"- {ply.Name} ";
                        try
                        {
                            if (ply.Account.Name == null) continue;
                            ulong getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(ply.Account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(ply.Account.Name);
                            stplayer += "[ <@!" + getuserid + "> ]";

                        }
                        catch (NullReferenceException) { }
                        stringplayers += stplayer + "\n";
                    }
                }

                if (stringplayers == "") stringplayers = MKLP.GetText("No Players Online...");


                #region { stringdefeatedbosses }
                string GetListDefeatedBoss()
                {
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
                            defeatedbosses.Add("Eye of Cthulhu", true);
                        }
                        else
                        {
                            defeatedbosses.Add("Eye of Cthulhu", false);
                        }
                    }
                    else if (NPC.downedBoss1)
                    {
                        defeatedbosses.Add("Eye of Cthulhu", true);
                    }
                    if ((bool)getenabledboss.AllowEaterOfWorlds || (bool)getenabledboss.AllowBrainOfCthulhu)
                    {
                        if (NPC.downedBoss2)
                        {
                            defeatedbosses.Add("Evil Boss", true);
                        }
                        else
                        {
                            defeatedbosses.Add("Evil Boss", false);
                        }
                    }
                    else if (NPC.downedBoss2)
                    {
                        defeatedbosses.Add("Evil Boss", true);
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
                            defeatedbosses.Add("QueenBee", true);
                        }
                        else
                        {
                            defeatedbosses.Add("QueenBee", false);
                        }
                    }
                    else if (NPC.downedQueenBee)
                    {
                        defeatedbosses.Add("QueenBee", true);
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
                            defeatedbosses.Add("Wall of Flesh", true);
                        }
                        else
                        {
                            defeatedbosses.Add("Wall of Flesh", false);
                        }
                    }
                    else if (Main.hardMode)
                    {
                        defeatedbosses.Add("Wall of Flesh", true);
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
                                defeatedbosses.Add("Destroyer", true);
                            }
                            else
                            {
                                defeatedbosses.Add("Destroyer", false);
                            }
                        }
                        else if (NPC.downedMechBoss1)
                        {
                            defeatedbosses.Add("Destroyer", true);
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
                            defeatedbosses.Add("Empress of Light", true);
                        }
                        else
                        {
                            defeatedbosses.Add("Empress of Light", false);
                        }
                    }
                    else if (NPC.downedEmpressOfLight)
                    {
                        defeatedbosses.Add("Empress of Light", true);
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
                    string result = "";
                    foreach (var boss in defeatedbosses)
                    {
                        result += $"{(boss.Value ? ":green_circle:" : ":yellow_circle:")} {boss.Key} {(boss.Value ? "[ defeated ]" : "[ enabled ]")}\n";
                    }

                    return result;
                }
                #endregion

                #region { stringdefeatedinvasion }
                string GetListDefeatedInvasion()
                {
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
                    string result = "";

                    foreach (var invasion in defeatedinvasion)
                    {
                        result += $"- {invasion.Key}\n";
                    }

                    return result;
                }
                #endregion

                #region { stringactivities }
                string GetListActivities()
                {
                    string result = "";

                    if (Main.bloodMoon) result += "- Blood Moon \n";

                    if (Main.eclipse) result += "- Solar Eclipse \n";

                    if (Main.invasionType == 1) result += $"- Goblin Army [ %{Main.invasionProgress} ]\n";

                    if (Main.invasionType == 2) result += $"- Frost Legion [ %{Main.invasionProgress} ]\n";

                    if (Main.invasionType == 3) result += $"- Pirate Invasion [ %{Main.invasionProgress} ]\n";

                    if (Main.invasionType == 4) result += $"- Martians [ %{Main.invasionProgress} ]\n";

                    if (Main.pumpkinMoon) result += $"- Pumkin Moon [ wave {NPC.waveNumber} ]\n";

                    if (Main.snowMoon) result += $"- Frost Moon [ wave {NPC.waveNumber} ]\n";

                    if (Terraria.GameContent.Events.DD2Event.Ongoing) result += $"- Old One's Army [ Wave {Main.invasionProgressWave} ]\n";

                    Dictionary<int, string> bosses = new();

                    bosses.Add(50, "- King Slime"); // King Slime
                    bosses.Add(4, "- Eye of Cthulhu"); // Eye of Cthulu

                    bosses.Add(13, "- Eater of Worlds"); // Eater of Worlds
                    bosses.Add(266, "- Brain of Cthulhu"); // Brain of Cthulu

                    bosses.Add(222, "- Queen Bee"); // Queen Bee
                    bosses.Add(35, "- Skeletron"); // Skeletron
                    bosses.Add(668, "- Deerclops"); // Deerclops
                    bosses.Add(113, "- Wall of Flesh"); // Wall of Flesh
                    bosses.Add(657, "- Queen Slime"); // Queen Slime

                    bosses.Add(125, "- Retinazer"); // Retinazer
                    bosses.Add(126, "- Spazmatism"); // Spazmatism
                    bosses.Add(134, "- The Destroyer"); // The Destroyer
                    bosses.Add(127, "- Skeletron Prime"); // Skeletron Prime

                    bosses.Add(262, "- Plantera"); // Plantera
                    bosses.Add(245, "- Golem"); // Golem

                    bosses.Add(636, "- Empress of Light"); // Empress Of Light

                    bosses.Add(370, "- Duke Fishron"); // Duke Fishron
                    bosses.Add(439, "- Lunatic Cultist");// Lunatic Cultist
                    bosses.Add(396, "- Moon Lord"); // Moon Lord

                    foreach (var npc in Main.npc)
                    {
                        if (!npc.active) continue;
                        if (bosses.ContainsKey(npc.netID))
                        {
                            result += $"- {bosses[npc.netID]} [ {npc.life}/{npc.lifeMax}:heart: ]\n";
                        }
                    }

                    return result;
                }

                #endregion

                string defeatedbosses = GetListDefeatedBoss();
                if (defeatedbosses == "") defeatedbosses = MKLP.GetText("No Bosses Defeated...");

                string defeatedinvasion = GetListDefeatedInvasion();
                if (defeatedinvasion == "") defeatedinvasion = MKLP.GetText("no Invasions Completed...");

                string OngoingActivity = GetListActivities();
                if (OngoingActivity == "") OngoingActivity = MKLP.GetText("Nothing is Happening...");

                string reportlist = "";

                foreach (MKLP_Report report in MKLP.DBManager.GetReportList(4))
                {
                    reportlist +=
                        $"**'{report.From}' {MKLP.GetText("Report")}** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                        $"\n> **{MKLP.GetText("ID")}:** {report.ID}" +
                        $"\n> **{MKLP.GetText("Location:")}** `{report.Location}`" +
                        $"\n> **{MKLP.GetText("Players online during report:")} ** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                        $"\n> " +
                        $"\n> **{MKLP.GetText("target:")}** {(report.Target == "" ? MKLP.GetText("none") : report.Target)}" +
                        $"\n> **{MKLP.GetText("Message:")}** {report.Message}\n\n";
                }

                embed = new EmbedBuilder()
                        .WithTitle(MKLP.GetText("Server Moderation Menu"))
                        .WithDescription($"## 📑 {MKLP.GetText("Latest Report")}" +
                        $"\n{(reportlist == "" ? MKLP.GetText("no latest reports today...") : reportlist)}")
                        .WithColor(DiscordKLP.EmbedColor)
                        .WithFields(
                            new EmbedFieldBuilder()
                                .WithName($"{MKLP.GetText("Online Players")} [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                .WithValue(stringplayers),
                            new EmbedFieldBuilder()
                                .WithName(MKLP.GetText("Bosses"))
                                .WithValue(defeatedbosses)
                                .WithIsInline(true),
                            new EmbedFieldBuilder()
                                .WithName(MKLP.GetText("Invasions Defeated"))
                                .WithValue(defeatedinvasion)
                                .WithIsInline(true),
                            new EmbedFieldBuilder()
                                .WithName(MKLP.GetText("Activities"))
                                .WithValue(OngoingActivity)
                        ).Build();

                message = "";
                if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                {

                    var menuBuilder = new SelectMenuBuilder()
                    .WithPlaceholder(MKLP.GetText("Select a Player"))
                    .WithCustomId("MKLP_SendMsg_PlayerModView_Main".Replace('_', S_))
                    .WithMinValues(1)
                    .WithMaxValues(1);

                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player == null) continue;
                        if (player.Name == "" ||
                            player.Name.Replace("*", "") == "" ||
                            player.Name == " ") continue;
                        if (!player.IsLoggedIn) continue;
                        menuBuilder.AddOption(player.Name, player.Account.Name, $"{MKLP.GetText("Account:")} {player.Account.Name}");
                    }

                    components = new ComponentBuilder()
                        .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0)
                        .WithButton(MKLP.GetText("Search Account"), "MKLP_SendModal_AccSearch".Replace('_', S_), ButtonStyle.Primary, row: 1)
                        .WithSelectMenu(menuBuilder, row: 2)
                        .Build();
                }
                else
                {
                    components = new ComponentBuilder()
                        .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0)
                        .WithButton(MKLP.GetText("Search Account"), "MKLP_SendModal_AccSearch".Replace('_', S_), ButtonStyle.Primary, row: 1)
                        .Build();
                }
                return true;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                message = MKLP.GetText("Something went wrong!");
                return false;
            }
            #endregion
        }

        public static bool ViewPlayerInventory(string type, string target, out string message, out Embed embed, out MessageComponent buttons)
        {
            #region [ ViewPlayerInventory ]
            buttons = null;
            embed = null;
            try
            {
                if (!TryGetPlrData(target, out PLR_Data getplrdata))
                {
                    message = MKLP.GetText("Invalid Target!");
                    return true;
                }
                if (!getplrdata.HasInvData)
                {
                    message = MKLP.GetText("Unable to get Inventory data from {0}", target);
                    return true;
                }

                string EmbedDescription = "```\n";

                #region { inventory type }
                switch (type)
                {
                    case "Inventory":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "XXX" + target, ButtonStyle.Primary, disabled: true)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetInventoryText(false);
                            break;
                        }
                    case "Equipment":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "XXX" + target, ButtonStyle.Primary, disabled: true)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetArmorEquipText(false);
                            break;
                        }
                    case "PiggyBank":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "XXX" + target, ButtonStyle.Secondary, row: 2, disabled: true)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetPiggyBankText(false);
                            break;
                        }
                    case "Safe":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "XXX" + target, ButtonStyle.Secondary, row: 2, disabled: true)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetSafeText(false);
                            break;
                        }
                    case "DefForge":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "XXX" + target, ButtonStyle.Secondary, row: 2, disabled: true)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetDefenderForgeText(false);
                            break;
                        }
                    case "VoidVault":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "XXX" + target, ButtonStyle.Secondary, row: 2, disabled: true)
                                .WithButton(MKLP.GetText("Inventory Logs"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 3)
                                .Build();
                            EmbedDescription += getplrdata.InvData.GetVoidVaultText(false);
                            break;
                        }
                    case "InventoryLogs":
                        {
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Inventory"), "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Equipment"), "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Piggy Bank"), "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Safe"), "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Defender Forge"), "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Void Vault"), "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + target, ButtonStyle.Secondary, row: 2)
                                .WithButton(MKLP.GetText("Inventory Logs"), "XXX" + target, ButtonStyle.Secondary, row: 3, disabled: true)
                                .Build();
                            int count = 0;
                            foreach (var invlog in LogKLP.GetLog_Inventory(LogKLP.GetPath(LogKLP.LogPath_Inventory, LogKLP.Currentlogfile), target))
                            {
                                count++;
                                if (count > 20) break;
                                EmbedDescription += $"{invlog.Item1}| {invlog.Item5} {invlog.Item4} | {invlog.Item2.ItemTagText()} => {invlog.Item3.ItemTagText()}\n";
                            }
                            break;
                        }
                }
                #endregion

                message = "";
                embed = new EmbedBuilder()
                    .WithTitle($"{(getplrdata.IsAccount ? MKLP.GetText("Account") : MKLP.GetText("Player"))} [ {target} ] {type}")
                    .WithDescription(EmbedDescription + "\n```")
                    .WithColor(DiscordKLP.EmbedColor)
                    .Build();

                return true;
            } catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                message = MKLP.GetText("Something went wrong!");
                return false;
            }
            #endregion
        }

        public static bool PlayerModView(string type, string target, out string message, out Embed embed, out MessageComponent buttons)
        {
            #region [ PlayerModView ]
            buttons = null;
            embed = null;
            try
            {

                if (!TryGetPlrData(target, out PLR_Data getplrdata))
                {
                    message = MKLP.GetText($"Invalid Target!");
                    return true;
                }

                switch (type)
                {
                    case "Main":
                        {
                            message = "";

                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + target, ButtonStyle.Secondary)
                                .WithButton(MKLP.GetText("Main"), "XXX" + target, ButtonStyle.Primary, disabled: true)
                                .WithButton(MKLP.GetText("Reports from them"), "MKLP_EditMsg_PlayerModView_Report1_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("{0} Reports", target), "MKLP_EditMsg_PlayerModView_Report2_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("View Inventory"), "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("Ban Player"), "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Disable Player"), "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Mute Player"), "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .Build();

                            if (!getplrdata.GetGroupName(out string GetGroupName)) { GetGroupName = "N/A"; }

                            embed = new EmbedBuilder()
                                .WithTitle($"{(getplrdata.IsAccount ? MKLP.GetText("Account") : MKLP.GetText("Player"))} [ {getplrdata.IDOrIndex} ] {getplrdata.GetName()}")
                                .WithDescription($"**{MKLP.GetText("Health:")}** `{getplrdata.GetPlayerData_Health()}❤️`" +
                                $"\n**{MKLP.GetText("Mana:")}** `{getplrdata.GetPlayerData_Mana()}⭐`" +
                                $"\n" +
                                $"\n**{MKLP.GetText("Coordinates:")}** {(getplrdata.GetPlayerData_Position(out Vector2 GetPos) ? $"`{GetPos.X}, {GetPos.Y}` `x y`" : "`N/A`")}" +
                                $"\n" +
                                $"\n**{MKLP.GetText("Misc")}**" +
                                (getplrdata.GetAccountID(out int GetAccountID) ? $"\n> **{MKLP.GetText("AccountID:")}** `{GetAccountID}`" : "") +
                                $"\n> **{MKLP.GetText("Group Name:")}** `{GetGroupName}`" +
                                (getplrdata.GetLastAccessed(out string GetLastAccessedTime, out string GetLastAccessedSince)  ? $"\n> **{MKLP.GetText("Last Accessed:")}** `{GetLastAccessedTime}` {GetLastAccessedSince}" : "") +
                                (getplrdata.GetRegisteredSince(out string GetRegisteredTime, out string GetRegisteredSince) ? $"\n> **{MKLP.GetText("Registered:")}** `{GetRegisteredTime}` {GetRegisteredSince}": "") +
                                $"\n" +
                                (getplrdata.GetIsLoggedIn(out bool GetIsLoggedIn) ? $"\n`{MKLP.GetText("LoggedIn:")} {(GetIsLoggedIn ? "✅" : "❌")}`" : "") +
                                $"`{MKLP.GetText("Disabled:")} {(getplrdata.IsDisabled ? "✅" : "❌")}` " +
                                $"`{MKLP.GetText("Muted:")} {(getplrdata.IsMuted ? "✅" : "❌")}`")
                                .WithColor(DiscordKLP.EmbedColor)
                                .Build();

                            return true;
                        }
                    case "Report1":
                        {
                            message = "";

                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerModView_Report1_".Replace('_', S_) + target, ButtonStyle.Secondary)
                                .WithButton(MKLP.GetText("Main"), "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Reports from them"), "XXX" + target, ButtonStyle.Primary, row: 1, disabled: true)
                                .WithButton(MKLP.GetText("{0} Reports", target), "MKLP_EditMsg_PlayerModView_Report2_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("View Inventory"), "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("Ban Player"), "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Disable Player"), "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Mute Player"), "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .Build();

                            string reportlist = "";

                            foreach (MKLP_Report report in MKLP.DBManager.GetReportList(5, target: target))
                            {
                                reportlist +=
                                    $"**'{report.From}' {MKLP.GetText("Report")}** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                    $"\n> **{MKLP.GetText("ID:")}** {report.ID}" +
                                    $"\n> **{MKLP.GetText("Location:")}** `{report.Location}`" +
                                    $"\n> **{MKLP.GetText("Players online during report:")}** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                    $"\n> " +
                                    $"\n> **{MKLP.GetText("target:")}** {(report.Target == "" ? MKLP.GetText("none") : report.Target)}" +
                                    $"\n> **{MKLP.GetText("Message:")}** {report.Message}\n\n";
                            }

                            if (reportlist == "") reportlist = MKLP.GetText("No reports...");

                            embed = new EmbedBuilder()
                                .WithTitle($"{MKLP.GetText("Account")} [ {target} ]")
                                .WithDescription(
                                    $"## {MKLP.GetText("Reports from them")}\n\n" +
                                    reportlist
                                ).WithColor(DiscordKLP.EmbedColor)
                                .Build();

                            return true;
                        }
                    case "Report2":
                        {
                            message = "";
                            buttons = new ComponentBuilder()
                                .WithButton(MKLP.GetText("Refresh"), "MKLP_EditMsg_PlayerModView_Report2_".Replace('_', S_) + target, ButtonStyle.Secondary)
                                .WithButton(MKLP.GetText("Main"), "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + target, ButtonStyle.Primary)
                                .WithButton(MKLP.GetText("Reports from them"), "MKLP_EditMsg_PlayerModView_Report1_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("{0} Reports", target), "XXX", ButtonStyle.Primary, row: 1, disabled: true)
                                .WithButton(MKLP.GetText("View Inventory"), "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + target, ButtonStyle.Primary, row: 1)
                                .WithButton(MKLP.GetText("Ban Player"), "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Disable Player"), "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .WithButton(MKLP.GetText("Mute Player"), "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + target, ButtonStyle.Danger, row: 2)
                                .Build();

                            string reportlist = "";

                            foreach (MKLP_Report report in MKLP.DBManager.GetReportList(5, from: target))
                            {
                                reportlist +=
                                    $"**'{report.From}' {MKLP.GetText("Report")}** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                    $"\n> **{MKLP.GetText("ID:")}** {report.ID}" +
                                    $"\n> **{MKLP.GetText("Location:")}** `{report.Location}`" +
                                    $"\n> **{MKLP.GetText("Players online during report:")}** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                    $"\n> " +
                                    $"\n> **{MKLP.GetText("target:")}** {(report.Target == "" ? MKLP.GetText("none") : report.Target)}" +
                                    $"\n> **{MKLP.GetText("Message:")}** {report.Message}\n\n";
                            }

                            if (reportlist == "") reportlist = "No reports...";

                            embed = new EmbedBuilder()
                                .WithTitle($"{MKLP.GetText("Account")} [ {target} ]")
                                .WithDescription(
                                    $"## ( {target} ) {MKLP.GetText("Reports")}\n\n" +
                                    reportlist
                                ).WithColor(DiscordKLP.EmbedColor)
                                .Build();

                            return true;
                        }
                }
                message = MKLP.Text_NA;
                return false;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                message = MKLP.GetText("Something went wrong!");
                return false;
            }
            #endregion
        }

        public static bool AccountSearch(int page, string search, out string message, out Embed embed, out MessageComponent components)
        {
            #region [ AccountSearch ]
            embed = null;
            components = null;
            try
            {
                if (search.Contains(S_))
                {
                    message = MKLP.GetText("Invalid Search!");
                    return false;
                }

                UserAccount[] AccountList = TShock.UserAccounts.GetUserAccountsByName(search).ToArray();

                string[] textlist = AccountList.Select(acc => $"`<{acc.ID}>` **{acc.Name}**\n").ToArray();

                StringPageKLP txtpage = new(10, textlist)
                {
                    DisplayText = MKLP.GetText("Searching Account [ {0} ]", search) + " ({0}/{1})",
                    EmptyText = MKLP.GetText("no accounts found...")
                };

                message = "";

                var SMenu = new SelectMenuBuilder()
                    .WithPlaceholder(MKLP.GetText("Select a Account"))
                    .WithCustomId("MKLP_SendMsg_PlayerModView_Main".Replace('_', DiscordKLP.S_))
                    .WithMinValues(1)
                    .WithMaxValues(1);

                foreach (UserAccount getaccount in AccountList)
                {
                    SMenu.AddOption(getaccount.Name, getaccount.Name);
                }

                embed = new EmbedBuilder()
                    .WithTitle(txtpage.GetDisplayText(page))
                    .WithDescription(txtpage.GetText(page))
                    .WithColor(DiscordKLP.EmbedColor)
                    .Build();

                var getcomponent = new ComponentBuilder()
                    .WithButton("⏪", $"MKLP_EditMsg_AccSearch_{search}_{1}_low".Replace('_', DiscordKLP.S_), ButtonStyle.Primary, disabled: page <= 1, row: 0)
                    .WithButton("◀️", $"MKLP_EditMsg_AccSearch_{search}_{page - 1}".Replace('_', DiscordKLP.S_), ButtonStyle.Primary, disabled: page <= 1, row: 0)
                    .WithButton("🔄️", $"MKLP_EditMsg_AccSearch_{search}_{page}".Replace('_', DiscordKLP.S_), ButtonStyle.Primary, row: 0)
                    .WithButton("▶️", $"MKLP_EditMsg_AccSearch_{search}_{page + 1}".Replace('_', DiscordKLP.S_), ButtonStyle.Primary, disabled: txtpage.IsMaxPage(page), row: 0)
                    .WithButton("⏩", $"MKLP_EditMsg_AccSearch_{search}_{txtpage.GetMaxPage()}_max".Replace('_', DiscordKLP.S_), ButtonStyle.Primary, disabled: txtpage.IsMaxPage(page), row: 0)
                    ;
                if (AccountList.Length >= 1) { getcomponent.WithSelectMenu(SMenu, row: 1); }

                components = getcomponent.Build();

                return true;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                message = MKLP.GetText("something went wrong!");
                return false;
            }
            #endregion
        }

        public static bool TryGetPlrData(string target, out PLR_Data data)
        {
            data = new();
            try
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null || !player.Active) continue;

                    if (player.Name == target)
                    {
                        data.Init(player);
                        return true;
                    }
                }

                UserAccount getacc = TShock.UserAccounts.GetUserAccountByName(target);
                if (getacc == null)
                {
                    return false;
                }

                data.Init(getacc);
                return true;
            } catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
                return false;
            }
        }

    }
    public class PLR_Data
    {
        public bool IsAccount { get; private set; }
        public int IDOrIndex { get; private set; }

        public bool IsMuted { get; private set; }
        public bool IsDisabled { get; private set; }
        public bool HasInvData { get; private set; }
        public InvData_PLR InvData = new();

        private TSPlayer tsplayer = null;
        private UserAccount account = null;

        public PLR_Data() { }

        public void Init(TSPlayer player)
        {
            IsAccount = false;
            IDOrIndex = player.Index;

            tsplayer = player;

            HasInvData = InvData.Init(player);

            IsMuted = player.mute;
            IsDisabled = ManagePlayer.PlayerIsDisable(GetActualName(player), player.IP, player.UUID);
        }
        public void Init(UserAccount account)
        {
            var GetIPs = JsonConvert.DeserializeObject<List<string>>(account.KnownIps);

            IsAccount = true;
            IDOrIndex = account.ID;

            this.account = account;

            HasInvData = InvData.Init(account.ID);

            IsMuted = MuteKLP.PlayerIsMuted(account.Name).muted;
            IsDisabled = ManagePlayer.PlayerIsDisable(account.Name, GetIPs[GetIPs.Count() - 1], account.UUID);
        }
        public string GetName()
        {
            if (IsAccount)
            {
                if (account == null) return MKLP.Text_NA;
                return account.Name;
            }
            else
            {
                if (tsplayer == null) return MKLP.Text_NA;
                return tsplayer.Name;
            }
            return MKLP.Text_NA;
        }
        #region ==[ Get ]==
        public bool GetGroupName(out string GroupName)
        {
            GroupName = "";
            string UTC = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");
            if (IsAccount)
            {
                if (account == null) return false;
                GroupName = account.Group;
                return true;
            }
            else
            {
                if (tsplayer == null) return false;
                GroupName = tsplayer.Group.Name;
                return true;
            }
            return false;
        }
        public bool GetAccountID(out int AccountID)
        {
            AccountID = -1;
            if (IsAccount)
            {
                if (account == null) return false;
                AccountID = account.ID;
                return true;
            }
            else
            {
                if (tsplayer == null) return false;
                if (tsplayer.Account == null) return false;
                AccountID = tsplayer.Account.ID;
                return true;
            }
            return false;
        }
        public bool GetLastAccessed(out string Time, out string Since)
        {
            Time = "";
            Since = "";
            string UTC = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");
            if (IsAccount)
            {
                if (account == null) return false;
                Time = $"{account.LastAccessed} UTC{UTC}";
                Since = GetSince(DateTime.Parse(account.LastAccessed));
                return true;
            }
            else
            {
                if (tsplayer == null) return false;
                if (tsplayer.Account == null) return false;
                Time = $"{tsplayer.Account.LastAccessed} UTC{UTC}";
                Since = GetSince(DateTime.Parse(tsplayer.Account.LastAccessed));
                return true;
            }
            return false;
        }
        public bool GetRegisteredSince(out string Time, out string Since)
        {
            Time = "";
            Since = "";
            string UTC = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");
            if (IsAccount)
            {
                if (account == null) return false;
                Time = $"{account.Registered} UTC{UTC}";
                Since = GetSince(DateTime.Parse(account.Registered));
                return true;
            }
            else
            {
                if (tsplayer == null) return false;
                if (tsplayer.Account == null) return false;
                Time = $"{tsplayer.Account.Registered} UTC{UTC}";
                Since = GetSince(DateTime.Parse(tsplayer.Account.Registered));
                return true;
            }
            return false;
        }
        public bool GetIsLoggedIn(out bool IsLoggedIn)
        {
            IsLoggedIn = false;
            if (IsAccount)
            {
                return false;
            }
            else
            {
                if (tsplayer == null) return false;
                IsLoggedIn = tsplayer.IsLoggedIn;
                return true;
            }
        }

        #region [ Player Data ]
        public string GetPlayerData_Health()
        {
            if (IsAccount)
            {
                if (account == null) return MKLP.Text_NA;
                if (InvData_PLR.GetPlayerData(account.ID, out PlayerData data))
                {
                    return $"{data.health}/{data.maxHealth}";
                }
                return MKLP.Text_NA;
            }
            else
            {
                if (tsplayer == null) return MKLP.Text_NA;
                return $"{tsplayer.TPlayer.statLife}/{tsplayer.TPlayer.statLifeMax2}";
            }
        }
        public string GetPlayerData_Mana()
        {
            if (IsAccount)
            {
                if (account == null) return MKLP.Text_NA;
                if (InvData_PLR.GetPlayerData(account.ID, out PlayerData data))
                {
                    return $"{data.mana}/{data.maxMana}";
                }
                return MKLP.Text_NA;
            }
            else
            {
                if (tsplayer == null) return MKLP.Text_NA;
                return $"{tsplayer.TPlayer.statMana}/{tsplayer.TPlayer.statManaMax2}";
            }
        }
        public bool GetPlayerData_Position(out Vector2 position)
        {
            position = Vector2.Zero;
            if (IsAccount)
            {
                if (account == null) return false;
                var GetIPs = JsonConvert.DeserializeObject<List<string>>(account.KnownIps);
                position = TShock.RememberedPos.GetLeavePos(account.Name, GetIPs[GetIPs.Count() - 1]);
                return true;
            }
            else
            {
                if (tsplayer == null) return false;
                position = new(tsplayer.TileX, tsplayer.TileY);
                return true;
            }
        }
        #endregion

        #endregion

        #region ==[ Sub Func ]==
        public static string GetActualName(TSPlayer player)
        {
            if (player.Account != null)
            {
                return player.Account.Name;
            }

            return player.Name;
        }
        private static string GetSince(DateTime Since)
        {
            TimeSpan getresult = (DateTime.UtcNow - Since);

            if (getresult.TotalDays >= 1)
            {
                return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays >= 2 ? "Days" : "Day")} ago";
            }
            if (getresult.TotalHours >= 1)
            {
                return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours >= 2 ? "Hours" : "Hour")} ago";
            }
            if (getresult.TotalMinutes >= 1)
            {
                return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes >= 2 ? "Minutes" : "Minute")} ago";
            }
            if (getresult.TotalSeconds >= 1)
            {
                return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")} ago";
            }
            if (getresult.TotalMilliseconds >= 1)
            {
                return $"{Math.Floor(getresult.TotalMilliseconds)}{(getresult.TotalMilliseconds >= 2 ? "Milliseconds" : "Millisecond")} ago";
            }
            return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")}";
        }

        #endregion
    }
}
