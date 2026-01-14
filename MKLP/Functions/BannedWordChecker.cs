using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TShockAPI;
using System.Reactive.Joins;
using MySqlX.XDevAPI.Common;
using Group = TShockAPI.Group;

namespace MKLP.Functions
{
    public static class BannedWordChecker
    {
        public static bool ISBannedWord(Group group, string text)
        {
            string[] dummy1;
            string dummy2;
            return ISBannedWord(group, text, out dummy1, out dummy2);
        }
        public static bool ISBannedWord(Group group, string text, out string[] badwords)
        {
            string dummy;
            return ISBannedWord(group, text, out badwords ,out dummy);
        }
        public static bool ISBannedWord(Group group, string text, out string censorText)
        {
            string[] dummy;
            return ISBannedWord(group, text, out dummy, out censorText);
        }
        public static bool ISBannedWord(Group group, string text, out string[] badwords, out string censorText)
        {
            string censorResult = text;
            List<string> badwordsResult = new();
            bool hasmatch = false;

            foreach (Config.BannedWordValue get in MKLP.Config.Main.ChatMod.Ban_MessageContains)
            {
                if (group.HasPermission(get.PermissionByPass)) continue;

                string pattern = $@"{GetRegexBadWord(get.Word, get.BannedWordDetectionRange, get.MustBeSeperated)}";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

                if (matches.Count == 0) continue;

                foreach (Match match in matches)
                {
                    badwordsResult.Add(match.Value);
                    censorResult = censorResult.Replace(match.Value, new string((char)MKLP.Config.Main.ChatMod.Censor_Character, match.Value.Length));
                    hasmatch = true;
                }
            }


            badwords = badwordsResult.ToArray();
            censorText = censorResult;
            return hasmatch;
        }

        public enum Detection
        {
            NA,
            VeryLow,
            Low,
            Moderate,
            High,
            VeryHigh,
        }
        public static string GetRegexBadWord(string word, Detection detection, bool MustBeSeperated)
        {
            word = word.ToLower();

            string result = "";

            string MBS = MustBeSeperated ? "" : "(?<![a-zA-Z])";

            switch (detection)
            {
                case Detection.VeryHigh:
                    {

                        result += $"\\b\\w*(?:{MBS}";

                        for (int i = 0; i < word.Length; i++)
                        {
                            if (i == (word.Length - 1))
                            {
                                result += word[i];
                                continue;
                            }
                            result += $"{CharToCharRegex(word[i])}[\\W_]*";
                        }

                        result += $"+{MBS})\\w*\\b";

                        return result;
                    }
                case Detection.High:
                    {

                        result += $"\\b\\w*(?:{MBS}";

                        for (int i = 0; i < word.Length; i++)
                        {
                            if (i == (word.Length - 1))
                            {
                                result += word[i];
                                continue;
                            }
                            result += $"{word[i]}[\\W_]*";
                        }

                        result += $"+{MBS})\\w*\\b";

                        return result;
                    }
                case Detection.Moderate:
                default: //Default | Moderate
                    {
                        result += $"\\b\\w*(?:{MBS}";

                        for (int i = 0; i < word.Length; i++)
                        {
                            if (i == (word.Length - 1))
                            {
                                result += word[i];
                                continue;
                            }
                            result += $"{word[i]}[\\W_]*";
                        }

                        result += $"+{MBS})\\w*\\b";

                        return result;
                    }
                case Detection.Low:
                    {
                        result += $"\\b\\w*({MBS}";

                        for (int i = 0; i < word.Length; i++)
                        {
                            result += $"{word[i]}+";
                        }

                        result += $"{MBS})\\w*\\b";

                        return result;
                    }
                case Detection.VeryLow:
                    {
                        result += $"\\b\\w*({MBS}";

                        for (int i = 0; i < word.Length; i++)
                        {
                            result += word[i];
                        }

                        result += $"{MBS})\\w*\\b";

                        return result;
                    }
                case Detection.NA:
                    {
                        return word;
                    }
            }
        }

        public static string CharToCharRegex(char character)
        {
            Dictionary<char, string> charregex = new()
            {
                {'a',  "[@4^a]"},
                {'b', "[8ßb]" },
                {'c', "[<[c]" },
                {'d', "d" },
                {'e', "[3€e]" },
                {'f', "f" },
                {'g', "[69g]" },
                {'h', "[#h]" },
                {'i', "[1!|i]" },
                {'j',  "j"},
                {'k', "k" },
                {'l', "[1|l]" },
                {'m', "m"},
                {'n', "n" },
                {'o', "[0°o]" },
                {'p', "p" },
                {'q', "[9q]" },
                {'r', "r" },
                {'s', "[5$zs]" },
                {'t', "[7+t]" },
                {'u', "[vµu]" },
                {'v', "[uv]" },
                {'w', "w" },
                {'x', "[%x]" },
                {'y', "[jy]" },
                {'z', "[2z]" }
            };

            if (charregex.ContainsKey(character))
            {
                return charregex[character];
            }

            return character.ToString();
        }


    }
}
