using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace DSharpPlus.EmojiListGenerator
{
    class Emoji
    {
        public string[] names { get; set; }
        public string surrogates { get; set; }
        // float unicodeVersion;
        // public bool hasDiversity = false;
        public Emoji[] diversityChildren { get; set; }
        // string[] diversity;
        // bool hasDiversityParent = false;
    }

    class EmojiShortcut
    {
        public string emoji { get; set; }
        public string[] shortcuts { get; set; }
    }

    class Program
    {
        // Ussage: dotnet run -- emojis.json emoji-shortcuts.json > output-file
        static void Main(string[] args)
        {
            string emojispath = args[0];
            string emojishortcutspath = args[1];

            Dictionary<string, Emoji[]> EmojiCategories = JsonSerializer.Deserialize<Dictionary<string, Emoji[]>>(File.ReadAllText(emojispath));
            List<Emoji> Emojis = new List<Emoji>();
            foreach (var es in EmojiCategories.Values)
            {
                Emojis.AddRange(es);
            }
            EmojiCategories = null;

            EmojiShortcut[] EmojiShortcuts = JsonSerializer.Deserialize<EmojiShortcut[]>(File.ReadAllText(emojishortcutspath));

            Dictionary<string, string> EmojiList = new Dictionary<string, string>();


            foreach (Emoji e in Emojis)
            {
                var unicode = EmojiToUnicode(e.surrogates);
                foreach (var name in e.names)
                {
                    EmojiList.Add($":{name}:", unicode);
                }

                if (e.diversityChildren != null)
                {
                    foreach (var dc in e.diversityChildren)
                    {
                        var du = EmojiToUnicode(dc.surrogates);
                        foreach (var name in dc.names)
                        {
                            EmojiList.Add($":{name}:", du);
                        }
                    }
                }
            }


            foreach (EmojiShortcut shortcut in EmojiShortcuts)
            {
                var unicode = EmojiToUnicode(Emojis.First(e => e.names.Contains(shortcut.emoji)).surrogates);

                foreach (var sh in shortcut.shortcuts)
                {
                    EmojiList.Add(sh.Replace("\\", "\\\\").Replace("\"", "\\\""), unicode);
                }
            }

            var el = EmojiList.ToList().OrderBy(x => x.Key).ToList();
            for (int i = 0; i < el.Count; i++)
            {
                var e = el[i];
                Console.WriteLine($"[\"{e.Key}\"] = \"{e.Value}\",");
            }
        }

        public static string EmojiToUnicode(string emoji)
        {
            string s = "";

            for (int i = 0; i < emoji.Length; i++)
            {
                int unicodeCodePoint = char.ConvertToUtf32(emoji, i);
                if (unicodeCodePoint > 0xffff) i++;
                s += "\\U" + unicodeCodePoint.ToString("x").PadLeft(8, '0');
            }

            return s;
        }
    }
}
