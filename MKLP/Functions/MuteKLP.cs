using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;
using static Terraria.DataStructures.RichRoomCheckFeedback;

namespace MKLP.Functions
{
    internal static class MuteKLP
    {
        public static Mute[] mute = new Mute[] {};
        public static DateTime NearestExpiredMute = DateTime.UtcNow;

        public static void SyncMute()
        {
            List<Mute> result = new();

            using var reader = MKLP.DBManager._db.QueryReader("SELECT * FROM Mute");
            while (reader.Read())
            {
                DateTime expire = reader.Get<DateTime>("Expiration");

                if ((expire - DateTime.UtcNow).TotalSeconds < (NearestExpiredMute - DateTime.UtcNow).TotalSeconds &&
                    (expire - DateTime.UtcNow).TotalSeconds > 0)
                {
                    NearestExpiredMute = expire;
                }

                result.Add(new(
                    reader.Get<int>("ID"),
                    reader.Get<string>("Identifier"),
                    reader.Get<string>("Reason"),
                    reader.Get<int>("Used"),
                    expire
                    ));
            }
            mute = result.ToArray();
        }
        public static (bool muted, bool used) PlayerIsMuted(string accountname)
        {
            UserAccount account = TShock.UserAccounts.GetUserAccountByName(accountname);
            if (account == null) { return (false, true); }

            var GetIPs = JsonConvert.DeserializeObject<List<string>>(account.KnownIps);

            if (mute.Length == 0) { return (false, true); }

            bool used = true;

            foreach (Mute gmute in mute)
            {
                if (gmute.Identifier == $"{Identifier.Name}{account.Name}" ||
                    gmute.Identifier == $"{Identifier.Account}{account.Name}" ||
                    gmute.Identifier == $"{Identifier.IP}{GetIPs[GetIPs.Count() - 1]}" ||
                    gmute.Identifier == $"{Identifier.UUID}{account.UUID}")
                {
                    if ((DateTime.UtcNow - gmute.Expiration).TotalSeconds < 0)
                    {
                        return (true, true);
                    }
                    if (!gmute.Used) { used = false; }
                }
            }
            return (false, used);
        }

        public static bool SetMuteUsed(TSPlayer player, bool Used)
        {
            bool success = true;
            if (SetMuteUsed($"{Identifier.Name}{player.Name}", Used) == false) { success = false; }
            if (SetMuteUsed($"{Identifier.Account}{player.Account.Name}", Used) == false) { success = false; }
            if (SetMuteUsed($"{Identifier.IP}{player.IP}", Used) == false) { success = false; }
            if (SetMuteUsed($"{Identifier.UUID}{player.UUID}", Used) == false) { success = false; }
            return success;
        }

        public static bool SetMuteUsed(string Identifier, bool Used)
        {
            for (int i = 0; i < mute.Length; i++)
            {
                if (mute[i].Identifier == Identifier)
                {
                    bool result = MKLP.DBManager._db.Query("UPDATE Mute SET Used = @0 WHERE Identifier = @1",
                        Used,
                        Identifier
                        ) != 0;
                    if (result) { mute[i].Used = Used; }

                    return result;
                }
            }
            return false;
        }

        public static bool AddMute(string Identifier, DateTime Expiration, string Reason = "No Reason Provided")
        {
            for (int i = 0; i < mute.Length; i++)
            {
                if (mute[i].Identifier == Identifier)
                {
                    bool result = MKLP.DBManager._db.Query("UPDATE Mute SET " +
                        "Reason = @0, " +
                        "Used = @1, " +
                        "Expiration = @2 " +
                        "WHERE Identifier = @3",
                        Reason,
                        false,
                        Expiration,
                        Identifier
                        ) != 0;
                    SyncMute();
                    return result;
                }
            }
            bool result2 = MKLP.DBManager._db.Query("INSERT INTO Mute (" +
                "ID, " +
                "Identifier, " +
                "Reason, " +
                "Used, " +
                "Expiration) " +
                "VALUES (@0, @1, @2, @3, @4)",
                null,
                Identifier,
                Reason,
                false,
                Expiration
                ) != 0;
            SyncMute();

            return result2;
        }

        public static bool DeleteMuteSafe(string Identifier)
        {
            bool result = MKLP.DBManager._db.Query("UPDATE Mute SET " +
                "Expiration = @0 " +
                "WHERE Identifier = @1",
                DateTime.MinValue,
                Identifier
                ) != 0;
            SyncMute();

            return result;
        }

        public static bool DeleteMute(string Identifier)
        {
            bool result = MKLP.DBManager._db.Query("DELETE FROM Mute WHERE Identifier = @0", Identifier) != 0;
            SyncMute();

            return result;
        }
    }



    public class Mute
    {
        public int ID;
        public string Identifier;
        public string Reason;
        public bool Used;
        public DateTime Expiration;

        public Mute(
            int ID,
            string Identifier,
            string Reason,
            int? Used,
            DateTime Expiration
            )
        {
            this.ID = ID;
            this.Identifier = Identifier;
            this.Reason = Reason;
            if (Used != null) { this.Used = Used.Value == 1; } else { this.Used = false; }
            this.Expiration = Expiration;
        }
    }
}
