using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TShockAPI;
using BanGuard;
using BanGuard.Models;

namespace MKLP.Functions
{
    public class BanGuardAPI
    {
        public static async Task<PlayerBan?> CheckPlayerBan(string uuid, string playerName, string playerIP)
        {
            if (MKLP.HasBanGuardPlugin && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                return await Plugin_CheckPlayerBan(uuid, playerName, playerIP);
            }
            return null;
        }

        public static async Task<APIResponse<ConnectionCode>> GenerateNewConnection(string uuid)
        {
            if (MKLP.HasBanGuardPlugin && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                return await Plugin_GenerateNewConnection(uuid);
            }
            return null;
        }

        public static async Task<APIResponse<bool>> BanPlayer(string uuid, string category, string ip)
        {
            if (MKLP.HasBanGuardPlugin && (bool)MKLP.Config.Main.UsingBanGuardPlugin)
            {
                return await Plugin_BanPlayer(uuid, category, ip);
            }
            return new APIResponse<bool>(success: false, data: false, "");
        }

        public static async Task<PlayerBan?> Plugin_CheckPlayerBan(string uuid, string playerName, string playerIP)
        {
            return await BanGuard.APIService.CheckPlayerBan(uuid, playerName, playerIP);
        }

        public static async Task<APIResponse<ConnectionCode>> Plugin_GenerateNewConnection(string uuid)
        {
            return await BanGuard.APIService.GenerateNewConnection(uuid);
        }

        public static async Task<APIResponse<bool>> Plugin_BanPlayer(string uuid, string category, string ip)
        {
            return await BanGuard.APIService.BanPlayer(uuid, category, ip);
        }

        /*
        public static async Task<DCAccount?> TryGetDiscordAccount(string uuid)
        {
            var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid }
            };

            try
            {
                JObject? response = await SendApiRequest(_discordCheckMessage, requestData);
                return DCAccount.FromJson(response!);
            }
            catch
            {
                Console.WriteLine("Error getting Discord account.");
                return null;
            }
        }
        */

        public static string GetCategoryFromReason(string reason)
        {
            switch (reason.ToLower())
            {
                case "dupe":
                case "duping":
                case "duplicating":
                case "duper":
                case "splitdupe":
                case "split dupe":
                case "splitduping":
                case "split duping":
                case "splitduplicating":
                case "split duplicating":
                case "splitduper":
                case "split duper":
                    {
                        return "duping";
                    }
                case "hack":
                case "hacks":
                case "hacker":
                case "hacking":
                case "cheating":
                case "godmode":
                case "godmodding":
                    {
                        return "hacks";
                    }
                case "nsfw":
                case "inappropriate":
                case "inappropriatecontent":
                case "inappropriate content":
                case "childsafety":
                case "child-safety":
                case "child safety":
                    {
                        return "child-safety";
                    }
                case "grief":
                case "griefing":
                case "destroying":
                case "flooding":
                    {
                        return "griefing";
                    }
                case "tunneling":
                case "hole maker":
                case "tunnel":
                    {
                        return "tunneling";
                    }
            }

            return "N/A";
        }

        public static bool IsCategory(string category)
        {
            return category is "duping" or "hacks" or "griefing" or "tunneling" or "child-safety";
        }

    }
}
