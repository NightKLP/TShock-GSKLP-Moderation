using Microsoft.Data.Sqlite;
using MKLP.Modules;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
namespace MKLP
{
    public class MKLP_DB
    {
        private IDbConnection _db;

        public MKLP_DB()
        {
            if (MKLP.Config.DataBaseMain.StorageType == "sqlite")
            {
                string sql = Path.Combine(TShock.SavePath, MKLP.Config.DataBaseMain.SqliteDBPath);
                Directory.CreateDirectory(Path.GetDirectoryName(sql));
                _db = new Microsoft.Data.Sqlite.SqliteConnection(string.Format("Data Source={0}", sql));
            }
            else if (MKLP.Config.DataBaseMain.StorageType == "mysql")
            {
                try
                {
                    var hostport = MKLP.Config.DataBaseMain.MySqlHost.Split(':');
                    MySqlConnection DB = new MySqlConnection();
                    DB.ConnectionString =
                        String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            hostport[0],
                            hostport.Length > 1 ? hostport[1] : "3306",
                            MKLP.Config.DataBaseMain.MySqlDbName,
                            MKLP.Config.DataBaseMain.MySqlUsername,
                            MKLP.Config.DataBaseMain.MySqlPassword
                            );
                    _db = DB;
                }
                catch (MySqlException ex)
                {
                    throw new Exception("MySql not setup correctly");
                }
            }
            else
            {
                throw new Exception("Invalid storage type");
            }

            var sqlCreator = new SqlTableCreator(_db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Reports",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("ReportType", MySqlDbType.VarChar, 20),
                new SqlColumn("Reporter", MySqlDbType.VarChar, 20),
                new SqlColumn("Target", MySqlDbType.VarChar, 20),
                new SqlColumn("Message", MySqlDbType.Text),
                new SqlColumn("Since", MySqlDbType.DateTime),
                new SqlColumn("Location", MySqlDbType.VarChar, 100),
                new SqlColumn("Players", MySqlDbType.Text)));

            sqlCreator.EnsureTableStructure(new SqlTable("Mute",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("Identifier", MySqlDbType.VarChar, 100),
                new SqlColumn("Reason", MySqlDbType.Text),
                new SqlColumn("Expiration", MySqlDbType.DateTime)));

            sqlCreator.EnsureTableStructure(new SqlTable("AccountDLinking",
                new SqlColumn("Name", MySqlDbType.VarChar, 20) { Primary = true, Unique = true },
                new SqlColumn("UserID", MySqlDbType.VarChar, 50)));

        }

        #region [ AccountDLinked ]

        public Dictionary<string, string> AccountDLinkingList()
        {
            using var reader = _db.QueryReader("SELECT * FROM AccountDLinking");

            Dictionary<string, string> result = new();

            while (reader.Read())
            {
                result.Add(reader.Get<string>("Name"), reader.Get<string>("UserID"));
            }
            return result;
            throw new NullReferenceException();
        }

        public bool AddAccountDLinkingUserID(string AccountName, string UserID)
        {
            return _db.Query("INSERT INTO AccountDLinking (" +
                "Name, " +
                "UserID) " +
                "VALUES (@0, @1)",
                AccountName,
                UserID
                ) != 0;
        }

        public bool ChangeAccountDLinkingUserID(string AccountName, string UserID)
        {
            return _db.Query("UPDATE AccountDLinking SET UserID = @0 WHERE Name = @1", UserID, AccountName) != 0;
        }

        public bool DeleteAccountDLinkingUserID(string AccountName)
        {
            return _db.Query("DELETE FROM AccountDLinking WHERE Name = @0", AccountName) != 0;
        }

        #endregion

        #region [ Reports ]
        public List<MKLP_Report> GetReportList(int maxreport = 10, string from = "", string target = "")
        {
            using var reader = _db.QueryReader("SELECT * FROM Reports ORDER BY ID DESC");

            List<MKLP_Report> result = new();

            int index = 0;

            if (maxreport < 0) { maxreport = 1; }

            while (reader.Read())
            {
                if (index > maxreport) break;

                if (reader.Get<string>("Reporter") != from && from != "") continue;

                if (reader.Get<string>("Target") != target && target != "") continue;

                result.Add(new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("ReportType"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        ));
                index++;
            }
            return result;
            throw new NullReferenceException();
        }

        public IEnumerable<MKLP_Report> GetReport(string from = "", string target = "")
        {
            if (target != "")
            {
                using var reader = _db.QueryReader("SELECT * FROM Reports WHERE Target = @0", target);
                while (reader.Read())
                {
                    yield return new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("ReportType"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        );
                }
            }
            else if (from != "")
            {
                using var reader = _db.QueryReader("SELECT * FROM Reports WHERE From = @0", from);
                while (reader.Read())
                {
                    yield return new(
                        reader.Get<int>("ID"),
                        reader.Get<string>("ReportType"),
                        reader.Get<string>("Reporter"),
                        reader.Get<string>("Target"),
                        reader.Get<string>("Message"),
                        reader.Get<DateTime>("Since"),
                        reader.Get<string>("Location"),
                        reader.Get<string>("Players")
                        );
                }
            }

        }

        public MKLP_Report GetReportByID(int ID)
        {
            using var reader = _db.QueryReader("SELECT * FROM Reports WHERE ID = @0", ID);
            while (reader.Read())
            {
                return new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("ReportType"),
                    reader.Get<string>("Reporter"),
                    reader.Get<string>("Target"),
                    reader.Get<string>("Message"),
                    reader.Get<DateTime>("Since"),
                    reader.Get<string>("Location"),
                    reader.Get<string>("Players")
                    );
            }

            throw new NullReferenceException();
        }

        public int AddReport(MKLP_Report.RType reportType, string reporter, string target, string message, DateTime Since, string location, string playerlist)
        {
            string query = "INSERT INTO Reports (" +
                "ReportType, " +
                "Reporter, " +
                "Target, " +
                "Message, " +
                "Since, " +
                "Location, " +
                "Players) " +
                "VALUES (@0, @1, @2, @3, @4, @5, @6);";
            if (_db.GetSqlType() == SqlType.Mysql)
            {
                query += "SELECT LAST_INSERT_ID();";
            }
            else
            {
                query += "SELECT CAST(last_insert_rowid() as INT);";
            }

            int id = _db.QueryScalar<int>(query,
                reportType.ToString(),
                reporter,
                target,
                message,
                Since,
                location,
                playerlist
                );

            if (id == 0)
            {
                throw new NullReferenceException();
            }

            return id;
        }

        public bool DeleteReport(int ID)
        {
            return _db.Query("DELETE FROM Reports WHERE ID = @0", ID) != 0;
        }

        #endregion

        #region [ Mutes ]

        public IEnumerable<Mute> GetMute(string Identifier)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE Identifier = @0", Identifier);
            while (reader.Read())
            {
                yield return new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<DateTime>("Expiration")
                    );
            }
        }

        public IEnumerable<Mute> GetMute(int ID)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE ID = @0", ID);
            while (reader.Read())
            {
                yield return new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<DateTime>("Expiration")
                    );
            }
        }

        public Mute GetMuteExpiration(string Identifier)
        {
            using var reader = _db.QueryReader("SELECT * FROM Mute WHERE Identifier = @0", Identifier);
            while (reader.Read())
            {
                return new Mute(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<DateTime>("Expiration")
                    );
            }
            throw new NullReferenceException();
        }

        public bool AddMute(string Identifier, DateTime Expiration, string Reason = "No Reason Provided")
        {
            return _db.Query("INSERT INTO Mute (" +
                "ID, " +
                "Identifier, " +
                "Reason, " +
                "Expiration) " +
                "VALUES (@0, @1, @2, @3)",
                null,
                Identifier,
                Reason,
                Expiration
                ) != 0;
        }

        public bool DeleteMute(string Identifier)
        {
            return _db.Query("DELETE FROM Mute WHERE Identifier = @0", Identifier) != 0;
        }


        public bool CheckPlayerMute(TSPlayer player, bool inform_unmuted = false)
        {
            bool muted = false;
            bool itExist = false;
            try
            {
                Mute get = GetMuteExpiration($"{Identifier.Name}{player.Name}");
                itExist = true;
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                } else
                {
                    DeleteMute(Identifier.Name + player.Name);
                }
            }
            catch (NullReferenceException) { }

            try
            {
                Mute get = GetMuteExpiration($"{Identifier.Account}{player.Account.Name}");
                itExist = true;
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
                else
                {
                    DeleteMute(Identifier.Account + player.Account.Name);
                }

            }
            catch (NullReferenceException) { }

            try
            {
                Mute get = GetMuteExpiration($"{Identifier.IP}{player.IP}");
                itExist = true;
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
                else
                {
                    DeleteMute(Identifier.IP + player.IP);
                }

            }
            catch (NullReferenceException) { }

            try
            {
                Mute get = GetMuteExpiration($"{Identifier.UUID}{player.UUID}");
                itExist = true;
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
                else
                {
                    DeleteMute(Identifier.UUID + player.UUID);
                }

            }
            catch (NullReferenceException) { }

            if (muted)
            {
                player.mute = true;
            }

            if (!player.mute && itExist && inform_unmuted)
            {
                player.SendSuccessMessage(MKLP.GetText("You're no longer muted"));
            }

            return player.mute;

        }
        public bool PlayerIsMuted(UserAccount account)
        {
            bool muted = false;
            try
            {
                Mute get = GetMuteExpiration($"{Identifier.Name}{account.Name}");
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
            }
            catch (NullReferenceException) { }

            try
            {
                Mute get = GetMuteExpiration($"{Identifier.Account}{account.Name}");
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
            }
            catch (NullReferenceException) { }

            try
            {
                var GetIPs = JsonConvert.DeserializeObject<List<string>>(account.KnownIps);
                Mute get = GetMuteExpiration($"{Identifier.IP}{GetIPs[GetIPs.Count() - 1]}");
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
            }
            catch (NullReferenceException) { }

            try
            {
                Mute get = GetMuteExpiration($"{Identifier.UUID}{account.UUID}");
                if ((DateTime.UtcNow - get.Expiration).TotalSeconds < 0)
                {
                    muted = true;
                }
            }
            catch (NullReferenceException) { }

            return muted;

        }

        #endregion

    }

    public class Mute
    {
        public int ID;
        public string Identifier;
        public string Reason;
        public DateTime Expiration;

        public Mute(
            int ID,
            string Identifier,
            string Reason,
            DateTime Expiration
            )
        {
            this.ID = ID;
            this.Identifier = Identifier;
            this.Reason = Reason;
            this.Expiration = Expiration;
        }
    }

    public class MKLP_Report
    {
        public int ID;
        public RType ReportType;
        public string From;
        public string Target;
        public string Message;
        public DateTime Since;
        public string Location;
        public string Players;

        public MKLP_Report(
            int ID,
            string ReportType,
            string From,
            string Target,
            string Message,
            DateTime Since,
            string Location,
            string Players
            )
        {
            this.ID = ID;
            if (ReportType == null)
            {
                this.ReportType = RType.NA;
            } else
            {
                switch (ReportType)
                {
                    case "NormalReport":
                        this.ReportType = RType.NormalReport;
                        break;
                    case "PlayerReport":
                        this.ReportType = RType.PlayerReport;
                        break;
                    case "BugReport":
                        this.ReportType = RType.BugReport;
                        break;
                    case "StaffReport":
                        this.ReportType = RType.StaffReport;
                        break;
                    default:
                        this.ReportType = RType.NA;
                        break;
                }
            }

                this.From = From;
            this.Target = Target;
            this.Message = Message;
            this.Since = Since;
            this.Location = Location;
            this.Players = Players;
        }

        public enum RType
        {
            NA,
            NormalReport,
            PlayerReport,
            BugReport,
            StaffReport
        }
    }
}
