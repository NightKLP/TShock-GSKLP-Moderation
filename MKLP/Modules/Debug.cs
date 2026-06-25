using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Microsoft.Xna.Framework;

//namespace AntiGodMode
//{
//    [ApiVersion(2, 1)]
//    public class AntiGodModePlugin : TerrariaPlugin
//    {
//        public override string Name => "AntiGodMode";
//        public override string Author => "NightKLP & IShadowDeep";
//        public override string Description => "anti godmode for cheaters";
//        public override Version Version => new Version(1, 0, 0);

//        private class PlayerInfo
//        {
//            public int hp = 100;
//            public int expectedHp = -1;
//            public int strikes = 0;
//            public bool waitingHp = false;
//            public DateTime hitTime = DateTime.MinValue;
//        }

//        private PlayerInfo[] players = new PlayerInfo[Main.maxPlayers];

//        public AntiGodModePlugin(Main game) : base(game) { }

//        public override void Initialize()
//        {
//            for (int i = 0; i < Main.maxPlayers; i++)
//                players[i] = new PlayerInfo();

//            ServerApi.Hooks.NetGetData.Register(this, OnData);
//            PlayerHooks.PlayerPostLogin += OnLogin;
//            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);

//            Commands.ChatCommands.Add(new Command("antigm.admin", CmdHandler, "antigm"));

//            TShock.Log.ConsoleInfo("[AntiGodMode] loaded.");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                ServerApi.Hooks.NetGetData.Deregister(this, OnData);
//                PlayerHooks.PlayerPostLogin -= OnLogin;
//                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
//            }
//            base.Dispose(disposing);
//        }

//        private void OnLogin(PlayerPostLoginEventArgs e)
//        {
//            var plr = e.Player;
//            players[plr.Index] = new PlayerInfo { hp = plr.TPlayer.statLife };

//            if (!plr.HasPermission("tshock.god") && !plr.HasPermission("antigm.bypass"))
//                plr.GodMode = false;
//        }

//        private void OnLeave(LeaveEventArgs e)
//        {
//            if (e.Who < Main.maxPlayers)
//                players[e.Who] = new PlayerInfo();
//        }

//        private void OnData(GetDataEventArgs args)
//        {
//            if (args.MsgID == PacketTypes.PlayerDamage)
//                HandleDamage(args);
//            else if (args.MsgID == PacketTypes.PlayerHp)
//                HandleHp(args);
//        }

//        private void HandleDamage(GetDataEventArgs args)
//        {
//            try
//            {
//                var r = args.Msg.reader;
//                int who = r.ReadByte();

//                if (who >= Main.maxPlayers) return;
//                var plr = TShock.Players[who];
//                if (plr == null || !plr.Active) return;

//                byte flags = r.ReadByte();
//                if ((flags & 1) != 0) r.ReadInt16();
//                if ((flags & 2) != 0) r.ReadInt16();
//                if ((flags & 4) != 0) r.ReadInt16();
//                if ((flags & 8) != 0) r.ReadInt16();
//                if ((flags & 16) != 0) r.ReadByte();
//                if ((flags & 32) != 0) r.ReadString();

//                int dmg = r.ReadInt16();
//                r.ReadByte();
//                byte extra = r.ReadByte();
//                bool crit = (extra & 2) != 0;

//                int def = plr.TPlayer.statDefense;
//                int realDmg = Math.Max(1, dmg - def / 2);
//                if (crit) realDmg = (int)(realDmg * 1.5);

//                var info = players[who];
//                info.waitingHp = true;
//                info.hitTime = DateTime.UtcNow;
//                info.expectedHp = plr.TPlayer.statLife - realDmg + 25;
//            }
//            catch { }
//        }

//        private void HandleHp(GetDataEventArgs args)
//        {
//            try
//            {
//                var r = args.GetReader();
//                int who = r.ReadByte();
//                int hp = r.ReadInt16();

//                if (who >= Main.maxPlayers) return;
//                var plr = TShock.Players[who];
//                if (plr == null || !plr.Active) return;

//                if (plr.HasPermission("tshock.god") || plr.HasPermission("antigm.bypass"))
//                {
//                    players[who].hp = hp;
//                    return;
//                }

//                var info = players[who];

//                if (info.waitingHp)
//                {
//                    double dt = (DateTime.UtcNow - info.hitTime).TotalSeconds;

//                    if (dt < 2.0 && info.expectedHp > 0)
//                    {
//                        if (hp > info.expectedHp && hp >= info.hp)
//                        {
//                            info.strikes++;
//                            TShock.Log.ConsoleWarn(
//                                $"[AntiGodMode] suspicious: {plr.Name} | " +
//                                $"got: {hp}  expected: <={info.expectedHp} | " +
//                                $"count: {info.strikes}"
//                            );

//                            if (info.strikes >= 10)
//                                Kick(plr);
//                            else if (info.strikes >= 5)
//                                Warn(plr, info.strikes);
//                        }
//                    }

//                    info.waitingHp = false;
//                }

//                info.hp = hp;
//            }
//            catch { }
//        }

//        private void Warn(TSPlayer plr, int count)
//        {
//            plr.SendErrorMessage($"[AntiGodMode] warning {count}/10");
//            TSPlayer.All.SendMessage($"[AntiGodMode] {plr.Name} is acting sus ({count}/10)", Color.Orange);
//        }

//        private void Kick(TSPlayer plr)
//        {
//            TShock.Log.ConsoleError($"[AntiGodMode] kicking {plr.Name} for godmode");
//            TSPlayer.All.SendMessage($"[AntiGodMode] {plr.Name} was kicked for using godmode.", Color.Red);
//            plr.Kick("godmode detected.", forceKick: true);
//        }

//        private void CmdHandler(CommandArgs args)
//        {
//            if (args.Parameters.Count == 0)
//            {
//                args.Player.SendInfoMessage(
//                    "antigm commands:\n" +
//                    "  /antigm list\n" +
//                    "  /antigm status <player>\n" +
//                    "  /antigm reset <player>\n" +
//                    "  /antigm godmode <on|off> <player>"
//                );
//                return;
//            }

//            var sub = args.Parameters[0].ToLower();

//            if (sub == "list")
//            {
//                bool found = false;
//                for (int i = 0; i < Main.maxPlayers; i++)
//                {
//                    var t = TShock.Players[i];
//                    if (t == null || !t.Active || players[i].strikes == 0) continue;
//                    args.Player.SendInfoMessage($"  {t.Name} → {players[i].strikes} strikes");
//                    found = true;
//                }
//                if (!found) args.Player.SendInfoMessage("no suspicious players.");
//            }
//            else if (sub == "status" && args.Parameters.Count > 1)
//            {
//                var res = TSPlayer.FindByNameOrID(args.Parameters[1]);
//                if (res.Count == 0) { args.Player.SendErrorMessage("player not found."); return; }

//                var t = res[0];
//                var i = players[t.Index];
//                args.Player.SendInfoMessage(
//                    $"{t.Name}  |  hp: {i.hp}  |  strikes: {i.strikes}  |  tshock gm: {t.GodMode}"
//                );
//            }
//            else if (sub == "reset" && args.Parameters.Count > 1)
//            {
//                var res = TSPlayer.FindByNameOrID(args.Parameters[1]);
//                if (res.Count == 0) { args.Player.SendErrorMessage("player not found."); return; }

//                players[res[0].Index].strikes = 0;
//                args.Player.SendSuccessMessage($"reset strikes for {res[0].Name}.");
//            }
//            else if (sub == "godmode" && args.Parameters.Count > 2)
//            {
//                var res = TSPlayer.FindByNameOrID(args.Parameters[2]);
//                if (res.Count == 0) { args.Player.SendErrorMessage("player not found."); return; }

//                bool on = args.Parameters[1] == "on";
//                res[0].GodMode = on;
//                args.Player.SendSuccessMessage($"godmode {(on ? "enabled" : "disabled")} for {res[0].Name}.");
//            }
//            else
//            {
//                args.Player.SendErrorMessage("unknown subcommand. use /antigm for help.");
//            }
//        }
//    }
//}
// NIGHT IF YOU ARE WATCHING THIS I LOVE YOU MAN!