//Microsoft
using Google.Protobuf.WellKnownTypes;
using IL.Terraria.DataStructures;
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Asn1.X509;
using Steamworks;
using System;
using System.Collections.Generic;





//System
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Streams;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Channels;

using OTAPI;

//Terraria
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.Configuration;
using TShockAPI.DB;
using TShockAPI.Hooks;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Net.Mime.MediaTypeNames;
using Terraria.Net.Sockets;
using System.Net.Sockets;
using Terraria.Net;

namespace MKLP
{
    [ApiVersion(2, 1)]

    public class MKLP : TerrariaPlugin
    {

        #region [ Plugin Info ]
        public override string Author => "Nightklp";
        public override string Description => "Makes Moderating a bit easy";
        public override string Name => "MKLP";
        public override System.Version Version => new System.Version(2, 1);
        #endregion

        #region [ Variables ]

        public static Config Config = Config.Read(); //CONFIG

        internal static MKLP_DB DBManager = new();

        internal static DiscordKLP Discordklp = new();

        internal static AccountDLinked LinkAccountManager = new();

        internal static Dictionary<string, (string, string, string)> DisabledKey = new();

        //illegal things list
        public static Dictionary<int, string> IllegalItemProgression = new();

        public static Dictionary<short, string> IllegalProjectileProgression = new();

        public static Dictionary<SurvivalManager.MKLP_Tile, string> IllegalTileProgression = new();

        public static Dictionary<ushort, string> IllegalWallProgression = new();

        public static bool HasBanGuardPlugin = File.Exists(Path.Combine("ServerPlugins", "BanGuard.dll"));
        #endregion

        #region [ Main Var ]

        public static string Text_NA = MKLP.GetText("N/A");

        #endregion
        public MKLP(Main game) : base(game)
        {
            //amogus
        }

        #region [ Initialize ]

        internal static DateTime InitializeSince = DateTime.UtcNow;
        static System.Version staticversion = new System.Version(999, 0);
        public override void Initialize()
        {
            staticversion = Version;

            InitializeSince = DateTime.UtcNow;

            if (!HasBanGuardPlugin && (bool)Config.Main.UsingBanGuardPlugin)
            {
                Config.Main.UsingBanGuardPlugin = false;
                Config.Changeall();
                MKLP_Console.SendLog_Warning("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!");
            }


            CommandsKLP.INIT();

            if (Config.Discord.BotToken != "NONE")
            {
                Discordklp.Initialize();
            }
            else
            {
                MKLP_Console.SendLog_Message_DiscordBot(GetText("Discord bot token has not been set!"), " {Setup} ");
            }

            HooksKLP.Initialize(this);

            LogKLP.InitializeLogging();

            InventoryLogHandler.Initialize(this);
            InventoryManager.Initialize(this);
            MuteKLP.SyncMute();
            AntiVPN.Initialize();
        }

        #endregion

        #region [ Dispose ]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HooksKLP.Dispose(this);

                InventoryLogHandler.Dispose(this);
                InventoryManager.Dispose(this);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region [ Get Latest Version ]

        public static async Task InformLatestVersion()
        {
            var http = HttpWebRequest.CreateHttp("https://raw.githubusercontent.com/Nightklpgaming/TShock-GSKLP-Moderation/master/version.txt");

            WebResponse res = await http.GetResponseAsync();

            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
            {
                System.Version latestversion = new(sr.ReadToEnd());

                if (latestversion > staticversion)
                {
                    MKLP_Console.SendLog_LatestVersion(staticversion.ToString(), latestversion.ToString());
                }

                return;
            }
        }

        #endregion

        #region [ Function ]

        #region [[{ GetText }]]
        public static string GetText(string text)
        {
            return text;
        }
        public static string GetText(string text, params object?[] obj)
        {
            return string.Format(text, obj);
        }
        #endregion

        public static void SendStaffMessage(string message, Microsoft.Xna.Framework.Color messagecolor)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (!player.HasPermission(Config.Permissions.Staff)) continue;
                player.SendMessage(message, messagecolor);
            }
        }

        public static void TogglePlayerVanish(TSPlayer executer, bool vanish)
        {
            #region code
            //PacketTypes.player
            // set player null? ( completely invisible ) in future

            if (vanish)
            {
                if ((bool)Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                {
                    executer.TPlayer.active = false;
                }
                executer.SetData("MKLP_Vanish", true);

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    if (player == executer) continue;
                    player.SendData(PacketTypes.PlayerActive, null, executer.Index, false.GetHashCode());
                }
            }
            else
            {
                if ((bool)Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                {
                    executer.TPlayer.active = true;
                }
                executer.SetData("MKLP_Vanish", false);

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    if (player == executer) continue;
                    player.SendData(PacketTypes.PlayerActive, null, executer.Index, true.GetHashCode());

                    for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots); k++)
                    {
                        try
                        {
                            executer.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)k);
                        }
                        catch (Exception e) { MKLP_Console.SendLog_Exception(e); }
                    }

                    player.SendData(PacketTypes.PlayerInfo, null, executer.Index);
                    player.SendData(PacketTypes.PlayerUpdate, null, executer.Index);
                    player.SendData(PacketTypes.PlayerMana, null, executer.Index);
                    player.SendData(PacketTypes.PlayerHp, null, executer.Index);
                    player.SendData(PacketTypes.PlayerBuff, null, executer.Index);

                    var trashSlot = NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots;

                    for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
                    {
                        player.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)k);
                    }
                    player.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)trashSlot);
                }
            }

            #endregion
        }

        public static void SyncProgression()
        {
            IllegalItemProgression = SurvivalManager.GetIllegalItem();

            IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

            IllegalTileProgression = SurvivalManager.GetIllegalTile();

            IllegalWallProgression = SurvivalManager.GetIllegalWall();
        }
        public static bool PunishPlayer(MKLP_CodeType CodeType, byte CodeNumber, TSPlayer player, string getReason, string getWarningMessage, bool RevertInventory = false, bool IsItemRelated = false)
        {
            #region code

            string Reason = GetText(getReason);
            string WarningMessage = GetText(getWarningMessage);

            if ((bool)Config.Main.Logging.ModLogTXT_Enable)
            {
                LogKLP.Log_ModLog(
                    $"▬▬▬▬▬▬▬▬▬ {CodeType.ToString()} code {CodeNumber} ▬▬▬▬▬▬▬▬▬" +
                    $"\n{WarningMessage}" +
                    $"\n▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬\n\n"
                    );
            }

            if (CodeType == MKLP_CodeType.Main)
            {
                if (Action((PunishmentType)Config.Main.DisableNode.Main_Code_PunishmentType, IsItemRelated))
                {
                    return true;
                }
                return false;
            }
            if (CodeType == MKLP_CodeType.Survival)
            {
                
                if (Action((PunishmentType)Config.Main.DisableNode.Survival_Code_PunishmentType, IsItemRelated))
                {
                    return true;
                }
                return false;
            }
            if (CodeType == MKLP_CodeType.Default)
            {
                if (Action((PunishmentType)Config.Main.DisableNode.Default_Code_PunishmentType, IsItemRelated))
                {
                    return true;
                }
            }
            if (CodeType == MKLP_CodeType.Dupe)
            {
                if (Action((PunishmentType)Config.Main.DisableNode.SuspiciousDupe_PunishmentType, IsItemRelated))
                {
                    return true;
                }
                return false;
            }
            return false;

            bool Action(PunishmentType type, bool ShowItemGiveLog)
            {
                string log = "";

                if (ShowItemGiveLog && HooksKLP.get_itemgive_log.Count > 0)
                {
                    log = GetText("\n\nHere is the last item command used:") +
                        $"\n```\n- {string.Join("\n- ", HooksKLP.get_itemgive_log)}\n```";
                    HooksKLP.get_itemgive_log.Clear();
                }

                switch (type)
                {
                    case PunishmentType.Ban:
                        {
                            ManagePlayer.OnlineBan(false, player, Reason, "MKLP-AntiCheat", DateTime.MaxValue);
                            return true;
                        }
                    case PunishmentType.Disable:
                        {
                            ManagePlayer.DisablePlayer(player, Reason, "MKLP-AntiCheat", WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", log);
                            return true;
                        }
                    case PunishmentType.KickAndLog:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason, log);
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.Kick:
                        {
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.RevertAndLog:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason, log);
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Revert:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Log:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason, log);
                            return false;
                        }
                }
                return false;
            }

            void RevertPlayerInv()
            {
                Item[] previnv = player.GetData<Item[]>("MKLP_PrevInventory");
                Item[] prevpig = player.GetData<Item[]>("MKLP_PrevPiggyBank");
                Item[] prevsafe = player.GetData<Item[]>("MKLP_PrevSafe");
                Item[] prevforge = player.GetData<Item[]>("MKLP_PrevDefenderForge");
                Item[] prevvault = player.GetData<Item[]>("MKLP_PrevVoidVault");

                player.SetData("MKLP_Confirmed_InvRev", 1);

                // Clear Main Inventory (slots 0–49)
                for (int i = 0; i < NetItem.InventorySlots; i++)
                    player.TPlayer.inventory[i] = previnv[i];

                // Clear Armor and Accessories (slots 50–79)
                //for (int i = 0; i < player.TPlayer.armor.Length; i++)
                //player.TPlayer.armor[i].SetDefaults(0);
                if ((bool)Config.Main.DetectAllPlayerInv)
                {
                    // Clear Piggy Bank
                    for (int i = 0; i < player.TPlayer.bank.item.Length; i++)
                        player.TPlayer.bank.item[i] = prevpig[i];

                    // Clear Safe
                    for (int i = 0; i < player.TPlayer.bank2.item.Length; i++)
                        player.TPlayer.bank2.item[i] = prevsafe[i];

                    // Clear Void Vault (Forge)
                    for (int i = 0; i < player.TPlayer.bank3.item.Length; i++)
                        player.TPlayer.bank3.item[i] = prevforge[i];


                    for (int i = 0; i < player.TPlayer.bank4.item.Length; i++)
                        player.TPlayer.bank4.item[i] = prevvault[i];
                }

                // Send the updated inventory to the client
                for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots + NetItem.VoidSlots); k++) //clear all slots excluding bank slots, bank slots cleared in ResetBanks method
                {
                    try
                    {
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
                    }
                    catch (Exception e)
                    {
                        MKLP_Console.SendLog_Exception(e);
                    }
                }
            }
            #endregion
        }

        public void Check_SentryAndSummons()
        {
            #region code
            /*
            short[] SentryID =
            {
                ProjectileID.FrostHydra,
                ProjectileID.SpiderHiver,
                ProjectileID.HoundiusShootius,
            };

            short[] SummonID =
            {
                ProjectileID.Pygmy,
                ProjectileID.Pygmy2,
                ProjectileID.Pygmy3,
                ProjectileID.Pygmy4,
                ProjectileID.BabySlime,
                ProjectileID.Raven,
                ProjectileID.Hornet,
                ProjectileID.FlyingImp,
                ProjectileID.Retanimini,
                ProjectileID.Spazmamini,
                ProjectileID.VenomSpider,
                ProjectileID.JumperSpider,
                ProjectileID.DangerousSpider,
                ProjectileID.OneEyedPirate,
                ProjectileID.SoulscourgePirate,
                ProjectileID.PirateCaptain,
                ProjectileID.UFOMinion,
                ProjectileID.DeadlySphere,
                ProjectileID.StardustDragon2,
                ProjectileID.StardustDragon3,
                ProjectileID.BatOfLight,
                ProjectileID.VampireFrog,
                ProjectileID.BabyBird,
                ProjectileID.FlinxMinion,

            };

            short[] OOASentryID =
            {
                ProjectileID.DD2FlameBurstTowerT1,
                ProjectileID.DD2FlameBurstTowerT2,
                ProjectileID.DD2FlameBurstTowerT3,
                ProjectileID.DD2BallistraTowerT1,
                ProjectileID.DD2BallistraTowerT2,
                ProjectileID.DD2BallistraTowerT3,
                ProjectileID.DD2LightningAuraT1,
                ProjectileID.DD2LightningAuraT2,
                ProjectileID.DD2LightningAuraT3,
                ProjectileID.DD2ExplosiveTrapT1,
                ProjectileID.DD2ExplosiveTrapT2,
                ProjectileID.DD2ExplosiveTrapT3,
            };
            */
            //desert tiger & Abigail stack
            /*
            foreach (var player in TShock.Players)
            {
                player.TPlayer.numMinions
            }
            */
            #endregion
        }

        public static List<UserAccount> GetMatchUUID_UserAccount(string playername, string UUID)
        {
            #region code
            using var reader = TShock.DB.QueryReader("SELECT * FROM Users WHERE UUID = @0", UUID);

            List<UserAccount> result = new();

            while (reader.Read())
            {
                if (reader.Get<string>("Username") == playername) continue;
                result.Add(new UserAccount
                {
                    ID = reader.Get<int>("ID"),
                    Group = reader.Get<string>("Usergroup"),
                    UUID = reader.Get<string>("UUID"),
                    Name = reader.Get<string>("Username"),
                    Registered = reader.Get<string>("Registered"),
                    LastAccessed = reader.Get<string>("LastAccessed"),
                    KnownIps = reader.Get<string>("KnownIps")
                });
            }

            return result;
            #endregion
        }

        #endregion
    }

    public enum MKLP_CodeType
    {
        Main,
        Survival,
        Default,
        Dupe
    }
    public enum PunishmentType
    {
        Ban,
        Disable,
        KickAndLog,
        Kick,
        RevertAndLog,
        Revert,
        Log
    }

    public class StopTCPKLP : TcpSocket, ISocket
    {
        private static object _clientJoinLock = new();

        public StopTCPKLP()
        {
        }

        public StopTCPKLP(TcpClient tcpClient) : base(tcpClient)
        {
        }

        public StopTCPKLP(TcpClient tcpClient, IPEndPoint remoteEndpoint) : base(tcpClient)
        {
            _remoteAddress = new TcpAddress(remoteEndpoint.Address, remoteEndpoint.Port);
        }

        // Override to prevent unexpected situation
        void ISocket.Connect(RemoteAddress address)
        {
            throw new NotImplementedException();
        }

        bool ISocket.StartListening(SocketConnectionAccepted callback)
        {
            _isListening = false;
            _connection.Close();
            return false;
        }

        public new void ListenLoop()
        {
            if (_isListening && !Netplay.Disconnect)
            {
                TcpClient client = _listener.AcceptTcpClient();

                client.Close();
            }

            _listener.Stop();
        }
    }

    #region [ Colored Console ]
    public static class MKLP_Console
    {
        public static void SendTitle()
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[MKLP]");
            Console.ResetColor();
        }

        public static void SendLog_LatestVersion(string oldversion, string newversion)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Warning: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"MKLP has updated to v{oldversion} to v{newversion}");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("> You can download the latest version at");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"> https://github.com/Nightklpgaming/TShock-GSKLP-Moderation/releases/tag/{newversion}");
            Console.ResetColor();
        }

        public static void SendLog_Warning(object? value)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Warning: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Info(object? value)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Info: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Exception(object? value)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" Error: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Message_StaffChat_InGame(string username, object? value, ConsoleColor consolecolor = ConsoleColor.White)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [InGame-StaffChat] ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(username + ": ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }
        public static void SendLog_Message_StaffChat_Discord(string username, object? value, ConsoleColor consolecolor = ConsoleColor.White)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [Discord-StaffChat] ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(username + ": ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Message_DiscordBot(object? value, string type, ConsoleColor typeconsolecolor = ConsoleColor.Yellow, ConsoleColor consolecolor = ConsoleColor.Cyan)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = typeconsolecolor;
            Console.Write(type);
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" Discord: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

    }
    #endregion


}