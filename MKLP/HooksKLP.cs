using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Streams;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace MKLP
{
    internal static class HooksKLP
    {
        internal static void Initialize(TerrariaPlugin plg)
        {

            //=====================Player===================
            #region { Player }

            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;
            GetDataHandlers.PlayerUpdate += BossEventManager.Hooks_OnPlayerUpdate;

            //GetDataHandlers.player

            ServerApi.Hooks.NetGreetPlayer.Register(plg, OnNetGreetPlayer);

            ServerApi.Hooks.ServerJoin.Register(plg, OnPlayerJoin);

            ServerApi.Hooks.ServerLeave.Register(plg, OnPlayerLeave, 999);

            PlayerHooks.PlayerCommand += OnPlayerCommand;

            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;

            //PlayerHooks.PlayerChat += OnPlayerChat;
            ServerApi.Hooks.ServerChat.Register(plg, OnChatReceived);

            #endregion

            //=====================Game=====================
            #region { Game }

            ServerApi.Hooks.NetGetData.Register(plg, OnGetData);

            ServerApi.Hooks.GameUpdate.Register(plg, OnGameUpdate);

            GetDataHandlers.TileEdit += OnTileEdit;

            GetDataHandlers.PlaceObject += OnPlaceObject;

            GetDataHandlers.PaintTile += OnPaintTile;

            GetDataHandlers.PaintWall += OnPaintWall;

            GetDataHandlers.MassWireOperation += OnMassWireOperation;

            GetDataHandlers.LiquidSet += HandleLiquidInteraction;

            GetDataHandlers.NewProjectile += OnNewProjectile;

            GetDataHandlers.HealOtherPlayer += OnHealOtherPlayer;

            //GetDataHandlers.ItemDrop

            ServerApi.Hooks.NpcSpawn.Register(plg, BossEventManager.Hooks_OnNPCSpawn);

            ServerApi.Hooks.NpcKilled.Register(plg, OnNPCKilled);

            GetDataHandlers.Sign += OnSignChange;

            //HookEvents.Terraria.Wiring.

            //ServerApi.Hooks.NpcAIUpdate.Register(this, OnNPCAIUpdate);

            //ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjectileAIUpdate);

            //ServerApi.Hooks.WireTriggerAnnouncementBox.Register(this, OnProjectileAIUpdate);

            #endregion

            //=====================GameTer==================
            #region { GameTer }

            HookEvents.Terraria.Wiring.Actuate += OnWiringActuate;

            HookEvents.Terraria.Wiring.HitWire += OnHitWire;

            #endregion

            //=====================Server===================
            #region { Server }

            //ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            ServerApi.Hooks.ServerBroadcast.Register(plg, OnServerBroadcast);
            ServerApi.Hooks.ServerBroadcast.Register(plg, BossEventManager.Hooks_OnServerBroadcast);

            ServerApi.Hooks.WorldSave.Register(plg, OnWorldSave);

            ServerApi.Hooks.GamePostInitialize.Register(plg, OnServerStart);

            GeneralHooks.ReloadEvent += OnReload;

            #endregion
        }
        internal static void Dispose(TerrariaPlugin plg)
        {

            //=====================Player===================
            #region { Player }

            GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;
            GetDataHandlers.PlayerUpdate -= BossEventManager.Hooks_OnPlayerUpdate;

            ServerApi.Hooks.NetGreetPlayer.Deregister(plg, OnNetGreetPlayer);

            ServerApi.Hooks.ServerJoin.Deregister(plg, OnPlayerJoin);

            ServerApi.Hooks.ServerLeave.Deregister(plg, OnPlayerLeave);

            PlayerHooks.PlayerCommand -= OnPlayerCommand;

            PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;

            //PlayerHooks.PlayerChat -= OnPlayerChat;
            ServerApi.Hooks.ServerChat.Deregister(plg, OnChatReceived);
            #endregion

            //=====================Game=====================
            #region { Game }
            ServerApi.Hooks.NetGetData.Deregister(plg, OnGetData);

            ServerApi.Hooks.GameUpdate.Deregister(plg, OnGameUpdate);

            GetDataHandlers.TileEdit -= OnTileEdit;

            GetDataHandlers.PlaceObject -= OnPlaceObject;

            GetDataHandlers.PaintTile -= OnPaintTile;

            GetDataHandlers.PaintWall -= OnPaintWall;

            GetDataHandlers.MassWireOperation -= OnMassWireOperation;

            GetDataHandlers.LiquidSet -= HandleLiquidInteraction;

            GetDataHandlers.NewProjectile -= OnNewProjectile;

            GetDataHandlers.HealOtherPlayer -= OnHealOtherPlayer;

            ServerApi.Hooks.NpcSpawn.Deregister(plg, BossEventManager.Hooks_OnNPCSpawn);

            ServerApi.Hooks.NpcKilled.Deregister(plg, OnNPCKilled);

            GetDataHandlers.Sign -= OnSignChange;
            GetDataHandlers.SignRead -= OnSignRead;

            GetDataHandlers.PlayerDamage -= OnPlayerDamage;

            GetDataHandlers.KillMe -= OnKillMe;

            //ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNPCAIUpdate);

            //ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjectileAIUpdate);

            #endregion

            //=====================Server===================
            #region { Server }

            //ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);

            ServerApi.Hooks.ServerBroadcast.Deregister(plg, OnServerBroadcast);
            ServerApi.Hooks.ServerBroadcast.Deregister(plg, BossEventManager.Hooks_OnServerBroadcast);

            ServerApi.Hooks.WorldSave.Deregister(plg, OnWorldSave);

            ServerApi.Hooks.GamePostInitialize.Deregister(plg, OnServerStart);

            GeneralHooks.ReloadEvent -= OnReload;
            #endregion
        }

        #region =={[ On Get Data ]}==

        private static async void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            var player = TShock.Players[args.Msg.whoAmI];

            if (args.MsgID == PacketTypes.ForceItemIntoNearestChest)
            {
                if ((bool)MKLP.Config.Main.ManagePackets.Disable_Packet85_QuickStackChest)
                {
                    args.Handled = true;
                    player.SendData(PacketTypes.PlayerUpdate, "", player.Index);
                    for (int i = 0; i < NetItem.InventorySlots; i++)
                    {
                        player.SendData(PacketTypes.PlayerSlot, "", player.Index, i);
                    }
                }
            }

            if (args.MsgID == PacketTypes.SyncExtraValue)
            {
                if ((bool)MKLP.Config.Main.ManagePackets.Disable_Packet92_MobPickupCoin)
                {
                    args.Handled = true;
                }
            }

            #region [ ItemDrop ]

            if (args.MsgID == PacketTypes.ItemDrop ||
                args.MsgID == PacketTypes.ItemDrop)
            {
                using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        int whoami = reader.ReadInt16();
                        Vector2 position = reader.ReadVector2();
                        Vector2 velocity = reader.ReadVector2();
                        int stack = reader.ReadInt16();
                        int prefix = reader.ReadByte();
                        BitsByte bitsByte = reader.ReadByte();
                        bool flag1 = bitsByte[0];
                        bool flag2 = bitsByte[1];
                        int type = reader.ReadInt16();

                        int[] NoDel =
                        {
                            ItemID.Heart,
                            ItemID.CandyApple,
                            ItemID.CandyCane,

                            ItemID.Star,
                            ItemID.SoulCake,
                            ItemID.SugarPlum,

                            ItemID.NebulaPickup1,
                            ItemID.NebulaPickup2,
                            ItemID.NebulaPickup3,

                            ItemID.ManaCloakStar
                        };
                        if (!NoDel.Contains(type))
                        {
                            if (MKLP.IllegalItemProgression.ContainsKey(type) &&
                                (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 && (bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1)
                            {
                                TSPlayer.All.SendData(PacketTypes.SyncItemDespawn, "", whoami);
                                args.Handled = true;
                            }
                            int maxvalue = 10;

                            if (Main.hardMode) { maxvalue = 100; }

                            if ((GetValue(type) * stack) / 5000000 >= maxvalue &&
                                type != 74
                                && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 && (bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1)
                            {
                                TSPlayer.All.SendData(PacketTypes.SyncItemDespawn, "", whoami);
                                args.Handled = true;
                            }
                        }

                        int GetValue(int itemtype)
                        {
                            Item e = new();
                            e.SetDefaults(itemtype);
                            return e.value;
                        }
                    }
                }
            }

            #endregion

            #region [ latency ]

            if (args.MsgID == PacketTypes.ItemOwner)
            {
                try
                {
                    if (player.ContainsData("MKLP_StartGetLatency"))
                    {
                        player.SetData("MKLP_GetLatency", (DateTime.UtcNow - player.GetData<DateTime>("MKLP_StartGetLatency")).TotalMilliseconds);
                        player.RemoveData("MKLP_StartGetLatency");
                    }
                }
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on ItemOwner\n");
                    MKLP_Console.SendLog_Exception(e);
                }
                /*
                var user = TShock.Players[args.Msg.whoAmI];
                if (user == null) return;
                using (BinaryReader date = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    int iid = date.ReadInt16();
                    int pid = date.ReadByte();
                    if (pid != 255) return;
                    var pingresponse = PlayerPing[args.Msg.whoAmI];
                    var ping = pingresponse?.RecentPings[iid];
                    if (ping != null)
                    {
                        ping.End = DateTime.Now;
                        ping.Channel!.Writer.TryWrite(iid);
                    }
                }
                */
            }

            #endregion

            #region [ Disable ]
            try
            {
                if (player != null)
                {
                    if (player.ContainsData("MKLP_IsDisabled"))
                    {
                        if (player.Active && !player.Dead && player.GetData<bool>("MKLP_IsDisabled"))
                        {
                            if (args.MsgID == PacketTypes.PlayerSlot ||
                                args.MsgID == PacketTypes.PlayerUpdate ||
                                args.MsgID == PacketTypes.ItemOwner ||
                                args.MsgID == PacketTypes.ClientSyncedInventory)
                                return;

                            if (TShockAPI.Utils.Distance(value2: new Vector2((int)player.TPlayer.position.X / 16, (int)player.TPlayer.position.Y / 16), value1: new Vector2(Main.spawnTileX, Main.spawnTileY)) >= 3f)
                            {
                                player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
                            }
                            player.SetBuff(149, 330, true);

                            // Prevent the packet from being processed
                            args.Handled = true;
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception("Error on Disable");
                MKLP_Console.SendLog_Exception(e);
            }
            #endregion

            #region [ Vanish ]

            if (args.MsgID == PacketTypes.PlayerActive)
            {
                if (player.ContainsData("MKLP_Vanish"))
                {
                    if (player.GetData<bool>("MKLP_Vanish"))
                    {
                        foreach (TSPlayer gplayer in TShock.Players)
                        {
                            if (gplayer == null) continue;
                            if (gplayer == player) continue;
                            gplayer.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                        }
                    }
                }
            }

            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                try
                {
                    exe1();
                    async void exe1()
                    {
                        await exe2();

                        async Task exe2()
                        {
                            if (!player.ContainsData("MKLP_Vanish")) return;

                            if (!player.GetData<bool>("MKLP_Vanish")) return;

                            while (player.Dead) { }
                            while (player.TPlayer.dead) { }
                            while (!player.Active) { }
                            while (!player.TPlayer.active) { }

                            if ((bool)MKLP.Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                            {
                                player.TPlayer.active = false;
                            }

                            for (int i = 0; i < 10; i++)
                            {
                                foreach (TSPlayer gplayer in TShock.Players)
                                {
                                    if (gplayer == null) continue;
                                    if (gplayer == player) continue;

                                    gplayer.SendData(PacketTypes.PlayerActive, null, player.Index, false.GetHashCode());
                                }
                                await Task.Delay(1000);
                            }

                        }
                    }
                }
                catch { }
            }

            #endregion

            #region { spawn boss/invasion }
            if (args.MsgID == PacketTypes.SpawnBossorInvasion)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    try
                    {
                        args.Handled = HandleSpawnBoss(new GetDataHandlerArgs(player, data));
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error(ex.ToString());
                        MKLP_Console.SendLog_Exception(ex.ToString());
                        return;
                    }
                }
            }
            #endregion

            #region { Ping Map }

            if (args.MsgID == PacketTypes.LoadNetModule)
            {
                try
                {
                    using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            var id = reader.ReadUInt16();
                            var module = Terraria.Net.NetManager.Instance._modules[id];
                            if (module.GetType() == typeof(Terraria.GameContent.NetModules.NetPingModule))
                            {
                                var position = reader.ReadVector2();

                                if (player.ContainsData("MKLP-Map_Ping_TP"))
                                {
                                    if (player.GetData<bool>("MKLP-Map_Ping_TP"))
                                    {
                                        player.Teleport(position.X * 16, position.Y * 16);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on LoadNetModule (Ping Map)");
                    MKLP_Console.SendLog_Exception(e);
                }
            }
            #endregion


        }

        #endregion

        #region [ OnGameUpdate ]

        private static void OnGameUpdate(EventArgs args)
        {

            //if ((DateTime.UtcNow - checklatency_interval).TotalSeconds >= 5)
            //{
            //    checklatency_interval = DateTime.UtcNow;
            //    foreach (TSPlayer player in TShock.Players)
            //    {
            //        if (player == null) continue;
            //        player.SetData("MKLP_StartGetLatency", DateTime.UtcNow);
            //        NetMessage.SendData((int)PacketTypes.ItemOwner, player.Index, -1, null, 0, player.Index);
            //    }
            //}

            if ((DateTime.UtcNow - MuteKLP.NearestExpiredMute).TotalSeconds > 0)
            {
                MuteKLP.NearestExpiredMute = DateTime.MaxValue;
                checkplayers();
                MuteKLP.SyncMute();
            }

            if (!(bool)MKLP.Config.Main.Use_OnUpdate_Func) return;

            CheckItemDrops();

            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (player.ContainsData("MKLP_TargetSpy"))
                {
                    player.TPlayer.position = player.GetData<TSPlayer>("MKLP_TargetSpy").TPlayer.position;
                    player.SendData(PacketTypes.PlayerUpdate, "", player.Index);

                    player.SetBuff(BuffID.Invisibility, 15 * 60);
                    player.SetBuff(BuffID.ObsidianSkin, 20 * 60);
                    player.SetBuff(BuffID.Webbed, 10 * 60);

                }
            }
        }


        static void CheckItemDrops()
        {
            try
            {
                int maxvalue = 10;

                if (Main.hardMode) maxvalue = 100;

                if ((bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 || (bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1)
                {
                    int[] NoDel =
                    {
                        ItemID.Heart,
                        ItemID.CandyApple,
                        ItemID.CandyCane,

                        ItemID.Star,
                        ItemID.SoulCake,
                        ItemID.SugarPlum,

                        ItemID.NebulaPickup1,
                        ItemID.NebulaPickup2,
                        ItemID.NebulaPickup3,

                        ItemID.ManaCloakStar
                    };
                    for (int i = 0; i < Main.maxItems; i++)
                    {
                        if (NoDel.Contains(Main.item[i].type)) continue;
                        if (MKLP.IllegalItemProgression.ContainsKey(Main.item[i].type) &&
                            (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 && (bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 && Main.item[i].active)
                        {
                            Main.item[i].TurnToAir(true);
                            TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                        }
                        if ((Main.item[i].value * Main.item[i].stack) / 5000000 >= maxvalue &&
                            Main.item[i].type != 74
                            && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 && (bool)MKLP.Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1 && Main.item[i].active)
                        {
                            Main.item[i].TurnToAir(true);
                            TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception("Error on ItemDrop");
                MKLP_Console.SendLog_Exception(e);
            }

        }

        #endregion

        #region { Player }




        /*
        private PingData[] PlayerPing { get; set; }
        public class PingData
        {
            public TimeSpan? LastPing;
            internal PingDetails?[] RecentPings = new PingDetails?[Terraria.Main.item.Length];
        }
        internal class PingDetails
        {
            internal Channel<int>? Channel;
            internal DateTime Start = DateTime.Now;
            internal DateTime? End = null;
        }

        public async Task<TimeSpan> Ping(TSPlayer player)
        {
            return await Ping(player, new CancellationTokenSource(1000).Token);
        }

        public async Task<TimeSpan> Ping(TSPlayer player, CancellationToken token)
        {
            var pingdata = PlayerPing[player.Index];
            if (pingdata == null) return TimeSpan.MaxValue;

            var inv = -1;
            for (var i = 0; i < Terraria.Main.item.Length; i++)
                if (Terraria.Main.item[i] != null)
                    if (!Terraria.Main.item[i].active || Terraria.Main.item[i].playerIndexTheItemIsReservedFor == 255)
                    {
                        if (pingdata.RecentPings[i]?.Channel == null)
                        {
                            inv = i;
                            break;
                        }
                    }

            if (inv == -1) return TimeSpan.MaxValue;

            var pd = pingdata.RecentPings[inv] ??= new PingDetails();

            pd.Channel ??= Channel.CreateBounded<int>(new BoundedChannelOptions(30)
            {
                SingleReader = true,
                SingleWriter = true
            });


            Terraria.NetMessage.TrySendData((int)PacketTypes.RemoveItemOwner, player.Index, -1, null, inv);

            await pd.Channel.Reader.ReadAsync(token);
            pd.Channel = null;

            return (pingdata.LastPing = pd.End!.Value - pd.Start).Value;
        }
        */

        /*
        private void Hook_Ping_GetData(GetDataEventArgs args)
        {
            if (args.Handled) return;
            if (args.MsgID != PacketTypes.ItemOwner) return;
            var user = TShock.Players[args.Msg.whoAmI];
            if (user == null) return;
            using (BinaryReader date = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                int iid = date.ReadInt16();
                int pid = date.ReadByte();
                if (pid != 255) return;
                var pingresponse = PlayerPing[args.Msg.whoAmI];
                var ping = pingresponse?.RecentPings[iid];
                if (ping != null)
                {
                    ping.End = DateTime.Now;
                    ping.Channel!.Writer.TryWrite(iid);
                }
            }
        }
        */

        static DateTime checklatency_interval = DateTime.MinValue;

        private static void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            #region code
            if ((bool)MKLP.Config.Main.Use_OnUpdate_Func) return;

            if ((DateTime.UtcNow - checklatency_interval).TotalSeconds >= 5)
            {
                checklatency_interval = DateTime.UtcNow;

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    player.SetData("MKLP_StartGetLatency", DateTime.UtcNow);
                    NetMessage.SendData((int)PacketTypes.ItemOwner, player.Index, -1, null, 0, player.Index);
                    //player.SetData("MKLP_GetLatency", Ping(player).Result.TotalMilliseconds);
                }
            }

            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (player.ContainsData("MKLP_TargetSpy"))
                {
                    if (player.GetData<TSPlayer>("MKLP_TargetSpy") == args.Player)
                    {
                        player.TPlayer.position = args.Player.TPlayer.position;
                        player.SendData(PacketTypes.PlayerUpdate, "", player.Index);

                        player.SetBuff(BuffID.Invisibility, 15 * 60);
                        player.SetBuff(BuffID.ObsidianSkin, 20 * 60);
                        player.SetBuff(BuffID.Webbed, 10 * 60);

                    }
                }
            }

            #endregion
        }

        private static void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            #region code
            TSPlayer player = TShock.Players[args.Who];

            if ((bool)MKLP.Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser)
            {
                player.SilentJoinInProgress = true;
            }

            #endregion
        }


        static int connectionQueueCount = 0;

        static DateTime playerjoin_temp1_Since = DateTime.MinValue;
        static Dictionary<string, List<string>> playerjoin_temp1 = new();
        private static async void OnPlayerJoin(JoinEventArgs args)
        {
            #region code
            if ((bool)MKLP.Config.Main.AntiRaid.Using_PriorityConnection)
            {
                if (connectionQueueCount >= (int)MKLP.Config.Main.AntiRaid.PriorityConnection_Max)
                {
                    TShock.Players[args.Who].Disconnect(MKLP.Config.Main.AntiRaid.PriorityConnection_Max_Reason);
                    args.Handled = true;
                    return;
                }
                connectionQueueCount++;
                e();

                async void e()
                {
                    await e();
                    return;
                    async Task e()
                    {
                        await Task.Delay(10000);
                        connectionQueueCount--;
                    }
                }
            }

            var player = TShock.Players[args.Who];
            if (player != null)
            {
                if (!(bool)MKLP.Config.Main.AllowEmptyWhiteSpaceName)
                { 
                    if (player.Name == null)
                    {
                        player.Disconnect("you cannot join with empty null name");
                        return;
                    }
                    if (string.IsNullOrEmpty(player.Name.Trim()))
                    {
                        player.Disconnect("you cannot join with empty null name");
                        return;
                    }
                }
                #region Anti VPN

                if ((bool)MKLP.Config.Main.AntiVPN.Using)
                {
                    if (await AntiVPN.IPCheck(player.IP))
                    {
                        player.Disconnect(MKLP.Config.Main.AntiVPN.KickUsingVPNReason);
                        return;
                    }
                }

                #endregion

                #region lockdown

                if ((bool)MKLP.Config.Main.AntiRaid.LockDown)
                {
                    if (MKLP.Config.Main.AntiRaid.LockDownReason == "")
                    {
                        player.Disconnect(MKLP.GetText("You cannot join the server yet!"));
                    }
                    else
                    {
                        player.Disconnect(MKLP.GetText("You cannot join the server by the reason of") + " " + MKLP.Config.Main.AntiRaid.LockDownReason);
                    }
                    return;
                }

                #endregion

                #region Prevent
                if (MKLP.Config.Main.IllegalNames.Contains(player.Name))
                {
                    player.Disconnect(MKLP.GetText("Illegal Name"));
                    return;
                }
                if (player.Name.Contains(DiscordKLP.S_))
                {
                    player.Disconnect(MKLP.GetText($"You Can't use {DiscordKLP.S_} in your Name!"));
                    return;
                }
                foreach (string contains in MKLP.Config.Main.Ban_NameContains)
                {
                    if (player.Name.Contains(contains))
                    {
                        player.Disconnect(MKLP.GetText($"You Can't use {contains} in your Name!"));
                        return;
                    }
                }
                if (player.Name.Length < (byte)MKLP.Config.Main.Minimum_CharacterName)
                {
                    player.Disconnect(MKLP.GetText($"You're Name has less than {((byte)MKLP.Config.Main.Minimum_CharacterName <= 1 ? $"{(byte)MKLP.Config.Main.Minimum_CharacterName} character" : $"{(byte)MKLP.Config.Main.Minimum_CharacterName} characters")}"));
                    return;
                }
                if (player.Name.Length > (byte)MKLP.Config.Main.Maximum_CharacterName)
                {
                    player.Disconnect(MKLP.GetText($"You're Name has more than {((byte)MKLP.Config.Main.Maximum_CharacterName <= 1 ? $"{(byte)MKLP.Config.Main.Maximum_CharacterName} character" : $"{(byte)MKLP.Config.Main.Maximum_CharacterName} characters")}"));
                    return;
                }
                if (!HasSymbols(player.Name) && !(bool)MKLP.Config.Main.Allow_PlayerName_Symbols)
                {
                    player.Disconnect(MKLP.GetText("Your name contains Symbols and is not allowed on this server."));
                    return;
                }

                string[] bannedwords;
                if (BannedWordChecker.ISBannedWord(TShockAPI.Group.DefaultGroup, player.Name, out bannedwords) && !(bool)MKLP.Config.Main.Allow_PlayerName_On_BannedWords)
                {
                    player.Disconnect(MKLP.GetText("Your name contains \"" + string.Join(',', bannedwords) + "\" and is not allowed on this server."));
                    return;
                }
                #endregion

                #region Boss Is Present
                if ((bool)MKLP.Config.BossManager.UsingBossManager)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (!(bool)MKLP.Config.BossManager.AllowJoinDuringBoss && Main.npc[i].active && Main.npc[i].boss)
                        {
                            player.Disconnect(MKLP.GetText("The current in-game players must defeat the current boss\nBefore you can join."));
                            return;
                        }
                    }
                }

                #endregion

                #region UUID Match [ alt-acc prevention ]

                var getuuidmatch = MKLP.GetMatchUUID_UserAccount(player.Name, player.UUID);
                UserAccount useraccount = TShock.UserAccounts.GetUserAccountByName(player.Name);
                if (getuuidmatch.Count != 0 && !(bool)MKLP.Config.Main.Allow_User_JoinMatchUUID && useraccount == null)
                {
                    string message = MKLP.Config.Main.Reason_User_JoinMatchUUID;
                    string getaccountname = getuuidmatch[0].Name;
                    bool whitelisted = false;
                    try
                    {
                        foreach (var check in (Config.WhiteListAlt[])MKLP.Config.Main.WhiteList_User_JoinMatchUUID)
                        {
                            if (check.MainName == player.Name || check.AltNames.Contains(player.Name))
                            {
                                whitelisted = true;
                            }
                        }
                    }
                    catch { }

                    if ((bool)MKLP.Config.Main.Target_UserMatchUUIDAndIP && !whitelisted)
                    {
                        foreach (UserAccount get in getuuidmatch)
                        {
                            if (JsonConvert.DeserializeObject<List<string>>(get.KnownIps).Contains(player.IP))
                            {
                                message = message.Replace("%matchtype%", "UUID & IP");
                                message = message.Replace("%accountname%", get.Name);
                                message = message.Replace("%existaccountnames%", string.Join(", ", getuuidmatch.Select(guuim => guuim.Name)));
                                player.Disconnect(message);
                                return;
                            }
                        }
                    }
                    else if (!whitelisted)
                    {
                        message = message.Replace("%matchtype%", "UUID");
                        message = message.Replace("%accountname%", getaccountname);
                        message = message.Replace("%existaccountnames%", string.Join(", ", getuuidmatch.Select(guuim => guuim.Name)));
                        player.Disconnect(message);
                        return;
                    }
                }

                #endregion

                #region AntiRaid Check

                if ((DateTime.UtcNow - MKLP.InitializeSince).Minutes >= (int)MKLP.Config.Main.AntiRaid.Disable_PlayerJoin_ThreshHold_Until_Minutes && (bool)MKLP.Config.Main.AntiRaid.Using_PlayerJoin_ThreshHold)
                {
                    if ((DateTime.UtcNow - playerjoin_temp1_Since).TotalSeconds >= (int)MKLP.Config.Main.AntiRaid.PlayerJoin_ThreshHold_Seconds)
                    {
                        playerjoin_temp1_Since = DateTime.UtcNow;
                        playerjoin_temp1.Clear();
                    }

                    bool add_pjt = true;
                    foreach (var get in playerjoin_temp1)
                    {
                        if (get.Key == player.UUID || get.Value.Contains(player.IP))
                        {
                            add_pjt = false;
                            break;
                        }
                    }
                    if (add_pjt)
                    {
                        if (playerjoin_temp1.ContainsKey(player.UUID))
                        {
                            playerjoin_temp1[player.UUID].Add(player.IP);
                        }
                        else
                        {
                            playerjoin_temp1.Add(player.UUID, new() { player.IP });
                        }
                    }

                    if (playerjoin_temp1.Count >= (int)MKLP.Config.Main.AntiRaid.PlayerJoin_ThreshHold)
                    {
                        MKLP.Config.Main.AntiRaid.LockDown = true;
                        MKLP.Config.Main.AntiRaid.LockDownReason = MKLP.Config.Main.AntiRaid.PlayerJoin_ThreshHold_LockdownReason;
                        MKLP.Config.Changeall();
                        foreach (var getp in TShock.Players)
                        {
                            if (getp == null) continue;
                            if (playerjoin_temp1.ContainsKey(getp.UUID))
                            {
                                getp.Disconnect(MKLP.GetText("[MKLP] AutoLockDown Due to many player's join at the same time!"));
                            }
                        }
                        MKLP.Discordklp.KLPBotSendMessageMainLog("## AutoLock has been enabled! Due to many player's join at the same time!");
                        return;
                    }
                }

                #endregion

                #region Check Disabled
                string DisableReason;
                if (ManagePlayer.PlayerIsDisable(player.Name, player.IP, player.UUID, out DisableReason))
                {
                    player.SetData("MKLP_IsDisabled", true);
                    player.SendErrorMessage(MKLP.GetText("Your still disabled Because of") + " " + DisableReason);
                }
                #endregion

                #region check if muted
                var mutedata = MuteKLP.PlayerIsMuted(player.Name);
                if (mutedata.muted && !mutedata.used)
                {
                    player.mute = true;
                    player.SendErrorMessage(MKLP.GetText("Your still muted!"));
                }
                else if (!mutedata.muted && !mutedata.used)
                {
                    MuteKLP.SetMuteUsed(player, true);
                    player.SendSuccessMessage(MKLP.GetText("You're no longer muted."));
                }
                #endregion

                #region check vanish players
                foreach (TSPlayer gplayer in TShock.Players)
                {
                    if (gplayer == null) continue;
                    if (gplayer == player) continue;
                    if (gplayer.ContainsData("MKLP_Vanish"))
                    {
                        if (gplayer.GetData<bool>("MKLP_Vanish"))
                        {
                            player.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                        }
                    }
                }
                #endregion
            }

            bool HasSymbols(string Name)
            {
                foreach (char remove in MKLP.Config.Main.WhiteList_PlayerName_Symbols)
                {
                    Name.Replace($"{remove}", "");
                }
                return Regex.IsMatch(Name, @"^[A-Za-z0-9\s@]*$");
            }
            #endregion
        }

        private static void OnPlayerLeave(LeaveEventArgs args)
        {
            #region code
            TSPlayer player = TShock.Players[args.Who];

            if ((bool)MKLP.Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser && !player.IsLoggedIn)
            {
                player.SilentKickInProgress = true;
            }

            var godPower = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.GodmodePower>();

            foreach (TSPlayer gplayer in TShock.Players)
            {
                if (gplayer == null) continue;
                if (gplayer.ContainsData("MKLP_TargetSpy"))
                {
                    if (gplayer.GetData<TSPlayer>("MKLP_TargetSpy") == player)
                    {
                        gplayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                        godPower.SetEnabledState(gplayer.Index, false);

                        MKLP.TogglePlayerVanish(gplayer, false);

                        gplayer.RemoveData("MKLP_TargetSpy");

                        gplayer.SendInfoMessage(MKLP.GetText($"You're no longer spying on someone"));
                    }
                }
            }

            #endregion
        }

        public static List<string> get_itemgive_log = new();
        private static void OnPlayerCommand(PlayerCommandEventArgs args)
        {
            #region code
            if (args.Handled || args.Player == null)
                return;

            Command command = args.CommandList.FirstOrDefault();

            if (command == null)
                return;

            if (MKLP.DisabledKey.ContainsKey(Identifier.Name + args.Player.Name) ||
                MKLP.DisabledKey.ContainsKey(Identifier.IP + args.Player.IP) ||
                MKLP.DisabledKey.ContainsKey(Identifier.UUID + args.Player.UUID))
            {
                if (command.Name == "register" ||
                    command.Name == "login")
                {
                    args.Player.SendErrorMessage(MKLP.GetText("You're currently Disabled! you cannot perform this command."));
                    args.Handled = true;
                    return;
                }
            }

            if (command.Name == "register" && (bool)MKLP.Config.Main.AntiRaid.RegisterUserLockDown)
            {
                args.Player.SendErrorMessage(MKLP.GetText("You do not have permission to register at the moment"));
                args.Handled = true;
                return;
            }
            SendCommandLog(command, args.Player, args.CommandPrefix, args.CommandName, args.CommandText);

            void SendCommandLog(Command cmd, TSPlayer player, string cmdprefix, string cmdName, string cmdtext)
            {
                if (MKLP.Config.Main.Logging.CommandLog_Ignore.Contains(cmd.Name)) return;

                string IsNormal = MKLP.Config.Main.Logging.CommandLog_Normal.Contains(cmd.Name) ? "☑️" : "⚠️NotNormal";
                string TypePlayer = "";
                string getPlayerName = args.Player.Name;
                if (!player.RealPlayer)
                {
                    TypePlayer = "( Not in a Server )";
                }
                else if (!player.IsLoggedIn)
                {
                    TypePlayer = "( Not Logged In )";
                }
                else
                {
                    getPlayerName = player.Account.Name;
                }

                string getcmdtext = MKLP.Config.Main.Logging.CommandLog_IgnoreARGS.Contains(cmd.Name) ? $"{cmdprefix}{cmdName} (args omitted)" : cmdprefix + cmdtext;
                if (cmd.Name is "register" or "login" or "password") getcmdtext = $"{cmdprefix}{cmdName} (args omitted)";

                MKLP.Discordklp.KLPBotSendMessageLog((ulong)MKLP.Config.Discord.CommandLogChannel,
                    MKLP.GetText($"{TypePlayer} Player **{getPlayerName}** {(cmd.CanRun(player) ? "✅Executed" : "⛔Tried")}|{IsNormal} `{getcmdtext}`"));

                if (MKLP.Config.Main.Logging.CommandLog_ItemGive.Contains(cmd.Name) && cmd.CanRun(player))
                {
                    AddItemLog($"{getPlayerName} Executed: {getcmdtext}");
                }
            }
            #endregion

            #region Func

            void AddItemLog(string logtext)
            {
                if (get_itemgive_log.Count() > 7)
                {
                    get_itemgive_log.RemoveAt(0);
                }
                get_itemgive_log.Add(logtext);
            }

            #endregion
        }

        private static void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            #region code

            if ((bool)MKLP.Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser)
            {
                TSPlayer.All.SendInfoMessage($"{args.Player.Name} has joined.");

                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{args.Player.Name} has joined.");
                Console.ResetColor();
            }

            #endregion
        }


        static int NumberOfMutedPlayers = 0;
        static PlayerMessageThreshold?[] PlayerMSGThreshold = new PlayerMessageThreshold?[Main.player.Length];
        static PlayerMessageThreshold?[] PlayerMSGThreshold2 = new PlayerMessageThreshold?[Main.player.Length];
        public struct PlayerMessageThreshold
        {
            public int Threshold;
            public DateTime Since;

            public PlayerMessageThreshold(int Threshold, DateTime Since)
            {
                this.Threshold = Threshold;
                this.Since = Since;
            }
        }
        //private void OnPlayerChat(PlayerChatEventArgs args)
        private static async void OnChatReceived(ServerChatEventArgs args)
        {
            #region code

            TSPlayer player = TShock.Players[args.Who];

            if (player == null)
            {
                args.Handled = true;
                return;
            }

            if (!player.HasPermission(Permissions.canchat))
            {
                return;
            }

            if (!player.mute)
            {
                return;
            }

            string text = args.Text;

            if (!(bool)MKLP.Config.Main.ChatMod.Using_Chat_AutoMod) return;

            if (text.StartsWith(Commands.Specifier) && text != Commands.Specifier) return;
            if (text.StartsWith(Commands.SilentSpecifier) && text != Commands.SilentSpecifier) return;

            string CensorText;

            TShockAPI.Group GetGroup()
            {
                if (player.tempGroup != null)
                {
                    return player.tempGroup;
                }

                return player.Group;
            }

            if (BannedWordChecker.ISBannedWord(GetGroup(), text, out CensorText))
            {
                if ((bool)MKLP.Config.Main.ChatMod.CensorInsteadOfBlock)
                {
                    text = CensorText;
                }
                else
                {
                    player.SendErrorMessage(MKLP.GetText("You can not send that message!"));
                    args.Handled = true;
                    return;
                }
            }

            // Forcefully change the Text property
            PropertyInfo? propertyInfo = args.GetType().GetProperty("Text");
            propertyInfo?.SetValue(args, text);


            if (args.Text.Length >= (int)MKLP.Config.Main.ChatMod.Maximum__MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                player.SendErrorMessage(MKLP.GetText("You can not send that message!"));
                args.Handled = true;
                return;
            }

            if (args.Text.Length >= (int)MKLP.Config.Main.ChatMod.Maximum__MessageLength_WithSpace)
            {
                player.SendErrorMessage(MKLP.GetText("You can not send that message!"));
                args.Handled = true;
                return;
            }
            if (args.Text.Length >= (int)MKLP.Config.Main.ChatMod.Maximum_Spammed_MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                if (PlayerMSGThreshold[player.Index] != null)
                {
                    if ((DateTime.UtcNow - ((PlayerMessageThreshold)PlayerMSGThreshold[player.Index]).Since).TotalMilliseconds < (int)MKLP.Config.Main.ChatMod.Millisecond_Threshold)
                    {
                        if (((PlayerMessageThreshold)PlayerMSGThreshold[player.Index]).Threshold >= (int)MKLP.Config.Main.ChatMod.Threshold_Spammed_MessageLength_NoSpace)
                        {
                            SendWarning();
                            return;
                        }

                        PlayerMSGThreshold[player.Index] = new PlayerMessageThreshold(((PlayerMessageThreshold)PlayerMSGThreshold[player.Index]).Threshold + 1, DateTime.UtcNow);

                    }
                    else
                    {
                        PlayerMSGThreshold[player.Index] = new PlayerMessageThreshold(0, DateTime.UtcNow);
                    }
                }
                else
                {
                    PlayerMSGThreshold[player.Index] = new PlayerMessageThreshold(1, DateTime.UtcNow);
                }
            }

            if (args.Text.Length >= (int)MKLP.Config.Main.ChatMod.Maximum_Spammed_MessageLength_WithSpace)
            {
                if (PlayerMSGThreshold2[player.Index] != null)
                {
                    if ((DateTime.UtcNow - ((PlayerMessageThreshold)PlayerMSGThreshold2[player.Index]).Since).TotalMilliseconds < (int)MKLP.Config.Main.ChatMod.Millisecond_Threshold)
                    {
                        if (((PlayerMessageThreshold)PlayerMSGThreshold2[player.Index]).Threshold >= (int)MKLP.Config.Main.ChatMod.Threshold_Spammed_MessageLength_WithSpace)
                        {
                            SendWarning();
                            return;
                        }

                        PlayerMSGThreshold2[player.Index] = new PlayerMessageThreshold(((PlayerMessageThreshold)PlayerMSGThreshold2[player.Index]).Threshold + 1, DateTime.UtcNow);

                    }
                    else
                    {
                        PlayerMSGThreshold2[player.Index] = new PlayerMessageThreshold(0, DateTime.UtcNow);
                    }
                }
                else
                {
                    PlayerMSGThreshold2[player.Index] = new PlayerMessageThreshold(1, DateTime.UtcNow);
                }
            }

            void SendWarning()
            {
                if (player.ContainsData("MKLP_Chat_Warning_message"))
                {
                    player.SetData("MKLP_Chat_Warning_message", player.GetData<int>("MKLP_Chat_Warning_message") + 1);
                }
                else
                {
                    player.SetData("MKLP_Chat_Warning_message", 1);
                }

                if (player.GetData<int>("MKLP_Chat_Warning_message") >= (int)MKLP.Config.Main.ChatMod.MutePlayer_AtWarning)
                {
                    if ((bool)MKLP.Config.Main.ChatMod.PermanentDuration)
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.MaxValue);
                    }
                    else
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.UtcNow.AddSeconds((int)MKLP.Config.Main.ChatMod.MuteDuration_Seconds));
                    }
                    NumberOfMutedPlayers++;
                    args.Handled = true;

                    if ((bool)MKLP.Config.Main.ChatMod.EnableLockDown_When_MultipleMutes)
                    {
                        if (NumberOfMutedPlayers >= (int)MKLP.Config.Main.ChatMod.NumberOFPlayersAutoMute_Lockdown)
                        {
                            MKLP.Discordklp.KLPBotSendMessageMainLog(MKLP.GetText("Server On 🔒LockDown🔒 Due to Multiple Player Mutes🔇!"));
                            MKLP.Config.Main.AntiRaid.LockDown = true;
                            MKLP.Config.Main.AntiRaid.LockDownReason = MKLP.Config.Main.ChatMod.AutoLockDown_Reason;
                            MKLP.Config.Changeall();
                        }
                    }

                    return;
                }

                player.SendWarningMessage(MKLP.GetText("Warning! please do not spam/flood the messages!"));
                args.Handled = true;
                return;
            }
            #endregion
        }

        #endregion

        #region { Game }

        private static void OnTileEdit(object? sender, GetDataHandlers.TileEditEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;

            if (KillThreshold()) return;
            if (PlaceThreshold()) return;

            #region [ Breakable Tiles ]

            ushort[] breakableTiles =
            {
                //vines
                TileID.Plants,
                TileID.Plants2,
                TileID.AshPlants,
                TileID.CorruptPlants,
                TileID.CrimsonPlants,
                TileID.HallowedPlants,
                TileID.HallowedPlants2,
                TileID.JunglePlants,
                TileID.JunglePlants2,
                TileID.MushroomPlants,
                TileID.OasisPlants,

                //vines
                TileID.VineFlowers,
                TileID.Vines,
                TileID.AshVines,
                TileID.CorruptVines,
                TileID.CrimsonVines,
                TileID.HallowedVines,
                TileID.JungleVines,
                TileID.MushroomVines,

                //pots
                

                //misc
                TileID.Cobweb,
                TileID.Pigronata,

            };

            #endregion

            if (args.Action == GetDataHandlers.EditAction.PlaceTile || args.Action == GetDataHandlers.EditAction.ReplaceTile)
            {
                if (MKLP.IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !SurvivalManager.MKLP_Tile.ObjectIDs[Main.tile[tileX, tileY].type] && !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_3) && (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code3)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;

                    }
                }
                if (MKLP.IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !SurvivalManager.MKLP_Tile.ObjectIDs[Main.tile[tileX, tileY].type] && !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_3) && (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code3)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            if (args.Action == GetDataHandlers.EditAction.PlaceWall)
            {
                if (MKLP.IllegalWallProgression.ContainsKey(Main.tile[tileX, tileY].wall) &&
                    !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_4) &&
                    (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code4)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 4, args.Player, $"{MKLP.IllegalWallProgression[Main.tile[tileX, tileY].wall]} Wall Place", $"Player **{args.Player.Name}** has placed illegal wall progression `wall id:{Main.tile[tileX, tileY].wall}` **{MKLP.IllegalWallProgression[Main.tile[tileX, tileY].wall]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            if (args.Action == GetDataHandlers.EditAction.KillTile ||
                args.Action == GetDataHandlers.EditAction.KillWall ||
                args.Action == GetDataHandlers.EditAction.TryKillTile)
            {
                if (tileY < (int)Main.worldSurface)
                {
                    if (args.Action == GetDataHandlers.EditAction.KillTile &&
                        breakableTiles.Contains(Main.tile[tileX, tileY].type)) return;


                    if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_protectsurface_break) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Surface_Break)
                    {
                        args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Surface_Break);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }


            }

            if (args.Action == GetDataHandlers.EditAction.PlaceTile ||
                args.Action == GetDataHandlers.EditAction.PlaceWall ||
                args.Action == GetDataHandlers.EditAction.ReplaceTile ||
                args.Action == GetDataHandlers.EditAction.ReplaceWall)
            {
                if (tileY < (int)Main.worldSurface)
                {
                    if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_protectsurface_place) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Surface_Place)
                    {
                        args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Surface_Place);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }

                ushort[] infectionb =
                {
                    TileID.CorruptGrass,
                    TileID.CrimsonGrass,
                    TileID.HallowedGrass,
                    TileID.CorruptJungleGrass,
                    TileID.CrimsonJungleGrass,

                    TileID.CorruptPlants,
                    TileID.CrimsonPlants,
                    TileID.HallowedPlants,
                    TileID.HallowedPlants2,

                    TileID.CorruptVines,
                    TileID.CrimsonVines,
                    TileID.HallowedVines,

                    TileID.CorruptThorns,
                    TileID.CrimsonThorns,

                    TileID.Ebonstone,
                    TileID.Crimstone,
                    TileID.Pearlstone,

                    TileID.Ebonsand,
                    TileID.Crimsand,
                    TileID.Pearlsand,
                    TileID.CorruptHardenedSand,
                    TileID.CrimsonHardenedSand,
                    TileID.HallowHardenedSand,
                    TileID.CorruptSandstone,
                    TileID.CrimsonSandstone,
                    TileID.HallowSandstone,

                    TileID.CorruptIce,
                    TileID.FleshIce,
                    TileID.HallowedIce
                };

                if (infectionb.Contains(Main.tile[tileX, tileY].type) && !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_infection) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Infection)
                {
                    args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Infection);
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            if (!NPC.downedBoss3)
            {
                if ((args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4) &&
                    (
                    !args.Player.HasPermission(MKLP.Config.Permissions.Ignore_IllegalWireProgression) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.item) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.give) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.manageitem)
                    ) && (bool)MKLP.Config.Main.Prevent_IllegalWire_Progression
                    )
                {
                    MKLP.Discordklp.KLPBotSendMessage_Warning($"Player **{args.Player.Name}** was Able to use Wire/Actuator on pre skeletron! `{tileX}, {tileY}`", args.Player.Account.Name, "Illegal Wire/Actuator Progression");
                    args.Player.SendErrorMessage("This is Illegal on this progression!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            if ((args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4) &&
                    tileY >= (int)Main.worldSurface &&
                    (bool)MKLP.Config.Main.ReceivedWarning_WirePlaceUnderground
                    )
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used Wire/Actuator below surface `{tileX}, {tileY}`");

            }

            if ((bool)MKLP.Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | {args.Action.ToString()}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )
            bool KillThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code1) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_1)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code1_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer,
                    ItemID.AncientChisel,
                    ItemID.MiningPotion
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code1_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.Bomb,
                    ItemID.StickyBomb,
                    ItemID.BouncyBomb,
                    ItemID.BombFish
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code1_maxbomb;
                        break;
                    }
                }
                int[] dynamite =
                {
                    ItemID.Dynamite,
                    ItemID.StickyDynamite,
                    ItemID.BouncyDynamite
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (dynamite.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code1_maxdynamite;
                        break;
                    }
                }

                if (args.Player.TileKillThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 1, args.Player, $"Breaking blocks to fast", $"Player **{args.Player.Name}** has exceeded TileKill Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }
                return false;
            }

            bool PlaceThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code2) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code2_maxdefault;


                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code2_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.DirtBomb,
                    ItemID.DirtStickyBomb
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code2_maxbomb;
                        break;
                    }
                }


                if (args.Player.TilePlaceThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 2, args.Player, $"Placing blocks too fast", $"Player **{args.Player.Name}** has exceeded TilePlace Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion



            #endregion
        }

        private static void OnPlaceObject(object? sender, GetDataHandlers.PlaceObjectEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            ushort Type = (ushort)args.Type;

            if (PlaceThreshold()) return;

            ushort[] SetupBastTile =
            {
                Terraria.ID.TileID.OpenDoor,
                Terraria.ID.TileID.ClosedDoor,
                Terraria.ID.TileID.TrapdoorOpen,
                Terraria.ID.TileID.TrapdoorClosed,
                Terraria.ID.TileID.Campfire
            };

            if ((bool)MKLP.Config.Main.Prevent_Place_BastStatueNearDoor && SetupBastTile.Contains(Type))
            {
                if (PossibleTransmutationGlitch1())
                {
                    args.Player.SendErrorMessage("You cannot place 'Bast_Statue & Door/Campfire' near each other!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }
            if ((bool)MKLP.Config.Main.Prevent_Place_BastStatueNearDoor && Type == Terraria.ID.TileID.CatBast)
            {
                if (PossibleTransmutationGlitch2())
                {
                    args.Player.SendErrorMessage("You cannot place 'Bast_Statue & Door/Campfire' near each other!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }

            if (MKLP.IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_3) && (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code3)
            {
                if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }
            if (MKLP.IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_3) && (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code3)
            {
                if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{MKLP.IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }


            if ((bool)MKLP.Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PlaceObject|type:{args.Type}|style:{args.Style}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( door near at bast statue )

            bool PossibleTransmutationGlitch1()
            {
                for (int x = tileX - 8; x <= tileX + 8; x++)
                {
                    for (int y = tileY - 8; y <= tileY + 8; y++)
                    {
                        if (x == tileX && y == tileY)
                            continue;
                        if (Main.tile[x, y].type == Terraria.ID.TileID.CatBast) return true;
                    }
                }
                return false;
            }

            bool PossibleTransmutationGlitch2()
            {
                for (int x = tileX - 8; x <= tileX + 8; x++)
                {
                    for (int y = tileY - 8; y <= tileY + 8; y++)
                    {
                        if (x == tileX && y == tileY)
                            continue;
                        if (SetupBastTile.Contains(Main.tile[x, y].type))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            #endregion

            #region ( Threshold )

            bool PlaceThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code2) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code2_maxdefault;


                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code2_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.DirtBomb,
                    ItemID.DirtStickyBomb
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code2_maxbomb;
                        break;
                    }
                }


                if (args.Player.TilePlaceThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 2, args.Player, $"Placing blocks to fast", $"Player **{args.Player.Name}** has exceeded TilePlace Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        private static void OnPaintTile(object? sender, GetDataHandlers.PaintTileEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            if (PaintThreshold()) return;


            if ((bool)MKLP.Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PaintTile|type:{args.type}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )

            bool PaintThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code3) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code3_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code3_maxboost;
                        break;
                    }
                }

                if (args.Player.PaintThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 3, args.Player, $"Painting too fast", $"Player **{args.Player.Name}** has exceeded Paint Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        private static  void OnPaintWall(object? sender, GetDataHandlers.PaintWallEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            if (PaintThreshold()) return;


            if ((bool)MKLP.Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PaintWall|type:{args.type}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )

            bool PaintThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code3) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code3_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code3_maxboost;
                        break;
                    }
                }

                if (args.Player.PaintThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 3, args.Player, $"Painting too fast", $"Player **{args.Player.Name}** has exceeded Paint Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        public static void OnMassWireOperation(object? sender, GetDataHandlers.MassWireOperationEventArgs args)
        {
            #region code

            if (!NPC.downedBoss3)
            {
                if ((
                    !args.Player.HasPermission(MKLP.Config.Permissions.Ignore_IllegalWireProgression) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.item) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.give) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.manageitem)
                    ) && (bool)MKLP.Config.Main.Prevent_IllegalWire_Progression
                    )
                {
                    MKLP.Discordklp.KLPBotSendMessage_Warning($"Player **{args.Player.Name}** was Able to use Wire/Actuator on pre skeletron! `start: {args.StartX}, {args.StartY} end: {args.EndX}, {args.EndY}`", args.Player.Account.Name, "Illegal Wire/Actuator Progression");
                    args.Player.SendErrorMessage("This is Illegal on this progression!");
                    args.Handled = true;
                    return;
                }
            }

            if ((args.StartY >= (int)Main.worldSurface || args.EndY >= (int)Main.worldSurface) && (bool)MKLP.Config.Main.ReceivedWarning_WirePlaceUnderground)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used mass Wire/Actuator below surface `start: {args.StartX}, {args.StartY} end: {args.EndX}, {args.EndY}`");

            }

            #endregion
        }

        private static void HandleLiquidInteraction(object? sender, GetDataHandlers.LiquidSetEventArgs args)
        {
            #region code
            int TileX = args.TileX;
            int TileY = args.TileY;

            // Log the interaction details
            string liquidName = args.Type switch
            {
                GetDataHandlers.LiquidType.Removal => "Removal",
                GetDataHandlers.LiquidType.Water => "Water",
                GetDataHandlers.LiquidType.Lava => "Lava",
                GetDataHandlers.LiquidType.Honey => "Honey",
                GetDataHandlers.LiquidType.Shimmer => "Shimmer"
            };
            if (LiquidThreshold()) return;


            if (TileY < (int)Main.worldSurface && args.Type != GetDataHandlers.LiquidType.Removal)
            {
                // Log liquid placed
                if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_protectsurface_placeliquid) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Surface_PlaceLiquid)
                {
                    args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Surface_PlaceLiquid);
                    args.Player.SendTileSquareCentered(TileX, TileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            #region ( Threshold )

            bool LiquidThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code4) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_4)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code4_maxdefault;

                int[] boost =
                {
                ItemID.HandOfCreation,
                ItemID.ArchitectGizmoPack,
                ItemID.BrickLayer,
                ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code4_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                ItemID.WetBomb,
                ItemID.LavaBomb,
                ItemID.HoneyBomb
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.type))
                    {
                        max = (int)MKLP.Config.Main.DisableNode.default_code4_maxbomb;
                        break;
                    }
                }

                if (args.Player.TileLiquidThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 4, args.Player, $"Exceeded Liquid place", $"Player **{args.Player.Name}** has exceeded TileLiquid Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(TileX, TileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion

            #endregion
        }

        public static List<int> WhiteList_Projectile_Identity = new();
        private static void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            #region code
            try
            {
                short ident = args.Identity;
                //Vector2 pos = args.Position;
                //Vector2 vel = args.Velocity;
                //float knockback = args.Knockback;
                //short damage = args.Damage;
                byte owner = args.Owner;
                short type = args.Type;
                //int index = args.Index;
                //float[] ai = args.Ai;

                if (WhiteList_Projectile_Identity.Contains(ident))
                {
                    WhiteList_Projectile_Identity.Remove(ident);
                    return;
                }
                if (ProjectileThreshold()) return;

                Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();

                if (args.Player.IsLoggedIn && MKLP.IllegalProjectileProgression.ContainsKey(type) &&
                    !args.Player.HasPermission(MKLP.Config.Permissions.IgnoreSurvivalCode_2) &&
                    (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code2)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Survival, 2, args.Player, $"{GetIllegalProj[type]} Projectile", $"Player **{args.Player.Name}** spawned illegal Projectile progression `itemheld: {args.Player.SelectedItem.type} projectile: {Lang.GetProjectileName(type)}` **{GetIllegalProj[type]}**"))
                    {
                        argsHandled();
                        return;
                    }
                }
                short[] InfectionProj =
                {
                    ProjectileID.ViciousPowder,
                    ProjectileID.VilePowder,

                    ProjectileID.CrimsonSpray,
                    ProjectileID.CorruptSpray,
                    ProjectileID.HallowSpray,

                    ProjectileID.BloodWater,
                    ProjectileID.UnholyWater,
                    ProjectileID.HolyWater
                };
                if (InfectionProj.Contains(type))
                {
                    if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_infection) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Infection)
                    {
                        args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Infection);
                        argsHandled();
                        return;
                    }
                }

                short[] SprayProj =
                {
                    ProjectileID.CorruptSpray,
                    ProjectileID.CrimsonSpray,
                    ProjectileID.DirtSpray,
                    ProjectileID.HallowSpray,
                    ProjectileID.MushroomSpray,
                    ProjectileID.PureSpray,
                    ProjectileID.SandSpray,
                    ProjectileID.SnowSpray
                };

                if (SprayProj.Contains(type))
                {
                    if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_spray) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Spray)
                    {
                        args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Spray);
                        argsHandled();
                        return;
                    }
                }

                if (args.Player.TileY <= (int)Main.worldSurface)
                {
                    short[] explosives =
                    {
                        //misc
                        ProjectileID.DirtBomb,
                        ProjectileID.DirtStickyBomb,

                        //reg bomb
                        ProjectileID.Bomb,
                        ProjectileID.StickyBomb,
                        ProjectileID.BouncyBomb,

                        //reg dynamite
                        ProjectileID.Dynamite,
                        ProjectileID.StickyDynamite,
                        ProjectileID.BouncyDynamite,

                        //others
                        ProjectileID.BombFish,
                        ProjectileID.LavaBomb,
                        ProjectileID.WetBomb,
                        ProjectileID.HoneyBomb,

                        //rocket
                        ProjectileID.RocketII,
                        ProjectileID.RocketSnowmanII,
                        ProjectileID.RocketIV,
                        ProjectileID.RocketSnowmanIV,

                        ProjectileID.ClusterFragmentsII,
                        ProjectileID.ClusterGrenadeII,
                        ProjectileID.ClusterMineII,
                        ProjectileID.ClusterRocketII,
                        ProjectileID.ClusterSnowmanFragmentsII,
                        ProjectileID.ClusterSnowmanRocketII,
                        ProjectileID.MiniNukeGrenadeII,
                        ProjectileID.MiniNukeMineII,
                        ProjectileID.MiniNukeRocketII,
                        ProjectileID.MiniNukeSnowmanRocketII,
                        ProjectileID.LavaGrenade,
                        ProjectileID.LavaMine,
                        ProjectileID.LavaRocket,
                        ProjectileID.LavaSnowmanRocket,

                        //celebratiomk
                        ProjectileID.Celeb2RocketExplosive,
                        ProjectileID.Celeb2RocketExplosiveLarge,
                        ProjectileID.Celeb2RocketLarge
                    };

                    if (explosives.Contains(type))
                    {
                        if (!args.Player.HasPermission(MKLP.Config.Permissions.IgnoreAntiGrief_protectsurface_explosive) && (bool)MKLP.Config.Main.AntiGrief.Using_AntiGrief_Surface_Explosive)
                        {
                            args.Player.SendErrorMessage(MKLP.Config.Main.AntiGrief.Message_AntiGrief_Surface_Explosive);
                            argsHandled();
                            return;
                        }
                    }
                }

                if (!(bool)MKLP.Config.Main.Allow_Players_MultipleFishingBobber && Main.projectile[ident].bobber)
                {
                    foreach (var get in Main.projectile)
                    {
                        if (get.identity == ident) continue;
                        if (get.owner != owner) continue;
                        if (!get.active) continue;
                        if (get.bobber)
                        {
                            argsHandled();
                            return;
                        }
                    }
                }
                if ((bool)MKLP.Config.Main.Prevent_Players_BypassMaxSummons)
                {
                    short[] SummonProjectiles =
                    {
                        //Pre-HM

                        ProjectileID.AbigailCounter,
                        ProjectileID.AbigailMinion,

                        ProjectileID.BabyBird,
                        ProjectileID.DeadCellsMushroomBoiMinion,
                        ProjectileID.FlinxMinion,
                        ProjectileID.BabySlime,
                        ProjectileID.VampireFrog,
                        ProjectileID.Hornet,
                        ProjectileID.FlyingImp,

                        //HM
                        ProjectileID.VenomSpider,
                        ProjectileID.JumperSpider,
                        ProjectileID.DangerousSpider,
                        ProjectileID.BatOfLight,//sanguine bat
                        ProjectileID.OneEyedPirate,
                        ProjectileID.SoulscourgePirate,
                        ProjectileID.PirateCaptain,
                        ProjectileID.Smolstar,//enchanted dagger
                        ProjectileID.Pygmy,
                        ProjectileID.Pygmy2,
                        ProjectileID.Pygmy3,
                        ProjectileID.Pygmy4,
                        ProjectileID.Pygmy4,

                        //require checking
                        ProjectileID.StormTigerGem,//1
                        ProjectileID.StormTigerTier1,//1
                        ProjectileID.StormTigerTier2,//4
                        ProjectileID.StormTigerTier3,//7

                        ProjectileID.DeadlySphere,
                        ProjectileID.Raven,
                        ProjectileID.UFOMinion,
                        ProjectileID.Tempest,
                        ProjectileID.StardustDragon2,
                        ProjectileID.StardustDragon3,
                        ProjectileID.StardustCellMinion,
                        ProjectileID.EmpressBlade,
                    };

                    if (SummonProjectiles.Contains(type))
                    {
                        int numberofsummons = 0;
                        int reti_sum = 0;
                        int spaz_sum = 0;
                        foreach (var get in Main.projectile)
                        {
                            if (get.identity == ident) continue;
                            if (get.owner != owner) continue;
                            if (!get.active) continue;
                            if (SummonProjectiles.Contains((short)get.type))
                            {
                                int minion = 1;
                                /*
                                switch ((short)get.type)
                                {
                                    case ProjectileID.StormTigerTier1:
                                        {
                                            if (get.originalDamage == 57 || get.originalDamage == 57)
                                            {
                                                minion = 2;
                                                break;
                                            } else if (get.originalDamage == 73 || get.originalDamage == 94)
                                            {
                                                minion = 3;
                                                break;
                                            }
                                            break;
                                        }
                                    case ProjectileID.StormTigerTier2:
                                        {
                                            minion = 4;
                                            if (get.originalDamage == 106 || get.originalDamage == 127)
                                            {
                                                minion = 5;
                                                break;
                                            } else if (get.originalDamage == 123 || get.originalDamage == 143)
                                            {
                                                minion = 6;
                                                break;
                                            }
                                            break;
                                        }
                                    case ProjectileID.StormTigerTier3:
                                        {
                                            minion = 7;
                                            if (get.originalDamage == 155 || get.originalDamage == 176)
                                            {
                                                minion = 8;
                                                break;
                                            } else if (get.originalDamage == 172 || get.originalDamage == 192)
                                            {
                                                minion = 9;
                                                break;
                                            }
                                            break;
                                        }
                                }
                                */

                                if (get.type == ProjectileID.Retanimini)
                                {
                                    reti_sum++;
                                    continue;
                                }
                                else if (get.type == ProjectileID.Spazmamini)
                                {
                                    spaz_sum++;
                                    continue;
                                }
                                numberofsummons += minion;
                                continue;
                            }
                        }
                        numberofsummons += (int)((reti_sum + spaz_sum) / 2);


                        if (ManagePlayer.GetPlayerMaxSummons(args.Player) <= numberofsummons)
                        {
                            if (type == ProjectileID.StormTigerGem ||
                                type == ProjectileID.StormTigerTier1 ||
                                type == ProjectileID.StormTigerTier2 ||
                                type == ProjectileID.StormTigerTier3)
                            {
                                foreach (var get in Main.projectile)
                                {
                                    if (get.identity == ident) continue;
                                    if (get.owner != owner) continue;
                                    if (!get.active) continue;
                                    if (type == ProjectileID.StormTigerTier1 ||
                                        type == ProjectileID.StormTigerTier2 ||
                                        type == ProjectileID.StormTigerTier3)
                                    {
                                        RemoveProj(get.whoAmI);
                                    }
                                }
                            }
                            if (type == ProjectileID.AbigailCounter ||
                                type == ProjectileID.AbigailMinion)
                            {
                                foreach (var get in Main.projectile)
                                {
                                    if (get.identity == ident) continue;
                                    if (get.owner != owner) continue;
                                    if (!get.active) continue;
                                    if (type == ProjectileID.AbigailMinion)
                                    {
                                        RemoveProj(get.whoAmI);
                                    }
                                }
                            }
                            argsHandled();
                            return;
                        }
                    }
                }
                if ((bool)MKLP.Config.Main.Prevent_Players_BypassMaxSentry)
                {
                    short[] OOASentry =
                    {

                        ProjectileID.DD2LightningAuraT2,
                        ProjectileID.DD2LightningAuraT3,
                        ProjectileID.DD2FlameBurstTowerT2,
                        ProjectileID.DD2FlameBurstTowerT3,
                        ProjectileID.DD2ExplosiveTrapT2,
                        ProjectileID.DD2ExplosiveTrapT3,
                        ProjectileID.DD2BallistraTowerT2,
                        ProjectileID.DD2BallistraTowerT3,
                    };
                    short[] SentryProjectiles =
                    {
                        //Pre-HM
                        ProjectileID.HoundiusShootius,

                        //OOA
                        ProjectileID.DD2LightningAuraT1,
                        ProjectileID.DD2FlameBurstTowerT1,
                        ProjectileID.DD2ExplosiveTrapT1,
                        ProjectileID.DD2BallistraTowerT1,

                        //HM
                        ProjectileID.SpiderHiver,//spider turret
                        ProjectileID.FrostHydra,
                        ProjectileID.MoonlordTurret,
                        ProjectileID.RainbowCrystal,
                        ProjectileID.DeadCellsBarnacle,

                        //OOA
                        ProjectileID.DD2LightningAuraT2,
                        ProjectileID.DD2LightningAuraT3,
                        ProjectileID.DD2FlameBurstTowerT2,
                        ProjectileID.DD2FlameBurstTowerT3,
                        ProjectileID.DD2ExplosiveTrapT2,
                        ProjectileID.DD2ExplosiveTrapT3,
                        ProjectileID.DD2BallistraTowerT2,
                        ProjectileID.DD2BallistraTowerT3,
                    };
                    if (SentryProjectiles.Contains(type) && !(Terraria.GameContent.Events.DD2Event.Ongoing && OOASentry.Contains(type)))
                    {
                        int numberofsentry = 0;
                        foreach (var get in Main.projectile)
                        {
                            if (get.identity == ident) continue;
                            if (get.owner != owner) continue;
                            if (!get.active) continue;
                            if (SentryProjectiles.Contains((short)get.type))
                            {
                                numberofsentry++;
                                continue;
                            }
                        }

                        if (ManagePlayer.GetPlayerMaxSentry(args.Player) <= numberofsentry)
                        {
                            argsHandled();
                            return;
                        }
                    }
                }

                bool ProjectileThreshold()
                {
                    if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code5) return false;
                    if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_5)) return false;

                    int max = (int)MKLP.Config.Main.DisableNode.default_code5_maxdefault;

                    if (Main.hardMode) max = (int)MKLP.Config.Main.DisableNode.default_code5_maxHM;

                    if (args.Player.ProjectileThreshold >= max)
                    {
                        if (MKLP.PunishPlayer(MKLP_CodeType.Default, 5, args.Player, $"Spawning too many projectiles at onces!", $"Player **{args.Player.Name}** Spawned to many projectile at onces! `itemheld: {args.Player.SelectedItem.type} projectile id: {type}` `Threshold: {max}`"))
                        {
                            argsHandled();
                            return true;
                        }
                    }

                    return false;
                }

                void argsHandled()
                {

                    args.Player.RemoveProjectile(ident, owner);
                    Main.projectile[ident].active = false;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", ident);
                    //TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", ident, owner);
                    args.Handled = true;
                }
                void RemoveProj(int projindex)
                {

                    args.Player.RemoveProjectile(projindex, owner);
                    Main.projectile[projindex].active = false;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", projindex);
                    //TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", ident, owner);
                    args.Handled = true;
                }
            }
            catch (OutOfMemoryException e)
            {
                MKLP_Console.SendLog_Exception(e);
                args.Handled = true;
            }

            #endregion
        }

        private static void OnHealOtherPlayer(object sender, GetDataHandlers.HealOtherPlayerEventArgs args)
        {
            #region code
            if (HealOtherThreshold()) return;

            bool HealOtherThreshold()
            {
                if (!(bool)MKLP.Config.Main.DisableNode.Using_Default_Code6) return false;
                if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreDefaultCode_6)) return false;

                int max = (int)MKLP.Config.Main.DisableNode.default_code6_maxdefault;

                if (NPC.downedPlantBoss) max = (int)MKLP.Config.Main.DisableNode.default_code6_maxPlant;

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    bool head = false; bool chestplate = false; bool leggings = false;
                    foreach (Item check in player.TPlayer.armor)
                    {
                        if (check.type == ItemID.SpectreHood) head = true;
                        if (check.type == ItemID.SpectreRobe) chestplate = true;
                        if (check.type == ItemID.SpectrePants) leggings = true;

                    }
                    if (head && chestplate && leggings)
                    {
                        max += (int)MKLP.Config.Main.DisableNode.default_code6_addmax_spectrehood;
                    }
                }

                if (args.Player.HealOtherThreshold >= max)
                {
                    if (MKLP.PunishPlayer(MKLP_CodeType.Default, 6, args.Player, $"Healing others to fast!", $"Player **{args.Player.Name}** has exceeded HealOther Threshold `itemheld: {args.Player.SelectedItem.type}` `Threshold: {max}`"))
                    {
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion
        }


        private static void OnNPCKilled(NpcKilledEventArgs args)
        {
            #region code
            //int[] BossIDs =
            //{
            //50, // King Slime
            // 4, // Eye of Cthulu			
            // 222, // Queen Bee
            // 13, // Eater of Worlds	
            // 266, // Brain of Cthulu
            // 35, // Skeletron
            // 668, // Deerclops
            // 113, // Wall of Flesh
            // 657, // Queen Slime
            // 125, // Retinazer
            // 127, // Skeletron Prime	
            // 134, // The Destroyer
            // 262, // Plantera
            // 245, // Golem
            // 636, // Empress Of Light
            // 370, // Duke Fishron
            // 439, // Lunatic Cultist
            // 396 // Moon Lord
            //};

            switch (args.npc.type)
            {
                case NPCID.KingSlime:
                    EUpdate(SurvivalManager.BossDType.KingSlime);
                    return;
                case NPCID.EyeofCthulhu:
                    EUpdate(SurvivalManager.BossDType.EyeOfCthulhu);
                    return;
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                    {
                        foreach (NPC gnpc in Main.npc)
                        {
                            if (gnpc == null) continue;
                            if (!gnpc.active) continue;
                            if (args.npc.whoAmI == gnpc.whoAmI) continue;
                            if (gnpc.type == NPCID.EaterofWorldsHead || gnpc.type == NPCID.EaterofWorldsBody || gnpc.type == NPCID.EaterofWorldsTail)
                            {
                                return;
                            }
                        }
                        EUpdate(SurvivalManager.BossDType.EvilBoss);
                        return;
                    }
                case NPCID.BrainofCthulhu:
                    EUpdate(SurvivalManager.BossDType.EvilBoss);
                    return;
                case NPCID.SkeletronHead:
                    EUpdate(SurvivalManager.BossDType.Skeletron);
                    return;
                case NPCID.QueenBee:
                    EUpdate(SurvivalManager.BossDType.QueenBee);
                    return;
                case NPCID.Deerclops:
                    EUpdate(SurvivalManager.BossDType.Deerclops);
                    return;
                case NPCID.WallofFlesh:
                    EUpdate(SurvivalManager.BossDType.WallOfFlesh);
                    return;
                case NPCID.QueenSlimeBoss:
                    EUpdate(SurvivalManager.BossDType.QueenSlime);
                    return;
                case NPCID.TheDestroyer:
                    EUpdate(SurvivalManager.BossDType.TheDestroyer);
                    return;
                case NPCID.Spazmatism:
                case NPCID.Retinazer:
                    {
                        foreach (NPC gnpc in Main.npc)
                        {
                            if (gnpc == null) continue;
                            if (!gnpc.active) continue;
                            if (args.npc.whoAmI == gnpc.whoAmI) continue;
                            if (args.npc.type == NPCID.Retinazer || args.npc.type == NPCID.Spazmatism)
                            {
                                return;
                            }
                        }

                        EUpdate(SurvivalManager.BossDType.TheTwins);
                        return;
                    }
                case NPCID.SkeletronPrime:
                    EUpdate(SurvivalManager.BossDType.SkeletronPrime);
                    return;
                case NPCID.Plantera:
                    EUpdate(SurvivalManager.BossDType.Plantera);
                    return;
                case NPCID.Golem:
                    EUpdate(SurvivalManager.BossDType.Golem);
                    return;
                case NPCID.DukeFishron:
                    EUpdate(SurvivalManager.BossDType.DukeFishron);
                    return;
                case NPCID.HallowBoss:
                    EUpdate(SurvivalManager.BossDType.EmpressOfLight);
                    return;
                case NPCID.CultistBoss:
                    EUpdate(SurvivalManager.BossDType.LunaticCultist);
                    return;
                case NPCID.MoonLordHead:
                case NPCID.MoonLordCore:
                    EUpdate(SurvivalManager.BossDType.MoonLord);
                    return;
            }

            return;

            void EUpdate(SurvivalManager.BossDType bosstype)
            {
                MKLP.IllegalItemProgression = SurvivalManager.GetIllegalItem(bosstype);

                MKLP.IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile(bosstype);

                MKLP.IllegalTileProgression = SurvivalManager.GetIllegalTile(bosstype);

                MKLP.IllegalWallProgression = SurvivalManager.GetIllegalWall(bosstype);
            }
            #endregion
        }

        private static bool nullboss_Confirmed_Twins = false;
        private static bool HandleSpawnBoss(GetDataHandlerArgs args)
        {
            #region code
            if (args.Player.IsBouncerThrottled())
            {
                return true;
            }

            var plr = args.Data.ReadInt16();
            var thingType = args.Data.ReadInt16();

            var isKnownBoss = thingType > 0 && thingType < Terraria.ID.NPCID.Count && NPCID.Sets.MPAllowedEnemies[thingType];

            NPC getnpc = new NPC();
            int npcid = 0;

            if (isKnownBoss)
            {
                getnpc.SetDefaults(thingType);
                npcid = getnpc.type;
            }

            if (plr != args.Player.Index)
                return true;

            if (args.Player.HasPermission(MKLP.Config.Permissions.IgnoreMainCode_2) || !(bool)MKLP.Config.Main.DisableNode.Using_Main_Code2) return false;

            switch (thingType)
            {
                /*
                case -18:
                    thing = GetString("{0} applied traveling merchant's satchel!", args.Player.Name);
                    break;
                case -17:
                    thing = GetString("{0} applied advanced combat techniques volume 2!", args.Player.Name);
                    break;
                */
                case -16: // Mechdusa
                    {
                        if (args.Player.SelectedItem.type != 5334)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Mechdusa`");
                        }

                        return false;
                    }
                /*
                case -15:
                    thing = GetString("{0} has sent a request to the slime delivery service!", args.Player.Name);
                    break;
                case -14:
                    thing = GetString("{0} has sent a request to the bunny delivery service!", args.Player.Name);
                    break;
                case -13:
                    thing = GetString("{0} has sent a request to the dog delivery service!", args.Player.Name);
                    break;
                case -12:
                    thing = GetString("{0} has sent a request to the cat delivery service!", args.Player.Name);
                    break;
                case -11:
                    thing = GetString("{0} applied advanced combat techniques!", args.Player.Name);
                    break;
                */
                case -10: // Blood Moon
                    {
                        if (args.Player.SelectedItem.type != 4271)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Blood Moon`");
                        }

                        break;
                    }
                case -8: // Impending doom approaches... ( Moon Lord )
                    {
                        if (args.Player.SelectedItem.type != 3601)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss/invasion: Impending doom approaches... (Moon Lord)`");
                        }

                        break;
                    }
                case -7: // Martians
                    {
                        return true;
                    }
                case -6: // Solar Eclipse
                    {
                        if (args.Player.SelectedItem.type != 2767)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Solar Eclipse`");
                        }

                        return false;
                    }
                case -5: // Frost Moon
                    {
                        if (args.Player.SelectedItem.type != 1958)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Frost Moon`");
                        }

                        return false;
                    }
                case -4: //  Pumpkin Moon
                    {
                        if (args.Player.SelectedItem.type != 1844)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Pumpkin Moon`");
                        }

                        return false;
                    }
                case -3: // Pirate Invasion
                    {
                        if (args.Player.SelectedItem.type != 1315)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Pirate Invasion`");
                        }

                        return false;
                    }
                case -2: // frost legion
                    {
                        if (args.Player.SelectedItem.type != 602)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Legion`");
                        }

                        return false;
                    }
                case -1: // goblin army
                    {
                        if (args.Player.SelectedItem.type != 361)
                        {
                            return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Goblin Army`");
                        }

                        return false;
                    }

                default:
                    NPC npc = new NPC();
                    npc.SetDefaults(thingType);

                    switch (npc.netID)
                    {
                        case 50: //king slime
                            {
                                if (args.Player.SelectedItem.type != 560)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: King Slime`");
                                }

                                break;
                            }
                        case 4: // Eye Of Cthulhu
                            {
                                if (args.Player.SelectedItem.type != 43)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eye Of Cthulhu`");
                                }

                                break;
                            }
                        case 13: // Eater Of Worlds
                        case 14:
                        case 15:
                            {
                                if (!args.Player.TPlayer.ZoneCorrupt)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eater Of Worlds` **not in corruption zone**");
                                }
                                if (args.Player.SelectedItem.type != 70)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eater Of Worlds`");
                                }

                                break;
                            }
                        case 266: // Brain Of Cthulhu
                            {
                                if (!args.Player.TPlayer.ZoneCrimson)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Brain Of Cthulhu` **not in crimson zone**");
                                }
                                if (args.Player.SelectedItem.type != 1331)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Brain Of Cthulhu`");
                                }

                                break;
                            }
                        case 222: // Queen Bee
                            {
                                if (!args.Player.TPlayer.ZoneJungle)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Bee` **not in jungle zone**");
                                }
                                if (args.Player.SelectedItem.type != 1133)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Bee`");
                                }

                                break;
                            }
                        case 668: // Deerclops
                            {
                                if (!args.Player.TPlayer.ZoneSnow)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Deerclops` **not in snow zone**");
                                }

                                if (args.Player.SelectedItem.type != 5120)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Deerclops`");
                                }

                                break;
                            }
                        case 35: // Skeletron
                        case 36:
                            {
                                if (NPC.downedBoss3)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Skeletron`");
                                }
                                break;
                            }
                        case 113: // Wall of Flesh
                            {
                                return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Wall Of Flesh`");
                            }
                        case 657: // Queen Slime
                            {
                                if (!args.Player.TPlayer.ZoneHallow)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Slime` **not in hallow zone**");
                                }
                                if (args.Player.SelectedItem.type != 4988)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Slime`");
                                }

                                break;
                            }
                        case 125: // The Twins
                            {
                                if (args.Player.SelectedItem.type != 544)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Twins`");
                                }
                                nullboss_Confirmed_Twins = true;
                                break;
                            }
                        case 126: // The Twins
                            {
                                if (!nullboss_Confirmed_Twins)
                                {
                                    if (args.Player.SelectedItem.type != 544)
                                    {
                                        return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Twins`");
                                    }
                                }
                                nullboss_Confirmed_Twins = false;
                                break;
                            }
                        case 134: // Destroyer
                        case 135:
                        case 136:
                            {
                                if (args.Player.SelectedItem.type != 556)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Destroyer`");
                                }

                                break;
                            }
                        case 127: // Skeletron Prime
                            {
                                if (args.Player.SelectedItem.type != 557)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Skeletron Prime`");
                                }

                                break;
                            }
                        case 262: // Plantera
                            {
                                return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Plantera`");
                            }
                        case 245: // Golem
                        case 246:
                        case 247:
                        case 248:
                            {
                                if (!NPC.downedPlantBoss)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Golem`");
                                }
                                /*
                                if (!args.Player.Inventory.All(i => i.type == 1293) && !args.Player.TPlayer.bank4.item.All(i => i.type == 1293))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                */
                                if (!IsNextGolemSpawn(args.Player))
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Golem` **not near on altar**");
                                }
                                break;
                            }
                        case 370: // Duke Fishron
                            {
                                if (!Main.hardMode)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron` **not hardmode**");
                                }
                                int[] fishing_rods =
                                {
                                    ItemID.WoodFishingPole,
                                    ItemID.ReinforcedFishingPole,
                                    ItemID.FisherofSouls,
                                    ItemID.Fleshcatcher,
                                    ItemID.ScarabFishingRod,
                                    4325, //chum caster
                                    ItemID.FiberglassFishingPole,
                                    ItemID.MechanicsRod,
                                    ItemID.SittingDucksFishingRod,
                                    ItemID.HotlineFishingHook,
                                    ItemID.GoldenFishingRod
                                };
                                if (!args.Player.TPlayer.ZoneBeach)
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron` **not in beach zone**");
                                }
                                if (!fishing_rods.Contains(args.Player.SelectedItem.type))
                                {
                                    return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron`");
                                }
                                /*
                                if (!args.Player.Inventory.All(i => fishing_rods.Contains(i.type)))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                if (!args.Player.Inventory.All(i => i.type == ItemID.TruffleWorm))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                */

                                break;
                            }
                        case 636: // Empress Of Light
                            {
                                return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Empress Of Light`");
                            }
                        case 440: // Lunatic Cultist
                            {
                                return MKLP.PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Lunatic Cultist`");
                            }
                    }

                    break;

            }

            return false;
            /*
            bool HasContainsItemID(TSPlayer player, params int[] itemids)
            {
                foreach (int itemid in  itemids)
                {
                    foreach (Item gettsitem in player.TPlayer.inventory)
                    {
                        if (gettsitem.netID == itemid)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            */

            bool IsNextGolemSpawn(TSPlayer player)
            {

                int playerX = (int)(player.TileX);
                int playerY = (int)(player.TileY);


                for (int x = playerX - 24; x <= playerX + 24; x++)
                {
                    for (int y = playerY - 24; y <= playerY + 24; y++)
                    {
                        if (x == playerX && y == playerY)
                            continue;
                        if (Main.tile[x, y].type == TileID.LihzahrdAltar)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            #endregion
        }

        private static void OnSignRead(object? sender, GetDataHandlers.SignReadEventArgs args)
        {
            #region code

            if (args.Player.ContainsData("MKLP_CheckLog_Sign"))
            {
                if (args.Player.GetData<string>("MKLP_CheckLog_Sign") == "OnTrack")
                {
                    args.Player.SetData("MKLP_CheckLog_Sign", $"{args.X}|{args.Y}");
                    args.Player.SendSuccessMessage($"[MKLP-SignLog] Selected SignPos ({args.X}, {args.Y})");
                }
            }

            #endregion
        }
        private static void OnSignChange(object? sender, GetDataHandlers.SignEventArgs args)
        {
            #region code
            // Reading the data
            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();

            if ((bool)MKLP.Config.Main.Logging.LogSign)
            {
                LogKLP.SignLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | ChangeSign|x:{posX}|y:{posY}|text : {newText}\n";
            }
            #endregion
        }

        #region ///
        /*
        public struct GetPlayerIG
        {
            public int PreviousHealth;
            public DateTime immunityTill;

            public GetPlayerIG(int prevhp)
            {
                PreviousHealth = prevhp;
                immunityTill = DateTime.MinValue;
            }
            public GetPlayerIG(int prevhp, DateTime immunity)
            {
                PreviousHealth = prevhp;
                immunityTill = immunity;
            }
        }

        private void OnNPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            #region code
            if ((bool)Config.Main.ServerSideDamage)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    //float getdistance = (float)Math.Sqrt(((args.Npc.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    * ((args.Npc.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    + ((args.Npc.Center.Y / 16) - (player.TPlayer.Center.Y / 16))
                    //    * ((args.Npc.Center.Y / 16) - (player.TPlayer.Center.Y / 16)));

                    float getdistance = player.TPlayer.Center.Distance(args.Npc.Center);

                    if (getdistance <= 32)
                    {
                        if (player.ContainsData("MKLP_GetPlayerIG"))
                        {
                            if (player.TPlayer.onHitDodge || player.TPlayer.shadowDodge) continue;

                            GetPlayerIG getplrdata = player.GetData<GetPlayerIG>("MKLP_GetPlayerIG");

                            if ((getplrdata.immunityTill - DateTime.UtcNow).TotalSeconds > 0) continue;

                            int totaldmg = args.Npc.damage - player.TPlayer.statDefense;
                            int hpresult = getplrdata.PreviousHealth - totaldmg;

                            if (player.TPlayer.statLife == getplrdata.PreviousHealth)
                            {
                                
                                //player.TPlayer.statLife -= totaldmg;
                                //player.TPlayer.statLife = hpresult;
                                //TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index);
                                //TSPlayer.All.SendData(PacketTypes.PlayerUpdate, number: player.Index);
                                //player.DamagePlayer(totaldmg);
                                
                                player.TPlayer.statLife = hpresult;
                                TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index, number2: hpresult);
                                TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended,
                                    $"=[{totaldmg}]=",
                                    (int)Color.Red.packedValue, player.X, player.Y);

                                DateTime getcd = DateTime.UtcNow;

                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife, getcd.AddMilliseconds(300)));
                            }
                            else
                            {
                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                            }
                        } else
                        {
                            player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                        }
                    }
                }
            }
            #endregion
        }


        private void OnProjectileAIUpdate(ProjectileAiUpdateEventArgs args)
        {
            #region code
            if (!args.Projectile.hostile) return;

            if ((bool)Config.Main.ServerSideDamage)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    //float getdistance = (float)Math.Sqrt(((args.Projectile.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    * ((args.Projectile.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    + ((args.Projectile.Center.Y / 16) - (player.TPlayer.Center.Y / 16))
                    //    * ((args.Projectile.Center.Y / 16) - (player.TPlayer.Center.Y / 16)));

                    float getdistance = player.TPlayer.Center.Distance(args.Projectile.Center);

                    if (getdistance <= 32)
                    {
                        if (player.ContainsData("MKLP_GetPlayerIG"))
                        {
                            if (player.TPlayer.onHitDodge || player.TPlayer.shadowDodge) continue;

                            GetPlayerIG getplrdata = player.GetData<GetPlayerIG>("MKLP_GetPlayerIG");

                            if ((getplrdata.immunityTill - DateTime.UtcNow).TotalSeconds > 0) continue;

                            int totaldmg = args.Projectile.damage - player.TPlayer.statDefense;
                            int hpresult = getplrdata.PreviousHealth - totaldmg;

                            if (player.TPlayer.statLife == getplrdata.PreviousHealth)
                            {
                                //player.TPlayer.statLife -= totaldmg;
                                player.TPlayer.statLife = hpresult;
                                TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index);
                                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, number: player.Index);
                                player.DamagePlayer(totaldmg);
                                TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended,
                                    $"=[{totaldmg}]=",
                                    (int)Color.Red.packedValue, player.X, player.Y);

                                DateTime getcd = DateTime.UtcNow;

                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife, getcd.AddMilliseconds(300)));
                            }
                            else
                            {
                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                            }
                        }
                        else
                        {
                            player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                        }
                    }
                }
            }
            #endregion
        }
        */
        #endregion

        private static void OnPlayerDamage(object sender, GetDataHandlers.PlayerDamageEventArgs args)
        {
            #region code

            if ((bool)MKLP.Config.Main.AntiRaid.DeathMessage_OnlyToLoginUser && args.Player.IsLoggedIn && args.ID != args.Player.Index && args.PlayerDeathReason._sourceOtherIndex != 16 &&
                (args.PlayerDeathReason._sourcePlayerIndex == -1 || args.PlayerDeathReason._sourceNPCIndex != -1 || args.PlayerDeathReason._sourceOtherIndex != -1 || args.PlayerDeathReason._sourceCustomReason != null))
            {
                args.Handled = true;
                return;
            }

            #endregion
        }

        private static void OnKillMe(object sender, GetDataHandlers.KillMeEventArgs args)
        {
            #region code

            if (args.PlayerDeathReason != null)
            {
                if ((bool)MKLP.Config.Main.AntiRaid.DeathMessage_OnlyToLoginUser && args.Player.IsLoggedIn && args.PlayerDeathReason._sourceCustomReason != null)
                {
                    args.Handled = true;
                    return;
                }
            }

            #endregion
        }

        #endregion

        #region {{ GameTer }}


        private static void OnWiringActuate(object? sender, HookEvents.Terraria.Wiring.ActuateEventArgs args)
        {

        }

        private static void OnHitWire(object? sender, HookEvents.Terraria.Wiring.HitWireEventArgs args)
        {

        }

        #endregion

        #region { Server }

        private static void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            #region code

            try
            {
                var literalText = Terraria.Localization.Language.GetText(args.Message._text).Value;

                if (args.Message._substitutions?.Length > 0)
                    literalText = string.Format(literalText, args.Message._substitutions);


                if (
                    literalText.EndsWith(" has joined.") ||
                    literalText.EndsWith(" has left.")
                    )
                {
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player == null) continue;
                        foreach (TSPlayer gplayer in TShock.Players)
                        {
                            if (gplayer == null) continue;
                            if (gplayer == player) continue;
                            if (gplayer.ContainsData("MKLP_Vanish"))
                            {
                                if (gplayer.GetData<bool>("MKLP_Vanish"))
                                {
                                    player.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                                }
                            }
                        }
                    }
                }

                if (literalText.EndsWith(" has awoken!"))
                {
                    args.Message._mode = NetworkText.Mode.LocalizationKey;
                    literalText = args.Message.ToString();

                    string bossName = literalText[..literalText.IndexOf(" has awoken!")];

                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.FullName.StartsWith(bossName) && npc.type == 0 && !npc.active)
                        {
                            args.Handled = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception(e);
            }
            #endregion
        }

        private static void OnReload(ReloadEventArgs args)
        {
            MKLP.Config = Config.Read();
            MKLP.LinkAccountManager.ReloadConfig();
            args.Player.SendMessage(MKLP.GetText("MKLP Config reloaded!"), Microsoft.Xna.Framework.Color.Purple);

            if (!MKLP.HasBanGuardPlugin && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                MKLP.Config.Main.UsingBanGuardPlugin = false;
                MKLP.Config.Changeall();
                args.Player.SendWarningMessage(MKLP.GetText("Warning: BanGuard plugin doesn't Exist on 'ServerPlugins' Folder!"));
                MKLP_Console.SendLog_Warning(MKLP.GetText("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!"));
            }
        }


        private static void OnWorldSave(WorldSaveEventArgs args)
        {
            checkplayers();

            if ((bool)MKLP.Config.BossManager.UseBossSchedule)
            {
                check_bosssched();
            }

            try
            {
                MKLP.InformLatestVersion();
            }
            catch { }

            LogKLP.SaveLog();

            AntiVPN.IPDataCleanSync();
        }

        #endregion

        #region { Auto Check }

        private static void OnServerStart(EventArgs args)
        {
            #region code

            MKLP.IllegalItemProgression = SurvivalManager.GetIllegalItem();

            MKLP.IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

            MKLP.IllegalTileProgression = SurvivalManager.GetIllegalTile();

            MKLP.IllegalWallProgression = SurvivalManager.GetIllegalWall();

            //Slow_Checking();

            #endregion
        }

        /*
        private static int interval_informlatestversion = 2;
        private async void Slow_Checking()
        {
            interval_informlatestversion++;

            checkplayermute();

            if ((bool)Config.BossManager.UseBossSchedule)
            {
                check_bosssched();
            }

            if (interval_informlatestversion >= 2)
            {
                try
                {
                    InformLatestVersion();
                } catch { }
                interval_informlatestversion = 0;
            }
            await Task.Delay(120000);
            Slow_Checking();
            
        }
        */

        public static void check_bosssched()
        {
            #region code
            if (!(bool)MKLP.Config.BossManager.UseBossSchedule) { return; }
            bool changed = false;
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowKingSlime && !(bool)MKLP.Config.BossManager.AllowKingSlime)
            {
                MKLP.Config.BossManager.AllowKingSlime = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("King Slime");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowEyeOfCthulhu && !(bool)MKLP.Config.BossManager.AllowEyeOfCthulhu)
            {
                MKLP.Config.BossManager.AllowEyeOfCthulhu = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Eye of Cthulhu");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowEaterOfWorlds && !(bool)MKLP.Config.BossManager.AllowEaterOfWorlds)
            {
                MKLP.Config.BossManager.AllowEaterOfWorlds = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowBrainOfCthulhu && !(bool)MKLP.Config.BossManager.AllowBrainOfCthulhu)
            {
                MKLP.Config.BossManager.AllowBrainOfCthulhu = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Brain of Cthulhu");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowQueenBee && !(bool)MKLP.Config.BossManager.AllowQueenBee)
            {
                MKLP.Config.BossManager.AllowQueenBee = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Queen Bee");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowSkeletron && !(bool)MKLP.Config.BossManager.AllowSkeletron)
            {
                MKLP.Config.BossManager.AllowSkeletron = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Skeletron");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowDeerclops && !(bool)MKLP.Config.BossManager.AllowDeerclops)
            {
                MKLP.Config.BossManager.AllowDeerclops = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Deerclops");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowWallOfFlesh && !(bool)MKLP.Config.BossManager.AllowWallOfFlesh)
            {
                MKLP.Config.BossManager.AllowWallOfFlesh = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Wall of Flesh");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowQueenSlime && !(bool)MKLP.Config.BossManager.AllowQueenSlime)
            {
                MKLP.Config.BossManager.AllowQueenSlime = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Queen Slime");
            }

            //mechanical boss
            if (Main.zenithWorld)
            {
                if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowMechdusa &&
                    (
                    !(bool)MKLP.Config.BossManager.AllowTheTwins &&
                    !(bool)MKLP.Config.BossManager.AllowTheDestroyer &&
                    !(bool)MKLP.Config.BossManager.AllowSkeletronPrime
                    )
                    )
                {
                    MKLP.Config.BossManager.AllowTheTwins = true;
                    MKLP.Config.BossManager.AllowTheDestroyer = true;
                    MKLP.Config.BossManager.AllowSkeletronPrime = true;
                    MKLP.Config.BossManager.AllowMechdusa = true;
                    changed = true;
                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa");
                }
            }
            else
            {
                if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowTheTwins && !(bool)MKLP.Config.BossManager.AllowTheTwins)
                {
                    MKLP.Config.BossManager.AllowTheTwins = true;
                    changed = true;
                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("The Twins");
                }
                if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowTheDestroyer && !(bool)MKLP.Config.BossManager.AllowTheDestroyer)
                {
                    MKLP.Config.BossManager.AllowTheDestroyer = true;
                    changed = true;
                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("The Destroyer");
                }
                if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowSkeletronPrime && !(bool)MKLP.Config.BossManager.AllowSkeletronPrime)
                {
                    MKLP.Config.BossManager.AllowSkeletronPrime = true;
                    changed = true;
                    MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Skeletron Prime");
                }
            }

            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowPlantera && !(bool)MKLP.Config.BossManager.AllowPlantera)
            {
                MKLP.Config.BossManager.AllowPlantera = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Plantera");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowGolem && !(bool)MKLP.Config.BossManager.AllowGolem)
            {
                MKLP.Config.BossManager.AllowGolem = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Golem");
            }

            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowDukeFishron && !(bool)MKLP.Config.BossManager.AllowDukeFishron)
            {
                MKLP.Config.BossManager.AllowDukeFishron = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Duke Fishron");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowEmpressOfLight && !(bool)MKLP.Config.BossManager.AllowEmpressOfLight)
            {
                MKLP.Config.BossManager.AllowEmpressOfLight = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Empress of Light");
            }

            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowLunaticCultist && !(bool)MKLP.Config.BossManager.AllowLunaticCultist)
            {
                MKLP.Config.BossManager.AllowLunaticCultist = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("Lunatic Cultist");
            }
            if (DateTime.UtcNow > (DateTime)MKLP.Config.BossManager.ScheduleAllowMoonLord && !(bool)MKLP.Config.BossManager.AllowMoonLord)
            {
                MKLP.Config.BossManager.AllowMoonLord = true;
                changed = true;
                MKLP.Discordklp.KLPBotSendMessage_BossEnabled("MoonLord");
            }

            if (changed)
            {
                MKLP.Config.Changeall();
            }
            #endregion
        }

        public static void checkplayers()
        {
            #region code
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                string getname = player.Name;
                if (player.IsLoggedIn) { getname = player.Account.Name; }

                var mutedata = MuteKLP.PlayerIsMuted(getname);

                if (!mutedata.muted && !mutedata.used)
                {
                    MuteKLP.SetMuteUsed(player, true);
                    player.SendSuccessMessage(MKLP.GetText("You're no longer muted."));
                }
            }
            #endregion
        }


        #endregion
    }
}
