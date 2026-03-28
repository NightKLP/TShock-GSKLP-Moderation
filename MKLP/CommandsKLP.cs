using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Configuration;
using Terraria;
using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
using Terraria.ID;
using Newtonsoft.Json;
using MKLP.Functions;


namespace MKLP
{
    public static class CommandsKLP
    {
        public static void INIT()
        {
            #region [ Commands Initialize ]

            #region { default }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.Default_CMD_Ping, CMD_ping, "ping")
            {
                HelpText = MKLP.GetText("Get Players Latency")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.Default_CMD_Progression, CMD_BE, "progression", "prog")
            {
                HelpText = MKLP.GetText("displays defeated bosses and events")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.Default_CMD_Report, CMD_Report, "report")
            {
                HelpText = MKLP.GetText("Report any suspicious activity by doing /report <message>"),
                AllowServer = false
            });

            if ((bool)MKLP.Config.Main.Replace_Who_TShockCommand)
            {
                Command VarMCMD_Who = new(MCMD_Playing, "playing", "online", "who") { HelpText = MKLP.GetText("Shows the currently connected players.") };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarMCMD_Who.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarMCMD_Who);
            }

            #endregion

            #region { Staff }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.Staff, CMD_StaffChat, "staffchat", "staff", "#")
            {
                HelpText = MKLP.GetText("Sends a message in staff chat")
            });

            if ((bool)MKLP.Config.Main.Replace_AccountInfo_TShockCommand)
            {
                Command VarMCMD_AccountInfo = new(Permissions.checkaccountinfo, MCMD_AccountInfo, "accountinfo", "ai") { HelpText = MKLP.GetText("Shows information about a user.") };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarMCMD_AccountInfo.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarMCMD_AccountInfo);
            }
            else
            {
                Commands.ChatCommands.Add(new Command(Permissions.checkaccountinfo, MCMD_AccountInfo, "klpaccountinfo")
                {
                    HelpText = MKLP.GetText("Shows information about a user.")
                });
            }

            #endregion

            #region { Admin }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_ClearMessage, CMD_ClearMessages, "clearmessages", "clearmessage", "messageclear", "purgemessage")
            {
                HelpText = MKLP.GetText("Clears the whole message chat")
            });

            if (!Commands.ChatCommands.Any(cmd => cmd.Name == "kickall"))
            {
                Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_KickAll, CMD_KickAll, "kickall")
                {
                    HelpText = MKLP.GetText("Kicks all the players on the server except you")
                });
            }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_LockDown, CMD_LockDown, "lockdown")
            {
                HelpText = MKLP.GetText("Prevents Players from joining the server")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_LockDownRegister, CMD_LockDownRegister, "lockdownregister", "lockdownreg")
            {
                HelpText = MKLP.GetText("Prevents Players to register their account")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_MapPingTP, CMD_MapPingTP, "tpmap", "pingmap", "maptp")
            {
                AllowServer = false,
                HelpText = MKLP.GetText("Allows you to teleporter anywhere using map ping")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_ClearLag, CMD_ClearLag, "clearlag")
            {
                HelpText = MKLP.GetText("Deletes low value npc/items")
            });

            if ((bool)MKLP.Config.BossManager.UsingBossManager)
            {
                Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_ManageBoss, CMD_ManageBoss, "manageboss", "mboss")
                {
                    HelpText = MKLP.GetText("Manage it by enable/disable boss or schedule it")
                });
            }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Vanish, CMD_Vanish, "vanish", "ghost")
            {
                AllowServer = false,
                HelpText = MKLP.GetText("allows you to become completely invisible to players.")
            });

            #endregion

            #region { moderation }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_ManageReport, CMD_ManageReport, "managereport", "mreport")
            {
                HelpText = MKLP.GetText("View/Delete any reports")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Ban, CMD_BanInfo, "baninfo")
            {
                HelpText = MKLP.GetText("Displays ban information using ban ticket number")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Disable, CMD_disable, "disable")
            {
                HelpText = MKLP.GetText("Acts as Ban but prevents players from doing anything \nwarning: disable's are temporary!")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Disable, CMD_undisable, "enable", "undisable")
            {
                HelpText = MKLP.GetText("enable's a player that got disabled")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Disable, CMD_disablelist, "disablelist")
            {
                HelpText = MKLP.GetText("list of player names that got disabled")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_UnBan, CMD_UnBan, "unban")
            {
                HelpText = MKLP.GetText("Removes ban tickets")
            });
            if ((bool)MKLP.Config.Main.Replace_Ban_TShockCommand)
            {
                if (MKLP.HasBanGuardPlugin && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
                {
                    MKLP_Console.SendLog_Warning(MKLP.GetText("Replacing TShock Ban Command When \"UsingPlugin\" on \"BanGuard\" might cause problems"));
                }
                Command VarCMD_Ban = new(MKLP.Config.Permissions.CMD_Ban, CMD_Ban, "ban") { HelpText = MKLP.GetText("Bans a player") };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarCMD_Ban.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarCMD_Ban);

            }
            else
            {
                Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Ban, CMD_Ban, "qban")
                {
                    HelpText = MKLP.GetText("Bans a player")
                });
            }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_UnMute, CMD_UnMute, "unmute")
            {
                HelpText = MKLP.GetText("Unmutes Player")
            });
            if ((bool)MKLP.Config.Main.Replace_Mute_TShockCommand)
            {
                Command VarCMD_Mute = new(MKLP.Config.Permissions.CMD_Mute, CMD_Mute, "mute") { HelpText = MKLP.GetText("Mutes Player") };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarCMD_Mute.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarCMD_Mute);
            }
            else
            {
                Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Mute, CMD_Mute, "qmute")
                {
                    HelpText = MKLP.GetText("Mutes Player")
                });
            }
            #endregion

            #region { Inspect }

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_InventoryView, InventoryManager.CMD_InventoryView, "inventoryview", "invview", "inview")
            {
                HelpText = MKLP.GetText("View's inventory of a player")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_Spy, CMD_Spy, "spy")
            {
                HelpText = MKLP.GetText("allows you to stalk a player")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_UUIDMatch, CMD_uuidmatch, "uuidmatch")
            {
                HelpText = MKLP.GetText("check useraccounts with following match uuid accounts")
            });

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_UUIDMatch, CMD_Logs, "logs")
            {
                HelpText = MKLP.GetText("check useraccounts with following match uuid accounts")
            });

            #endregion

            #region [ Manager ]

            Commands.ChatCommands.Add(new Command(MKLP.Config.Permissions.CMD_MKLPDiscord, CMD_MKLPDiscord, "mklpdiscord")
            {
                HelpText = MKLP.GetText("Manage Linked Account Players")
            });

            #endregion

            #endregion
        }


        #region [ Commands ]

        #region { Default }

        private static void CMD_ping(CommandArgs args)
        {
            #region code
            string result = "";


            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                result += $"{player.Name} : {player.GetData<double>("MKLP_GetLatency")}ms\n";
            }


            if (result == "") result = MKLP.GetText("No Latency has been check...");

            args.Player.SendMessage(MKLP.GetText("List Of Players Latency:\n\n") +
                result, Color.Yellow);

            #endregion
        }

        private static void CMD_BE(CommandArgs args)
        {
            #region code

            MKLP.check_bosssched();

            string GetDefeatedBoss = "";
            string GetEnableBoss = "";

            string GetDefeatedEvents = "";

            foreach (var get in BossEventManager.GetDefeatedBoss())
            {
                if ((bool)MKLP.Config.Main.Simplified_DefeatedBossEventList)
                {
                    if (get.Value)
                    {
                        GetDefeatedBoss += BossEventManager.GetBossIconFromName(get.Key) + ",";
                    }
                    else
                    {
                        GetEnableBoss += BossEventManager.GetBossIconFromName(get.Key) + ",";
                    }
                }
                else
                {
                    if (get.Value)
                    {
                        GetDefeatedBoss += $"- {BossEventManager.GetBossIconFromName(get.Key)} [c/87ff74:{MKLP.GetText(get.Key)}]\n";
                    }
                    else
                    {
                        GetEnableBoss += $"- {BossEventManager.GetBossIconFromName(get.Key)} [c/fefe6a:{MKLP.GetText(get.Key)}]\n";
                    }
                }
            }
            foreach (var get in BossEventManager.GetDefeatedEvents())
            {
                if ((bool)MKLP.Config.Main.Simplified_DefeatedBossEventList)
                {
                    GetDefeatedEvents += BossEventManager.GetEventsIconFromName(get.Key) + ",";
                    
                } else
                {
                    GetDefeatedEvents += $"- {BossEventManager.GetEventsIconFromName(get.Key)} [c/8dfdf1:{MKLP.GetText(get.Key)}]\n";
                    
                }
            }
            GetDefeatedBoss = GetDefeatedBoss.TrimEnd(',');
            GetEnableBoss = GetEnableBoss.TrimEnd(',');
            GetDefeatedEvents = GetDefeatedEvents.TrimEnd(',');

            if (GetDefeatedBoss == "" && GetEnableBoss == "" && GetDefeatedEvents == "")
            {
                GetDefeatedBoss = MKLP.GetText("There aren't any progress yet");
            } else
            {
                GetDefeatedBoss = GetDefeatedBoss == "" ? $"[c/23ff00:{MKLP.GetText("No Defeated Bosses...")}]\n\n" : $"[c/23ff00:{MKLP.GetText("Defeated Bosses:")}]\n" + ((bool)MKLP.Config.Main.Simplified_DefeatedBossEventList ? $"- {GetDefeatedBoss}\n" : GetDefeatedBoss);
                GetEnableBoss = GetEnableBoss == "" ? $"[c/ffff00:{MKLP.GetText("No Enable Bosses...")}]\n\n" : $"[c/ffff00:{MKLP.GetText("Enabled Bosses:")}]\n" + ((bool)MKLP.Config.Main.Simplified_DefeatedBossEventList ? $"- {GetEnableBoss}\n" : GetEnableBoss);
                GetDefeatedEvents = GetDefeatedEvents == "" ? $"[c/00fde2:{MKLP.GetText("No Defeated Events...")}]\n\n" : $"[c/00fde2:{MKLP.GetText("Defeated Events:")}]\n" + ((bool)MKLP.Config.Main.Simplified_DefeatedBossEventList ? $"- {GetDefeatedEvents}\n" : GetDefeatedEvents);
            }

            var getnextbosssched = BossEventManager.GetNextBossSchedule();
            args.Player.SendMessage(
                    $"{MKLP.GetText("List Of Bosses:")}\n" +
                    $"{GetDefeatedBoss}" +
                    $"{GetEnableBoss}" +
                    $"{GetDefeatedEvents}" +
                    (getnextbosssched.Item1 == "" ? "" : $"\n{MKLP.GetText($"{getnextbosssched.Item1} in")} {GetTimetring((int)(getnextbosssched.Item2 - DateTime.UtcNow).TotalSeconds)}")
                ,
                Color.Gray);


            string GetTimetring(int seconds)
            {
                TimeSpan t = TimeSpan.FromSeconds(seconds);

                if (seconds < 0)
                {
                    return MKLP.GetText("{0}" + $"  second{(seconds > 1 ? "s" : "")}", seconds);
                }

                if (t.TotalMinutes < 1)
                {
                    return MKLP.GetText("{0}" + $" second{(t.Seconds > 1 ? "s" : "")}", t.Seconds);
                }
                else if (t.TotalHours < 1)
                {
                    return MKLP.GetText("{0}" + $" minute{(t.Minutes > 1 ? "s" : "")}", t.Minutes);
                }
                else if (t.TotalDays < 1)
                {
                    return MKLP.GetText("{0}" + $" hour{(t.Hours > 1 ? "s" : "")}", t.Hours);
                }
                else
                {
                    return MKLP.GetText("{0}" + $" day{(t.Days > 1 ? "s" : "")}", t.Days);
                }
            }
            #endregion
        }

        private static Dictionary<string, DateTime> ReportCD = new();
        private static void CMD_Report(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}report <type> <message>") +
                    MKLP.GetText($"\nmore information at '{Commands.Specifier}report help'"));
                return;
            }

            if (args.Player.Account != null)
            {
                if (ReportCD.ContainsKey(args.Player.Account.Name))
                {
                    int cdtotal_min = ((int)(DateTime.UtcNow - ReportCD[args.Player.Account.Name]).TotalMinutes - 10) * -1;
                    int cdtotal_sec = ((int)(DateTime.UtcNow - ReportCD[args.Player.Account.Name]).TotalSeconds - 60) * -1;
                    if ((DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes < 10)
                    {
                        args.Player.SendErrorMessage(MKLP.GetText($"You can report again in {(cdtotal_min == 0 ? $"{(cdtotal_sec <= 1 ? $"{cdtotal_sec} second" : $"{cdtotal_sec} seconds")}" : $"{(cdtotal_min <= 1 ? $"{cdtotal_min} minute" : $"{cdtotal_min} minutes")}")}"));
                        return;
                    }
                }
            }
            else
            {
                if (ReportCD.ContainsKey(args.Player.Name))
                {
                    int cdtotal_min = (int)(DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes;
                    int cdtotal_sec = (int)(DateTime.UtcNow - ReportCD[args.Player.Name]).TotalSeconds;
                    if ((DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes < 10)
                    {
                        args.Player.SendErrorMessage(MKLP.GetText($"You can report again in {(cdtotal_min == 0 ? $"{(cdtotal_sec <= 1 ? $"{cdtotal_sec} second" : $"{cdtotal_sec} seconds")}" : $"{(cdtotal_min <= 1 ? $"{cdtotal_min} minute" : $"{cdtotal_min} minutes")}")}"));
                        return;
                    }
                }
            }

            bool istemp = true;
            string getexecutername = args.Player.Name;
            if (args.Player.Account != null)
            {
                istemp = false;
                getexecutername = args.Player.Account.Name;
            }

            string get_target_name = "";
            bool isAccount = false;
            int get_target_index = -1;

            string playernames = "";
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                playernames += player.Name + DiscordKLP.S_;
            }
            playernames.TrimEnd(DiscordKLP.S_);


            string report_message = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);

            switch (args.Parameters[0].ToLower())
            {
                case "help":
                    #region [ sub-command | Help ]
                    {
                        args.Player.SendMessage(
                            $"Usage: {Commands.Specifier}report <type> <message>" +
                            $"\n" +
                            $"\n=== Sub-Command ===" +
                            $"\n'{Commands.Specifier}report normal <message>' : a normal report message" +
                            $"\n'{Commands.Specifier}report player <playername> <message>' : report a player" +
                            $"\n'{Commands.Specifier}report bug <message>' : report a bug" +
                            $"\n'{Commands.Specifier}report staff <playername> <message>' : report a staff",
                            Color.WhiteSmoke);
                        return;
                    }
                #endregion
                case "na":
                case "message":
                case "normal":
                    #region [ sub-command | NormalReport ]
                    {
                        if (args.Parameters.Count < 2)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}report normal <player> <message>"));
                            return;
                        }

                        int id = -1;
                        if (!istemp) id = MKLP.DBManager.AddReport(MKLP_Report.RType.NormalReport, args.Player.Account.Name, "", report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.Discordklp.KLPBotSendMessage_Report_Main(id, "", args.Player.Account.Name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.SendStaffMessage(MKLP.GetText($"[{(istemp ? "Temporary-" : "")}Report] from {args.Player.Account.Name}" +
                            "\nMessage: {0}", report_message), Color.OrangeRed);
                        args.Player.SendSuccessMessage(MKLP.GetText($"{(istemp ? "Temporary-" : "")}Report Sent!" +
                            "\nmessage: {0}", report_message));
                        return;
                    }
                #endregion
                case "player":
                    #region [ sub-command | PlayerReport ]
                    {
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}report player <player> <message>"));
                            return;
                        }

                        report_message = string.Join(" ", args.Parameters.ToArray(), 2, args.Parameters.Count - 2);

                        var targetplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);

                        if (targetplayers.Count == 1)
                        {
                            if (targetplayers[0].Account != null)
                            {
                                get_target_name = targetplayers[0].Account.Name;
                                isAccount = true;
                            }
                            else
                            {
                                get_target_name = targetplayers[0].Name;
                            }

                            get_target_index = targetplayers[0].Index;
                        }
                        else
                        {
                            get_target_name = args.Parameters[1];
                        }
                        int id = -1;

                        if (!istemp) id = MKLP.DBManager.AddReport(MKLP_Report.RType.PlayerReport, args.Player.Account.Name, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.Discordklp.KLPBotSendMessage_Report_Player(id, args.Player.Account.Name, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.SendStaffMessage(MKLP.GetText($"[{(istemp ? "Temporary " : "")}Player-Report] from {args.Player.Account.Name}" +
                            "\nTarget: {0}" +
                            "\nMessage: {1}",
                            (get_target_name == "" ? $"{DiscordKLP.S_}{MKLP.GetText("none")}{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}"),
                            report_message), Color.OrangeRed);
                        args.Player.SendSuccessMessage(MKLP.GetText($"{(istemp ? "[Temporary] " : "")}Player Report Sent!" +
                            "\ntarget: {0}" +
                            "\nmessage: {1}",
                            (get_target_name == "" ? $"{DiscordKLP.S_}{MKLP.GetText("none")}{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}"),
                            report_message));
                        return;
                    }
                #endregion
                case "bug":
                    #region [ sub-command | BugReport ]
                    {
                        if (args.Parameters.Count < 2)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}report bug <message>"));
                            return;
                        }

                        int id = -1;

                        if (!istemp) id = MKLP.DBManager.AddReport(MKLP_Report.RType.BugReport, args.Player.Account.Name, "", report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.Discordklp.KLPBotSendMessage_Report_Main(id, "🐛Bug ", args.Player.Account.Name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.SendStaffMessage(MKLP.GetText($"[{(istemp ? "Temporary " : "")}Bug-Report] from {args.Player.Account.Name}" +
                            "\nMessage: {0}",
                            report_message), Color.OrangeRed);
                        args.Player.SendSuccessMessage(MKLP.GetText($"{(istemp ? "[Temporary] " : "")}Bug Report Sent!" +
                            "\nmessage: {0}",
                            report_message));
                        return;
                    }
                #endregion
                case "staff":
                    #region [ sub-command | StaffPlayerReport ]
                    {
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}report staff <player> <message>"));
                            return;
                        }

                        report_message = string.Join(" ", args.Parameters.ToArray(), 2, args.Parameters.Count - 2);

                        var targetplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);

                        if (targetplayers.Count == 1)
                        {
                            if (targetplayers[0].Account != null)
                            {
                                get_target_name = targetplayers[0].Account.Name;
                                isAccount = true;
                            }
                            else
                            {
                                get_target_name = targetplayers[0].Name;
                            }

                            get_target_index = targetplayers[0].Index;
                        }
                        else
                        {
                            get_target_name = args.Parameters[1];
                        }
                        int id = -1;

                        if (!istemp) id = MKLP.DBManager.AddReport(MKLP_Report.RType.StaffReport, getexecutername, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        MKLP.Discordklp.KLPBotSendMessage_Report_Staff(id, getexecutername, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                        args.Player.SendSuccessMessage(MKLP.GetText($"{(istemp ? "[Temporary] " : "")}Staff Report Sent!" +
                            "\ntarget: {0}" +
                            "\nmessage: {1}",
                            (get_target_name == "" ? $"{DiscordKLP.S_}{MKLP.GetText("none")}{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}"),
                            report_message));
                        return;
                    }
                #endregion
                default:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText($"invalid sub-command" +
                            $"\nmore info at '{Commands.Specifier}report help'"));
                        return;
                    }
            }


            void ToggleCooldown()
            {
                if (args.Player.Account != null)
                {
                    if (ReportCD.ContainsKey(args.Player.Account.Name))
                    {
                        ReportCD[args.Player.Account.Name] = DateTime.UtcNow;
                    }
                    else { ReportCD.Add(args.Player.Account.Name, DateTime.Now); }

                }
                else
                {
                    if (ReportCD.ContainsKey(args.Player.Name))
                    {
                        ReportCD[args.Player.Name] = DateTime.UtcNow;
                    }
                    else { ReportCD.Add(args.Player.Name, DateTime.Now); }
                }
            }
            #endregion
        }

        #endregion

        #region { Staff }

        private static void CMD_StaffChat(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}staffchat <message>" +
                    $"\nshortcuts: {Commands.Specifier}staff, {Commands.Specifier}#"));
                return;
            }

            Config.CONFIG_COLOR_RBG Config_messagecolor = (Config.CONFIG_COLOR_RBG)MKLP.Config.Main.StaffChat.StaffChat_MessageRecieved_InGame_RBG;

            MKLP.SendStaffMessage(GetSendMessageInGameResult(args.Player, MKLP.Config.Main.StaffChat.StaffChat_MessageSend_Discord, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)), new(Config_messagecolor.R, Config_messagecolor.G, Config_messagecolor.B));
            MKLP.Discordklp.KLPBotSendMessageMain(GetMessageInGameResult(args.Player, MKLP.Config.Main.StaffChat.StaffChat_MessageRecieved_InGame, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)));
            MKLP_Console.SendLog_Message_StaffChat_InGame(args.Player.Name, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count));

            #region GetMessageInGameResult
            string GetMessageInGameResult(TSPlayer tsplayer, string Text, string message)
            {
                string Context = Text;
                Context = Context.Replace("%ingamename%", tsplayer.Name);
                if (tsplayer.Account != null && tsplayer != TSPlayer.Server)
                {
                    Context = Context.Replace("%ingameaccountname%", tsplayer.Account.Name);
                    if (tsplayer.Name != tsplayer.Account.Name)
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Account.Name);
                    }
                    else
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);
                    }

                    try
                    {
                        ulong getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

                        Context = Context.Replace("%ingamelinkedusername%", MKLP.Discordklp.GetUser(getuserid).Username);
                        Context = Context.Replace("%ingamelinkedicon%", MKLP.Config.Main.StaffChat.StaffChat_Message_ingamelinkedicon);
                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%ingamelinkedicon%", "");
                    }

                }
                else
                {
                    Context = Context.Replace("%ingameaccountname%", "");
                    Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);

                    try
                    {
                        ulong getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

                        Context = Context.Replace("%ingamelinkedusername%", MKLP.Discordklp.GetUser(getuserid).Username);
                        Context = Context.Replace("%ingamelinkedicon%", MKLP.Config.Main.StaffChat.StaffChat_Message_ingamelinkedicon);
                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%ingamelinkedicon%", "");
                    }
                }


                if (tsplayer.Group != null)
                {
                    Context = Context.Replace("%groupname%", tsplayer.Group.Name);
                    Context = Context.Replace("%groupprefix%", tsplayer.Group.Prefix);
                    Context = Context.Replace("%groupsuffix%", tsplayer.Group.Suffix);
                }
                else
                {
                    Context = Context.Replace("%groupname%", "");
                    Context = Context.Replace("%groupprefix%", "");
                    Context = Context.Replace("%groupsuffix%", "");
                }

                if (tsplayer.tempGroup != null)
                {
                    Context = Context.Replace("%tempgroupname%", tsplayer.tempGroup.Name);
                    Context = Context.Replace("%tempgroupprefix%", tsplayer.tempGroup.Prefix);
                    Context = Context.Replace("%tempgroupsuffix%", tsplayer.tempGroup.Suffix);
                }
                else
                {
                    Context = Context.Replace("%tempgroupname%", "");
                    Context = Context.Replace("%tempgroupprefix%", "");
                    Context = Context.Replace("%tempgroupsuffix%", "");
                }

                Context = Context.Replace("%message%", message);

                return Context;
            }
            #endregion

            #region GetSendMessageInGameResult
            string GetSendMessageInGameResult(TSPlayer tsplayer, string Text, string message)
            {
                string Context = Text;
                Context = Context.Replace("%ingamename%", tsplayer.Name);
                if (tsplayer.Account != null && tsplayer != TSPlayer.Server)
                {
                    Context = Context.Replace("%ingameaccountname%", tsplayer.Account.Name);
                    if (tsplayer.Name != tsplayer.Account.Name)
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Account.Name);
                    }
                    else
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);
                    }

                    try
                    {
                        ulong getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

                        Context = Context.Replace("%ingamelinkedusername%", MKLP.Discordklp.GetUser((ulong)getuserid).Username);
                        Context = Context.Replace("%discordacclinkedicon%", MKLP.Config.Main.StaffChat.StaffChat_Message_discordacclinkedicon);

                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%discordacclinkedicon%", "");
                    }
                }
                else
                {
                    Context = Context.Replace("%ingameaccountname%", "");
                    Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);

                    try
                    {
                        ulong getuserid = getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

                        Context = Context.Replace("%ingamelinkedusername%", MKLP.Discordklp.GetUser((ulong)getuserid).Username);
                        Context = Context.Replace("%discordacclinkedicon%", MKLP.Config.Main.StaffChat.StaffChat_Message_discordacclinkedicon);

                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%discordacclinkedicon%", "");
                    }
                }

                if (tsplayer.Group != null)
                {
                    Context = Context.Replace("%groupname%", tsplayer.Group.Name);
                }
                else
                {
                    Context = Context.Replace("%groupname%", "");
                }

                if (tsplayer.tempGroup != null)
                {
                    Context = Context.Replace("%tempgroupname%", tsplayer.tempGroup.Name);
                }
                else
                {
                    Context = Context.Replace("%tempgroupname%", "");
                }

                Context = Context.Replace("%message%", message);

                return Context;
            }
            #endregion

            #endregion
        }

        #endregion

        #region { Admin }

        private static void CMD_ClearMessages(CommandArgs args)
        {
            #region code

            for (int i = 0; i < 130; i++)
            {
                TSPlayer.All.SendMessage("\n\n\n\n", Color.Black);
            }

            args.Player.SendSuccessMessage(MKLP.GetText("Messages Cleared!"));

            #endregion
        }

        private static void CMD_KickAll(CommandArgs args)
        {
            #region code

            string reason = (args.Parameters.Count == 0 ? string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) : MKLP.GetText("Misbehaviour."));
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (!player.RealPlayer) continue;
                if (args.Player == player) continue;
                player.Kick(reason, !args.Player.RealPlayer, silent: false, args.Player.Name);
            }
            args.Player.SendSuccessMessage(MKLP.GetText("Successfully kick all the players..."));

            #endregion
        }

        private static void CMD_LockDown(CommandArgs args)
        {
            #region code
            if (!MKLP.LockDown)
            {
                if (args.Parameters.Count == 0)
                {
                    MKLP.LockDown = true;
                    TShock.Utils.Broadcast(MKLP.GetText("Server is on LockDown!"), Color.OrangeRed);
                    MKLP.Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** " + MKLP.GetText("Server is on lockdown!"));
                }
                else
                {
                    MKLP.LockDown = true;
                    MKLP.LockDownReason = string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count);
                    TShock.Utils.Broadcast(MKLP.GetText("Server is on LockDown by the reason of") + " " + MKLP.LockDownReason, Color.OrangeRed);
                    MKLP.Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** " + MKLP.GetText("Server is on lockdown! `reason: {0}`", MKLP.LockDownReason));
                }
            }
            else
            {
                MKLP.LockDown = false;
                TShock.Utils.Broadcast(MKLP.GetText("Server is no longer on LockDown!"), Color.LightGreen);
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**🔓{args.Player.Name}🔓** " + MKLP.GetText("Server is no longer on lockdown!"));
            }


            #endregion
        }

        private static void CMD_LockDownRegister(CommandArgs args)
        {
            #region code
            if (!MKLP.LockDownRegister)
            {
                MKLP.LockDownRegister = true;
                args.Player.SendSuccessMessage(MKLP.GetText("Guest can no longer resgister!"));
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** " + MKLP.GetText("Guest can no longer register!"));
            }
            else
            {
                MKLP.LockDownRegister = false;
                args.Player.SendSuccessMessage(MKLP.GetText("Guest can now resgister!"));
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**🔓{args.Player.Name}🔓** " + MKLP.GetText("Guest can now register!"));
            }


            #endregion
        }

        private static void CMD_MapPingTP(CommandArgs args)
        {
            #region code
            if (!args.Player.ContainsData("MKLP-Map_Ping_TP"))
            {
                args.Player.SetData("MKLP-Map_Ping_TP", true);
                args.Player.SendSuccessMessage(MKLP.GetText("you're now able to tp ping"));
            }
            else
            {
                if (args.Player.GetData<bool>("MKLP-Map_Ping_TP"))
                {
                    args.Player.SetData("MKLP-Map_Ping_TP", false);
                    args.Player.SendSuccessMessage(MKLP.GetText("you can no longer able to tp ping"));
                }
                else
                {
                    args.Player.SetData("MKLP-Map_Ping_TP", true);
                    args.Player.SendSuccessMessage(MKLP.GetText("you're now able to tp ping"));
                }
            }
            #endregion
        }

        private static void CMD_ClearLag(CommandArgs args)
        {
            #region code
            int ClearedItems = 0;
            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i].value < MKLP.Config.Main.Ignore_Value_ClearLag && Main.item[i].active)
                {
                    Main.item[i].TurnToAir(true);
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    ClearedItems++;
                }
            }
            int ClearedNPC = 0;
            int[] IgnoreNPCIDs = {
                NPCID.DungeonGuardian,

                NPCID.DD2EterniaCrystal,
                NPCID.DD2LanePortal,

                NPCID.DD2DarkMageT1,
                NPCID.DD2DarkMageT3,
                NPCID.DD2OgreT2,
                NPCID.DD2OgreT3,
                NPCID.DD2Betsy,

                NPCID.PirateShip,
                NPCID.PirateShipCannon,

                NPCID.Everscream,
                NPCID.SantaNK1,
                NPCID.IceQueen,

                NPCID.MourningWood,
                NPCID.Pumpking,

                NPCID.MartianSaucer,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerTurret,

                NPCID.LunarTowerSolar,
                NPCID.LunarTowerNebula,
                NPCID.LunarTowerStardust,
                NPCID.LunarTowerVortex,
            };
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].boss) continue;
                if (Main.npc[i].townNPC) continue;
                if (IgnoreNPCIDs.Contains(Main.npc[i].type)) continue;
                if (Main.npc[i].rarity > 0) continue;
                if (Main.npc[i].active)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    ClearedNPC++;
                }
            }
            int ClearedProjectile = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].minion) continue;
                if (Main.projectile[i].active)
                {
                    Main.projectile[i].active = false;
                    Main.projectile[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
                    ClearedProjectile++;
                }
            }

            TShock.Utils.Broadcast(MKLP.GetText($"[MKLP] ClearLag - {args.Player.Name} Removed" +
                $" {(ClearedItems >= 2 ? "{0} Items" : "{0} Item")}" +
                $" {(ClearedProjectile >= 2 ? "{1} Projectiles" : "{1} Projectile")}" +
                $" {(ClearedNPC >= 2 ? "{2} Entities" : "{2} Entity")}",
                ClearedItems,
                ClearedProjectile,
                ClearedNPC), Color.Yellow);
            #endregion
        }

        private static void CMD_ManageBoss(CommandArgs args)
        {
            #region code

            if (!(bool)MKLP.Config.BossManager.UsingBossManager)
            {
                args.Player.SendErrorMessage(MKLP.GetText("MKLP BossManager isn't used"));
                return;
            }

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper usage: {Commands.Specifier}manageboss <type> <args...>" +
                    $"\ndo '{Commands.Specifier}manageboss help' for more details"));
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                #region [ Help Text ]
                case "help":
                    {
                        args.Player.SendMessage(MKLP.GetText($"Proper Usage: [c/31ff77:{Commands.Specifier}manageboss <type> <args...>]" +
                            "\n[c/ffd531:== Available Sub-Command ==]" +
                            "\n'enable <boss name>' : Enable a boss" +
                            "\n[c/b6b6b6:'disable <boss name>' : Disable a boss to prevent it from spawning]" +
                            "\n'enableall' : Enables all bosses" +
                            "\n[c/b6b6b6:'disableall' : Disables all bosses]" +
                            $"{(args.Player.HasPermission(MKLP.Config.Permissions.CMD_ManageBoss_SetKilled) ? "'setkilled <boss name>' : set boss killed or not\n" : "")}\n" +
                            "\n[c/96b85f:== Boss Schedule Sub-Command ==]" +
                            "\n'enablesched <boss name> <MM/DD/YY>' : Enable a boss in specific time" +
                            "\n[c/b6b6b6:'disablesched <boss name> : cancel a specific boss schedule]" +
                            "\n'disableschedall' : cancel all boss schedule" +
                            "\n[c/b6b6b6:'usingschedule <yes/no>' : activate or deactivate boss schedule]" +
                            "\n'resetschedule' or 'resetschedule <MM/DD/YY>' : Restart the whole boss schedule to day 0 and assign the bosses each days on config file")
                            , Color.WhiteSmoke);
                        return;
                    }
                #endregion
                #region ( Type : Enable )
                case "enable":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Please specify a boss!");
                            return;
                        }
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowKingSlime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("King Slime is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowKingSlime = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("King Slime is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("King Slime", args.Player.Name);
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowEyeOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eye Of Cthulhu is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEyeOfCthulhu = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eye Of Cthulhu is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Eye of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowEaterOfWorlds && (bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eater Of Worlds & Brain Of Cthulhu is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEaterOfWorlds = true;
                                    MKLP.Config.BossManager.AllowBrainOfCthulhu = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eater Of Worlds & Brain Of Cthulhu is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds & Brain of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowEaterOfWorlds)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eater Of Worlds is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEaterOfWorlds = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eater Of Worlds is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds", args.Player.Name);
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Brain Of Cthulhu is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowBrainOfCthulhu = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Brain Of Cthulhu is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Brain of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowSkeletron)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Skeletron is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowSkeletron = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Skeletron is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Skeletron", args.Player.Name);
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowQueenBee)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Queen Bee is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowQueenBee = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Queen Bee is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Queen Bee", args.Player.Name);
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowDeerclops)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Deerclops is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowDeerclops = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Deerclops is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Deerclops", args.Player.Name);
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowWallOfFlesh)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Wall Of Flesh is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowWallOfFlesh = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Wall Of Flesh is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Wall of Flesh", args.Player.Name);
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowQueenSlime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Queen Slime is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowQueenSlime = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Queen Slime is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Queen Slime", args.Player.Name);
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Enabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = true;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now enabled"));
                                        MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)MKLP.Config.BossManager.AllowTheDestroyer)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("The Destroyer is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowTheDestroyer = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("The Destroyer is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Destroyer", args.Player.Name);
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Enabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = true;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now enabled"));
                                        MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)MKLP.Config.BossManager.AllowTheTwins)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("The Twins is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowTheTwins = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("The Twins is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("The Twins", args.Player.Name);
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Enabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = true;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now enabled"));
                                        MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)MKLP.Config.BossManager.AllowSkeletronPrime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Skeletron Prime is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowSkeletronPrime = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Skeletron Prime is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Skeletron Prime", args.Player.Name);
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is only available on zenith seed!"));
                                        return;
                                    }

                                    if ((bool)MKLP.Config.BossManager.AllowMechdusa)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowMechdusa = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                    break;
                                }
                            case "plantera":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowPlantera)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Plantera is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowPlantera = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Plantera is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Plantera", args.Player.Name);
                                    break;
                                }
                            case "golem":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowGolem)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Golem is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowGolem = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Golem is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Golem", args.Player.Name);
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowDukeFishron)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Duke Fishron is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowDukeFishron = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Duke Fishron is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Duke Fishron", args.Player.Name);
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowLunaticCultist)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Lunatic Cultist is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowLunaticCultist = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Lunatic Cultist is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Lunatic Cultist", args.Player.Name);
                                    break;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowEmpressOfLight)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Empress Of Light is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEmpressOfLight = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Empress Of Light is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Empress of Light", args.Player.Name);
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    if ((bool)MKLP.Config.BossManager.AllowMoonLord)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Moon Lord is already Enabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowMoonLord = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("Moon Lord is now enabled"));
                                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Moon Lord", args.Player.Name);
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage(MKLP.GetText("Please specify the boss!"));
                                    return;
                                }
                        }
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : Disabled )
                case "disable":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Please specify a boss!");
                            return;
                        }
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowKingSlime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("King Slime is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowKingSlime = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("King Slime is now disabled"));
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowEyeOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eye Of Cthulhu is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEyeOfCthulhu = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eye Of Cthulhu is now disabled"));
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowEaterOfWorlds && !(bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eater Of Worlds & Brain Of Cthulhu is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEaterOfWorlds = false;
                                    MKLP.Config.BossManager.AllowBrainOfCthulhu = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eater Of Worlds & Brain Of Cthulhu is now disabled"));
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowEaterOfWorlds)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Eater Of Worlds is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEaterOfWorlds = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Eater Of Worlds is now disabled"));
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Brain Of Cthulhu is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowBrainOfCthulhu = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Brain Of Cthulhu is now disabled"));
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowSkeletron)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Skeletron is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowSkeletron = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Skeletron is now disabled"));
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowQueenBee)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Queen Bee is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowQueenBee = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Queen Bee is now disabled"));
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowDeerclops)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Deerclops is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowDeerclops = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Deerclops is now disabled"));
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowWallOfFlesh)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Wall Of Flesh is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowWallOfFlesh = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Wall Of Flesh is now disabled"));
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowQueenSlime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Queen Slime is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowQueenSlime = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Queen Slime is now disabled"));
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Disabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now disabled"));
                                        break;
                                    }
                                    if (!(bool)MKLP.Config.BossManager.AllowTheDestroyer)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("The Destroyer is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowTheDestroyer = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("The Destroyer is now disabled"));
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Disabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now disabled"));
                                        break;
                                    }
                                    if (!(bool)MKLP.Config.BossManager.AllowTheTwins)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("The Twins is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowTheTwins = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("The Twins is now disabled"));
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)MKLP.Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Disabled!"));
                                            return;
                                        }

                                        MKLP.Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now disabled"));
                                        break;
                                    }
                                    if (!(bool)MKLP.Config.BossManager.AllowSkeletronPrime)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Skeletron Prime is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowSkeletronPrime = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Skeletron Prime is now disabled"));
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is only available on zenith seed!"));
                                        return;
                                    }

                                    if (!(bool)MKLP.Config.BossManager.AllowMechdusa)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowMechdusa = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Mechdusa is now disabled"));
                                    break;
                                }
                            case "plantera":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowPlantera)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Plantera is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowPlantera = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Plantera is now disabled"));
                                    break;
                                }
                            case "golem":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowGolem)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Golem is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowGolem = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Golem is now disabled"));
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowDukeFishron)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Duke Fish is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowDukeFishron = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Duke Fish is now disable"));
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowLunaticCultist)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Lunatic Cultist is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowLunaticCultist = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Lunatic Cultist is now disabled"));
                                    break;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowEmpressOfLight)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Empress Of Light is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowEmpressOfLight = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Empress Of Light is now disabled"));
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    if (!(bool)MKLP.Config.BossManager.AllowMoonLord)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Moon Lord is already Disabled!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.AllowMoonLord = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("Moon Lord is now disabled"));
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage(MKLP.GetText("Please specify the boss!"));
                                    return;
                                }
                        }
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : EnableAll )
                case "enableall":
                    {
                        MKLP.Config.BossManager.AllowKingSlime = true;
                        MKLP.Config.BossManager.AllowEyeOfCthulhu = true;
                        MKLP.Config.BossManager.AllowEaterOfWorlds = true;
                        MKLP.Config.BossManager.AllowBrainOfCthulhu = true;
                        MKLP.Config.BossManager.AllowQueenBee = true;
                        MKLP.Config.BossManager.AllowDeerclops = true;
                        MKLP.Config.BossManager.AllowSkeletron = true;
                        MKLP.Config.BossManager.AllowWallOfFlesh = true;
                        MKLP.Config.BossManager.AllowQueenSlime = true;
                        MKLP.Config.BossManager.AllowTheTwins = true;
                        MKLP.Config.BossManager.AllowTheDestroyer = true;
                        MKLP.Config.BossManager.AllowSkeletronPrime = true;
                        MKLP.Config.BossManager.AllowMechdusa = true;
                        MKLP.Config.BossManager.AllowDukeFishron = true;
                        MKLP.Config.BossManager.AllowPlantera = true;
                        MKLP.Config.BossManager.AllowEmpressOfLight = true;
                        MKLP.Config.BossManager.AllowGolem = true;
                        MKLP.Config.BossManager.AllowLunaticCultist = true;
                        MKLP.Config.BossManager.AllowMoonLord = true;

                        args.Player.SendInfoMessage(MKLP.GetText("All Bosses are enabled"));
                        MKLP.Discordklp.KLPBotSendMessage_BossEnabled("All Bosses", args.Player.Name);
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableAll )
                case "disableall":
                    {
                        MKLP.Config.BossManager.AllowKingSlime = false;
                        MKLP.Config.BossManager.AllowEyeOfCthulhu = false;
                        MKLP.Config.BossManager.AllowEaterOfWorlds = false;
                        MKLP.Config.BossManager.AllowBrainOfCthulhu = false;
                        MKLP.Config.BossManager.AllowQueenBee = false;
                        MKLP.Config.BossManager.AllowDeerclops = false;
                        MKLP.Config.BossManager.AllowSkeletron = false;
                        MKLP.Config.BossManager.AllowWallOfFlesh = false;
                        MKLP.Config.BossManager.AllowQueenSlime = false;
                        MKLP.Config.BossManager.AllowTheTwins = false;
                        MKLP.Config.BossManager.AllowTheDestroyer = false;
                        MKLP.Config.BossManager.AllowSkeletronPrime = false;
                        MKLP.Config.BossManager.AllowMechdusa = false;
                        MKLP.Config.BossManager.AllowDukeFishron = false;
                        MKLP.Config.BossManager.AllowPlantera = false;
                        MKLP.Config.BossManager.AllowEmpressOfLight = false;
                        MKLP.Config.BossManager.AllowGolem = false;
                        MKLP.Config.BossManager.AllowLunaticCultist = false;
                        MKLP.Config.BossManager.AllowMoonLord = false;

                        args.Player.SendInfoMessage(MKLP.GetText("All Bosses are disabled!"));
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : SetKilled )
                case "setkilled":
                case "setkill":
                    {
                        if (!args.Player.HasPermission(MKLP.Config.Permissions.CMD_ManageBoss_SetKilled))
                        {
                            args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to set bosses as kill or not!"));
                            return;
                        }
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Please specify a boss!");
                            return;
                        }
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    NPC.downedSlimeKing = !NPC.downedSlimeKing;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set King Slime as {(NPC.downedSlimeKing ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    NPC.downedBoss1 = !NPC.downedBoss1;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Eye of Cthulhu as {(NPC.downedBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "evilboss":
                            case "evil boss":
                            case "boc":
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                            case "eater":
                                {
                                    NPC.downedBoss2 = !NPC.downedBoss2;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set {(WorldGen.crimson ? "Brain of Cthulhu" : "Eater of Worlds")} as {(NPC.downedBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    NPC.downedBoss3 = !NPC.downedBoss3;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Skeletron as {(NPC.downedBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    NPC.downedQueenBee = !NPC.downedQueenBee;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Queen Bee as {(NPC.downedQueenBee ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    NPC.downedDeerclops = !NPC.downedDeerclops;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Deerclops as {(NPC.downedDeerclops ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "hardmode":
                            case "wallofflesh":
                            case "wall of flesh":
                            case "wof":
                                {
                                    Main.hardMode = !Main.hardMode;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Wall of Flesh (Hardmode) as {(Main.hardMode ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    args.Player.SendInfoMessage(MKLP.GetText("Note: This is the same as the '/hardmode' command."));
                                    return;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    NPC.downedQueenSlime = !NPC.downedQueenSlime;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Queen Slime as {(NPC.downedQueenSlime ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    NPC.downedMechBoss1 = !NPC.downedMechBoss1;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set The Destroyer as {(NPC.downedMechBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    NPC.downedMechBoss2 = !NPC.downedMechBoss2;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set The Twins as {(NPC.downedMechBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    NPC.downedMechBoss3 = !NPC.downedMechBoss3;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Skeletron Prime as {(NPC.downedMechBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "plantera":
                                {
                                    NPC.downedPlantBoss = !NPC.downedPlantBoss;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Plantera as {(NPC.downedPlantBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "golem":
                                {
                                    NPC.downedGolemBoss = !NPC.downedGolemBoss;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Golem as {(NPC.downedGolemBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    NPC.downedFishron = !NPC.downedFishron;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Duke Fishron as {(NPC.downedFishron ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    NPC.downedAncientCultist = !NPC.downedAncientCultist;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Lunatic Cultist as {(NPC.downedAncientCultist ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    NPC.downedEmpressOfLight = !NPC.downedEmpressOfLight;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Empress of Light as {(NPC.downedEmpressOfLight ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    NPC.downedMoonlord = !NPC.downedMoonlord;
                                    args.Player.SendInfoMessage(MKLP.GetText($"Set Moonlord as {(NPC.downedMoonlord ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!"));
                                    return;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage(MKLP.GetText("Please specify which boss to setkilled!"));
                                    args.Player.SendInfoMessage(MKLP.GetText("ex. /mboss setkilled king - sets King Slime killed or not"));
                                    return;
                                }
                        }
                        return;
                    }
                #endregion

                //schedule
                #region ( Type : EnableSched )
                case "enableschedule":
                case "enablesched":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Please specify a boss!");
                            return;
                        }
                        if (args.Parameters.Count == 2)
                        {
                            args.Player.SendErrorMessage("Please specify a Time!" +
                                "\nexample: MM/DD/YY | jan 1 2025 = 01/01/2025");
                            return;
                        }
                        DateTime schedule;
                        if (!DateTime.TryParse(args.Parameters[2], out schedule))
                        {
                            args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                            return;
                        }
                        string[] getmonth_str = {
                            "Jan",
                            "feb",
                            "Mar",
                            "Apr",
                            "May",
                            "Jun",
                            "Jul",
                            "Aug",
                            "Sep",
                            "Oct",
                            "Nov",
                            "Dec"
                        };

                        string stringsched =
                            $"> target: {schedule.Hour}:{schedule.Minute} {getmonth_str[schedule.Month - 1]} {schedule.Day} {schedule.Year}" +
                            $"\n> server time: {DateTime.UtcNow.Hour}:{DateTime.UtcNow.Minute} {getmonth_str[DateTime.UtcNow.Month - 1]} {DateTime.UtcNow.Day} {DateTime.UtcNow.Year}";
                        //TShock.Utils.TryParseTime()
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowKingSlime = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set King Slime Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Eye Of Cthulhu Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = schedule;
                                    MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Eater Of Worlds & Brain Of Cthulhu Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Eater Of Worlds Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Brain Of Cthulhu Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowSkeletron = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Skeletron Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowQueenBee = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Queen Bee Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowDeerclops = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Deerclops Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowSkeletron = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Wall Of Flesh Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowQueenSlime = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Queen Slime Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage(MKLP.GetText("set Mechdusa Schedule" +
                                            $"\n{stringsched}"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowTheDestroyer = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set the Destroyer Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage(MKLP.GetText("set Mechdusa Schedule" +
                                            $"\n{stringsched}"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowTheTwins = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set The Twins Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage(MKLP.GetText("set Mechdusa Schedule" +
                                            $"\n{stringsched}"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowSkeletronPrime = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Skeletron Prime Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is only available on zenith seed!"));
                                        return;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowMechdusa = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Mechdusa Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "plantera":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowPlantera = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Plantera Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "golem":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowGolem = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Golem Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowDukeFishron = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Duke Fishron Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowLunaticCultist = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Lunatic Cultist Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEmpressOfLight = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Empress Of Light Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowMoonLord = schedule;
                                    args.Player.SendInfoMessage(MKLP.GetText("set Moon Lord Schedule" +
                                        $"\n{stringsched}"));
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage(MKLP.GetText("Please specify the boss!"));
                                    return;
                                }
                        }
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableSched )
                case "disableenableschedule":
                case "disablesched":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Please specify a boss!");
                            return;
                        }
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowKingSlime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled King Slime Schedule"));
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Eye Of Cthulhu Schedule"));
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                                    MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Eater Of Worlds & Brain Of Cthulhu Schedule"));
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Eater Of Worlds Schedule"));
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Brain Of Cthulhu Schedule"));
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Skeletron Schedule"));
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowQueenBee = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Queen Bee Schedule"));
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowDeerclops = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Deerclops Schedule"));
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Wall Of Flesh Schedule"));
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowQueenSlime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Queen Slime Schedule"));
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage(MKLP.GetText("Cancelled Mechdusa Schedule"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowTheDestroyer = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled the Destroyer Schedule"));
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage(MKLP.GetText("Cancelled Mechdusa Schedule"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowTheTwins = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled The Twins Schedule"));
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        MKLP.Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage(MKLP.GetText("Cancelled Mechdusa Schedule"));
                                        break;
                                    }
                                    MKLP.Config.BossManager.ScheduleAllowSkeletronPrime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Skeletron Prime Schedule"));
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Mechdusa is only available on zenith seed!"));
                                        return;
                                    }

                                    MKLP.Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Mechdusa Schedule"));
                                    break;
                                }
                            case "plantera":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowPlantera = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Plantera Schedule"));
                                    break;
                                }
                            case "golem":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowGolem = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Golem Schedule"));
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowDukeFishron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Duke Fishron Schedule"));
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowLunaticCultist = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Lunatic Cultist Schedule"));
                                    break;
                                }
                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowEmpressOfLight = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Empress Of Light Schedule"));
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    MKLP.Config.BossManager.ScheduleAllowMoonLord = DateTime.MaxValue;
                                    args.Player.SendInfoMessage(MKLP.GetText("Cancelled Moon Lord Schedule"));
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage(MKLP.GetText("Please specify the boss!"));
                                    return;
                                }
                        }
                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableSchedall )
                case "disableenablescheduleall":
                case "disableschedall":
                    {

                        MKLP.Config.BossManager.ScheduleAllowKingSlime = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowQueenBee = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowDeerclops = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowWallOfFlesh = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowQueenSlime = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowTheDestroyer = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowTheTwins = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowSkeletronPrime = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowPlantera = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowGolem = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowDukeFishron = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowEmpressOfLight = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowLunaticCultist = DateTime.MaxValue;
                        MKLP.Config.BossManager.ScheduleAllowMoonLord = DateTime.MaxValue;

                        args.Player.SendInfoMessage(MKLP.GetText("Cancelled All Boss Schedule"));

                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : usingschedule )
                case "usingschedule":
                case "usingsched":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText("Proper Usage: /mboss usingschedule <yes/no>"));
                            return;
                        }

                        switch (args.Parameters[1].ToLower())
                        {
                            case "true":
                            case "yes":
                                {
                                    MKLP.Config.BossManager.UseBossSchedule = true;
                                    args.Player.SendInfoMessage(MKLP.GetText("You are now using the boss schedule"));
                                    break;
                                }
                            case "false":
                            case "no":
                                {
                                    MKLP.Config.BossManager.UseBossSchedule = false;
                                    args.Player.SendInfoMessage(MKLP.GetText("You are no longer using the boss schedule"));
                                    break;
                                }
                            default:
                                {
                                    if ((bool)MKLP.Config.BossManager.UseBossSchedule)
                                    {
                                        MKLP.Config.BossManager.UseBossSchedule = false;
                                        args.Player.SendInfoMessage(MKLP.GetText("You are no longer using the boss schedule"));
                                        break;
                                    }
                                    else
                                    {
                                        MKLP.Config.BossManager.UseBossSchedule = true;
                                        args.Player.SendInfoMessage(MKLP.GetText("You are now using the boss schedule"));
                                        break;
                                    }
                                }
                        }

                        MKLP.Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : ResetSchedule )
                case "resetschedule":
                case "resetsched":
                    {
                        if (args.Parameters.Count != 1)
                        {
                            DateTime gettoday;
                            if (!DateTime.TryParse(args.Parameters[1], out gettoday))
                            {
                                args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                                return;
                            }

                            DateTime today = new(gettoday.Year, gettoday.Month, gettoday.Day, (int)MKLP.Config.BossManager.Default_ScheduleDay_Hour, 0, 0);

                            string[] getmonth_str = {
                                "Jan",
                                "feb",
                                "Mar",
                                "Apr",
                                "May",
                                "Jun",
                                "Jul",
                                "Aug",
                                "Sep",
                                "Oct",
                                "Nov",
                                "Dec"
                            };

                            string resultstr = $"{getmonth_str[today.Month - 1]} {today.Day}, {today.Year}";

                            //DateTime today = DateTime.Parse($"{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{DateTime.UtcNow.Year}");

                            MKLP.Config.BossManager.ScheduleAllowKingSlime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowKingSlime);
                            MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEyeOfCthulhu);
                            MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEaterOfWorlds);
                            MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowBrainOfCthulhu);
                            MKLP.Config.BossManager.ScheduleAllowQueenBee = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowQueenBee);
                            MKLP.Config.BossManager.ScheduleAllowSkeletron = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowSkeletron);
                            MKLP.Config.BossManager.ScheduleAllowDeerclops = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowDeerclops);
                            MKLP.Config.BossManager.ScheduleAllowWallOfFlesh = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowWallOfFlesh);
                            MKLP.Config.BossManager.ScheduleAllowQueenSlime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowQueenSlime);
                            MKLP.Config.BossManager.ScheduleAllowTheDestroyer = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowTheDestroyer);
                            MKLP.Config.BossManager.ScheduleAllowTheTwins = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowTheTwins);
                            MKLP.Config.BossManager.ScheduleAllowSkeletronPrime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowSkeletronPrime);
                            MKLP.Config.BossManager.ScheduleAllowMechdusa = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowMechdusa);
                            MKLP.Config.BossManager.ScheduleAllowPlantera = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowPlantera);
                            MKLP.Config.BossManager.ScheduleAllowGolem = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowGolem);
                            MKLP.Config.BossManager.ScheduleAllowDukeFishron = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowDukeFishron);
                            MKLP.Config.BossManager.ScheduleAllowEmpressOfLight = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEmpressOfLight);
                            MKLP.Config.BossManager.ScheduleAllowLunaticCultist = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowLunaticCultist);
                            MKLP.Config.BossManager.ScheduleAllowMoonLord = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowMoonLord);



                            args.Player.SendInfoMessage(MKLP.GetText($"Reload Boss Schedule" +
                                $"\nStarting Point: {resultstr}"));

                            MKLP.Config.Changeall();
                            Config.Read();
                            return;
                        }
                        else
                        {
                            DateTime today = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, (int)MKLP.Config.BossManager.Default_ScheduleDay_Hour, 0, 0);

                            //DateTime today = DateTime.Parse($"{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{DateTime.UtcNow.Year}");

                            MKLP.Config.BossManager.ScheduleAllowKingSlime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowKingSlime);
                            MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEyeOfCthulhu);
                            MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEaterOfWorlds);
                            MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowBrainOfCthulhu);
                            MKLP.Config.BossManager.ScheduleAllowQueenBee = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowQueenBee);
                            MKLP.Config.BossManager.ScheduleAllowSkeletron = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowSkeletron);
                            MKLP.Config.BossManager.ScheduleAllowDeerclops = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowDeerclops);
                            MKLP.Config.BossManager.ScheduleAllowWallOfFlesh = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowWallOfFlesh);
                            MKLP.Config.BossManager.ScheduleAllowQueenSlime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowQueenSlime);
                            MKLP.Config.BossManager.ScheduleAllowTheDestroyer = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowTheDestroyer);
                            MKLP.Config.BossManager.ScheduleAllowTheTwins = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowTheTwins);
                            MKLP.Config.BossManager.ScheduleAllowSkeletronPrime = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowSkeletronPrime);
                            MKLP.Config.BossManager.ScheduleAllowMechdusa = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowMechdusa);
                            MKLP.Config.BossManager.ScheduleAllowPlantera = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowPlantera);
                            MKLP.Config.BossManager.ScheduleAllowGolem = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowGolem);
                            MKLP.Config.BossManager.ScheduleAllowDukeFishron = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowDukeFishron);
                            MKLP.Config.BossManager.ScheduleAllowEmpressOfLight = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowEmpressOfLight);
                            MKLP.Config.BossManager.ScheduleAllowLunaticCultist = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowLunaticCultist);
                            MKLP.Config.BossManager.ScheduleAllowMoonLord = today.AddDays((double)MKLP.Config.BossManager.Default_ScheduleDay_AllowMoonLord);

                            args.Player.SendInfoMessage("Reset Boss Schedule");

                            MKLP.Config.Changeall();
                            Config.Read();
                            return;
                        }


                    }
                #endregion
                default:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Invalid Sub-Command!" +
                            $"\ndo '{Commands.Specifier}manageboss help' for more info"));
                        return;
                    }
            }

            #endregion
        }

        private static void CMD_Vanish(CommandArgs args)
        {
            #region code
            //args.TPlayer.active = !args.TPlayer.active;

            if (args.Player.ContainsData("MKLP_Vanish"))
            {
                if (!args.Player.GetData<bool>("MKLP_Vanish"))
                {
                    MKLP.TogglePlayerVanish(args.Player, true);

                    foreach (var player in TShock.Players)
                    {
                        if (player != null && player.Active && player != args.Player)
                        {
                            player.SendInfoMessage($"{args.Player.Name} has left.", Color.Yellow);
                        }
                    }
                    args.Player.SendSuccessMessage(MKLP.GetText("You're on Vanish"));
                }
                else
                {
                    MKLP.TogglePlayerVanish(args.Player, false);

                    foreach (var player in TShock.Players)
                    {
                        if (player != null && player.Active && player != args.Player)
                        {
                            player.SendInfoMessage($"{args.Player.Name} has joined.", Color.Yellow);
                        }
                    }
                    args.Player.SendSuccessMessage(MKLP.GetText("You're no longer on Vanish"));
                }
            }
            else
            {
                MKLP.TogglePlayerVanish(args.Player, true);

                foreach (var player in TShock.Players)
                {
                    if (player != null && player.Active && player != args.Player)
                    {
                        player.SendInfoMessage($"{args.Player.Name} has left.", Color.Yellow);
                    }
                }
                args.Player.SendSuccessMessage(MKLP.GetText("You're on Vanish"));
            }

            #endregion
        }

        #endregion

        #region { Moderator }

        private static void CMD_ManageReport(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}managereport <info/delete>"));
                args.Player.SendMessage(MKLP.GetText(
                    $"'{Commands.Specifier}report info <id>': get report info by id" +
                    $"\n'{Commands.Specifier}report info fromlist <accountname>' : get this user list of his reports" +
                    $"\n'{Commands.Specifier}report info targetlist <targetname>' : get list of report from this player" +
                    "\n" +
                    $"\n'{Commands.Specifier}report delete <id>': delete any reports by id"), Color.WhiteSmoke);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "info":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            return;
                        }

                        int getid = -1;

                        if (int.TryParse(args.Parameters[1], out getid))
                        {
                            try
                            {
                                MKLP_Report getreport = MKLP.DBManager.GetReportByID(getid);

                                args.Player.SendMessage($"Report id: {getreport.ID} Info" +
                                    $"\nFrom: [c/ff7c34:{getreport.From}]" +
                                    $"\nTarget: [c/ff7c34:{(getreport.Target == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{getreport.Target}")}]" +
                                    $"\nLocation: [c/ff7c34:{getreport.Location}]" +
                                    $"\nPlayers Online: [c/ff7c34:{getreport.Players.Replace($"{DiscordKLP.S_}", ", ")}]" +
                                    $"\n" +
                                    $"\nMessage: [c/ff7c34:{getreport.Message}]" +
                                    $"\n" +
                                    $"\nReported Since: [c/ff7c34:{getreport.Since}]"
                                    , Color.OrangeRed);
                                return;
                            }
                            catch (NullReferenceException)
                            {
                                args.Player.SendErrorMessage(MKLP.GetText("No reports found with this ID"));
                                return;
                            }
                        }
                        else
                        {
                            switch (args.Parameters[1])
                            {
                                case "fromlist":
                                    {
                                        if (args.Parameters.Count == 2)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}managereport info fromlist <accountname>"));
                                            return;
                                        }

                                        MKLP_Report[] getreport = MKLP.DBManager.GetReport(from: args.Parameters[2]).ToArray();

                                        if (getreport.Count() <= 0)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText($"No reports from {args.Parameters[2]}"));
                                            return;
                                        }

                                        string result = "";

                                        foreach (MKLP_Report r in getreport)
                                        {
                                            result += $"[c/ff7c34:<{r.ID}> target {r.Target}]\n";
                                        }

                                        args.Player.SendMessage(MKLP.GetText($"Report List from [ {args.Parameters[2]} ]") +
                                            $"\n{result}", Color.OrangeRed);

                                        return;
                                    }
                                case "targetlist":
                                    {
                                        if (args.Parameters.Count == 2)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}managereport info target <accountname>"));
                                            return;
                                        }

                                        MKLP_Report[] getreport = MKLP.DBManager.GetReport(target: args.Parameters[2]).ToArray();

                                        if (getreport.Count() <= 0)
                                        {
                                            args.Player.SendErrorMessage(MKLP.GetText($"{args.Parameters[2]} has no reports from someone"));
                                            return;
                                        }

                                        string result = "";

                                        foreach (MKLP_Report r in getreport)
                                        {
                                            result += $"[c/ff7c34:<{r.ID}> from {r.From}]\n";
                                        }

                                        args.Player.SendMessage(MKLP.GetText($"Players Report List from [ {args.Parameters[2]} ]") +
                                            $"\n{result}", Color.OrangeRed);

                                        return;
                                    }
                                default:
                                    {
                                        args.Player.SendErrorMessage(MKLP.GetText("Invalid Report ID"));
                                        return;
                                    }
                            }
                        }
                    }
                case "delete":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}managereport delete <reportID>"));
                            return;
                        }

                        int getid = -1;

                        if (int.TryParse(args.Parameters[1], out getid))
                        {
                            if (MKLP.DBManager.DeleteReport(getid))
                            {
                                args.Player.SendSuccessMessage(MKLP.GetText($"Successfully deleted report no. {getid}"));
                                return;
                            }
                            else
                            {
                                args.Player.SendSuccessMessage(MKLP.GetText($"Unable to delete report no. {getid}"));
                                return;
                            }
                        }

                        return;
                    }
                default:
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText($"Invalid Sub-Command"));
                        args.Player.SendMessage(MKLP.GetText($"Do '{Commands.Specifier}report help' for more info"), Color.WhiteSmoke);
                        return;
                    }
            }
            #endregion
        }

        private static void CMD_BanInfo(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}baninfo <ticketban>"));
                return;
            }

            if (args.Parameters[0].ToLower() == "list")
            {
                string PickColorForBan(Ban ban)
                {
                    double hoursRemaining = (ban.ExpirationDateTime - DateTime.UtcNow).TotalHours;
                    double hoursTotal = (ban.ExpirationDateTime - ban.BanDateTime).TotalHours;
                    double percentRemaining = TShock.Utils.Clamp(hoursRemaining / hoursTotal, 100, 0);

                    int red = TShock.Utils.Clamp((int)(255 * 2.0f * percentRemaining), 255, 0);
                    int green = TShock.Utils.Clamp((int)(255 * (2.0f * (1 - percentRemaining))), 255, 0);

                    return $"{red:X2}{green:X2}{0:X2}";
                }

                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pageNumber))
                {
                    args.Player.SendMessage(MKLP.GetText($"Invalid Ban page"), Color.White);
                    return;
                }

                var bans = from bban in TShock.Bans.Bans
                           where bban.Value.ExpirationDateTime > DateTime.UtcNow
                           orderby bban.Value.ExpirationDateTime ascending
                           select $"[{bban.Key.Color(TShockAPI.Utils.GreenHighlight)}] {bban.Value.Identifier.Color(PickColorForBan(bban.Value))}";

                PaginationTools.SendPage(args.Player, pageNumber, bans.ToList(),
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Bans ({{0}}/{{1}}):",
                        FooterFormat = MKLP.GetText("Type " + Commands.Specifier + "ban list {{0}} for more."),
                        NothingToDisplayString = MKLP.GetText("There are currently no active bans.")
                    });
                return;
            }

            int targetid;
            if (!int.TryParse(args.Parameters[0], out targetid))
            {
                args.Player.SendErrorMessage(MKLP.GetText("Invalid ticket number"));
                return;
            }

            Ban ban = TShock.Bans.GetBanById(targetid);

            if (ban == null)
            {
                args.Player.SendErrorMessage(MKLP.GetText("No bans found"));
                return;
            }

            args.Player.SendMessage(MKLP.GetText("[c/3bf000:Ban Info]") +
                $"\nTicket No. : [c/84ff5b:{ban.TicketNumber}]" +
                $"\nIdentifier : [c/84ff5b:{ban.Identifier}]" +
                $"\nBanned by : [c/84ff5b:{ban.BanningUser}]" +
                $"\nReason : [c/84ff5b:{ban.Reason}]" +
                $"\n{(ban.ExpirationDateTime < DateTime.UtcNow ? $":Ban expired : [c/84ff5b:{ban.ExpirationDateTime.ToString("yyyy/MM/dd")} ({ban.GetPrettyExpirationString()} ago)]" : "")}"
                , Color.Green);

            /*
            switch (args.Parameters[0])
            {
                default:
                    {
                        return;
                    }
            }
            */
            #endregion
        }
        private static void CMD_Ban(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}{((bool)MKLP.Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} <player> <reason> <duration> <args...>"));
                args.Player.SendMessage(MKLP.GetText($"[c/8fbfd4:Example:] [c/a5d063:{Commands.Specifier}{((bool)MKLP.Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} {args.Player.Name} \"cheating\" \"1d 1m\" -offline]" +
                    $"\n[c/8fbfd4:duration:] 1d = 1day (d,h,m,s = day,hour,minute,second)" +
                    $"\n" +
                    $"\n[c/8fbfd4:args:] " +
                    $"\n( -alt = bans only name )" +
                    $"\n( -account = only used when banning a offline player )" +
                    $"\n( -accountid = only used when banning a offline player account id )" +
                    ((bool)MKLP.Config.Main.UsingBanGuardPlugin ? $"\n( -banguard = must have a category to use banguard ex.'/ban \"{args.Player.Name}\" -banguard hacks' )\n( -banguardauto = automatically assign banguard category from your reason must be accurate )" : "")),
                    Color.Gray);
                return;
            }

            bool altban = args.Parameters.Any(p => p == "-alt");
            bool accountban = args.Parameters.Any(p => p == "-account");
            bool accountidban = args.Parameters.Any(p => p == "-accountid");
            bool usingbanguardauto = args.Parameters.Any(p => p == "-banguardauto");
            int usingbanguardi = args.Parameters.FindIndex(p => p == "-banguard") + 1;
            string usingbanguardcat = "N/A";

            if (usingbanguardi > 0 && usingbanguardi < args.Parameters.Count)
            {
                usingbanguardcat = args.Parameters[usingbanguardi];
            }

            bool uuidip = true;

            if (altban == true) uuidip = false;


            List<string> flags = new List<string>() { "-alt", "-account", "-accountid", "-banguardauto", "-banguard" };

            string reason = MKLP.GetText("No Reason Specified");
            string duration = null;
            DateTime expiration = DateTime.MaxValue;

            for (int i = 1; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param) || (usingbanguardcat != "N/A" && i == usingbanguardi))
                {
                    reason = param;
                    break;
                }
            }
            for (int i = 2; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param) || (usingbanguardcat != "N/A" && i == usingbanguardi))
                {
                    duration = param;
                    break;
                }
            }

            if (TShock.Utils.TryParseTime(duration, out ulong seconds))
            {
                expiration = DateTime.UtcNow.AddSeconds(seconds);
            }

            if (!(bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                if (usingbanguardauto)
                {
                    usingbanguardcat = BanGuardAPI.GetCategoryFromReason(reason);
                }
                if (usingbanguardcat != "N/A")
                {
                    if (!BanGuardAPI.IsCategory(usingbanguardcat))
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Invalid BanGuard Category!"));
                        return;
                    }
                }
            }

            if (accountidban)
            {
                if (!args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineBan))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to ban offline players!"));
                    return;
                }

                int accountidtarget = 0;

                if (!int.TryParse(args.Parameters[0], out accountidtarget))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Number!"));
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(accountidtarget);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account ID {0} doesn't exist", accountidtarget));
                    return;
                }

                var players = TSPlayer.FindByNameOrID(targetaccount.Name);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (usingbanguardcat != "N/A" && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
                {
                    args.Player.SendInfoMessage(MKLP.GetText("Using BanGuard Ban..."));
                }

                if (targetplayer != null)
                {
                    if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Successfully banned {0} for {1}", targetplayer.Name, reason));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Error occur banning {0}", targetplayer.Name));
                    }
                }
                else
                {
                    if (ManagePlayer.OfflineBan(targetaccount, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Successfully banned Acc: {0} for {1}", targetaccount.Name, reason));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Error occur banning Acc: {0}", targetaccount.Name));
                    }
                }

            }
            else if (accountban)
            {
                if (!args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineBan))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to ban offline players!"));
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account name {0} doesn't exist", args.Parameters[0]));
                    return;
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }


                if (usingbanguardcat != "N/A" && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
                {
                    args.Player.SendInfoMessage(MKLP.GetText("Using BanGuard Ban..."));
                }

                if (targetplayer != null)
                {
                    if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Successfully banned {0} for {1}", targetplayer.Name, reason));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Error occur banning {0}", targetplayer.Name));
                    }
                }
                else
                {
                    if (ManagePlayer.OfflineBan(targetaccount, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Successfully banned Acc: {0} for {1}", targetaccount.Name, reason));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Error occur banning Acc: {0}", targetaccount.Name));
                    }
                }

            }
            else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                    return;
                }

                var targetplayer = players[0];


                if (usingbanguardcat != "N/A" && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
                {
                    args.Player.SendInfoMessage(MKLP.GetText("Using BanGuard Ban..."));
                }

                if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Successfully banned {0} for {1}", targetplayer.Name, reason));
                }
                else
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Error occur banning {0}", targetplayer.Name));
                }
            }
            #endregion
        }

        private static void CMD_UnBan(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}unban <ticket number>" +
                    $"\nor use '{Commands.Specifier}unban <account name> -account' to unban a account" +
                    $"\nor use '{Commands.Specifier}unban <account id> -accountid' to unban a account"));
                return;
            }

            bool accountunban = args.Parameters.Any(p => p == "-account");
            bool accountidunban = args.Parameters.Any(p => p == "-accountid");

            if (accountidunban)
            {
                int accountid = 0;

                if (!int.TryParse(args.Parameters[0], out accountid))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Number!"));
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(accountid);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Account"));
                    return;
                }

                if (ManagePlayer.UnBanAccount(targetaccount, args.Player.Name))
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Removing Ban Tickets from account: {0}", targetaccount.Name));
                    return;
                }
                else
                {
                    args.Player.SendErrorMessage(MKLP.GetText("AccountID: '{0}' could not be found...", accountid));
                    return;
                }

            }
            else if (accountunban)
            {
                string targetname = string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count);
                targetname = targetname.Replace(" -account", "");
                targetname = targetname.Replace("-account", "");

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(targetname);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Account"));
                    return;
                }

                if (ManagePlayer.UnBanAccount(targetaccount, args.Player.Name))
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Removing Ban Tickets from account: {0}", targetaccount.Name));
                    return;
                }
                else
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account: '{0}' could not be found...", targetname));
                    return;
                }

            }
            else
            {
                int ticketnumber = -1;

                if (int.TryParse(args.Parameters[0], out ticketnumber))
                {
                    if (ManagePlayer.UnBanTicketNumber(ticketnumber, args.Player.Account.Name))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Removed Ban Ticket Number: " + ticketnumber));
                        return;
                    }
                    else
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Invalid Ticket number!"));
                        return;
                    }
                }
                else
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Invalid Ticket number!"));
                    return;
                }
            }

            #endregion
        }

        private static void CMD_Mute(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}{((bool)MKLP.Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <player> <duration> <reason>" +
                    (args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineUnMute) ? $"\nMuting Offline Player: {Commands.Specifier}{((bool)MKLP.Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <accountname> <duration> <reason> -account" : "")));
                return;
            }

            bool offlineMute = args.Parameters.Any(p => p == "-account");

            List<string> flags = new List<string>() { "-account" };

            string reason = "";
            string duration = null;
            DateTime expiration = DateTime.MaxValue;

            for (int i = 2; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    reason = param;
                    break;
                }
            }
            for (int i = 1; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    duration = param;
                    break;
                }
            }

            if (TShock.Utils.TryParseTime(duration, out ulong seconds))
            {
                expiration = DateTime.UtcNow.AddSeconds(seconds);
            }


            if (offlineMute)
            {
                if (!args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineMute))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to mute offline players"));
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account name {0} doesn't exist", args.Parameters[0]));
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (targetplayer != null)
                {
                    if (targetplayer.mute)
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("{0} was already muted!", targetplayer.Name));
                        return;
                    }

                    if (ManagePlayer.OnlineMute(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Muted {0}", targetplayer.Name));
                    }
                    else
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Error occur Muting {0}", targetplayer.Name));
                    }

                }
                else
                {
                    if (ManagePlayer.OfflineMute(targetaccount, reason, args.Player.Account.Name, expiration))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Muted {0}", targetaccount.Name));
                    }
                    else
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Error occur Muting {0}", targetaccount.Name));
                    }
                }



            }
            else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                    return;
                }

                var targetplayer = players[0];

                if (targetplayer.mute)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("{0} was already muted!", targetplayer.Name));
                    return;
                }

                if (ManagePlayer.OnlineMute(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration))
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("muted {0}", targetplayer.Name));
                }
                else
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Error occur Muting {0}", targetplayer.Name));
                }
            }
            //DBManager.CheckPlayer(player, true);
            #endregion
        }

        private static void CMD_UnMute(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}unmute <player>" +
                    (args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineUnMute) ? $"\nUnmuting Offline Player: {Commands.Specifier}unmute <accountname> -account" : "")));
                return;
            }

            bool offlineMute = args.Parameters.Any(p => p == "-account");

            if (offlineMute)
            {
                if (!args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineMute))
                {
                    args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to mute offline players"));
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Account name {0} doesn't exist", args.Parameters[0]));
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (targetplayer != null)
                {
                    if (!targetplayer.mute)
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("{0} hasn't been muted!", targetplayer.Name));
                        return;
                    }

                    if (ManagePlayer.OnlineUnMute(args.Silent, targetplayer, args.Player.Account.Name))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("unmuted {0}", targetplayer.Name));
                    }
                    else
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Error occur unmuting {0}", targetplayer.Name));
                    }

                }
                else
                {
                    if (ManagePlayer.OfflineUnMute(targetaccount, args.Player.Account.Name))
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("unmuted {0}", targetaccount.Name));
                    }
                    else
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Error occur unmuting {0}", targetaccount.Name));
                    }
                }



            }
            else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                    return;
                }

                var targetplayer = players[0];

                if (!targetplayer.mute)
                {
                    args.Player.SendErrorMessage(MKLP.GetText("{0} hasn't been muted!", targetplayer.Name));
                    return;
                }

                if (ManagePlayer.OnlineUnMute(args.Silent, targetplayer, args.Player.Account.Name))
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Unmuted {0}", targetplayer.Name));
                }
                else
                {
                    args.Player.SendSuccessMessage(MKLP.GetText("Error occur unmuting {0}", targetplayer.Name));
                }
            }
            //DBManager.CheckPlayer(player, true);
            #endregion
        }

        private static void CMD_disable(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}disable <player> <reason>"));
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                return;
            }

            if (players.Count < 1)
            {
                args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                return;
            }

            var targetplayer = players[0];

            if (args.Parameters.Count == 1)
            {

                ManagePlayer.DisablePlayer(targetplayer, executername: args.Player.Name);
                args.Player.SendSuccessMessage(MKLP.GetText("Player {0} disabled", targetplayer.Name));

            }
            else
            {
                ManagePlayer.DisablePlayer(targetplayer, args.Parameters[1], executername: args.Player.Name);
                args.Player.SendSuccessMessage(MKLP.GetText("Player {0} disabled for {1}", targetplayer.Name, args.Parameters[1]));

            }

            return;
            #endregion
        }

        private static void CMD_undisable(CommandArgs args)
        {
            #region code
            bool enableoffline = args.Parameters.Any(p => p == "-offline");

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}enable <player>" +
                    (args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineEnable) ? $"\nEnable Offline by '{Commands.Specifier}enable <playername> -offline'" : "")));
                return;
            }

            if (enableoffline && !args.Player.HasPermission(MKLP.Config.Permissions.CMD_OfflineEnable))
            {
                args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to Enable Offline Players"));
            }

            IEnumerable<string> mplayers;
            string targetplayername;

            switch (ManagePlayer.UnDisablePlayer(args.Parameters[0], enableoffline, false, out targetplayername, out mplayers, args.Player.Name))
            {
                case ManagePlayer.DisableResult.Success:
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Player {0} enabled", targetplayername));
                        break;
                    }
                case ManagePlayer.DisableResult.SuccessOffline:
                    {
                        args.Player.SendSuccessMessage(MKLP.GetText("Offline Player {0} enabled", targetplayername));
                        break;
                    }
                case ManagePlayer.DisableResult.MultiplePlayerMatch:
                    {
                        args.Player.SendMultipleMatchError(mplayers);
                        break;
                    }
                case ManagePlayer.DisableResult.NotFoundOffline:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                        break;
                    }
                case ManagePlayer.DisableResult.AlreadyEnabled:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Player {0} isn't disabled", targetplayername));
                        break;
                    }
                default:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("an error occur"));
                        break;
                    }
            }
            #endregion
        }

        private static void CMD_disablelist(CommandArgs args)
        {
            #region code
            var DisableList = MKLP.DisabledKey.Select(dp => dp.Key);

            int page = 1;

            if (args.Parameters.Count >= 1) { int.TryParse(args.Parameters[0], out page); }

            PaginationTools.SendPage(args.Player, page, DisableList.ToList(), new PaginationTools.Settings()
            {
                HeaderFormat = string.Format(MKLP.GetText("Disable Players page ({{0}}/{{1}}):")),
                FooterFormat = string.Format(MKLP.GetText("Type '" + Commands.Specifier + "disablelist {{0}}' for more.")),
                NothingToDisplayString = MKLP.GetText("No Players are currently disabled...")
            });

            #endregion
        }

        #endregion

        #region { Inspect }

        private static void CMD_Spy(CommandArgs args)
        {
            #region code
            var godPower = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.GodmodePower>();

            if (args.Parameters.Count == 0)
            {
                if (args.Player.ContainsData("MKLP_TargetSpy"))
                {
                    MKLP.TogglePlayerVanish(args.Player, false);

                    args.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                    godPower.SetEnabledState(args.Player.Index, false);

                    args.Player.RemoveData("MKLP_TargetSpy");

                    args.Player.SendInfoMessage(MKLP.GetText("Your no longer spying on someone"));
                    return;
                }
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}spy <player>"));
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                return;
            }

            if (players.Count < 1)
            {
                args.Player.SendErrorMessage(MKLP.GetText("Could not find the target specified. Check that you have the correct spelling."));
                return;
            }

            TSPlayer player = players[0];

            MKLP.TogglePlayerVanish(args.Player, true);

            args.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

            godPower.SetEnabledState(args.Player.Index, true);

            args.Player.SetData("MKLP_TargetSpy", player);

            args.Player.SendInfoMessage(MKLP.GetText("Spying {0}", player.Name));
            return;

            #endregion
        }

        private static void CMD_uuidmatch(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}uuidmatch <accountname>"));
                return;
            }

            UserAccount getuser = TShock.UserAccounts.GetUserAccountByName(string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count));

            if (getuser == null)
            {
                args.Player.SendErrorMessage(MKLP.GetText("Invalid Player"));
                return;
            }

            var getresult = MKLP.GetMatchUUID_UserAccount(getuser.Name, getuser.UUID);

            if (getresult.Count == 0)
            {
                args.Player.SendWarningMessage(MKLP.GetText("No Accounts Match"));
            }
            else
            {
                string result = "";

                foreach (var get in getresult)
                {
                    result += get.ID + " : " + get.Name + "\n";
                }

                args.Player.SendMessage(MKLP.GetText("Following Users match their UUID:") +
                    "\n" + result, Color.Gray);
            }
            #endregion
        }


        private static void CMD_Logs(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Use '/logs help' for more info.");
                return;
            }


            switch (args.Parameters[0].ToLower())
            {
                #region [ Help Text ]
                case "help":
                    {
                        return;
                    }
                #endregion
                #region [ sub-command : tile ]
                case "tile":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("do '/logs tile help' for more info.");
                            return;
                        }

                        Vector2 getpos;
                        int getdistance = 10;
                        DateTime timedateprev = DateTime.Now;
                        DateTime timedate = timedateprev;


                        int getpage;

                        switch (args.Parameters[1].ToLower())
                        {
                            case "help":
                                {
                                    args.Player.SendMessage(
                                        "=== tile Sub-Commands ===" +
                                        "\n'/logs tile pos <x> <y> <distance> <page> <date/time>' : shows the log near on that specific position you choose" +
                                        "\n" +
                                        "\n'/log tile near <distance> <page> <date/time>' : shows the log near your standing on" +
                                        "\n" +
                                        "\n---------------" +
                                        "\nDistance Ex. 30 = get log 30 blocks near you" +
                                        "\n---------------" +
                                        "\nDate/Time Ex. MM/DD/YYYY" +
                                        "\n---------------"
                                        , Color.Gray);
                                    return;
                                }
                            case "pos":
                                {
                                    if (args.Parameters.Count < 4)
                                    {
                                        args.Player.SendErrorMessage("Proper Usage: /logs tile pos <x> <y> <page> <datetime>");
                                        return;
                                    }

                                    int x;
                                    int y;

                                    if (!int.TryParse(args.Parameters[2], out x))
                                    {
                                        args.Player.SendErrorMessage("Invalid X Position!");
                                        return;
                                    }
                                    if (!int.TryParse(args.Parameters[3], out y))
                                    {
                                        args.Player.SendErrorMessage("Invalid Y Position!");
                                        return;
                                    }

                                    getpos = new(x, y);


                                    getpage = StringPageKLP.ParsePage(4, args.Parameters.ToArray());

                                    if (args.Parameters.Count >= 6)
                                    {
                                        if (!DateTime.TryParse(args.Parameters[5], out timedate))
                                        {
                                            args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                                            return;
                                        }
                                    }
                                    break;
                                }
                            case "near":
                                {

                                    getpage = StringPageKLP.ParsePage(2, args.Parameters.ToArray());

                                    if (args.Parameters.Count >= 4)
                                    {
                                        if (!DateTime.TryParse(args.Parameters[3], out timedate))
                                        {
                                            args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                                            return;
                                        }
                                    }

                                    getpos = new (args.Player.TileX, args.Player.TileY);
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please Specify a Sub-command!" +
                                        "\ndo '/logs tile help' for more info.");
                                    return;
                                }
                        }

                        string filepath = LogKLP.GetPath(LogKLP.LogPath_Tile, timedate);

                        if (timedate == timedateprev)
                        {
                            int breakf = 0;
                            while (!File.Exists(filepath))
                            {
                                if (breakf >= 8)
                                {
                                    args.Player.SendErrorMessage("Unable to find log");
                                    return;
                                }
                                breakf++;
                                timedate = timedate.AddDays(-1);
                                filepath = LogKLP.GetPath(LogKLP.LogPath_Tile, timedate);
                            }
                        }
                        else
                        {
                            if (!File.Exists(filepath))
                            {
                                args.Player.SendErrorMessage("tile log does not exist!");
                                return;
                            }
                        }

                        

                        string[] text = LogKLP.GetLog_Tile(filepath, getpos, getdistance).Select(get => 
                            $"\n=== {get.Key} ===" +
                            $"\n{string.Join("\n", get.Value.Select(e => $"- ({e.Key.X}, {e.Key.Y}) {e.Value}"))}"
                        ).ToArray();

                        StringPageKLP pageklp = new(6, text)
                        {
                            DisplayText = "=== Tile Log Info page ({0}/{1}) ==="
                        };

                        args.Player.SendMessage(
                            pageklp.GetDisplayText(getpage) +
                            $"= {getdistance} near position ({getpos.X}, {getpos.Y})\n" +
                            pageklp.GetText(getpage)
                            , Color.LightGray);


                        return;
                    }
                #endregion
                #region [ sub-command : sign ]
                case "sign":
                    {
                        DateTime timedateprev = DateTime.Now;
                        DateTime timedate = timedateprev;

                        if (args.Player.ContainsData("MKLP_CheckLog_Sign"))
                        {
                            string getdata = args.Player.GetData<string>("MKLP_CheckLog_Sign");

                            if (getdata == "OnTrack")
                            {

                                args.Player.SendErrorMessage("please select a sign you want to check" +
                                    "\ndo '/logs sign select' and interact any sign!");
                                return;
                            }

                            Vector2 getpos;

                            try
                            {
                                getpos = new(int.Parse(getdata.Split("|")[0]), int.Parse(getdata.Split("|")[0]));
                            } catch
                            {
                                args.Player.SendErrorMessage("Unable to get SignPOS please retry '/logs sign select'");
                                return;
                            }
                            int page = StringPageKLP.ParsePage(1, args.Parameters.ToArray());

                            string filepath = LogKLP.GetPath(LogKLP.LogPath_Sign, timedate);

                            if (timedate == timedateprev)
                            {
                                int breakf = 0;
                                while (!File.Exists(filepath))
                                {
                                    if (breakf >= 8)
                                    {
                                        args.Player.SendErrorMessage("Unable to find log");
                                        return;
                                    }
                                    breakf++;
                                    timedate = timedate.AddDays(-1);
                                    filepath = LogKLP.GetPath(LogKLP.LogPath_Tile, timedate);
                                }
                            }
                            else
                            {
                                if (!File.Exists(filepath))
                                {
                                    args.Player.SendErrorMessage("tile log does not exist!");
                                    return;
                                }
                            }

                            string[] text = LogKLP.GetLog_Sign(filepath, getpos).Select(
                                e => $"-- {e.Item1} | {e.Item2} --" +
                                $"\ntext: {e.Item3}"
                                ).ToArray();

                            StringPageKLP getpage = new(3, text)
                            {
                                DisplayText = $"SignLog | position: ({getpos.X}, {getpos.Y})"
                            };

                            args.Player.SendMessage(
                                getpage.GetDisplayText(page) + "\n" +
                                getpage.GetText(page)
                                , Color.WhiteSmoke);
                        } else
                        {
                            if (args.Parameters.Count == 1)
                            {
                                if (args.Parameters[1].ToLower() != "select")
                                {
                                    args.Player.SetData("MKLP_CheckLog_Sign", "OnTrack");
                                    args.Player.SendSuccessMessage("please interact a sign to select.");
                                    return;
                                }
                            }

                            args.Player.SendErrorMessage("please select a sign you want to check" +
                                "\ndo '/logs sign select' and interact any sign!");
                        }
                        return;
                    }
                    #endregion
            }


            #endregion
        }

        #endregion

        #region { Manager }

        private static void CMD_MKLPDiscord(CommandArgs args)
        {
            #region code

            if ((MKLP.Config.DataBaseDLink.StorageType != "sqlite" && MKLP.Config.DataBaseDLink.SqliteDBPath != Path.Combine(TShock.SavePath, "MKLP.sqlite")) || !(bool)MKLP.Config.DataBaseDLink.UsingDB)
            {
                args.Player.SendErrorMessage(MKLP.GetText("You cannot use this command" +
                    "\nas if 'UsingDB' and 'UsingMKLPDatabase' in Config file is set to false..."));
                return;
            }

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}mklpdiscord <type> <args...>" +
                    $"\nDo '{Commands.Specifier}mklpdiscord help' for more details"));
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                #region [ helptext ]
                case "help":
                    {
                        args.Player.SendMessage(MKLP.GetText(
                            $"Proper Usage: {Commands.Specifier}mklpdiscord <sub-command> <args...>" +
                            $"\n\n[ Sub Commands ]" +
                            $"\n[c/4089ff:'list <page>' :] list of accountlinked users" +
                            $"\n" +
                            $"\n[c/4089ff:'set <player> <userid>' :] sets or adds a accountlink user" +
                            $"\n" +
                            $"\n[c/4089ff:'remove <player>' :] removes a accountlink user")
                            , Color.WhiteSmoke);
                        return;
                    }
                #endregion
                case "list":
                    #region ( Type: List )
                    {
                        try
                        {

                            Dictionary<string, string> DLinkList = MKLP.DBManager.AccountDLinkingList();

                            decimal maxpage = Math.Ceiling((decimal)DLinkList.Count() / 10);

                            if (maxpage == 0)
                            {
                                args.Player.SendInfoMessage(MKLP.GetText("No Link Accounts Assigned..."));
                                return;
                            }

                            if (args.Parameters.Count == 2)
                            {
                                args.Player.SendInfoMessage(MKLP.GetText("Linked Accounts 1/{0}", maxpage) +
                                    $"\n{valuepage()}");
                                return;
                            }
                            else
                            {
                                try
                                {
                                    int page = int.Parse(args.Parameters[2]);
                                    args.Player.SendInfoMessage(MKLP.GetText("Linked Accounts {0}/{1}", page, maxpage) +
                                        $"\n{valuepage(page)}");
                                    return;
                                }
                                catch (Exception)
                                {
                                    args.Player.SendInfoMessage(MKLP.GetText("Linked Accounts 1/{0}", maxpage) +
                                        $"\n{valuepage()}");
                                    return;
                                }
                            }

                            string valuepage(int page = 1)
                            {
                                string result = "";
                                if (page <= 0) page = 1;

                                int i = 0;
                                int Index = (page - 1) * 6;
                                int max = Index + 6;
                                if (max > DLinkList.Count()) max = DLinkList.Count();
                                foreach (var Tvalue in DLinkList)
                                {
                                    i++;
                                    if (i < Index || i > max) continue;
                                    result += $"\n[ {Tvalue.Key} ]" +
                                        $"\nUserID: {Tvalue.Value}\n";
                                }

                                if (result == "") result = MKLP.GetText("Empty...");

                                return result;
                            }

                        }
                        catch { }

                        return;
                    }
                #endregion
                case "set":
                    #region ( Type: Set )
                    {
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage(MKLP.GetText($"Usage: {Commands.Specifier}mklpdiscord set <player> <userid>"));
                            return;
                        }

                        string targetname = "";

                        var getplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);
                        if (getplayers.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(getplayers.Select(p => p.Name));
                            return;
                        }
                        if (getplayers.Count < 1)
                        {
                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            if (getuseraccount == null)
                            {
                                args.Player.SendErrorMessage(MKLP.GetText("Invalid Player User"));
                                return;
                            }
                            targetname = getuseraccount.Name;
                        }
                        if (getplayers.Count == 1)
                        {
                            targetname = getplayers[0].Name;
                        }
                        if (targetname == "")
                        {
                            args.Player.SendErrorMessage(MKLP.GetText("Invalid Player User"));
                            return;
                        }

                        ulong useridtarget = 0;

                        if (!ulong.TryParse(args.Parameters[2], out useridtarget))
                        {
                            args.Player.SendErrorMessage(MKLP.GetText("Invalid UserID"));
                            return;
                        }

                        if (MKLP.DBManager.ChangeAccountDLinkingUserID(targetname, useridtarget.ToString()))
                        {
                            args.Player.SendSuccessMessage(MKLP.GetText("Change {0} UserID to {1}", targetname, useridtarget));
                            return;
                        }
                        else
                        {
                            if (MKLP.DBManager.AddAccountDLinkingUserID(targetname, useridtarget.ToString()))
                            {
                                args.Player.SendSuccessMessage(MKLP.GetText("Added new linked account {0} UserID: {1}", targetname, useridtarget));
                                return;
                            }
                            else
                            {
                                args.Player.SendSuccessMessage(MKLP.GetText("Unable to add new link account"));
                                return;
                            }
                        }
                    }
                #endregion
                case "remove":
                    #region ( Type: Removed )
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage($"Usage: {Commands.Specifier}mklpdiscord remove <player>");
                            return;
                        }

                        string targetname = "";

                        var getplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);
                        if (getplayers.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(getplayers.Select(p => p.Name));
                            return;
                        }
                        if (getplayers.Count < 1)
                        {
                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            if (getuseraccount == null)
                            {
                                args.Player.SendErrorMessage("Invalid Player User");
                                return;
                            }
                            targetname = getuseraccount.Name;
                        }
                        if (getplayers.Count == 0)
                        {
                            targetname = getplayers[0].Name;
                        }
                        if (targetname == "")
                        {
                            args.Player.SendErrorMessage("Invalid Player User");
                            return;
                        }

                        if (MKLP.DBManager.DeleteAccountDLinkingUserID(targetname))
                        {
                            args.Player.SendSuccessMessage($"Removed {targetname} accountlink");
                        }
                        else
                        {
                            args.Player.SendErrorMessage($"Unable to remove {targetname} accountlink" +
                                $"\nplease check '{Commands.Specifier}mklpdiscord list' if that name exist");
                        }

                        return;
                    }
                #endregion
                default:
                    {
                        args.Player.SendErrorMessage(MKLP.GetText("Invalid Sub-Command" +
                            $"\nDo '{Commands.Specifier}mklpdiscord help' for more info"));
                        return;
                    }
            }

            #endregion
        }

        #endregion

        #endregion

        #region [ Modified Commands ]

        private static void MCMD_AccountInfo(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                if ((bool)MKLP.Config.Main.Replace_AccountInfo_TShockCommand)
                {
                    args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}accountinfo <account name>"));
                }
                else
                {
                    args.Player.SendErrorMessage(MKLP.GetText($"Proper Usage: {Commands.Specifier}klpaccountinfo <account name>"));
                }
                return;
            }

            UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

            if (targetaccount == null)
            {
                args.Player.SendErrorMessage(MKLP.GetText("Invalid Account!"));
                return;
            }

            sendinfo(targetaccount);

            /*
            int targetaccid = -1;

            if (int.TryParse(args.Parameters[0], out targetaccid))
            {
                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(targetaccid);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage("Invalid Account!");
                    return;
                }

                sendinfo(targetaccount);
            } else
            {
                
            }

            */

            void sendinfo(UserAccount account)
            {
                ulong userid = 0;

                try
                {
                    userid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(account.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(account.Name);
                }
                catch { }


                List<string> iplist = JsonConvert.DeserializeObject<List<string>>(account.KnownIps?.ToString() ?? string.Empty);
                string lastknownip = iplist?[iplist.Count - 1] ?? "N/A";

                string UTC = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");

                args.Player.SendMessage(
                    $"== Account Info ==\n" +
                    $"Name: [c/ffffff:{account.Name}]\n" +
                    $"Account ID: [c/ffffff:{account.ID}]\n" +
                    $"group: [c/ffffff:{account.Group}]\n" +
                    $"\n{(userid != 0 ? $"Discord UserID: {userid}\n\n" : "")}" +
                    $"Last  known IP: [c/ffffff:{lastknownip}]\n" +
                    $"Last accessed: [c/ffffff:{account.LastAccessed} UTC{UTC}] [c/82ff91:{GetSince(DateTime.Parse(account.LastAccessed))}]\n" +
                    $"Registered Since: [c/ffffff:{account.Registered} UTC{UTC}] [c/82ff91:{GetSince(DateTime.Parse(account.Registered))}]",
                    Color.Gray);
            }

            string GetSince(DateTime Since)
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

        private static void MCMD_Playing(CommandArgs args)
        {
            #region code

            bool invalidUsage = (args.Parameters.Count > 2);

            bool displayIdsRequested = false;
            int pageNumber = 1;
            if (!invalidUsage)
            {
                foreach (string parameter in args.Parameters)
                {
                    if (parameter.Equals("-i", StringComparison.InvariantCultureIgnoreCase))
                    {
                        displayIdsRequested = true;
                        continue;
                    }

                    if (!int.TryParse(parameter, out pageNumber))
                    {
                        invalidUsage = true;
                        break;
                    }
                }
            }
            if (invalidUsage)
            {
                args.Player.SendMessage(MKLP.GetText("List Online Players Syntax"), Color.White);
                args.Player.SendMessage(MKLP.GetText($"{"playing".Color(TShockAPI.Utils.BoldHighlight)} {"[-i]".Color(TShockAPI.Utils.RedHighlight)} {"[page]".Color(TShockAPI.Utils.GreenHighlight)}"), Color.White);
                args.Player.SendMessage(MKLP.GetText($"Command aliases: {"playing".Color(TShockAPI.Utils.GreenHighlight)}, {"online".Color(TShockAPI.Utils.GreenHighlight)}, {"who".Color(TShockAPI.Utils.GreenHighlight)}"), Color.White);
                args.Player.SendMessage(MKLP.GetText($"Example usage: {"who".Color(TShockAPI.Utils.BoldHighlight)} {"-i".Color(TShockAPI.Utils.RedHighlight)}"), Color.White);
                return;
            }

            if (displayIdsRequested && !args.Player.HasPermission(Permissions.seeids))
            {
                args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to see player IDs."));
                return;
            }

            if (TShock.Utils.GetActivePlayerCount() == 0)
            {
                args.Player.SendMessage(MKLP.GetText("There are currently no players online."), Color.White);
                return;
            }
            args.Player.SendMessage($"Online Players ({TShock.Utils.GetActivePlayerCount().Color(TShockAPI.Utils.GreenHighlight)}/{TShock.Config.Settings.MaxSlots})", Color.White);

            var players = new List<string>();

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply != null && ply.Active)
                {
                    if (ply.ContainsData("MKLP_Vanish"))
                    {
                        if (ply.GetData<bool>("MKLP_Vanish")) continue;
                    }

                    if (displayIdsRequested)
                        if (ply.Account != null)
                            players.Add($"{ply.Name} (Index: {ply.Index}, Account ID: {ply.Account.ID})");
                        else
                            players.Add($"{ply.Name} (Index: {ply.Index})");
                    else
                        players.Add(ply.Name);
                }
            }

            PaginationTools.SendPage(
                args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(players),
                new PaginationTools.Settings
                {
                    IncludeHeader = false,
                    FooterFormat = MKLP.GetText($"Type {Commands.Specifier}who {(displayIdsRequested ? "-i" : string.Empty)} for more.")
                }
            );

            #endregion
        }

        #endregion
    }
}
