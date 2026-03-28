using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria;
using MKLP.Modules;

namespace MKLP
{
    public static class LogKLP
    {
        public static string LogPath_ModLog = Path.Combine(TShock.SavePath, "logs", "MKLP", "MKLP-ModLogs");
        public static string LogPath_ReportLog = Path.Combine(TShock.SavePath, "logs", "MKLP", "MKLP-ReportLogs");

        public static string LogPath_Tile = Path.Combine(TShock.SavePath, "logs", "MKLP", "Log-Tile");
        public static string LogPath_Sign = Path.Combine(TShock.SavePath, "logs", "MKLP", "Log-Sign");
        public static string LogPath_Inventory = Path.Combine(TShock.SavePath, "logs", "MKLP", "Log-Inventory");
        public static DateTime Currentlogfile = DateTime.Now;

        public static void InitializeLogging()
        {
            Currentlogfile = DateTime.Now;

            if (!Directory.Exists(LogPath_ModLog) && (bool)MKLP.Config.Main.Logging.ModLogTXT_Enable) Directory.CreateDirectory(LogPath_ModLog);
            if (!Directory.Exists(LogPath_ReportLog) && (bool)MKLP.Config.Main.Logging.ReportLogTXT_Enable) Directory.CreateDirectory(LogPath_ReportLog);

            if (!Directory.Exists(LogPath_Tile) && (bool)MKLP.Config.Main.Logging.LogTile) Directory.CreateDirectory(LogPath_Tile);
            if (!Directory.Exists(LogPath_Sign) && (bool)MKLP.Config.Main.Logging.LogSign) Directory.CreateDirectory(LogPath_Sign);
            if (!Directory.Exists(LogPath_Inventory) && (bool)MKLP.Config.Main.Logging.LogInventory) Directory.CreateDirectory(LogPath_Inventory);


        }

        public static string GetPath(string path, DateTime time)
        {
            return Path.Combine(path, $"{time.ToString("yyyy-MM-dd")}.log");
        }

        public static string TileLogS = "";
        public static string SignLogS = "";
        public static string InventoryLogS = "";

        #region Main
        public static void Log_ModLog(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.ModLogTXT_Enable) return;

            using (StreamWriter writer = new StreamWriter(GetPath(LogPath_ModLog, Currentlogfile), true))
            {
                writer.WriteLine($"{text}");
            }
        }
        public static void Log_Report(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.ReportLogTXT_Enable) return;

            using (StreamWriter writer = new StreamWriter(GetPath(LogPath_ReportLog, Currentlogfile), true))
            {
                writer.WriteLine($"{text}");
            }
        }
        #endregion

        #region server
        public static void Log_Tile(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.LogTile) return;

            using (StreamWriter writer = new StreamWriter(GetPath(LogPath_Tile, Currentlogfile), true))
            {
                writer.WriteLine($"{text}");
            }
        }
        public static void Log_Sign(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.LogSign) return;

            using (StreamWriter writer = new StreamWriter(GetPath(LogPath_Sign, Currentlogfile), true))
            {
                writer.WriteLine($"{text}");
            }
        }
        public static void Log_Inventory(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.LogInventory) return;

            using (StreamWriter writer = new StreamWriter(GetPath(LogPath_Inventory, Currentlogfile), true))
            {
                writer.WriteLine($"{text}");
            }
        }
        #endregion


        #region GetLog

        public static List<(string, NetItemKLP, NetItemKLP, int, string)> GetLog_Inventory(string filepath, string targetname)
        {
            string filecontext = File.ReadAllText(filepath);

            List<(string, NetItemKLP, NetItemKLP, int, string)> log = new();

            foreach (string gcontext in filecontext.Split("\n"))
            {
                if (gcontext == "" || gcontext == " ") continue;

                string playername = gcontext.Split("|")[0].Split(" ")[1];
                string GetPreviousItem = gcontext.Split("|")[1].Split(":")[1];
                string GetCurrentItem = gcontext.Split("|")[2].Split(":")[1];

                string ActualItemSlot = gcontext.Split("|")[3].Split(":")[1];
                string ItemSlotType = gcontext.Split("|")[3].Split(":")[1];

                if (playername != targetname) continue;

                log.Add(new(
                    playername,
                    new NetItemKLP(int.Parse(GetPreviousItem.Split(",")[0]),int.Parse(GetPreviousItem.Split(",")[1]),int.Parse(GetPreviousItem.Split(",")[2])),
                    new NetItemKLP(int.Parse(GetCurrentItem.Split(",")[0]),int.Parse(GetCurrentItem.Split(",")[1]),int.Parse(GetCurrentItem.Split(",")[2])),
                    int.Parse(ActualItemSlot),
                    ItemSlotType
                    ));
            }

            return log;
        }

        public static List<(string, string, string)> GetLog_Sign(string filepath, Vector2 pos)
        {
            string filecontext = File.ReadAllText(filepath);

            List<(string, string, string)> log = new();

            foreach (string gcontext in filecontext.Split("\n"))
            {
                if (gcontext == "" || gcontext == " ") continue;

                string playername = gcontext.Split("|")[0].Split(" ")[1];
                string edittype = gcontext.Split("|")[1].Trim();

                string X = gcontext.Split("|")[2].Split(":")[1];
                string Y = gcontext.Split("|")[3].Split(":")[1];
                Vector2 getposlog = new(int.Parse(X), int.Parse(Y));

                if (pos != getposlog) continue;

                Regex regex = new Regex(@"text\s*:\s*(.*?)(?=\s*\||$)", RegexOptions.IgnoreCase);

                MatchCollection matches = regex.Matches(gcontext);

                if (matches.Count > 0)
                {
                    string gettext = matches[matches.Count - 1].Groups[1].Value.Trim();

                    log.Add((playername, edittype, gettext));
                }

            }

            return log;
        }
        public static Dictionary<string, Dictionary<Vector2, int>> GetLog_Tile(string filepath, Vector2 pos, int getdistance)
        {
            string filecontext = File.ReadAllText(filepath);

            Dictionary<string, Dictionary<Vector2, int>> log = new();

            foreach (string gcontext in filecontext.Split("\n"))
            {
                if (gcontext == "" || gcontext == " ") continue;

                string playername = gcontext.Split("|")[0].Split(" ")[1];
                string edittype = gcontext.Split("|")[1].Trim();

                string title = $"{playername}, {edittype}";

                string X = gcontext.Split("|")[2].Split(":")[1];
                string Y = gcontext.Split("|")[3].Split(":")[1];
                Vector2 getposlog = new(int.Parse(X), int.Parse(Y));

                if (pos.Distance(getposlog) <= getdistance)
                {
                    if (log.ContainsKey(title))
                    {
                        if (log[title].ContainsKey(getposlog))
                        {
                            log[title][getposlog]++;
                            continue;
                        }
                        log[title].Add(getposlog, 1);
                    }
                    else
                    {
                        Dictionary<Vector2, int> temp = new();
                        temp.Add(getposlog, 1);
                        log.Add(title, temp);
                    }
                }
            }

            return log;
        }


        #endregion

        #region SaveLog

        public static void SaveLog()
        {
            if ((bool)MKLP.Config.Main.Logging.LogTile && TileLogS != "")
            {
                LogKLP.Log_Tile(TileLogS);
            }
            TileLogS = "";
            if ((bool)MKLP.Config.Main.Logging.LogSign && SignLogS != "")
            {
                LogKLP.Log_Sign(SignLogS);
            }
            SignLogS = "";
            if ((bool)MKLP.Config.Main.Logging.LogInventory && InventoryLogS != "")
            {
                LogKLP.Log_Sign(InventoryLogS);
            }
            InventoryLogS = "";
            Currentlogfile = DateTime.Now;
        }

        #endregion
    }
}
