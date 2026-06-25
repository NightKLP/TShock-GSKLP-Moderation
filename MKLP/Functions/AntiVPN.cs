using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TShockAPI.DB;

namespace MKLP.Functions
{

    internal static class AntiVPN
    {
        public static readonly HttpClient client = new HttpClient();
        public static string LastApiResult;

        public static Dictionary<string, bool> IPData = new();

        static (Func<string, string, Task<bool>> function, string key)[] services;
        public static void Initialize()
        {
            List<(Func<string, string, Task<bool>> function, string key)> syncservices = new();
            foreach (var service in MKLP.Config.Main.AntiVPN.Services)
            {
                switch (service.Name.ToLower())
                {
                    case "iphub":
                        syncservices.Add(new (VPNCheck.IpHub, service.Key));
                        break;
                    case "proxycheck":
                        syncservices.Add(new (VPNCheck.ProxyCheck, service.Key));
                        break;
                    case "getipintel":
                        syncservices.Add(new (VPNCheck.GetIpIntel, service.Key));
                        break;
                    case "iptrooper":
                        syncservices.Add(new (VPNCheck.IpTrooper, service.Key));
                        break;
                    case "ipqualityscore":
                        syncservices.Add(new (VPNCheck.IpQualityScore, service.Key));
                        break;
                    case "iphunter":
                        syncservices.Add(new (VPNCheck.IpHunter, service.Key));
                        break;
                    case "vpnblocker":
                        syncservices.Add(new (VPNCheck.VpnBlocker, service.Key));
                        break;
                    case "ip2location":
                        syncservices.Add(new (VPNCheck.Ip2Location, service.Key));
                        break;
                    case "shodan":
                        syncservices.Add(new (VPNCheck.Shodan, service.Key));
                        break;
                }
            }

            services = syncservices.ToArray();

            IPDataSync();
        }

        public static void IPDataCleanSync()
        {
            using var reader = MKLP.DBManager._db.QueryReader("SELECT * FROM IPDataList");

            Dictionary<string, string> result = new();

            while (reader.Read())
            {
                string ip = reader.Get<string>("IP");

                if (IPData.ContainsKey(ip))
                {
                    if (reader.Get<bool>("IsVPN") && (DateTime.UtcNow - reader.Get<DateTime>("TimeAdded")).TotalDays >= (int)MKLP.Config.Main.AntiVPN.IPDataExpireDay_Untrusted)
                    {
                        DeleteIPData(ip);
                    } else if ((DateTime.UtcNow - reader.Get<DateTime>("TimeAdded")).TotalDays >= (int)MKLP.Config.Main.AntiVPN.IPDataExpireDay_Trusted)
                    {
                        DeleteIPData(ip);
                    }
                }
            }
        }

        public static void IPDataSync()
        {
            using var reader = MKLP.DBManager._db.QueryReader("SELECT * FROM IPDataList");

            Dictionary<string, string> result = new();

            while (reader.Read())
            {
                string ip = reader.Get<string>("IP");

                if (!IPData.ContainsKey(ip))
                {
                    IPData.Add(ip, reader.Get<bool>("IsVPN"));
                }
            }
        }

        public static void AddIPData(string ip, bool isVPN)
        {
            if (!IPData.ContainsKey(ip))
            {
                IPData.Add(ip, isVPN);
                MKLP.DBManager._db.Query("INSERT INTO IPDataList (IP, IsVPN, TimeAdded) VALUES (@0, @1, @2)", ip, isVPN, DateTime.UtcNow);
            }
        }

        public static void DeleteIPData(string ip)
        {
            if (IPData.ContainsKey(ip))
            {
                IPData.Remove(ip);
                MKLP.DBManager._db.Query("DELETE FROM IPDataList WHERE IP = @0", ip);
            }
        }


        public async static Task<bool> IPCheck(string ip)
        {
            MKLP_Console.SendLog_Info(MKLP.GetText("AntiVPN - Checking IP of {0}", ip));
            if (IPData.ContainsKey(ip))
            {
                //MKLP_Console.SendLog_Info($"AntiVPN - Existing IP Data: {IPData[ip]}");
                return IPData[ip];
            }

            foreach (var func in services)
            {
                if (await func.function(ip, func.key))
                {
                    MKLP_Console.SendLog_Info(MKLP.GetText("AntiVPN - IP of {0} confirmed VPN!", ip));
                    AddIPData(ip, true);
                    return true;
                }
            }
            MKLP_Console.SendLog_Info(MKLP.GetText("AntiVPN - IP of {0} is good", ip));
            AddIPData(ip, false);
            return false;
        }




        public static class VPNCheck
        {

            public static Func<string, string, Task<bool>> IpHub = async (ip, key) =>
            {

                string getVPN;
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://v2.api.iphub.info/ip/" + ip);

                requestMessage.Headers.Remove("X-Key");
                requestMessage.Headers.Add("X-Key", key);

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = (string)data.SelectToken("block");
                }

                var statusCode = httpResponse.StatusCode;

                if (statusCode.ToString() == "OK"
                    && getVPN == "1")
                {
                    return true;

                }
                else
                {
                    return false;
                }

            };

            public static Func<string, string, Task<bool>> ProxyCheck = async (ip, key) =>
            {

                string getVPN;
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://proxycheck.io/v2/{ip}?vpn=1&key={key}");

                var httpResponse = await client.SendAsync(requestMessage);

                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    var d1 = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                    var d = d1[ip];
                    getVPN = d["proxy"];
                }

                if (getVPN == "yes")
                    return true;

                else
                    return false;

            };

            public static Func<string, string, Task<bool>> GetIpIntel = async (ip, contact) =>
            {
                float getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://check.getipintel.net/check.php?ip={ip}&contact={contact}&format=json");

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = (float)data.result;
                }

                if (getVPN >= 0.98)
                    return true;

                else
                    return false;

            };

            public static Func<string, string, Task<bool>> IpTrooper = async (ip, key) =>
            {
                string getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"http://api.iptrooper.net/check/{ip}?key={key}&full=1");

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data.type;
                }

                if (String.Equals(getVPN, "proxy", StringComparison.InvariantCultureIgnoreCase))
                    return true;
                else
                    return false;
            };

            public static Func<string, string, Task<bool>> IpQualityScore = async (ip, key) =>
            {
                bool getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://ipqualityscore.com/api/json/ip/{key}/{ip}?strictness=1&allow_public_access_points=false&fast=false");

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data.proxy;
                }

                if (getVPN)
                    return true;

                else
                    return false;
            };

            public static Func<string, string, Task<bool>> IpHunter = async (ip, key) =>
            {
                int getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://www.iphunter.info:8082/v1/ip/" + ip);

                requestMessage.Headers.Remove("X-Key");
                requestMessage.Headers.Add("X-Key", key);

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data.data.block;
                }

                if (getVPN == 1)
                    return false;

                else
                    return false;


            };

            public static Func<string, string, Task<bool>> VpnBlocker = async (ip, key) =>
            {
                bool getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.vpnblocker.net/v2/json/" + ip);

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data["host-ip"];
                }

                if (getVPN)
                    return true;

                else
                    return false;

            };

            public static Func<string, string, Task<bool>> Ip2Location = async (ip, key) =>
            {
                bool getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.ip2location.io/?key={key}&ip=" + ip);

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data.is_proxy;
                }

                if (getVPN)
                    return true;

                else
                    return false;

            };

            public static Func<string, string, Task<bool>> Shodan = async (ip, key) =>
            {
                string[] getVPN;

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.shodan.io/shodan/host/{ip}?key={key}");

                var httpResponse = await client.SendAsync(requestMessage);
                using (var streamReader = new StreamReader(httpResponse.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    LastApiResult = result;
                    dynamic data = JObject.Parse(result);
                    getVPN = data.tags;
                }

                if (getVPN.Count() > 0 && getVPN.Any(i => i.Equals("proxy", StringComparison.InvariantCultureIgnoreCase) || i.Equals("vpn", StringComparison.InvariantCultureIgnoreCase)))
                    return true;

                else
                    return false;


            };
        }
    }
}
