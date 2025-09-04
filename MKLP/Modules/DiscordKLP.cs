
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//microsoft
using Microsoft.Xna.Framework;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
//discord
using Discord;
using Discord.Net;
using Discord.Interactions;
using Discord.WebSocket;
using TShockAPI;
using Color = Microsoft.Xna.Framework.Color;
using TShockAPI.DB;
using Terraria;
using Microsoft.Xna.Framework.Input;
using System.Drawing;
using MKLP.Functions;

namespace MKLP.Modules
{
    public class DiscordKLP
    {

        private DiscordSocketClient _client;
        //private MessageQueue messageQueue { get; set; }

        private static readonly string TSStaffPermission = MKLP.Config.Permissions.Staff;

        private static readonly Discord.Color EmbedColor = Discord.Color.DarkBlue;

        public static char S_ = MKLP.Config.Main.Seperator;

        public async void Initialize()
        {
            #region | Discord Initialize |

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
            });

            _client.Log += Log;
            _client.Ready += Ready;
            _client.ButtonExecuted += ButtonHandler;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.MessageReceived += MessageRecieved;
            _client.ModalSubmitted += ModalHandler;
            _client.SelectMenuExecuted += SelectMenuHandler;

            _client.LoginAsync(TokenType.Bot, MKLP.Config.Discord.BotToken).Wait();
            _client.StartAsync().Wait();

            //messageQueue = new MessageQueue(500);
            //messageQueue.OnReadyToSend += this.OnMessageReadyToSend;

            await Task.Delay(-1);

            #endregion
        }


        #region ==[ Discord ]==

        private Task Log(LogMessage args)
        {
            #region code
            if (args.Exception != null)
            {
                MKLP_Console.SendLog_Message_DiscordBot($"{args.Exception}", $" =(Exception)=", ConsoleColor.Gray, ConsoleColor.DarkRed);
                return Task.CompletedTask;
            }

            if (args.Source != "Gateway") return Task.CompletedTask;

            ConsoleColor typeconsolecolor = ConsoleColor.DarkYellow;

            switch (args.Severity)
            {
                case LogSeverity.Warning:
                    typeconsolecolor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    typeconsolecolor = ConsoleColor.White;
                    break;
                case LogSeverity.Error:
                    typeconsolecolor = ConsoleColor.Red;
                    break;
                default:
                    return Task.CompletedTask;
            }
            switch (args.Message)
            {
                case "Disconnecting":
                    MKLP_Console.SendLog_Message_DiscordBot($"{args.Message}", $" -{args.Severity}-", typeconsolecolor, ConsoleColor.DarkRed);
                    break;
                case "Disconnected":
                    MKLP_Console.SendLog_Message_DiscordBot($"{args.Message}", $" -{args.Severity}-", typeconsolecolor, ConsoleColor.DarkRed);
                    break;
                case "Connecting":
                    MKLP_Console.SendLog_Message_DiscordBot($"{args.Message}", $" -{args.Severity}-", typeconsolecolor, ConsoleColor.DarkGreen);
                    break;
                case "Connected":
                    MKLP_Console.SendLog_Message_DiscordBot($"{args.Message}", $" -{args.Severity}-", typeconsolecolor, ConsoleColor.DarkGreen);
                    break;
                case "Ready":
                    MKLP_Console.SendLog_Message_DiscordBot($"Bot is connected and ready!", $" -{args.Severity}-", typeconsolecolor, ConsoleColor.Green);
                    break;
                default:
                    MKLP_Console.SendLog_Message_DiscordBot($"{args.Message}", $" -{args.Severity}-", typeconsolecolor);
                    break;
            }
            return Task.CompletedTask;
            #endregion
        }

        Dictionary<string, ulong> CommandIDs = new();
        void CommandIDs_Add(string commandname, ulong id)
        {
            if (CommandIDs.ContainsKey(commandname)) return;

            CommandIDs.Add(commandname, id);
        }
        string CommandIDs_GetMention(string commandname)
        {
            if (!CommandIDs.ContainsKey(commandname)) return $"/{commandname}";

            return $"</{commandname}:{CommandIDs[commandname]}>";
        }
        private async Task Ready()
        {
            #region code

            if ((ulong)MKLP.Config.Discord.MainGuildID == 0) return;
            if ((ulong)MKLP.Config.Discord.MainGuildID == null) return;

            var guild = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID);

            #region [ Slash Commands ]

            string SlashCommandName = "";

            List<SlashCommandBuilder> Guildcommands = new()
            {
                new SlashCommandBuilder()
                    .WithName(SlashCommandName + "mklp-help")
                    .WithDescription("get list of commands"),
                new SlashCommandBuilder()
                    .WithName(SlashCommandName + "moderation")
                    .WithDescription("Manage Server in-game"),
                new SlashCommandBuilder()
                    .WithName(SlashCommandName + "moderation-user")
                    .WithDescription("Manage players account")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("user")
                        .WithDescription("ingame account you want to moderate")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(true)
                        ),
                new SlashCommandBuilder()
                    .WithName(SlashCommandName + "ingame-command")
                    .WithDescription("execute a command ingame!")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("command")
                        .WithDescription("type a command")
                        .WithType(ApplicationCommandOptionType.String)
                        .WithRequired(true)
                        )
            };

            if (MKLP.Config.Discord.SlashCommandName != "")
            {
                SlashCommandName = $"{MKLP.Config.Discord.SlashCommandName}";

                Guildcommands = new()
                {
                    new SlashCommandBuilder()
                    .WithName(SlashCommandName)
                    .WithDescription("MKLP Command")
                    .AddOption(new SlashCommandOptionBuilder()
                            .WithName("mklp-help")
                            .WithDescription("get list of commands")
                            .WithType(ApplicationCommandOptionType.SubCommand))
                    .AddOption(new SlashCommandOptionBuilder()
                            .WithName("moderation")
                            .WithDescription("Manage Server in-game")
                            .WithType(ApplicationCommandOptionType.SubCommand))
                    .AddOption(new SlashCommandOptionBuilder()
                            .WithName("moderation-user")
                            .WithDescription("Manage players account")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("user")
                                .WithDescription("ingame account you want to moderate")
                                .WithType(ApplicationCommandOptionType.String)
                                .WithRequired(true)
                                ))
                    .AddOption(new SlashCommandOptionBuilder()
                            .WithName("ingame-command")
                            .WithDescription("execute a command ingame!")
                                .WithType(ApplicationCommandOptionType.SubCommand)
                            .AddOption(new SlashCommandOptionBuilder()
                                .WithName("command")
                                .WithDescription("type a command")
                                .WithType(ApplicationCommandOptionType.String)
                                .WithRequired(true)
                                ))
                };
            }

            #endregion


            try
            {
                // building slash command commands
                foreach (var command in Guildcommands)
                {
                    var cmdbuild = command.Build();
                    var resultcmd = await guild.CreateApplicationCommandAsync(cmdbuild);
                    if (SlashCommandName != "")
                    {
                        foreach (var subcmd in resultcmd.Options)
                        {
                            CommandIDs_Add(resultcmd.Name + " " + subcmd.Name, resultcmd.Id);
                        }
                    } else
                    {
                        CommandIDs_Add(resultcmd.Name, resultcmd.Id);
                    }
                }
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }

            // logs
            #endregion
        }

        private async Task ButtonHandler(SocketMessageComponent message)
        {
            #region code

            if (message.Data.CustomId.Split(S_)[0] != "MKLP") return;

            UserAccount executer = GetUserIDAccHasPermission(message.User.Id, TSStaffPermission);
            if (executer == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            switch (message.Data.CustomId.Split(S_)[1])
            {
                case "DismissMsg":
                    #region ( Type | DismissMessage )
                    {
                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "Disabled":
                                #region ( Type: disable message )
                                {
                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton("Check Player", "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton("Quick Ban [ permanent ]", "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true)
                                        .WithButton("Enable", "X3", ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1, disabled: true);
                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });
                                    return;
                                }
                            #endregion
                            case "Warning":
                                #region ( Type: warning message )
                                {
                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton("Check Player", "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton("Quick Ban [ permanent ]", "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true);
                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });
                                    return;
                                }
                            #endregion
                            case "Report1":
                                #region ( Type: Report1 message )
                                {

                                    MKLP.DBManager.DeleteReport(int.Parse(message.Data.CustomId.Split(S_)[3]));

                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton("Check Player", "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton("Quick Ban [ permanent ]", "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true);

                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });

                                    await message.RespondAsync($"Report Ticket no. {message.Data.CustomId.Split(S_)[3]} Dismissed", ephemeral: true);
                                    return;
                                }
                            #endregion
                            case "Report2":
                                #region ( Type: Report2 message )
                                {

                                    MKLP.DBManager.DeleteReport(int.Parse(message.Data.CustomId.Split(S_)[3]));

                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true);

                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });

                                    await message.RespondAsync($"Report Ticket no. {message.Data.CustomId.Split(S_)[3]} Dismissed", ephemeral: true);
                                    return;
                                }
                                #endregion
                        }
                        return;
                    }
                #endregion
                case "SendMsg":
                    #region ( Type | SendMessage )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split(S_)[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.TPlayer.statLife}/{targetplayer.TPlayer.statLifeMax}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.TPlayer.statMana}/{targetplayer.TPlayer.statManaMax}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.ContainsKey(Identifier.Name + targetplayer.Name) || MKLP.DisabledKey.ContainsKey(Identifier.IP + targetplayer.IP) || MKLP.DisabledKey.ContainsKey(Identifier.UUID + targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());


                                                break;
                                            }
                                    }



                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split(S_)[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                int total = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split(S_)[0] == targetplayer.Account.Name)
                                                    {
                                                        total++;
                                                    }
                                                }
                                                int count = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split(S_)[0] == targetplayer.Account.Name)
                                                    {
                                                        count++;
                                                        string previtem = $"{Lang.GetItemNameValue(int.Parse(invlog.Split(S_)[3].Split(",")[0]))} ({invlog.Split(S_)[3].Split(",")[1]})";
                                                        string item = $"{Lang.GetItemNameValue(int.Parse(invlog.Split(S_)[4].Split(",")[0]))} ({invlog.Split(S_)[4].Split(",")[1]})";
                                                        if (count < (total - 20)) continue;
                                                        EmbedDescription += $"{invlog.Split(S_)[1]}{invlog.Split(S_)[2]}: {previtem} => {item}\n";
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription + "```")
                                        .WithColor(EmbedColor);

                                    await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                #endregion
                case "EditMsg":
                    #region ( Type | EditMessage )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split(S_)[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.TPlayer.statLife}/{targetplayer.TPlayer.statLifeMax}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.TPlayer.statMana}/{targetplayer.TPlayer.statManaMax}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.ContainsKey(Identifier.Name + targetplayer.Name) || MKLP.DisabledKey.ContainsKey(Identifier.IP + targetplayer.IP) || MKLP.DisabledKey.ContainsKey(Identifier.UUID + targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.DeferAsync(true);

                                                await message.ModifyOriginalResponseAsync(msg => {
                                                    msg.Embed = embed.Build();
                                                    msg.Components = buttons.Build();
                                                });

                                                break;
                                            }
                                    }



                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split(S_)[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n**ActiveChest:** `{targetplayer.ActiveChest}`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                int total = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split(S_)[0] == targetplayer.Account.Name)
                                                    {
                                                        total++;
                                                    }
                                                }
                                                int count = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split(S_)[0] == targetplayer.Account.Name)
                                                    {
                                                        count++;
                                                        string previtem = $"{Lang.GetItemNameValue(int.Parse(invlog.Split(S_)[3].Split(",")[0]))} ({invlog.Split(S_)[3].Split(",")[1]})";
                                                        string item = $"{Lang.GetItemNameValue(int.Parse(invlog.Split(S_)[4].Split(",")[0]))} ({invlog.Split(S_)[4].Split(",")[1]})";
                                                        if (count < (total - 20)) continue;
                                                        EmbedDescription += $"{invlog.Split(S_)[1]}{invlog.Split(S_)[2]}: {previtem} => {item}\n";
                                                    }
                                                }

                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription + "```")
                                        .WithColor(EmbedColor);

                                    await message.DeferAsync(true);

                                    await message.ModifyOriginalResponseAsync(msg => {
                                        msg.Embed = embed.Build();
                                        msg.Components = buttons.Build();
                                    });

                                    return;
                                }
                            #endregion
                            case "ServerModView":
                                #region ( Type: ServerModView )
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

                                    if (stringplayers == "") stringplayers = "No Players Online...";


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
                                    if (defeatedbosses == "") defeatedbosses = "No Bosses Defeated...";

                                    string defeatedinvasion = GetListDefeatedInvasion();
                                    if (defeatedinvasion == "") defeatedinvasion = "no Invasions Completed...";

                                    string OngoingActivity = GetListActivities();
                                    if (OngoingActivity == "") OngoingActivity = "Nothing is Happening...";

                                    string reportlist = "";

                                    foreach (MKLP_Report report in MKLP.DBManager.GetReportList(4))
                                    {
                                        reportlist +=
                                            $"**'{report.From}' Report** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                            $"\n> **ID:** {report.ID}" +
                                            $"\n> **Location:** `{report.Location}`" +
                                            $"\n> **Players online during report:** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                            $"\n> " +
                                            $"\n> **target:** {(report.Target == "" ? "none" : report.Target)}" +
                                            $"\n> **Message:** {report.Message}\n\n";
                                    }

                                    var embed = new EmbedBuilder()
                                            .WithTitle("Server Moderation Menu")
                                            .WithDescription("## 📑 Latest Report" +
                                            $"\n{(reportlist == "" ? "no latest reports today..." : reportlist)}")
                                            .WithColor(EmbedColor)
                                            .WithFields(
                                                new EmbedFieldBuilder()
                                                    .WithName($"Online Players [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                                    .WithValue(stringplayers),
                                                new EmbedFieldBuilder()
                                                    .WithName("Bosses")
                                                    .WithValue(defeatedbosses)
                                                    .WithIsInline(true),
                                                new EmbedFieldBuilder()
                                                    .WithName("Invasions Defeated")
                                                    .WithValue(defeatedinvasion)
                                                    .WithIsInline(true),
                                                new EmbedFieldBuilder()
                                                    .WithName("Activities")
                                                    .WithValue(OngoingActivity)
                                            )
                                            .Build();


                                    if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                                    {

                                        var menuBuilder = new SelectMenuBuilder()
                                        .WithPlaceholder("Select a Player")
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
                                            menuBuilder.AddOption(player.Name, player.Account.Name, $"Account: {player.Account.Name}");
                                        }

                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0)
                                            .WithSelectMenu(menuBuilder, row: 1);

                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = component.Build();
                                        });

                                    }
                                    else
                                    {
                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0);

                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = component.Build();
                                        });
                                    }
                                    return;
                                }
                            #endregion
                            case "PlayerModViewOffline":
                                #region ( Type: PlayerModViewOffline )
                                {
                                    UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(message.Data.CustomId.Split(S_)[4]);
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModViewOffline_Main_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX", ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Reports from them", "MKLP_EditMsg_PlayerModViewOffline_Report1_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton($"{getuseraccount.Name} Reports", "MKLP_EditMsg_PlayerModViewOffline_Report2_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2);

                                                ulong? getuserid = null;

                                                try
                                                {
                                                    getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(getuseraccount.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(getuseraccount.Name);
                                                }
                                                catch { }

                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Account [ {getuseraccount.Name} ]")
                                                    .WithDescription(
                                                        $"**Account ID:** `{getuseraccount.ID}`" +
                                                        $"\n**Is in the server?:** `{(targetplayer == null ? "no" : "yes")}`" +
                                                        $"\n**Group:** `{getuseraccount.Group}`" +
                                                        $"\n**Registered Since:** `{getuseraccount.Registered}` {GetSince(DateTime.Parse(getuseraccount.Registered))}" +
                                                        $"\n**Last Accessed:** `{getuseraccount.LastAccessed}` {GetSince(DateTime.Parse(getuseraccount.LastAccessed))}" +
                                                        $"{(getuserid == null ? "" : $"\n\n**Discord UserID:** `{(ulong)getuserid}`")}"
                                                    ).WithColor(EmbedColor);

                                                await message.DeferAsync(true);

                                                await message.ModifyOriginalResponseAsync(msg => {
                                                    msg.Embed = embed.Build();
                                                    msg.Components = buttons.Build();
                                                });

                                                return;
                                            }
                                        case "Report1":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModViewOffline_Report1_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "MKLP_EditMsg_PlayerModViewOffline_Main_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary)
                                                    .WithButton("Reports from them", "XXX" + getuseraccount.Name, ButtonStyle.Primary, row: 1, disabled: true)
                                                    .WithButton($"{getuseraccount.Name} Reports", "MKLP_EditMsg_PlayerModViewOffline_Report2_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2);

                                                string reportlist = "";

                                                foreach (MKLP_Report report in MKLP.DBManager.GetReportList(5, target: getuseraccount.Name))
                                                {
                                                    reportlist +=
                                                        $"**'{report.From}' Report** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                                        $"\n> **ID:** {report.ID}" +
                                                        $"\n> **Location:** `{report.Location}`" +
                                                        $"\n> **Players online during report:** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                                        $"\n> " +
                                                        $"\n> **target:** {(report.Target == "" ? "none" : report.Target)}" +
                                                        $"\n> **Message:** {report.Message}\n\n";
                                                }

                                                if (reportlist == "") reportlist = "No reports...";

                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Account [ {getuseraccount.Name} ]")
                                                    .WithDescription(
                                                        "## Reports from them\n\n" +
                                                        reportlist
                                                    ).WithColor(EmbedColor);

                                                await message.DeferAsync(true);

                                                await message.ModifyOriginalResponseAsync(msg => {
                                                    msg.Embed = embed.Build();
                                                    msg.Components = buttons.Build();
                                                });

                                                return;
                                            }
                                        case "Report2":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModViewOffline_Report2_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "MKLP_EditMsg_PlayerModViewOffline_Main_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary)
                                                    .WithButton("Reports from them", "MKLP_EditMsg_PlayerModViewOffline_Report1_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton($"{getuseraccount.Name} Reports", "XXX", ButtonStyle.Primary, row: 1, disabled: true)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2);

                                                string reportlist = "";

                                                foreach (MKLP_Report report in MKLP.DBManager.GetReportList(5, from: getuseraccount.Name))
                                                {
                                                    reportlist +=
                                                        $"**'{report.From}' Report** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                                        $"\n> **ID:** {report.ID}" +
                                                        $"\n> **Location:** `{report.Location}`" +
                                                        $"\n> **Players online during report:** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                                        $"\n> " +
                                                        $"\n> **target:** {(report.Target == "" ? "none" : report.Target)}" +
                                                        $"\n> **Message:** {report.Message}\n\n";
                                                }

                                                if (reportlist == "") reportlist = "No reports...";

                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Account [ {getuseraccount.Name} ]")
                                                    .WithDescription(
                                                        $"## ( {getuseraccount.Name} ) Reports\n\n" +
                                                        reportlist
                                                    ).WithColor(EmbedColor);

                                                await message.DeferAsync(true);

                                                await message.ModifyOriginalResponseAsync(msg => {
                                                    msg.Embed = embed.Build();
                                                    msg.Components = buttons.Build();
                                                });

                                                return;
                                            }
                                    }


                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                #endregion
                case "InGame":
                    #region ( Type | InGame )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "PlayerAction":
                                #region ( Type => PlayerAction )
                                {

                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Ban":
                                            #region ( Type: Ban )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Ban))
                                                {
                                                    await message.RespondAsync("You do not have permission to Ban a player!", ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Banning [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput("Reason", "Ban_reason".Replace('_', S_), TextInputStyle.Paragraph, "Cheating")
                                                    .AddTextInput("Duration", "Ban_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15);

                                                if ((bool)MKLP.Config.Main.UsingBanGuardPlugin)
                                                {
                                                    modal.AddTextInput("BanGuardType", "Ban_BGCategory".Replace('_', S_), TextInputStyle.Short, "Type -auto if you want to automatic categories ( leave it blank if you don't want to use BanGuard )", maxLength: 30, required: false);
                                                }

                                                await message.RespondWithModalAsync(modal.Build());

                                                return;
                                            }
                                        #endregion
                                        case "QBan":
                                            #region ( Type: QBan )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Ban))
                                                {
                                                    await message.RespondAsync("You do not have permission to Ban a player!", ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Banning [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput("Reason", "Ban_reason".Replace('_', S_), TextInputStyle.Paragraph, "Cheating", value: message.Data.CustomId.Split(S_)[5])
                                                    .AddTextInput("Duration", "Ban_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15, value: "Permanent");

                                                if ((bool)MKLP.Config.Main.UsingBanGuardPlugin)
                                                {
                                                    modal.AddTextInput("BanGuardType", "Ban_BGCategory".Replace('_', S_), TextInputStyle.Short, "Type -auto if you want to automatic categories ( leave it blank if you don't want to use BanGuard )", maxLength: 30, required: false);
                                                }

                                                await message.RespondWithModalAsync(modal.Build());

                                                return;
                                                /*
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Ban))
                                                {
                                                    await message.RespondAsync("You do not have permission to Ban a player!", ephemeral: true);
                                                    return;
                                                }

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer != null)
                                                {
                                                    if (ManagePlayer.OnlineBan(false, targetplayer, message.Data.CustomId.Split(S_)[5], executer.Name, DateTime.MaxValue, true, true))
                                                    {
                                                        await message.RespondAsync($"Successfully Banned **{targetplayer.Name}**", ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync($"Player **{targetplayer.Name}** was already banned", ephemeral: true);
                                                    }

                                                } else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(message.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await message.RespondAsync($"Account **{message.Data.CustomId.Split(S_)[4]}** does'nt exist!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineBan))
                                                    {
                                                        await message.RespondAsync("You do not have permission to Offline Ban a player!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineBan(account, message.Data.CustomId.Split(S_)[5], executer.Name, DateTime.MaxValue, true, true))
                                                    {
                                                        await message.RespondAsync($"Successfully Banned **{account.Name}**", ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync($"Player **{account.Name}** was already banned", ephemeral: true);
                                                    }
                                                }

                                                return;
                                                */
                                            }
                                        #endregion
                                        case "Disable":
                                            #region ( Type: Disable )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Disable))
                                                {
                                                    await message.RespondAsync("You do not have permission to Disable a player!", ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Disable [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput("Reason", "Disable_reason".Replace('_', S_), TextInputStyle.Paragraph, "Cheating");

                                                await message.RespondWithModalAsync(modal.Build());
                                                return;
                                            }
                                        #endregion
                                        case "Undisable":
                                            #region ( Type: Undisable )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Disable))
                                                {
                                                    await message.RespondAsync("You do not have permission to Enable a player!", ephemeral: true);
                                                    return;
                                                }
                                                string dummy1;
                                                IEnumerable<string> dummy2;

                                                switch (ManagePlayer.UnDisablePlayer(message.Data.CustomId.Split(S_)[4], AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineEnable), true, out dummy1, out dummy2, executer.Name))
                                                {
                                                    case ManagePlayer.DisableResult.AlreadyEnabled:
                                                        {
                                                            await message.RespondAsync($"Player **{message.Data.CustomId.Split(S_)[4]}** isn't disabled", ephemeral: true);
                                                            break;
                                                        }
                                                    case ManagePlayer.DisableResult.SuccessOffline:
                                                        {
                                                            await message.RespondAsync($"(Offline) Successfully Enable **{message.Data.CustomId.Split(S_)[4]}**", ephemeral: true);
                                                            break;
                                                        }
                                                    case ManagePlayer.DisableResult.Success:
                                                        {
                                                            await message.RespondAsync($"Successfully Enable **{message.Data.CustomId.Split(S_)[4]}**", ephemeral: true);
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            await message.RespondAsync($"something went wrong...", ephemeral: true);
                                                            break;
                                                        }
                                                }

                                                return;
                                            }
                                        #endregion
                                        case "Mute":
                                            #region ( Type: Mute )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Mute))
                                                {
                                                    await message.RespondAsync("You do not have permission to Mute a player!", ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Mute [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput("Reason", "Mute_reason".Replace('_', S_), TextInputStyle.Paragraph, "Spamming")
                                                    .AddTextInput("Duration", "Mute_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15);

                                                await message.RespondWithModalAsync(modal.Build());

                                                return;
                                            }
                                        #endregion
                                        case "UnMute":
                                            #region ( Type: UnMute )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_UnMute))
                                                {
                                                    await message.RespondAsync("You do not have permission to UnMute a player!", ephemeral: true);
                                                    return;
                                                }

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == message.Data.CustomId.Split(S_)[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer != null)
                                                {
                                                    if (ManagePlayer.OnlineUnMute(false, targetplayer, executer.Name))
                                                    {
                                                        await message.RespondAsync($"Successfully Unmute **{targetplayer.Name}**", ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync($"Player **{targetplayer.Name}** isn't muted", ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(message.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await message.RespondAsync($"Account **{message.Data.CustomId.Split(S_)[4]}** does'nt exist!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineUnMute))
                                                    {
                                                        await message.RespondAsync("You do not have permission to Offline UnMute a player!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineBan(account, message.Data.CustomId.Split(S_)[5], executer.Name, DateTime.MaxValue, true, true))
                                                    {
                                                        await message.RespondAsync($"Successfully Unmute **{account.Name}**", ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync($"Player **{account.Name}** isn't muted", ephemeral: true);
                                                    }
                                                }

                                                return;
                                            }
                                            #endregion
                                    }

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                #endregion
                case "Discord":
                    #region ( Type | Discord )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "GiveRole":
                                #region ( Type => GiveRole )
                                {

                                    try
                                    {
                                        ulong roleid = 0;

                                        if (!ulong.TryParse(message.Data.CustomId.Split(S_)[3], out roleid))
                                        {
                                            await message.RespondAsync("Error: Unable to add/remove this role from you!" +
                                                "\n-# Contact any administrator to resolve this issue", ephemeral: true);
                                            return;
                                        }

                                        var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole(roleid);
                                        if (_client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).Roles.Any(r => r == role))
                                        {
                                            await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).RemoveRoleAsync(roleid);

                                            await message.RespondAsync($"{role.Mention} is removed on you!", ephemeral: true);
                                        }
                                        else
                                        {
                                            await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).AddRoleAsync(roleid);

                                            await message.RespondAsync($"{role.Mention} is added on you!", ephemeral: true);
                                        }

                                    }
                                    catch
                                    {
                                        await message.RespondAsync("Error: Unable to add/remove this role from you!" +
                                                "\n-# Contact any administrator to resolve this issue", ephemeral: true);
                                        return;
                                    }

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task ModalHandler(SocketModal modal)
        {
            #region code

            if (modal.Data.CustomId.Split(S_)[0] != "MKLP") return;

            UserAccount executer = GetUserIDAccHasPermission(modal.User.Id, TSStaffPermission);
            if (executer == null)
            {
                await modal.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();

            switch (modal.Data.CustomId.Split(S_)[1])
            {
                case "InGame":
                    #region ( Type | InGame )
                    {

                        switch (modal.Data.CustomId.Split(S_)[2])
                        {
                            case "PlayerAction":
                                #region ( Type => PlayerAction )
                                {

                                    switch (modal.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Ban":
                                            #region ( Type: Ban )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Ban))
                                                {
                                                    await modal.RespondAsync("You do not have permission to Ban a player!", ephemeral: true);
                                                    return;
                                                }

                                                string reason = components
                                                    .First(x => x.CustomId == "Ban_reason".Replace('_', S_)).Value;
                                                string duration = components
                                                    .First(x => x.CustomId == "Ban_duration".Replace('_', S_)).Value;


                                                string BGCategory = "N/A";
                                                if ((bool)MKLP.Config.Main.UsingBanGuardPlugin)
                                                {
                                                    string banguardcat = components
                                                        .First(x => x.CustomId == "Ban_BGCategory".Replace('_', S_)).Value;
                                                    if (banguardcat != "")
                                                    {
                                                        if (banguardcat == "-auto")
                                                        {
                                                            BGCategory = BanGuardAPI.GetCategoryFromReason(reason);
                                                        }
                                                        if (BanGuardAPI.IsCategory(banguardcat))
                                                        {
                                                            BGCategory = banguardcat;
                                                        }
                                                    }
                                                }

                                                DateTime expiration = DateTime.MaxValue;

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == modal.Data.CustomId.Split(S_)[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (TShock.Utils.TryParseTime(duration, out ulong seconds))
                                                {
                                                    expiration = DateTime.UtcNow.AddSeconds(seconds);
                                                }

                                                if (targetplayer != null)
                                                {

                                                    if (ManagePlayer.OnlineBan(false, targetplayer, reason, executer.Name, expiration, true, true, BGCategory))
                                                    {
                                                        await modal.RespondAsync($"Successfully banned **{targetplayer.Name}**" +
                                                            "\n**Reason:** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync($"Player **{targetplayer.Name}** was already banned", ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(modal.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await modal.RespondAsync($"Account **{account.Name}** does'nt exist!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineBan))
                                                    {
                                                        await modal.RespondAsync("You do not have permission to Mute a player!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineBan(account, reason, executer.Name, expiration, true, true, BGCategory))
                                                    {
                                                        await modal.RespondAsync($"Successfully banned **{account.Name}**" +
                                                            "\n**Reason:** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync($"Player **{account.Name}** was already banned", ephemeral: true);
                                                    }
                                                }


                                                return;
                                            }
                                        #endregion
                                        case "Disable":
                                            #region ( Type: Disable )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Disable))
                                                {
                                                    await modal.RespondAsync("You do not have permission to Disable a player!", ephemeral: true);
                                                    return;
                                                }

                                                string reason = components
                                                    .First(x => x.CustomId == "Disable_reason".Replace('_', S_)).Value;

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == modal.Data.CustomId.Split(S_)[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer == null)
                                                {
                                                    await modal.RespondAsync($"Player **{modal.Data.CustomId.Split(S_)[4]}** is offline", ephemeral: true);
                                                    return;
                                                }

                                                if (ManagePlayer.DisablePlayer(targetplayer, reason, executer.Name))
                                                {
                                                    await modal.RespondAsync($"Successfully disabled **{targetplayer.Name}**", ephemeral: true);
                                                }
                                                else
                                                {
                                                    await modal.RespondAsync($"Player **{targetplayer.Name}** was already disabled", ephemeral: true);
                                                }

                                                return;
                                            }
                                        #endregion
                                        case "Mute":
                                            #region ( Type: Mute )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Mute))
                                                {
                                                    await modal.RespondAsync("You do not have permission to Mute a player!", ephemeral: true);
                                                    return;
                                                }
                                                string reason = components
                                                    .First(x => x.CustomId == "Mute_reason".Replace('_', S_)).Value;
                                                string duration = components
                                                    .First(x => x.CustomId == "Mute_duration".Replace('_', S_)).Value;

                                                DateTime expiration = DateTime.MaxValue;

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == modal.Data.CustomId.Split(S_)[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (TShock.Utils.TryParseTime(duration, out ulong seconds))
                                                {
                                                    expiration = DateTime.UtcNow.AddSeconds(seconds);
                                                }

                                                if (targetplayer != null)
                                                {

                                                    if (ManagePlayer.OnlineMute(false, targetplayer, reason, executer.Name, expiration))
                                                    {
                                                        await modal.RespondAsync($"Successfully Mute **{targetplayer.Name}**" +
                                                            "\n**Reason:** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync($"Player **{targetplayer.Name}** was already muted", ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(modal.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await modal.RespondAsync($"Account **{account.Name}** does'nt exist!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineMute))
                                                    {
                                                        await modal.RespondAsync("You do not have permission to Offline Mute a player!", ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineMute(account, reason, executer.Name, expiration))
                                                    {
                                                        await modal.RespondAsync($"Successfully Mute **{account.Name}** -offline" +
                                                            "\n**Reason:** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync($"Player **{account.Name}** was already muted", ephemeral: true);
                                                    }
                                                }


                                                return;
                                            }
                                            #endregion
                                    }

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task SelectMenuHandler(SocketMessageComponent message)
        {
            #region code

            if (message.Data.CustomId.Split(S_)[0] != "MKLP") return;

            UserAccount executer = GetUserIDAccHasPermission(message.User.Id, TSStaffPermission);
            if (executer == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            var Value = string.Join(", ", message.Data.Values);

            switch (message.Data.CustomId.Split(S_)[1])
            {
                case "SendMsg":
                    #region ( Type | SendMessage )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == Value)
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {Value} isn't online", ephemeral: true);
                                        return;
                                    }

                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.PlayerData.maxHealth}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.PlayerData.maxMana}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.ContainsKey(Identifier.Name + targetplayer.Name) || MKLP.DisabledKey.ContainsKey(Identifier.IP + targetplayer.IP) || MKLP.DisabledKey.ContainsKey(Identifier.UUID + targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());


                                                break;
                                            }
                                    }



                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == Value)
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {Value} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split(S_)[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_".Replace('_', S_) + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription + "```")
                                        .WithColor(EmbedColor);

                                    await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            #region code
            string SlashCommandName = "";

            if (MKLP.Config.Discord.SlashCommandName != "")
            {
                SlashCommandName = $"{MKLP.Config.Discord.SlashCommandName}";
                if (command.Data.Name == SlashCommandName)
                {
                    switch (command.Data.Options.First().Name)
                    {
                        case "mklp-help":
                            #region ( Command | help )
                            {
                                string commands = 
                                    $"{CommandIDs_GetMention(SlashCommandName + " moderation")} : shows mklp panel in discord" +
                                    $"\n\n" +
                                    $"{CommandIDs_GetMention(SlashCommandName + " moderation-user")} : Select a Account you want to view" +
                                    $"\n\n" +
                                    $"{CommandIDs_GetMention(SlashCommandName + " ingame-command")} : execute a command in server";

                                var embed = new EmbedBuilder()
                                    .WithTitle("List of Commands")
                                    .WithDescription(commands)
                                    .WithColor(EmbedColor);

                                command.RespondAsync(embed: embed.Build(), ephemeral: true);

                                return;
                            }
                        #endregion
                        case "moderation":
                            #region ( Command | moderation )
                            {
                                try
                                {
                                    UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                                    if (executer == null)
                                    {
                                        await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                                        return;
                                    }

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

                                    if (stringplayers == "") stringplayers = "No Players Online...";

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
                                    if (defeatedbosses == "") defeatedbosses = "No Bosses Defeated...";

                                    string defeatedinvasion = GetListDefeatedInvasion();
                                    if (defeatedinvasion == "") defeatedinvasion = "no Invasions Completed...";

                                    string OngoingActivity = GetListActivities();
                                    if (OngoingActivity == "") OngoingActivity = "Nothing is Happening...";

                                    string reportlist = "";

                                    foreach (MKLP_Report report in MKLP.DBManager.GetReportList(4))
                                    {
                                        reportlist +=
                                            $"**'{report.From}' Report** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                            $"\n> **ID:** {report.ID}" +
                                            $"\n> **Location:** `{report.Location}`" +
                                            $"\n> **Players online during report:** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                            $"\n> " +
                                            $"\n> **target:** {(report.Target == "" ? "none" : report.Target)}" +
                                            $"\n> **Message:** {report.Message}\n\n";
                                    }

                                    var embed = new EmbedBuilder()
                                            .WithTitle("Server Moderation Menu")
                                            .WithDescription("## 📑 Latest Report" +
                                            $"\n{(reportlist == "" ? "no latest reports today..." : reportlist)}")
                                            .WithColor(EmbedColor)
                                            .WithFields(
                                                new EmbedFieldBuilder()
                                                    .WithName($"Online Players [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                                    .WithValue(stringplayers),
                                                new EmbedFieldBuilder()
                                                    .WithName("Bosses")
                                                    .WithValue(defeatedbosses)
                                                    .WithIsInline(true),
                                                new EmbedFieldBuilder()
                                                    .WithName("Invasions Defeated")
                                                    .WithValue(defeatedinvasion)
                                                    .WithIsInline(true),
                                                new EmbedFieldBuilder()
                                                    .WithName("Activities")
                                                    .WithValue(OngoingActivity)
                                            )
                                            .Build();


                                    if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                                    {

                                        var menuBuilder = new SelectMenuBuilder()
                                        .WithPlaceholder("Select a Player")
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
                                            menuBuilder.AddOption(player.Name, player.Account.Name, $"Account: {player.Account.Name}");
                                        }

                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0)
                                            .WithSelectMenu(menuBuilder, row: 1);

                                        await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);

                                    }
                                    else
                                    {
                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0);
                                        await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);
                                    }
                                }
                                catch (Exception e)
                                {
                                    await command.RespondAsync("An error occur executing this command", ephemeral: true);
                                    MKLP_Console.SendLog_Exception(e);
                                }
                                return;
                            }
                        #endregion
                        case "moderation-user":
                            #region ( Command | moderation-user )
                            {
                                try
                                {
                                    UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                                    if (executer == null)
                                    {
                                        await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                                        return;
                                    }

                                    UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(command.Data.Options.First().Options.First().Value.ToString());

                                    if (getuseraccount == null)
                                    {
                                        await command.RespondAsync("Invalid User Account!", ephemeral: true);
                                        return;
                                    }

                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null) continue;

                                        if (player.Account.Name == command.Data.Options.First().Options.First().Value.ToString())
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    var buttons = new ComponentBuilder()
                                        .WithButton("Refresh", "MKLP_EditMsg_PlayerModViewOffline_Main_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Secondary)
                                        .WithButton("Main", "XXX", ButtonStyle.Primary, disabled: true)
                                        .WithButton("Reports from them", "MKLP_EditMsg_PlayerModViewOffline_Report1_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                        .WithButton($"{getuseraccount.Name} Reports", "MKLP_EditMsg_PlayerModViewOffline_Report2_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                        .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2)
                                        .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2);

                                    ulong? getuserid = null;


                                    try
                                    {
                                        getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(getuseraccount.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(getuseraccount.Name);
                                    }
                                    catch { }

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Account [ {getuseraccount.Name} ]")
                                        .WithDescription(
                                            $"**Account ID:** `{getuseraccount.ID}`" +
                                            $"\n**Is in the server?:** `{(targetplayer == null ? "no" : "yes")}`" +
                                            $"\n**Group:** `{getuseraccount.Group}`" +
                                            $"\n**Registered Since:** `{getuseraccount.Registered}` {GetSince(DateTime.Parse(getuseraccount.Registered))}" +
                                            $"\n**Last Accessed:** `{getuseraccount.LastAccessed}` {GetSince(DateTime.Parse(getuseraccount.LastAccessed))}" +
                                            $"{(getuserid == null ? "" : $"\n\n**Discord UserID:** `{(ulong)getuserid}`")}"
                                        ).WithColor(EmbedColor);

                                    await command.RespondAsync(embed: embed.Build(), components: buttons.Build(), ephemeral: true);
                                }
                                catch (Exception e)
                                {
                                    await command.RespondAsync("An error occur executing this command", ephemeral: true);
                                    MKLP_Console.SendLog_Exception(e);
                                }

                                return;
                            }
                        #endregion
                        case "ingame-command":
                            #region ( Command | ingame-command )
                            {
                                UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                                if (executer == null)
                                {
                                    await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                                    return;
                                }

                                if (executer == null)
                                {
                                    await command.RespondAsync("⚠️Warning⚠️ your Account does not Exist!", null, false, true);
                                    return;
                                }

                                var getgroup = TShock.Groups.GetGroupByName(executer.Group);
                                TSRestPlayer player = new TSRestPlayer(executer.Name, getgroup);

                                player.Account = executer;


                                try
                                {
                                    string option1 = command.Data.Options.First().Options.First().Value.ToString();


                                    Commands.HandleCommand(player, option1);

                                    string OutPutResult = "";

                                    foreach (string output in player.GetCommandOutput())
                                    {
                                        OutPutResult += output;
                                    }

                                    if (OutPutResult == "") OutPutResult = "   ";

                                    if (OutPutResult.Length > 4096) OutPutResult = OutPutResult.Substring(0, 4096);

                                    var embed = new EmbedBuilder()
                                        .WithTitle("Command OutPut")
                                        .WithDescription("```\n" + OutPutResult + "\n```")
                                        .WithColor(Discord.Color.Purple)
                                        .Build();

                                    await command.RespondAsync($"## Command executed! `{option1}`", embed: embed, ephemeral: true);



                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    await command.RespondAsync("there was an error trying to execute the command!", ephemeral: true);
                                    return;
                                }
                                return;
                            }
                            #endregion
                    }
                    return;
                }
            }

            switch (command.Data.Name)
            {
                case "mklp-help":
                    #region ( Command | help )
                    {
                        string commands =
                            $"{CommandIDs_GetMention("moderation")} : shows mklp panel in discord" +
                            $"\n\n" +
                            $"{CommandIDs_GetMention("moderation-user")} : Select a Account you want to view" +
                            $"\n\n" +
                            $"{CommandIDs_GetMention("ingame-command")} : execute a command in server";

                        var embed = new EmbedBuilder()
                            .WithTitle("List of Commands")
                            .WithDescription(commands)
                            .WithColor(EmbedColor);

                        command.RespondAsync(embed: embed.Build(), ephemeral: true);

                        return;
                    }
                #endregion
                case "moderation":
                    #region ( Command | moderation )
                    {
                        try
                        {
                            UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                            if (executer == null)
                            {
                                await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                                return;
                            }

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

                            if (stringplayers == "") stringplayers = "No Players Online...";

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
                            if (defeatedbosses == "") defeatedbosses = "No Bosses Defeated...";

                            string defeatedinvasion = GetListDefeatedInvasion();
                            if (defeatedinvasion == "") defeatedinvasion = "no Invasions Completed...";

                            string OngoingActivity = GetListActivities();
                            if (OngoingActivity == "") OngoingActivity = "Nothing is Happening...";

                            string reportlist = "";

                            foreach (MKLP_Report report in MKLP.DBManager.GetReportList(4))
                            {
                                reportlist +=
                                    $"**'{report.From}' Report** {TimestampTag.FormatFromDateTime(report.Since, TimestampTagStyles.Relative)}" +
                                    $"\n> **ID:** {report.ID}" +
                                    $"\n> **Location:** `{report.Location}`" +
                                    $"\n> **Players online during report:** `{report.Players.Replace(S_.ToString(), ", ")}`" +
                                    $"\n> " +
                                    $"\n> **target:** {(report.Target == "" ? "none" : report.Target)}" +
                                    $"\n> **Message:** {report.Message}\n\n";
                            }

                            var embed = new EmbedBuilder()
                                    .WithTitle("Server Moderation Menu")
                                    .WithDescription("## 📑 Latest Report" +
                                    $"\n{(reportlist == "" ? "no latest reports today..." : reportlist)}")
                                    .WithColor(EmbedColor)
                                    .WithFields(
                                        new EmbedFieldBuilder()
                                            .WithName($"Online Players [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                            .WithValue(stringplayers),
                                        new EmbedFieldBuilder()
                                            .WithName("Bosses")
                                            .WithValue(defeatedbosses)
                                            .WithIsInline(true),
                                        new EmbedFieldBuilder()
                                            .WithName("Invasions Defeated")
                                            .WithValue(defeatedinvasion)
                                            .WithIsInline(true),
                                        new EmbedFieldBuilder()
                                            .WithName("Activities")
                                            .WithValue(OngoingActivity)
                                    )
                                    .Build();


                            if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                            {

                                var menuBuilder = new SelectMenuBuilder()
                                .WithPlaceholder("Select a Player")
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
                                    menuBuilder.AddOption(player.Name, player.Account.Name, $"Account: {player.Account.Name}");
                                }

                                var component = new ComponentBuilder()
                                    .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0)
                                    .WithSelectMenu(menuBuilder, row: 1);

                                await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);

                            }
                            else
                            {
                                var component = new ComponentBuilder()
                                    .WithButton("Refresh", "MKLP_EditMsg_ServerModView".Replace('_', S_), ButtonStyle.Secondary, row: 0);
                                await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);
                            }
                        }
                        catch (Exception e)
                        {
                            await command.RespondAsync("An error occur executing this command", ephemeral: true);
                            MKLP_Console.SendLog_Exception(e);
                        }
                        return;
                    }
                #endregion
                case "moderation-user":
                    #region ( Command | moderation-user )
                    {
                        try
                        {
                            UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                            if (executer == null)
                            {
                                await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                                return;
                            }

                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(command.Data.Options.First().Value.ToString());

                            if (getuseraccount == null)
                            {
                                await command.RespondAsync("Invalid User Account!", ephemeral: true);
                                return;
                            }

                            TSPlayer? targetplayer = null;
                            foreach (TSPlayer player in TShock.Players)
                            {
                                if (player == null) continue;

                                if (player.Account.Name == command.Data.Options.First().Value.ToString())
                                {
                                    targetplayer = player;
                                }
                            }

                            var buttons = new ComponentBuilder()
                                .WithButton("Refresh", "MKLP_EditMsg_PlayerModViewOffline_Main_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Secondary)
                                .WithButton("Main", "XXX", ButtonStyle.Primary, disabled: true)
                                .WithButton("Reports from them", "MKLP_EditMsg_PlayerModViewOffline_Report1_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                .WithButton($"{getuseraccount.Name} Reports", "MKLP_EditMsg_PlayerModViewOffline_Report2_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Primary, row: 1)
                                .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2)
                                .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + getuseraccount.Name, ButtonStyle.Danger, row: 2);

                            ulong? getuserid = null;


                            try
                            {
                                getuserid = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? MKLP.LinkAccountManager.GetUserIDByAccountID(getuseraccount.ID) : MKLP.LinkAccountManager.GetUserIDByAccountName(getuseraccount.Name);
                            }
                            catch { }

                            var embed = new EmbedBuilder()
                                .WithTitle($"Account [ {getuseraccount.Name} ]")
                                .WithDescription(
                                    $"**Account ID:** `{getuseraccount.ID}`" +
                                    $"\n**Is in the server?:** `{(targetplayer == null ? "no" : "yes")}`" +
                                    $"\n**Group:** `{getuseraccount.Group}`" +
                                    $"\n**Registered Since:** `{getuseraccount.Registered}` {GetSince(DateTime.Parse(getuseraccount.Registered))}" +
                                    $"\n**Last Accessed:** `{getuseraccount.LastAccessed}` {GetSince(DateTime.Parse(getuseraccount.LastAccessed))}" +
                                    $"{(getuserid == null ? "" : $"\n\n**Discord UserID:** `{(ulong)getuserid}`")}"
                                ).WithColor(EmbedColor);

                            await command.RespondAsync(embed: embed.Build(), components: buttons.Build(), ephemeral: true);
                        }
                        catch (Exception e)
                        {
                            await command.RespondAsync("An error occur executing this command", ephemeral: true);
                            MKLP_Console.SendLog_Exception(e);
                        }

                        return;
                    }
                #endregion
                case "ingame-command":
                    #region ( Command | ingame-command )
                    {
                        UserAccount executer = GetUserIDAccHasPermission(command.User.Id, TSStaffPermission);
                        if (executer == null)
                        {
                            await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                            return;
                        }

                        if (executer == null)
                        {
                            await command.RespondAsync("⚠️Warning⚠️ your Account does not Exist!", null, false, true);
                            return;
                        }

                        var getgroup = TShock.Groups.GetGroupByName(executer.Group);
                        TSRestPlayer player = new TSRestPlayer(executer.Name, getgroup);

                        player.Account = executer;


                        try
                        {
                            string option1 = command.Data.Options.First().Value.ToString();


                            Commands.HandleCommand(player, option1);

                            string OutPutResult = "";

                            foreach (string output in player.GetCommandOutput())
                            {
                                OutPutResult += output;
                            }

                            if (OutPutResult == "") OutPutResult = "   ";

                            if (OutPutResult.Length > 4096) OutPutResult = OutPutResult.Substring(0, 4096);

                            var embed = new EmbedBuilder()
                                .WithTitle("Command OutPut")
                                .WithDescription("```\n" + OutPutResult + "\n```")
                                .WithColor(Discord.Color.Purple)
                                .Build();

                            await command.RespondAsync($"## Command executed! `{option1}`", embed: embed, ephemeral: true);



                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            await command.RespondAsync("there was an error trying to execute the command!", ephemeral: true);
                            return;
                        }
                        return;
                    }
                    #endregion
            }

            async void UnavailableCommand()
            {
                await command.RespondAsync("This Command Is Unavailable!", ephemeral: true);
            }

            #endregion
        }

        private Task MessageRecieved(SocketMessage message)
        {
            #region chat relay
            if (message is IUserMessage userMessage && !userMessage.Author.IsBot)
            {
                string messagecontent = message.Content;
                if (MKLP.Config.Discord.StaffChannel == null) return Task.CompletedTask;
                if (userMessage.Channel.Id == (ulong)MKLP.Config.Discord.StaffChannel)
                {

                    if (messagecontent == "" || messagecontent == null)
                    {
                        return Task.CompletedTask;
                    }

                    //messagecontent = messageparse.ConvertUserIdsToNames(messagecontent, message.MentionedUsers);
                    //messagecontent = messageparse.ShortenEmojisToName(messagecontent);

                    foreach (var user in message.MentionedUsers)
                    {
                        messagecontent = messagecontent.Replace($"<@{user.Id}>", $"[c/{MKLP.Config.Main.StaffChat.StaffChat_HexColor_Discord_Mention_User}:@{user.Username.Replace("[", "").Replace("]", "")}]");
                        messagecontent = messagecontent.Replace($"<@!{user.Id}>", $"[c/{MKLP.Config.Main.StaffChat.StaffChat_HexColor_Discord_Mention_User}:@{user.Username.Replace("[", "").Replace("]", "")}]");
                    }

                    foreach (var roles in message.MentionedRoles)
                    {
                        messagecontent = messagecontent.Replace($"<@&{roles.Id}>", $"[c/{MKLP.Config.Main.StaffChat.StaffChat_HexColor_Discord_Mention_Role}:@" + roles.Name.Replace("[", "").Replace("]", "") + "]");
                    }

                    foreach (var channel in message.MentionedChannels)
                    {
                        messagecontent = messagecontent.Replace($"<@{channel.Id}>", $"[c/{MKLP.Config.Main.StaffChat.StaffChat_HexColor_Discord_Mention_Channel}:#{channel.Name.Replace("[", "").Replace("]", "")}]");
                    }

                    if (message.Attachments.Count > 0) messagecontent += MKLP.Config.Main.StaffChat.StaffChat_Message_Discord_HasAttachment;

                    Config.CONFIG_COLOR_RBG Config_messagecolor = (Config.CONFIG_COLOR_RBG)MKLP.Config.Main.StaffChat.StaffChat_MessageRecieved_InGame_RBG;

                    MKLP.SendStaffMessage(GetMessageDiscordResult(message.Author, MKLP.Config.Main.StaffChat.StaffChat_MessageRecieved_Discord, messagecontent), new(Config_messagecolor.R, Config_messagecolor.G, Config_messagecolor.B));

                    MKLP_Console.SendLog_Message_StaffChat_Discord(message.Author.Username, messagecontent);
                }
            }
            return Task.CompletedTask;
            #region GetMessageDiscordResult
            string GetMessageDiscordResult(Discord.WebSocket.SocketUser discorduser, string Text, string message)
            {
                string Context = Text;

                Context = Context.Replace("%discordname%", discorduser.Username);

                try
                {
                    string getlinkaccountname = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? TShock.UserAccounts.GetUserAccountByID(MKLP.LinkAccountManager.GetAccountIDByUserID(discorduser.Id)).Name : TShock.UserAccounts.GetUserAccountByName(MKLP.LinkAccountManager.GetAccountNameByUserID(discorduser.Id)).Name;

                    Context = Context.Replace("%discordingame%", getlinkaccountname);
                    Context = Context.Replace("%discordoringame%", getlinkaccountname);
                    Context = Context.Replace("%discordacclinkedicon%", MKLP.Config.Main.StaffChat.StaffChat_Message_discordacclinkedicon);

                }
                catch (NullReferenceException)
                {
                    Context = Context.Replace("%discordingame%", "");
                    Context = Context.Replace("%discordoringame%", discorduser.Username);
                    Context = Context.Replace("%discordacclinkedicon%", "");
                }

                Context = Context.Replace("%message%", message);

                return Context;
            }
            #endregion

            #endregion
        }

        #region [ Actions ]
        /*
        public async void KLPBotSendMessage(ulong channel, string message)
        {
            if (channel == 0) return;

            try
            {
                var targetchannel = _client.GetChannel(channel);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }
        */
        public async void KLPBotSendMessageMain(string message)
        {
            if (MKLP.Config.Discord.StaffChannel == null) return;
            if ((ulong)MKLP.Config.Discord.StaffChannel == 0) return;

            var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.StaffChannel);

            await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
            return;
        }

        public async void KLPBotSendMessage_BossEnabled(string bossname)
        {
            if (MKLP.Config.BossManager.Discord_BossEnableChannel == null) return;
            if ((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel == 0) return;

            string message = MKLP.Config.BossManager.Discord_BossEnableMessage;

            message = message.Replace("%bossname%", bossname);

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel);
                try
                {
                    var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole((ulong)MKLP.Config.BossManager.Discord_BossEnableRole);

                    var buttons = new ComponentBuilder()
                        .WithButton("Get Notify", $"MKLP_Discord_GiveRole_{role.Id}".Replace('_', S_), ButtonStyle.Secondary);

                    message = message.Replace("%notification%", role.Mention);
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message, components: buttons.Build());
                }
                catch
                {
                    message = message.Replace("%notification%", "`@notifity`");
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }
        public async void KLPBotSendMessage_BossEnabled(string bossname, string playername)
        {
            if (MKLP.Config.BossManager.Discord_BossEnableChannel == null) return;
            if ((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel == 0) return;

            string message = MKLP.Config.BossManager.Discord_BossEnableCMDMessage;

            message = message.Replace("%bossname%", bossname);
            message = message.Replace("%playername%", playername);

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel);
                try
                {
                    var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole((ulong)MKLP.Config.BossManager.Discord_BossEnableRole);

                    var buttons = new ComponentBuilder()
                        .WithButton(MKLP.GetText("Get Notify"), $"MKLP_Discord_GiveRole_{role.Id}".Replace('_', S_), ButtonStyle.Secondary);

                    message = message.Replace("%notification%", role.Mention);
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message, components: buttons.Build());
                }
                catch
                {
                    message = message.Replace("%notification%", "`@notifity`");
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }
        /*
        public async void KLPBotSendMessage_BossDisable(string bossname)
        {
            if (MKLP.Config.BossManager.Discord_BossEnableChannel == null) return;
            if ((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel == 0) return;

            string message = MKLP.Config.BossManager.Discord_BossDisableMessage;

            message = message.Replace("%bossname%", bossname);

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel);
                try
                {
                    var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole((ulong)MKLP.Config.BossManager.Discord_BossEnableRole);

                    var buttons = new ComponentBuilder()
                        .WithButton("Get Notify", $"MKLP_Discord_GiveRole_{role.Id}".Replace('_', S_), ButtonStyle.Secondary);

                    message = message.Replace("%notification%", role.Mention);
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message, components: buttons.Build());
                } catch
                {
                    message = message.Replace("%notification%", "`@notifity`");
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }
        public async void KLPBotSendMessage_BossDisable(string bossname, string playername)
        {
            if (MKLP.Config.BossManager.Discord_BossEnableChannel == null) return;
            if ((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel == 0) return;

            string message = MKLP.Config.BossManager.Discord_BossEnableCMDMessage;

            message = message.Replace("%bossname%", bossname);
            message = message.Replace("%playername%", playername);

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.BossManager.Discord_BossEnableChannel);
                try
                {
                    var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole((ulong)MKLP.Config.BossManager.Discord_BossEnableRole);

                    var buttons = new ComponentBuilder()
                        .WithButton("Get Notify", $"MKLP_Discord_GiveRole_{role.Id}".Replace('_', S_), ButtonStyle.Secondary);

                    message = message.Replace("%notification%", role.Mention);
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message, components: buttons.Build());
                } catch
                {
                    message = message.Replace("%notification%", "`@notifity`");
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(message);
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }
        */

        string TitleLog = "⚙️ **[ MKLP ] :** ";

        public async void KLPBotSendMessageLog(ulong channel, string message)
        {
            if (channel == 0) return;

            try
            {
                var targetchannel = _client.GetChannel(channel);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message);
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessageMainLog(string message)
        {
            if (MKLP.Config.Discord.MainChannelLog == null) return;
            if ((ulong)MKLP.Config.Discord.MainChannelLog == 0) return;

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.MainChannelLog);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message);
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessage_Disabled(string message, string playername = "none", string reason = "No Reason Provided")
        {
            try
            {
                if (MKLP.Config.Discord.MainChannelLog == null) return;
                if ((ulong)MKLP.Config.Discord.MainChannelLog == 0) return;

                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.MainChannelLog);

                var buttons = new ComponentBuilder()
                    .WithButton("Dismiss", "MKLP_DismissMsg_Disabled".Replace('_', S_), ButtonStyle.Secondary)
                    .WithButton("Check Player", "MKLP_SendMsg_PlayerModView_Main_".Replace('_', S_) + playername, emote: new Emoji("\U0001F4B3"))
                    .WithButton("Quick Ban [ permanent ]", $"MKLP_InGame_PlayerAction_QBan_".Replace('_', S_) + playername + S_ + reason, ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1)
                    .WithButton("Enable", "MKLP_InGame_PlayerAction_Undisable_".Replace('_', S_) + playername, ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message, components: buttons.Build());
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessage_Warning(string message, string playername = "none", string reason = "No Reason Provided")
        {
            if (MKLP.Config.Discord.MainChannelLog == null) return;
            if ((ulong)MKLP.Config.Discord.MainChannelLog == 0) return;

            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.MainChannelLog);

                var buttons = new ComponentBuilder()
                    .WithButton(MKLP.GetText("Dismiss"), "MKLP_DismissMsg_Warning".Replace('_', S_), ButtonStyle.Secondary)
                    .WithButton(MKLP.GetText("Check Player"), "MKLP_SendMsg_PlayerModView_Main_".Replace('_', S_) + playername, emote: new Emoji("\U0001F4B3"))
                    .WithButton(MKLP.GetText("Quick Ban [ permanent ]"), $"MKLP_InGame_PlayerAction_QBan_".Replace('_', S_) + playername + S_ + reason, ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + "**Warning!** " + message, components: buttons.Build());
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }


        #region [ Report ]

        public async void KLPBotSendMessage_Report_Main(int ID, string type, string reporter, string message, DateTime Since, string location, string playerlist)
        {
            if (MKLP.Config.Discord.ReportChannel == null) return;
            if ((ulong)MKLP.Config.Discord.ReportChannel == 0) return;
            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.ReportChannel);

                var buttons = new ComponentBuilder()
                    .WithButton(MKLP.GetText("Dismiss [ Report ]"), "MKLP_DismissMsg_Report2_".Replace('_', S_) + ID, ButtonStyle.Secondary);

                if (ID == -1) buttons = new ComponentBuilder();

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + $"New {type}report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                    (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                    $"\n> **Location:** `{location}`" +
                    $"\n> **Players Online:** `{playerlist}`" +
                    $"\n\n> **Message:** `{message}`",
                    components: buttons.Build());
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessage_Report_Player(int ID, string reporter, string target, string message, DateTime Since, string location, string playerlist)
        {
            if (MKLP.Config.Discord.ReportChannel == null) return;
            if ((ulong)MKLP.Config.Discord.ReportChannel == 0) return;
            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.MainChannelLog);

                if (target != DiscordKLP.S_ + "none" + DiscordKLP.S_)
                {
                    var buttons = new ComponentBuilder()
                    .WithButton(MKLP.GetText("Dismiss [ Report ]"), "MKLP_DismissMsg_Report1_".Replace('_', S_) + ID, ButtonStyle.Secondary)
                    .WithButton(MKLP.GetText("Check Player"), "MKLP_SendMsg_PlayerModView_Main_".Replace('_', S_) + target, emote: new Emoji("\U0001F4B3"))
                    .WithButton(MKLP.GetText("Ban"), $"MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + target, ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1);

                    if (ID == -1) buttons = new ComponentBuilder()
                            .WithButton("Check Player", "MKLP_SendMsg_PlayerModView_Main_".Replace('_', S_) + target, emote: new Emoji("\U0001F4B3"))
                            .WithButton("Ban", $"MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + target, ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1);

                    playerlist = playerlist.Replace($"{DiscordKLP.S_}", ", ");
                    playerlist = playerlist.TrimEnd(',');

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + $"New 👤Player report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n" +
                        $"\n> **Target:** `{target}`" +
                        $"\n> **Message:** `{message}`",
                        components: buttons.Build());
                }
                else
                {
                    var buttons = new ComponentBuilder()
                        .WithButton("Dismiss [ Report ]", "MKLP_DismissMsg_Report2_".Replace('_', S_) + ID, ButtonStyle.Secondary);

                    if (ID == -1) buttons = new ComponentBuilder();

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + $"New 👤Player report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n\n> **Message:** `{message}`",
                        components: buttons.Build());
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessage_Report_Staff(int ID, string reporter, string target, string message, DateTime Since, string location, string playerlist)
        {
            if (MKLP.Config.Discord.StaffReportChannel == null) return;
            if ((ulong)MKLP.Config.Discord.StaffReportChannel == 0) return;
            try
            {
                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.StaffReportChannel);

                var buttons = new ComponentBuilder()
                    .WithButton("Dismiss [ Report ]", "MKLP_DismissMsg_Report2_".Replace('_', S_) + ID, ButtonStyle.Secondary);

                if (ID == -1) buttons = new ComponentBuilder();

                if (target != DiscordKLP.S_ + "none" + DiscordKLP.S_)
                {

                    playerlist = playerlist.Replace($"{DiscordKLP.S_}", ", ");
                    playerlist = playerlist.TrimEnd(',');

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + $"New 👮Staff report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n" +
                        $"\n> **Target:** `{target}`" +
                        $"\n> **Message:** `{message}`",
                        components: buttons.Build());

                    if ((bool)MKLP.Config.Discord.Discord_Send_DM_OnStaffReport)
                    {
                        await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).Owner.SendMessageAsync(TitleLog + $"New 👮Staff report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n" +
                        $"\n> **Target:** `{target}`" +
                        $"\n> **Message:** `{message}`");
                    }
                }
                else
                {
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + $"New 👮Staff report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n\n> **Message:** `{message}`",
                        components: buttons.Build());

                    if ((bool)MKLP.Config.Discord.Discord_Send_DM_OnStaffReport)
                    {
                        await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).Owner.SendMessageAsync(TitleLog + $"New 👮Staff report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? "\n> **__[Temporary!]__**" : $"\n> **ID:** `{ID}`") +
                        $"\n> **Location:** `{location}`" +
                        $"\n> **Players Online:** `{playerlist}`" +
                        $"\n\n> **Message:** `{message}`");
                    }
                }
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }



        #endregion

        #endregion


        #endregion

        public bool AccountHasPermission(UserAccount Account, string Permission)
        {
            var getgroup = TShock.Groups.GetGroupByName(Account.Group);

            if (getgroup == null)
            {
                return false;
            }

            return getgroup.HasPermission(Permission);
        }

        UserAccount GetUserIDAccHasPermission(ulong UserID, string Permission)
        {
            try
            {
                UserAccount executer = (bool)MKLP.Config.DataBaseDLink.Target_UserAccount_ID ? TShock.UserAccounts.GetUserAccountByID(MKLP.LinkAccountManager.GetAccountIDByUserID(UserID)) : TShock.UserAccounts.GetUserAccountByName(MKLP.LinkAccountManager.GetAccountNameByUserID(UserID));

                if (executer == null)
                {
                    return CheckDiscordServer();
                }

                var getgroup = TShock.Groups.GetGroupByName(executer.Group);

                if (getgroup == null)
                {
                    return CheckDiscordServer();
                }

                if (!getgroup.HasPermission(Permission))
                {
                    return CheckDiscordServer();
                }

                return executer;
            }
            catch
            {
                return CheckDiscordServer();
            }



            UserAccount? CheckDiscordServer()
            {
                if ((ulong)MKLP.Config.Discord.MainGuildID == null)
                {
                    return null;
                }
                if ((ulong)MKLP.Config.Discord.MainGuildID == 0)
                {
                    return null;
                }
                if (_client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).OwnerId == UserID)
                {
                    return TSPlayer.Server.Account;
                }
                if ((bool)MKLP.Config.Discord.AllowUser_UseIngame_ModPermission &&
                    (
                    _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(UserID).GuildPermissions.Administrator ||
                    _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(UserID).GuildPermissions.BanMembers ||
                    _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(UserID).GuildPermissions.ManageGuild
                    )
                    )
                {
                    return TSPlayer.Server.Account;
                }
                return null;
            }
        }

        public SocketUser? GetUser(ulong UserID)
        {
            try
            {
                return _client.GetUser(UserID);
            }
            catch
            {
                return null;
            }
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
    }

}
