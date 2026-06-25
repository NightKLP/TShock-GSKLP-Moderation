//Microsoft
using Terraria.ID;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
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

        public static bool DisablePlayer(TSPlayer player, string Reason = "No Reason Specified", string executername = "Unknown", string ServerReason = "", string ServerLog = "")
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
                    MKLP.Discordklp.KLPBotSendMessage_Disabled(ServerReason, player.Name, Reason, ServerLog);
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
                    TargetPlayerName = "";
                    return DisableResult.MultiplePlayerMatch;
                }

                TSPlayer player = getplayers[0];

                if (!PlayerIsDisable(player.Name, player.IP, player.UUID))
                {
                    TargetPlayerName = player.Name;
                    return DisableResult.AlreadyEnabled;
                }

                player.SetData("MKLP_IsDisabled", false);

                player.SendMessage(MKLP.GetText("You're now enabled"), Microsoft.Xna.Framework.Color.Lime);

                MKLP.SendStaffMessage(MKLP.GetText("{0} was enable by {1}", player.Name, executername), Microsoft.Xna.Framework.Color.DarkRed);

                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("Player **{0}** was Enabled by **{1}**", player.Name, executername));

                MKLP.DisabledKey.Remove(player.Name);

                TargetPlayerName = player.Name;
                return DisableResult.Success;
            }

            if (!PlayerIsDisable(playername, "", ""))
            {
                TargetPlayerName = playername;
                return DisableResult.NotFoundOffline;
            }

            if (!UsingOffline)
            {
                TargetPlayerName = "";
                return DisableResult.OfflinePermission;
            }

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

            if (MuteKLP.AddMute(Identifier.Name + Player.Name, Duration, Reason)) MuteSuccess = true;
            if (Player.Account != null)
            {
                if (MuteKLP.AddMute(Identifier.Account + Player.Account.Name, Duration, Reason)) MuteSuccess = true;
            }
            if (MuteKLP.AddMute(Identifier.IP + Player.IP, Duration, Reason)) MuteSuccess = true;
            if (MuteKLP.AddMute(Identifier.UUID + Player.UUID, Duration, Reason)) MuteSuccess = true;

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

            if (MuteKLP.DeleteMute(Identifier.Name + Player.Name)) UnMuteSuccess = true;
            if (Player.Account != null)
            {
                if (MuteKLP.DeleteMute(Identifier.Account + Player.Account.Name)) UnMuteSuccess = true;
            }
            if (MuteKLP.DeleteMute(Identifier.IP + Player.IP)) UnMuteSuccess = true;
            if (MuteKLP.DeleteMute(Identifier.UUID + Player.UUID)) UnMuteSuccess = true;

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

            if (MuteKLP.AddMute(Identifier.Account + Account.Name, Duration, Reason)) MuteSuccess = true;
            var GetIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);
            if (MuteKLP.AddMute(Identifier.IP + GetIPs[GetIPs.Count() - 1], Duration, Reason)) MuteSuccess = true;
            if (MuteKLP.AddMute(Identifier.UUID + Account.UUID, Duration, Reason)) MuteSuccess = true;

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

        public static bool OfflineUnMute(UserAccount Account, string Executer, bool deletefully = false)
        {
            bool UnMuteSuccess = false;
            var GetIPs = JsonConvert.DeserializeObject<List<string>>(Account.KnownIps);

            if (deletefully)
            {
                if (MuteKLP.DeleteMute(Identifier.Name + Account.Name)) UnMuteSuccess = true;
                if (MuteKLP.DeleteMute(Identifier.Account + Account.Name)) UnMuteSuccess = true;
                if (MuteKLP.DeleteMute(Identifier.IP + GetIPs[GetIPs.Count() - 1])) UnMuteSuccess = true;
                if (MuteKLP.DeleteMute(Identifier.UUID + Account.UUID)) UnMuteSuccess = true;
            } else
            {
                if (MuteKLP.DeleteMuteSafe(Identifier.Name + Account.Name)) UnMuteSuccess = true;
                if (MuteKLP.DeleteMuteSafe(Identifier.Account + Account.Name)) UnMuteSuccess = true;
                if (MuteKLP.DeleteMuteSafe(Identifier.IP + GetIPs[GetIPs.Count() - 1])) UnMuteSuccess = true;
                if (MuteKLP.DeleteMuteSafe(Identifier.UUID + Account.UUID)) UnMuteSuccess = true;
            }
            if (UnMuteSuccess)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("**{0}** 🔊Unmuted **{1}**", Executer, Account.Name));

                MKLP.SendStaffMessage(MKLP.GetText("[MKLP] Account: [c/09c100:{0}] was Unmuted by [c/09c100:{1}]", Account.Name, Executer), Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return UnMuteSuccess;
        }

        #endregion


        #region [[ Player Max Summons ]]

        private enum Summon_Armor
        {
            Head,
            Body,
            Leggings
        }
        public static int GetPlayerMaxSummons(TSPlayer player)
        {
            #region code
            int maxsummons = 1;
            //( ItemArmorID | ArmorType | IncreaseSummonBy )
            (int, Summon_Armor, int)[] summonarmor =
            {
                (ItemID.FlinxFurCoat, Summon_Armor.Body, 1),

                (ItemID.ObsidianShirt, Summon_Armor.Body, 1),

                (ItemID.BeeHeadgear, Summon_Armor.Head, 1),
                (ItemID.BeeBreastplate, Summon_Armor.Body, 1),

                (ItemID.SpiderMask, Summon_Armor.Head, 1),
                (ItemID.SpiderBreastplate, Summon_Armor.Body, 1),
                (ItemID.SpiderGreaves, Summon_Armor.Leggings, 1),

                (ItemID.AncientBattleArmorShirt, Summon_Armor.Body, 1),
                (ItemID.AncientBattleArmorPants, Summon_Armor.Leggings, 1),

                (ItemID.HallowedHood, Summon_Armor.Head, 1),
                (ItemID.AncientHallowedHood, Summon_Armor.Head, 1),

                (ItemID.TikiMask, Summon_Armor.Head, 1),
                (ItemID.TikiShirt, Summon_Armor.Body, 1),
                (ItemID.TikiPants, Summon_Armor.Leggings, 1),

                (ItemID.SpookyHelmet, Summon_Armor.Head, 1),
                (ItemID.SpookyBreastplate, Summon_Armor.Body, 2),
                (ItemID.SpookyLeggings, Summon_Armor.Leggings, 1),

                (ItemID.StardustHelmet, Summon_Armor.Head, 1),
                (ItemID.StardustBreastplate, Summon_Armor.Body, 2),
                (ItemID.StardustLeggings, Summon_Armor.Leggings, 2),
            };
            //( ArmorSet [ head | body | leggings ] | IncreaseSummonBy )
            ((int[], int[], int[]), int)[] setbonussummonarmor =
            {
                ((new int[]{ ItemID.AncientHallowedHood, ItemID.HallowedHood }, new int[]{ ItemID.AncientHallowedPlateMail, ItemID.HallowedPlateMail }, new int[]{ ItemID.AncientHallowedGreaves, ItemID.HallowedGreaves }), 2),
                ((new int[]{ ItemID.TikiMask }, new int[]{ ItemID.TikiShirt }, new int[]{ ItemID.TikiPants }), 1)
            };

            #region =[ Accessory Check ]=
            bool has_PygmyNecklace = false;
            bool has_NecromanticScroll = false;
            bool has_PapyrusScarab = false;
            for (int i = 0; i < player.TPlayer.armor.Length; i++)
            {
                Item item = player.TPlayer.armor[i];

                if (item.type == ItemID.PygmyNecklace)
                {
                    has_PygmyNecklace = true;
                }
                else if (item.type == ItemID.NecromanticScroll)
                {
                    has_NecromanticScroll = true;
                }
                else if (item.type == ItemID.PapyrusScarab)
                {
                    has_PapyrusScarab = true;
                }
            }
            #endregion

            #region =[ Accessory & Armor Caculate ]=

            Item head = player.TPlayer.armor[0];
            Item body = player.TPlayer.armor[1];
            Item leggings = player.TPlayer.armor[2];

            #region [ Individual Armor Piece ]
            foreach (var getarmor in summonarmor)
            {
                switch (getarmor.Item2)
                {
                    case Summon_Armor.Head:
                        {
                            if (head.type == getarmor.Item1)
                            {
                                maxsummons += getarmor.Item3;
                            }
                            break;
                        }
                    case Summon_Armor.Body:
                        {
                            if (body.type == getarmor.Item1)
                            {
                                maxsummons += getarmor.Item3;
                            }
                            break;
                        }
                    case Summon_Armor.Leggings:
                        {
                            if (leggings.type == getarmor.Item1)
                            {
                                maxsummons += getarmor.Item3;
                            }
                            break;
                        }
                }
            }
            #endregion

            #region [ Armor Set ]
            foreach (var getarmorset in setbonussummonarmor)
            {
                if (getarmorset.Item1.Item1.Contains(head.type) && getarmorset.Item1.Item2.Contains(head.type) && getarmorset.Item1.Item3.Contains(head.type))
                {
                    maxsummons += getarmorset.Item2;
                }
            }
            #endregion

            if (has_PygmyNecklace) { maxsummons += 1; }
            if (has_NecromanticScroll) { maxsummons += 1; }
            if (has_PapyrusScarab) { maxsummons += 1; }
            #endregion

            if (player.TPlayer != null && player.TPlayer.buffType != null)
            {
                for (int i = 0; i < Terraria.Player.maxBuffs; i++)
                {
                    if (player.TPlayer.buffType[i] == BuffID.Summoning)
                    {
                        maxsummons += 1;
                        break;
                    }
                }
                for (int i = 0; i < Terraria.Player.maxBuffs; i++)
                {
                    if (player.TPlayer.buffType[i] == BuffID.Bewitched)
                    {
                        maxsummons += 1;
                        break;
                    }
                }
            }

            return maxsummons;
            #endregion
        }
        public static int GetPlayerMaxSentry(TSPlayer player)
        {
            #region code
            int maxsentry= 1;
            //( ItemArmorID | ArmorType | IncreaseSummonBy )
            (int, Summon_Armor, int)[] summonarmor =
            {
                (ItemID.SquireGreatHelm, Summon_Armor.Head, 1),
                (ItemID.MonkBrows, Summon_Armor.Head, 1),
                (ItemID.HuntressWig, Summon_Armor.Head, 1),
                (ItemID.ApprenticeHat, Summon_Armor.Head, 1),

                (ItemID.SquireAltHead, Summon_Armor.Head, 2),
                (ItemID.MonkAltHead, Summon_Armor.Head, 2),
                (ItemID.HuntressAltHead, Summon_Armor.Head, 2),
                (ItemID.ApprenticeAltHead, Summon_Armor.Head, 2),

                (ItemID.StardustHelmet, Summon_Armor.Head, 1),
            };
            //( ArmorSet [ head | body | leggings ] | IncreaseSummonBy )
            ((int[], int[], int[]), int)[] setbonussummonarmor =
            {
                ((new int[]{ ItemID.SquireGreatHelm }, new int[]{ ItemID.SquirePlating }, new int[]{ ItemID.SquireGreaves }), 1),
                ((new int[]{ ItemID.MonkBrows }, new int[]{ ItemID.MonkShirt }, new int[]{ ItemID.MonkPants }), 1),
                ((new int[]{ ItemID.HuntressWig }, new int[]{ ItemID.HuntressJerkin }, new int[]{ ItemID.HuntressPants }), 1),
                ((new int[]{ ItemID.ApprenticeHat }, new int[]{ ItemID.ApprenticeRobe }, new int[]{ ItemID.ApprenticeTrousers }), 1),

                ((new int[]{ ItemID.SquireAltHead }, new int[]{ ItemID.SquireAltShirt }, new int[]{ ItemID.SquireAltPants }), 1),
                ((new int[]{ ItemID.MonkAltHead }, new int[]{ ItemID.MonkAltShirt }, new int[]{ ItemID.MonkAltPants }), 1),
                ((new int[]{ ItemID.HuntressAltHead }, new int[]{ ItemID.HuntressAltShirt }, new int[]{ ItemID.HuntressAltPants }), 1),
                ((new int[]{ ItemID.ApprenticeAltHead }, new int[]{ ItemID.ApprenticeAltShirt }, new int[]{ ItemID.ApprenticeAltPants }), 1),
            };

            #region =[ Accessory Check ]=
            bool has_SquiresShield = false;
            bool has_ApprenticesScarf = false;
            bool has_MonksBelt = false;
            bool has_HuntresssBuckler = false;
            for (int i = 0; i < player.TPlayer.armor.Length; i++)
            {
                Item item = player.TPlayer.armor[i];

                if (item.type == ItemID.SquireShield)
                {
                    has_SquiresShield = true;
                }
                else if (item.type == ItemID.ApprenticeScarf)
                {
                    has_ApprenticesScarf = true;
                }
                else if (item.type == ItemID.MonkBelt)
                {
                    has_MonksBelt = true;
                }
                else if (item.type == ItemID.HuntressBuckler)
                {
                    has_HuntresssBuckler = true;
                }
            }
            #endregion

            #region =[ Accessory & Armor Caculate ]=

            Item head = player.TPlayer.armor[0];
            Item body = player.TPlayer.armor[1];
            Item leggings = player.TPlayer.armor[2];

            #region [ Individual Armor Piece ]
            foreach (var getarmor in summonarmor)
            {
                switch (getarmor.Item2)
                {
                    case Summon_Armor.Head:
                        {
                            if (head.type == getarmor.Item1)
                            {
                                maxsentry += getarmor.Item3;
                            }
                            break;
                        }
                    case Summon_Armor.Body:
                        {
                            if (body.type == getarmor.Item1)
                            {
                                maxsentry += getarmor.Item3;
                            }
                            break;
                        }
                    case Summon_Armor.Leggings:
                        {
                            if (leggings.type == getarmor.Item1)
                            {
                                maxsentry += getarmor.Item3;
                            }
                            break;
                        }
                }
            }
            #endregion

            #region [ Armor Set ]
            foreach (var getarmorset in setbonussummonarmor)
            {
                if (getarmorset.Item1.Item1.Contains(head.type) && getarmorset.Item1.Item2.Contains(head.type) && getarmorset.Item1.Item3.Contains(head.type))
                {
                    maxsentry += getarmorset.Item2;
                }
            }
            #endregion

            if (has_SquiresShield) { maxsentry += 1; }
            if (has_ApprenticesScarf) { maxsentry += 1; }
            if (has_MonksBelt) { maxsentry += 1; }
            if (has_HuntresssBuckler) { maxsentry += 1; }
            #endregion

            if (player.TPlayer != null && player.TPlayer.buffType != null)
            {
                for (int i = 0; i < Terraria.Player.maxBuffs; i++)
                {
                    if (player.TPlayer.buffType[i] == BuffID.WarTable)
                    {
                        maxsentry += 1;
                        break;
                    }
                }
            }

            return maxsentry;
            #endregion
        }

        #endregion
    }
}
