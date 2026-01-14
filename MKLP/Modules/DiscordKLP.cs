
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

        public static readonly string TSStaffPermission = MKLP.Config.Permissions.Staff;

        public static readonly Discord.Color EmbedColor = Discord.Color.DarkBlue;

        public static readonly char S_ = MKLP.Config.Main.Seperator;

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
                await message.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
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
                                        .WithButton(MKLP.GetText("Dismiss"), "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton(MKLP.GetText("Check Player"), "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton(MKLP.GetText("Quick Ban [ permanent ]"), "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true)
                                        .WithButton(MKLP.GetText("Enable"), "X3", ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1, disabled: true);
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
                                        .WithButton(MKLP.GetText("Dismiss"), "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton(MKLP.GetText("Check Player"), "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton(MKLP.GetText("Quick Ban [ permanent ]"), "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true);
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
                                        .WithButton(MKLP.GetText("Dismiss"), "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton(MKLP.GetText("Check Player"), "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton(MKLP.GetText("Quick Ban [ permanent ]"), "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true);

                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });

                                    await message.RespondAsync(MKLP.GetText("Report Ticket no. {0} Dismissed", message.Data.CustomId.Split(S_)[3]), ephemeral: true);
                                    return;
                                }
                            #endregion
                            case "Report2":
                                #region ( Type: Report2 message )
                                {

                                    MKLP.DBManager.DeleteReport(int.Parse(message.Data.CustomId.Split(S_)[3]));

                                    var buttons = new ComponentBuilder()
                                        .WithButton(MKLP.GetText("Dismiss"), "XXX", ButtonStyle.Secondary, disabled: true);

                                    await message.Message.ModifyAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });

                                    await message.RespondAsync(MKLP.GetText("Report Ticket no. {0} Dismissed", message.Data.CustomId.Split(S_)[3]), ephemeral: true);
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
                                    if (DiscordKLP_Func.PlayerModView(
                                        message.Data.CustomId.Split(S_)[3],
                                        message.Data.CustomId.Split(S_)[4],
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    if (DiscordKLP_Func.ViewPlayerInventory(
                                        message.Data.CustomId.Split(S_)[3],
                                        message.Data.CustomId.Split(S_)[4],
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
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
                                    if (DiscordKLP_Func.PlayerModView(
                                        message.Data.CustomId.Split(S_)[3],
                                        message.Data.CustomId.Split(S_)[4],
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = components;
                                        });
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }

                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    if (DiscordKLP_Func.ViewPlayerInventory(
                                        message.Data.CustomId.Split(S_)[3],
                                        message.Data.CustomId.Split(S_)[4],
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = components;
                                        });
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                    return;
                                }
                            #endregion
                            case "ServerModView":
                                #region ( Type: ServerModView )
                                {
                                    if (DiscordKLP_Func.ServerModView(
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = components;
                                        });
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                    return;
                                }
                            #endregion
                            case "AccSearch":
                                #region ( Type: AccountSearch )
                                {
                                    string search = message.Data.CustomId.Split(S_)[3];
                                    if (!int.TryParse(message.Data.CustomId.Split(S_)[4], out int page))
                                    {
                                        await message.RespondAsync(MKLP.GetText("Invalid Page..."), ephemeral: true);
                                        return;
                                    }
                                    if (DiscordKLP_Func.AccountSearch(
                                        page,
                                        search,
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = components;
                                        });
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                    return;
                                }
                            #endregion
                        }

                        return;
                    }
                #endregion
                case "SendModal":
                    #region ( Type | SendModal )
                    {

                        switch (message.Data.CustomId.Split(S_)[2])
                        {
                            case "AccSearch":
                                #region ( Type: AccountSearch )
                                {
                                    var modal = new ModalBuilder()
                                        .WithTitle(MKLP.GetText("Account Searching"))
                                        .WithCustomId("MKLP_SendMsg_AccSearch".Replace('_', S_))
                                        .AddTextInput(MKLP.GetText("Account Name"), "Search".Replace('_', S_), TextInputStyle.Short, MKLP.GetText("Search"));

                                    await message.RespondWithModalAsync(modal.Build());
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
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to Ban a player!"), ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"{MKLP.GetText("Banning")} [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput(MKLP.GetText("Reason"), "Ban_reason".Replace('_', S_), TextInputStyle.Paragraph, MKLP.GetText("Cheating"))
                                                    .AddTextInput(MKLP.GetText("Duration"), "Ban_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15);

                                                if ((bool)MKLP.Config.Main.UsingBanGuardPlugin)
                                                {
                                                    modal.AddTextInput(MKLP.GetText("BanGuardType"), "Ban_BGCategory".Replace('_', S_), TextInputStyle.Short, MKLP.GetText("Type -auto if you want to automatic categories ( leave it blank if you don't want to use BanGuard )"), maxLength: 30, required: false);
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
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to Ban a player!"), ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"{MKLP.GetText("Banning")} [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Ban_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput(MKLP.GetText("Reason"), "Ban_reason".Replace('_', S_), TextInputStyle.Paragraph, MKLP.GetText("Cheating"), value: message.Data.CustomId.Split(S_)[5])
                                                    .AddTextInput(MKLP.GetText("Duration"), "Ban_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15, value: "Permanent");

                                                if ((bool)MKLP.Config.Main.UsingBanGuardPlugin)
                                                {
                                                    modal.AddTextInput(MKLP.GetText("BanGuardType"), "Ban_BGCategory".Replace('_', S_), TextInputStyle.Short, MKLP.GetText("Type -auto if you want to automatic categories ( leave it blank if you don't want to use BanGuard )"), maxLength: 30, required: false);
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
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to Disable a player!"), ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"{MKLP.GetText("Disable")} [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Disable_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput(MKLP.GetText("Reason"), "Disable_reason".Replace('_', S_), TextInputStyle.Paragraph, MKLP.GetText("Cheating"));

                                                await message.RespondWithModalAsync(modal.Build());
                                                return;
                                            }
                                        #endregion
                                        case "Undisable":
                                            #region ( Type: Undisable )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Disable))
                                                {
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to Enable a player!"), ephemeral: true);
                                                    return;
                                                }
                                                string dummy1;
                                                IEnumerable<string> dummy2;

                                                switch (ManagePlayer.UnDisablePlayer(message.Data.CustomId.Split(S_)[4], AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineEnable), true, out dummy1, out dummy2, executer.Name))
                                                {
                                                    case ManagePlayer.DisableResult.AlreadyEnabled:
                                                        {
                                                            await message.RespondAsync(MKLP.GetText("Player **{0}** isn't disabled", message.Data.CustomId.Split(S_)[4]), ephemeral: true);
                                                            break;
                                                        }
                                                    case ManagePlayer.DisableResult.SuccessOffline:
                                                        {
                                                            await message.RespondAsync(MKLP.GetText("(Offline) Successfully Enable **{0}**", message.Data.CustomId.Split(S_)[4]), ephemeral: true);
                                                            break;
                                                        }
                                                    case ManagePlayer.DisableResult.Success:
                                                        {
                                                            await message.RespondAsync(MKLP.GetText("Successfully Enable **{0}**", message.Data.CustomId.Split(S_)[4]), ephemeral: true);
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            await message.RespondAsync(MKLP.GetText("something went wrong..."), ephemeral: true);
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
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to Mute a player!"), ephemeral: true);
                                                    return;
                                                }

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"{MKLP.GetText("Mute")} [ {message.Data.CustomId.Split(S_)[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Mute_".Replace('_', S_) + message.Data.CustomId.Split(S_)[4])
                                                    .AddTextInput(MKLP.GetText("Reason"), "Mute_reason".Replace('_', S_), TextInputStyle.Paragraph, MKLP.GetText("Spamming"))
                                                    .AddTextInput(MKLP.GetText("Duration"), "Mute_duration".Replace('_', S_), TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15);

                                                await message.RespondWithModalAsync(modal.Build());

                                                return;
                                            }
                                        #endregion
                                        case "UnMute":
                                            #region ( Type: UnMute )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_UnMute))
                                                {
                                                    await message.RespondAsync(MKLP.GetText("You do not have permission to UnMute a player!"), ephemeral: true);
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
                                                        await message.RespondAsync(MKLP.GetText("Successfully Unmute **{0}**", targetplayer.Name), ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync(MKLP.GetText("Player **{0}** isn't muted", targetplayer.Name), ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(message.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await message.RespondAsync(MKLP.GetText("Account **{0}** does'nt exist!", message.Data.CustomId.Split(S_)[4]), ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineUnMute))
                                                    {
                                                        await message.RespondAsync(MKLP.GetText("You do not have permission to Offline Unmute a player!"), ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineBan(account, message.Data.CustomId.Split(S_)[5], executer.Name, DateTime.MaxValue, true, true))
                                                    {
                                                        await message.RespondAsync(MKLP.GetText("Successfully Unmute **{0}**", account.Name), ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await message.RespondAsync(MKLP.GetText("Player **{0}** isn't muted", account.Name), ephemeral: true);
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
                                            await message.RespondAsync(MKLP.GetText("Error: Unable to add/remove this role from you!") +
                                                $"\n-# {MKLP.GetText("Contact any administrator to resolve this issue")}", ephemeral: true);
                                            return;
                                        }

                                        var role = _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetRole(roleid);
                                        if (_client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).Roles.Any(r => r == role))
                                        {
                                            await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).RemoveRoleAsync(roleid);

                                            await message.RespondAsync(MKLP.GetText("{0} is removed on you!", role.Mention), ephemeral: true);
                                        }
                                        else
                                        {
                                            await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).GetUser(message.User.Id).AddRoleAsync(roleid);

                                            await message.RespondAsync(MKLP.GetText("{0} is added on you!", role.Mention), ephemeral: true);
                                        }

                                    }
                                    catch
                                    {
                                        await message.RespondAsync(MKLP.GetText("Error: Unable to add/remove this role from you!") +
                                                $"\n-# {MKLP.GetText("Contact any administrator to resolve this issue")}", ephemeral: true);
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
                case "SendMsg":
                    #region ( Type | SendMessage )
                    {

                        switch (modal.Data.CustomId.Split(S_)[2])
                        {
                            case "AccSearch":
                                #region ( Type => AccountSearch )
                                {

                                    string search = components
                                        .First(x => x.CustomId == "Search".Replace('_', S_)).Value;
                                    if (DiscordKLP_Func.AccountSearch(
                                        1,
                                        search,
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent gcomponents
                                        ))
                                    {
                                        await modal.RespondAsync(embed: embed, ephemeral: true, components: gcomponents);
                                    }
                                    else
                                    {
                                        await modal.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
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
                                                    await modal.RespondAsync(MKLP.GetText("You do not have permission to Ban a player!"), ephemeral: true);
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
                                                        await modal.RespondAsync(MKLP.GetText("Successfully banned **{0}**", targetplayer.Name) +
                                                            $"\n**{MKLP.GetText("Reason:")}** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Player **{0}** was already banned", targetplayer.Name), ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(modal.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Account **{0}** does'nt exist!", account.Name), ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineBan))
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("You do not have permission to Mute a player!"), ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineBan(account, reason, executer.Name, expiration, true, true, BGCategory))
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Successfully banned **{0}**", account.Name) +
                                                            $"\n**{MKLP.GetText("Reason:")}** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Player **{0}** was already banned", account.Name), ephemeral: true);
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
                                                    await modal.RespondAsync(MKLP.GetText("You do not have permission to Disable a player!"), ephemeral: true);
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
                                                    await modal.RespondAsync(MKLP.GetText("Player **{0}** is offline", modal.Data.CustomId.Split(S_)[4]), ephemeral: true);
                                                    return;
                                                }

                                                if (ManagePlayer.DisablePlayer(targetplayer, reason, executer.Name))
                                                {
                                                    await modal.RespondAsync(MKLP.GetText("Successfully disabled **{0}**", targetplayer.Name), ephemeral: true);
                                                }
                                                else
                                                {
                                                    await modal.RespondAsync(MKLP.GetText("Player **{0}** was already disabled", targetplayer.Name), ephemeral: true);
                                                }

                                                return;
                                            }
                                        #endregion
                                        case "Mute":
                                            #region ( Type: Mute )
                                            {
                                                if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_Mute))
                                                {
                                                    await modal.RespondAsync(MKLP.GetText("You do not have permission to Mute a player!"), ephemeral: true);
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
                                                        await modal.RespondAsync(MKLP.GetText("Successfully Mute **{0}**", targetplayer.Name) +
                                                            $"\n**{MKLP.GetText("Reason:")}** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Player **{0}** was already muted", targetplayer.Name), ephemeral: true);
                                                    }

                                                }
                                                else
                                                {
                                                    UserAccount account = TShock.UserAccounts.GetUserAccountByName(modal.Data.CustomId.Split(S_)[4]);

                                                    if (account == null)
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Account **{0}** does'nt exist!", account.Name), ephemeral: true);
                                                        return;
                                                    }

                                                    if (!AccountHasPermission(executer, MKLP.Config.Permissions.CMD_OfflineMute))
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("You do not have permission to Offline Mute a player!"), ephemeral: true);
                                                        return;
                                                    }

                                                    if (ManagePlayer.OfflineMute(account, reason, executer.Name, expiration))
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Successfully Mute **{0}** -offline", account.Name) +
                                                            "\n**Reason:** " + reason, ephemeral: true);
                                                    }
                                                    else
                                                    {
                                                        await modal.RespondAsync(MKLP.GetText("Player **{0}** was already muted", account.Name), ephemeral: true);
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
                await message.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
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
                                    if (DiscordKLP_Func.PlayerModView(
                                        "Main",
                                        Value,
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    if (DiscordKLP_Func.ViewPlayerInventory(
                                        "Inventory",
                                        Value,
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await message.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await message.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
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
                                    $"{CommandIDs_GetMention(SlashCommandName + "moderation")} : {MKLP.GetText("shows {0} panel in discord", "MKLP")}" +
                                    $"\n\n" +
                                    $"{CommandIDs_GetMention(SlashCommandName + "moderation-user")} : {MKLP.GetText("Select a account you want to view")}" +
                                    $"\n\n" +
                                    $"{CommandIDs_GetMention(SlashCommandName + "ingame-command")} : {MKLP.GetText("execute a command in server")}";

                                var embed = new EmbedBuilder()
                                    .WithTitle(MKLP.GetText("List of Commands"))
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
                                        await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                                        return;
                                    }


                                    if (DiscordKLP_Func.ServerModView(
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await command.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await command.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                }
                                catch (Exception e)
                                {
                                    await command.RespondAsync(MKLP.GetText("An error occur executing this command"), ephemeral: true);
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
                                        await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                                        return;
                                    }

                                    UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(command.Data.Options.First().Options.First().Value.ToString());

                                    if (getuseraccount == null)
                                    {
                                        await command.RespondAsync(MKLP.GetText("Invalid User Account!"), ephemeral: true);
                                        return;
                                    }

                                    if (DiscordKLP_Func.PlayerModView(
                                        "Main",
                                        getuseraccount.Name,
                                        out string txt,
                                        out Embed embed,
                                        out MessageComponent components
                                        ))
                                    {
                                        await command.RespondAsync(embed: embed, ephemeral: true, components: components);
                                    }
                                    else
                                    {
                                        await command.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                                    }
                                }
                                catch (Exception e)
                                {
                                    await command.RespondAsync(MKLP.GetText("An error occur executing this command"), ephemeral: true);
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
                                    await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                                    return;
                                }

                                if (executer == null)
                                {
                                    await command.RespondAsync(MKLP.GetText("⚠️Warning⚠️ your Account does not Exist!"), null, false, true);
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
                                        .WithTitle(MKLP.GetText("Command OutPut"))
                                        .WithDescription("```\n" + OutPutResult + "\n```")
                                        .WithColor(Discord.Color.Purple)
                                        .Build();

                                    await command.RespondAsync($"## {MKLP.GetText("Command executed!")} `{option1}`", embed: embed, ephemeral: true);



                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    await command.RespondAsync(MKLP.GetText("there was an error trying to execute the command!"), ephemeral: true);
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
                            $"{CommandIDs_GetMention("moderation")} : {MKLP.GetText("shows {0} panel in discord", "MKLP")}" +
                            $"\n\n" +
                            $"{CommandIDs_GetMention("moderation-user")} : {MKLP.GetText("Select a account you want to view")}" +
                            $"\n\n" +
                            $"{CommandIDs_GetMention("ingame-command")} : {MKLP.GetText("execute a command in server")}";

                        var embed = new EmbedBuilder()
                            .WithTitle(MKLP.GetText("List of Commands"))
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
                                await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                                return;
                            }

                            if (DiscordKLP_Func.ServerModView(
                                out string txt,
                                out Embed embed,
                                out MessageComponent components
                                ))
                            {
                                await command.RespondAsync(embed: embed, ephemeral: true, components: components);
                            }
                            else
                            {
                                await command.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                            }
                        }
                        catch (Exception e)
                        {
                            await command.RespondAsync(MKLP.GetText("An error occur executing this command"), ephemeral: true);
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
                                await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                                return;
                            }

                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(command.Data.Options.First().Value.ToString());

                            if (getuseraccount == null)
                            {
                                await command.RespondAsync(MKLP.GetText("Invalid User Account!"), ephemeral: true);
                                return;
                            }


                            if (DiscordKLP_Func.PlayerModView(
                                "Main",
                                getuseraccount.Name,
                                out string txt,
                                out Embed embed,
                                out MessageComponent components
                                ))
                            {
                                await command.RespondAsync(embed: embed, ephemeral: true, components: components);
                            }
                            else
                            {
                                await command.RespondAsync(MKLP.GetText("Something went wrong!"), ephemeral: true);
                            }
                        }
                        catch (Exception e)
                        {
                            await command.RespondAsync(MKLP.GetText("An error occur executing this command"), ephemeral: true);
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
                            await command.RespondAsync(MKLP.GetText("You do not have permission to proceed this interaction!"), ephemeral: true);
                            return;
                        }

                        if (executer == null)
                        {
                            await command.RespondAsync(MKLP.GetText("⚠️Warning⚠️ your Account does not Exist!"), null, false, true);
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
                                .WithTitle(MKLP.GetText("Command OutPut"))
                                .WithDescription("```\n" + OutPutResult + "\n```")
                                .WithColor(Discord.Color.Purple)
                                .Build();

                            await command.RespondAsync($"## {MKLP.GetText("Command executed!")} `{option1}`", embed: embed, ephemeral: true);



                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            await command.RespondAsync(MKLP.GetText("there was an error trying to execute the command!"), ephemeral: true);
                            return;
                        }
                        return;
                    }
                    #endregion
            }

            async void UnavailableCommand()
            {
                await command.RespondAsync(MKLP.GetText("This Command Is Unavailable!"), ephemeral: true);
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

        public async void KLPBotSendMessage_Disabled(string message, string playername = "none", string reason = "No Reason Provided", string log = "")
        {
            try
            {
                if (MKLP.Config.Discord.MainChannelLog == null) return;
                if ((ulong)MKLP.Config.Discord.MainChannelLog == 0) return;

                var targetchannel = _client.GetChannel((ulong)MKLP.Config.Discord.MainChannelLog);

                var buttons = new ComponentBuilder()
                    .WithButton(MKLP.GetText("Dismiss"), "MKLP_DismissMsg_Disabled".Replace('_', S_), ButtonStyle.Secondary)
                    .WithButton(MKLP.GetText("Check Player"), "MKLP_SendMsg_PlayerModView_Main_".Replace('_', S_) + playername, emote: new Emoji("\U0001F4B3"))
                    .WithButton(MKLP.GetText("Quick Ban [ permanent ]"), $"MKLP_InGame_PlayerAction_QBan_".Replace('_', S_) + playername + S_ + reason, ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1)
                    .WithButton(MKLP.GetText("Enable"), "MKLP_InGame_PlayerAction_Undisable_".Replace('_', S_) + playername, ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1);

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message + log, components: buttons.Build());
                return;
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Message_DiscordBot(e, "=[Log Exception]=", ConsoleColor.Red, ConsoleColor.DarkRed);
            }
        }

        public async void KLPBotSendMessage_Warning(string message, string playername = "none", string reason = "No Reason Provided", string log = "")
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

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + "**Warning!** " + message + log, components: buttons.Build());
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

                await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + MKLP.GetText("New {0}report from **{1}** {2}", type, reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                    (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                    $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                    $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                    $"\n\n> **{MKLP.GetText("Message:")}** `{message}`",
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

                if (target != DiscordKLP.S_ + MKLP.GetText("none") + DiscordKLP.S_)
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

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + MKLP.GetText("New 👤Player report from **{0}**, {1}", reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                        $"\n" +
                        $"\n> **{MKLP.GetText("Target:")}** `{target}`" +
                        $"\n> **{MKLP.GetText("Message:")}** `{message}`",
                        components: buttons.Build());
                }
                else
                {
                    var buttons = new ComponentBuilder()
                        .WithButton(MKLP.GetText("Dismiss [ Report ]"), "MKLP_DismissMsg_Report2_".Replace('_', S_) + ID, ButtonStyle.Secondary);

                    if (ID == -1) buttons = new ComponentBuilder();

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + MKLP.GetText("New 👤Player report from **{0}** {1}", reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                        $"\n\n> **{MKLP.GetText("Message:")}** `{message}`",
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
                    .WithButton(MKLP.GetText("Dismiss [ Report ]"), "MKLP_DismissMsg_Report2_".Replace('_', S_) + ID, ButtonStyle.Secondary);

                if (ID == -1) buttons = new ComponentBuilder();

                if (target != DiscordKLP.S_ + MKLP.GetText("none") + DiscordKLP.S_)
                {

                    playerlist = playerlist.Replace($"{DiscordKLP.S_}", ", ");
                    playerlist = playerlist.TrimEnd(',');

                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + MKLP.GetText("New 👮Staff report from **{0}** {1}", reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> ** {MKLP.GetText("Players Online:")} ** `{playerlist}`" +
                        $"\n" +
                        $"\n> **{MKLP.GetText("Target:")}** `{target}`" +
                        $"\n> **{MKLP.GetText("Message:")}** `{message}`",
                        components: buttons.Build());

                    if ((bool)MKLP.Config.Discord.Discord_Send_DM_OnStaffReport)
                    {
                        await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).Owner.SendMessageAsync(TitleLog + MKLP.GetText("New 👮Staff report from **{0}** {1}", reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                        $"\n" +
                        $"\n> **{MKLP.GetText("Target:")}** `{target}`" +
                        $"\n> **{MKLP.GetText("Message:")}** `{message}`");
                    }
                }
                else
                {
                    await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + MKLP.GetText("New 👮Staff report from **{0}** {1}", reporter, TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)) +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                        $"\n\n> **{MKLP.GetText("Message:")}** `{message}`",
                        components: buttons.Build());

                    if ((bool)MKLP.Config.Discord.Discord_Send_DM_OnStaffReport)
                    {
                        await _client.GetGuild((ulong)MKLP.Config.Discord.MainGuildID).Owner.SendMessageAsync(TitleLog + $"New 👮Staff report from **{reporter}** {TimestampTag.FormatFromDateTime(Since, TimestampTagStyles.Relative)}" +
                        (ID == -1 ? $"\n> **__[{MKLP.GetText("Temporary!")}]__**" : $"\n> **{MKLP.GetText("ID:")}** `{ID}`") +
                        $"\n> **{MKLP.GetText("Location:")}** `{location}`" +
                        $"\n> **{MKLP.GetText("Players Online:")}** `{playerlist}`" +
                        $"\n\n> **{MKLP.GetText("Message:")}** `{message}`");
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
    }

}
