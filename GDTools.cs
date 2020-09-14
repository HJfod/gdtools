using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace gdtools {
    public class GDTools {
        private static string _GMSaveData;
        private static string _LLSaveData;
        private static string _CCDirPath;
        private static List<dynamic> _LevelList;
        public static string _BackupDirectory;
        public static string _UserDataName = "userdata";
        public static int _GDCheckLoopTime = 5000;
        public static class Ext {
            public static string Level = "gmd";
            public static string LevelAlt = "lvl";
            public static string LevelCompressed = "gmdc";
            public static string LevelZipped = "gmdz";
            public static string Backup = "gdb";
            public static string LevelList = $".{Level}, .{LevelCompressed}";
            public static string UserData = "user";
            public static string Filter = $"Level files (*.{LevelAlt};*.{Level};*.{LevelCompressed})|*.{LevelAlt};*.{Level};*.{LevelCompressed}|All files (*.*)|*.*";
            public static string BackupFilter = $"Level files (*.zip;*.{Backup})|*.zip;*.{Backup}|All files (*.*)|*.*";
            public static dynamic ExtArray = new {
                Levels = new string[] { Level, LevelAlt, LevelCompressed, LevelZipped },
                Backups = new string[] { ".zip", Backup }
            };
        }

        private static string DecryptXOR(string str, int key) {
            byte[] xor = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < str.Length; i++) {
                xor[i] = (byte)(xor[i] ^ key);
            }
            return Encoding.UTF8.GetString(xor);
        }

        private static Byte[] DecryptBase64(string istr) {
            return Convert.FromBase64String(istr);
        }

        private static string DecompressGZip(Byte[] data) {
            // i would once again like to thank https://github.com/gd-edit/GDAPI for being open source
            MemoryStream compressedStream = new MemoryStream(data);
            MemoryStream resultStream = new MemoryStream();

            GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);

            zipStream.CopyTo(resultStream);
            return Encoding.UTF8.GetString(resultStream.ToArray());
        }

        private static Byte[] CompressGZip(Byte[] data) {
            MemoryStream uncompressedStream = new MemoryStream(data);
            MemoryStream resultStream = new MemoryStream();
            GZipStream zipStream = new GZipStream(resultStream, CompressionMode.Compress);

            uncompressedStream.CopyTo(zipStream);
            zipStream.Close();
            return resultStream.ToArray();
        }

        public static bool CheckIfGDIsOpen() {
            return Process.GetProcessesByName("geometrydash").Length > 0;
        }

        public static async Task<bool> CheckGDOpenLoop() {
            return await Task.Run<bool>(() => {
                while (CheckIfGDIsOpen()) {
                    Thread.Sleep(_GDCheckLoopTime);
                }
                return true;
            });
        }

        public static string DecodeCCFile(string path, Action<string, int> callback, bool MutateVars = true) {
            string data;

            bool isLL = (path.Split("\\").Last() == "CCGameManager.dat") ? false : true;

            callback($"Loading {path.Split("\\").Last()}...", 0 + 0);

            Stopwatch watch = new Stopwatch();

            watch.Start();
            string file = File.ReadAllText($"{path}");
            watch.Stop();

            if (!file.StartsWith("<?xml version=\"1.0\"?>")) {

                callback($"\t{watch.ElapsedMilliseconds}ms\r\nDecrypting XOR...", 25);

                watch.Reset();
                watch.Start();
                data = DecryptXOR(file, 11);
                data = data.Replace("-", "+").Replace("_", "/").Replace("\0", string.Empty);
                int remaining = data.Length % 4;
                if (remaining > 0) data += new string('=', 4 - remaining);  // thank you to GDEdit / GDAPI for being open source
                watch.Stop();

                callback($"\t{watch.ElapsedMilliseconds}ms\r\nDecrypting Base64...", 50);

                watch.Reset();
                watch.Start();
                Byte[] gzib64 = DecryptBase64(data);
                watch.Stop();

                callback($"\t{watch.ElapsedMilliseconds}ms\r\nDecompressing GZip...", 75);

                watch.Reset();
                watch.Start();
                data = DecompressGZip(gzib64);
                watch.Stop();

                callback($"\t{watch.ElapsedMilliseconds}ms\r\nDecoded {path}!", 100);
                
                if (MutateVars) _CCDirPath = path.Substring(0, path.LastIndexOf("\\"));
                if (MutateVars) if (isLL) _LLSaveData = data; else _GMSaveData = data;
                return data;
            } else {
                callback($"\t{watch.ElapsedMilliseconds}ms\r\nSkipped decoding {path}!", 100);
                
                if (MutateVars) _CCDirPath = path.Substring(0, path.LastIndexOf("\\"));
                if (MutateVars) if (isLL) _LLSaveData = file; else _GMSaveData = file;
                return file;
            }
        }
 
        public static string GetCCPath(string which) {
            if (which == "") return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\..\\Local\\GeometryDash";
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\..\\Local\\GeometryDash\\CC{which}.dat";
        }

        private static string GetKey(string savedata, string key, string type = ".*?", bool legacy = false) {
            if (type == null) {
                Match match = Regex.Match(savedata, $"<k>{key}</k>.*?>", RegexOptions.None, Regex.InfiniteMatchTimeout);
                if (match.Value != "") {
                    return match.Value.Substring(match.Value.LastIndexOf("<")).IndexOf("t") > -1 ? "True" : "False";
                } else {
                    return "False";
                }
            }
            string actualTypeMatch = Regex.Match(savedata, $"<k>{key}</k><{type}>", RegexOptions.None, Regex.InfiniteMatchTimeout).Value;
            if (actualTypeMatch == "") return "";
            string actualType = actualTypeMatch.Substring(actualTypeMatch.LastIndexOf("<")+1, 1);

            string matcher = $"<k>{key}</k><{actualType}>.*?</{actualType}>";
            Match m = Regex.Match(savedata, matcher, RegexOptions.None, Regex.InfiniteMatchTimeout);
            return m.Value == "" ? "" : m.Value.Substring($"<k>{key}</k><A>".Length, m.Value.Length - $"<k>{key}</k><A>".Length - $"</A>".Length);
        }
        
        public static dynamic GetGDUserInfo(string savedata) {
            if (savedata == null) savedata = _GMSaveData;

            string statdata = GetKey(savedata, "GS_value");
            return new {
                Name = GetKey(savedata, "playerName"),
                UserID = GetKey(savedata, "playerUserID"),
                Stats = new {
                    Jumps = GetKey(statdata, "1"),
                    Total_Attempts = GetKey(statdata, "2"),
                    Completed_Online_Levels = GetKey(statdata, "4"),
                    Demons = GetKey(statdata, "5"),
                    Stars = GetKey(statdata, "6"),
                    Diamonds = GetKey(statdata, "13"),
                    Orbs = GetKey(statdata, "14"),
                    Coins = GetKey(statdata, "8"),
                    User_Coins = GetKey(statdata, "12"),
                    Killed_Players = GetKey(statdata, "9"),
                    Game_Opened = $"{GetKey(savedata, "bootups")} times"
                }
            };
        }

        public static List<dynamic> GetLevelList(string savedata = null, Action<string, int> callback = null) {
            if (savedata == null) savedata = _LLSaveData;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            List<dynamic> levels = new List<dynamic>();
            string matcher = @"<k>k_\d+<\/k>.+?<\/d>\n? *<\/d>";
            
            foreach (Match lvl in Regex.Matches(savedata, matcher, RegexOptions.Singleline, Regex.InfiniteMatchTimeout)) {
                if (lvl.Value != "") {
                    string Name = GetKey(lvl.Value, "k2", "s");

                    if (callback != null) callback($"Loaded {Name}", 100);

                    levels.Add(new { Name = Name, Data = lvl.ToString() });
                }
            }

            watch.Stop();

            _LevelList = levels;

            return levels;
        }

        private static string ReplaceOfficialSongName(string Song) {
            return (new Dictionary<string, string> {
                { "0", "Stereo Madness" },
                { "1", "Back on Track" },
                { "2", "Polargeist" },
                { "3", "Dry Out" },
                { "4", "Base After Base" },
                { "5", "Cant Let Go" },
                { "6", "Jumper" },
                { "7", "Time Machine" },
                { "8", "Cycles" },
                { "9", "xStep" },
                { "10", "Clutterfunk" },
                { "11", "Theory of Everything" },
                { "12", "Electroman Adventures" },
                { "13", "Clubstep" },
                { "14", "Electrodynamix" },
                { "15", "Hexagon Force" },
                { "16", "Blast Processing" },
                { "17", "Theory of Everything 2" },
                { "18", "Geometrical Dominator" },
                { "19", "Deadlocked" },
                { "20", "Fingerdash" },
            })[Song];
        }

        public static dynamic GetLevelInfo(string name, List<dynamic> from = null) {
            dynamic lvl = null;

            if (name.IndexOf("\\") > -1) {
                lvl = new { Data = name.EndsWith(Ext.LevelCompressed) ? DecompressGZip(File.ReadAllBytes(name)) : File.ReadAllText(name), Name = "" };
            } else {
                foreach (dynamic x in from == null ? _LevelList : from) {
                    if (x.Name == name) {
                        lvl = x;
                        break;
                    }
                }
            }

            if (lvl == null) {
                return null;
            } else {
                int editorTime = Int32.Parse(GetKey(lvl.Data, "k80"));
                string P = GetKey(lvl.Data, "k41");
                string Song = GetKey(lvl.Data, "k8");
                string Desc = Encoding.UTF8.GetString(DecryptBase64(GetKey(lvl.Data, "k3")));
                string Rev = GetKey(lvl.Data, "k46");
                string Copy = GetKey(lvl.Data, "k42");

                return new {
                    Name = lvl.Name == "" ? GetKey(lvl.Data, "k2") : lvl.Name,
                    Length = GetKey(lvl.Data, "k23"),
                    Creator = GetKey(lvl.Data, "k5"),
                    Version = GetKey(lvl.Data, "k16"),
                    Password = P == "1" ? "Free to Copy" : P == "" ? "No copy" : P.Substring(1),
                    Song = Song != "" ? ReplaceOfficialSongName(Song) : GetKey(lvl.Data, "k45"),
                    Description = Desc,
                    Object_count = GetKey(lvl.Data, "k48"),
                    Editor_time = editorTime > 3600 ? $"{Math.Round((float)editorTime / 3600F, 2)}h" : $"{Math.Round((float)editorTime / 60F, 2)}m",
                    Verified = GetKey(lvl.Data, "k14", null),
                    Attempts = GetKey(lvl.Data, "k18"),
                    Revision = Rev == "" ? "None" : Rev,
                    Copied_from = Copy == "" ? "None" : Copy
                };
            }
        }

        public static string ExportLevel(string name, string path = "", bool comp = false) {
            try {
                dynamic lvl = null;

                foreach (dynamic x in _LevelList) {
                    if (x.Name == name) {
                        lvl = x;
                        break;
                    }
                }

                if (lvl == null) {
                    return $"Level {name} not found.";
                } else {
                    string output = $@"{path}\{name}.{(comp ? Ext.LevelCompressed : Ext.Level)}";

                    string NewData = Regex.Replace(lvl.Data, @"<k>k_\d+<\/k>", "");

                    if (comp) {
                        File.WriteAllBytes(output, CompressGZip(Encoding.UTF8.GetBytes(NewData)));
                    } else {
                        File.WriteAllText(output, NewData);
                    }

                    return null;
                }
            } catch (Exception e) {
                return $"Error exporting {name}: {e}.";
            }
        }

        public static string ConvertLvlToGmd(string lvl) {
            lvl = DecompressGZip(Encoding.Unicode.GetBytes(lvl));
            lvl = $"<d>{lvl}</d>";
            return lvl;
        }

        public static string ImportLevel(string path) {
            if (File.Exists(path)) {
                try {
                    string lvl = path.EndsWith(Ext.LevelCompressed) ? "" : File.ReadAllText(path);
                    string data = _LLSaveData;

                    if (path.EndsWith(Ext.LevelAlt)) lvl = ConvertLvlToGmd(lvl);

                    if (path.EndsWith(Ext.LevelCompressed)) lvl = DecompressGZip(File.ReadAllBytes(path));
                    
                    data = Regex.Replace(data, @"<k>k1<\/k><i>\d+?<\/i>", "");
                    string[] splitData = data.Split("<k>_isArr</k><t />");
                    splitData[1] = Regex.Replace(splitData[1], @"<k>k_(\d+)<\/k><d><k>kCEK<\/k>",
                    (Match m) => $"<k>k_{(Int32.Parse((Regex.Match(m.Value, @"k_\d+").Value.Substring(2))) + 1)}</k><d><k>kCEK</k>");
                    data = splitData[0] + "<k>_isArr</k><t /><k>k_0</k>" + lvl + splitData[1];

                    File.WriteAllText($"{_CCDirPath}\\CCLocalLevels.dat", data);

                    return null;
                } catch (Exception e) {
                    return e.ToString();
                }
            } else {
                return "File doesn't exist!";
            }
        }

        public static bool SaveKeyToUserData(string key, string value, string type = "") {
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), $"{_UserDataName}.{Ext.UserData}");
            string NewData = "";
            type = type == "" ? Int32.TryParse(value, out int res) ? "int" : "str" : type;
            if (File.Exists(DataPath)) {
                bool found = false;
                foreach (string line in File.ReadAllLines(DataPath, Encoding.UTF8)) {
                    if (line.StartsWith(key)) {
                        NewData += $"{line.Substring(0, line.IndexOf("=") + 1)}{type}>{value}\n";
                        found = true;
                    } else {
                        NewData += $"{line}\n";
                    }
                }
                if (!found) {
                    NewData += $"{key}={type}>{value}\n";
                }
            } else {
                NewData = $"{key}={type}>{value}\n";
            }
            File.WriteAllText(DataPath, NewData, Encoding.UTF8);
            return true;
        }

        public static bool LoadUserData() {
            string DataPath = Path.Combine(Directory.GetCurrentDirectory(), $"{_UserDataName}.{Ext.UserData}");
            if (File.Exists(DataPath)) {
                foreach (string line in File.ReadAllLines(DataPath, Encoding.UTF8)) {
                    string val = line.Substring(line.IndexOf(">") + 1);
                    switch (line.Substring(0, line.IndexOf("="))) {
                        case "backup-directory":
                            _BackupDirectory = val;
                            break;
                        case "dark-mode":
                            Settings.DarkTheme = Int32.Parse(val) == 1 ? true : false;
                            break;
                        case "compress-backups":
                            Settings.CompressBackups = Int32.Parse(val) == 1 ? true : false;
                            break;
                    }
                }
            }
            return true;
        }

        public class Backups {
            public static bool InitBackups() {
                if (_BackupDirectory == null) _BackupDirectory = GetCCPath("");
                return true;
            }

            public static List<dynamic> GetBackups() {
                List<dynamic> res = new List<dynamic>{};
                foreach (string file in Directory.GetDirectories(_BackupDirectory)) {
                    res.Add(new {
                        Name = file.Substring(file.LastIndexOf("\\") + 1)
                    });
                }
                foreach (string file in Directory.GetFiles(_BackupDirectory)) {
                    if (file.EndsWith($".{Ext.Backup}")) {
                        res.Add(new {
                            Name = file.Substring(file.LastIndexOf("\\") + 1)
                        });
                    }
                }
                return res;
            }

            public static dynamic GetBackupInfo(string path, Action<string, int> callback = null) {
                if (!path.Contains("\\")) {
                    path = $"{_BackupDirectory}\\{path}";
                }
                
                string rem = "";
                if (path.EndsWith($".{Ext.Backup}")) {
                    rem = $"{_BackupDirectory}\\GDTOOLS_TEMP_{new Random().Next(1000)}";
                    ZipFile.ExtractToDirectory(path, rem);
                }

                if (rem != "") path = rem;

                string LLdata = DecodeCCFile($"{path}\\CCLocalLevels.dat", (s, e) => {}, false);
                string GMdata = DecodeCCFile($"{path}\\CCGameManager.dat", (s, e) => {}, false);

                if (rem != "") Directory.Delete(rem, true);

                return new {
                    User = GetGDUserInfo(GMdata),
                    Levels = GetLevelList(LLdata)
                };
            }

            public static string SetBackupLocation(string path) {
                string prevFolder = _BackupDirectory;
                foreach (string folder in Directory.GetDirectories(prevFolder)) {
                    if (File.Exists($"{folder}\\CCLocalLevels.dat") || File.Exists($"{folder}\\CCGameManager.dat")) {
                        Directory.Move(folder, $"{path}\\{folder.Substring(folder.LastIndexOf("\\") + 1)}");
                    }
                }
                foreach (string file in Directory.GetFiles(_BackupDirectory)) {
                    if (file.EndsWith($".{Ext.Backup}")) {
                        File.Move(file, $"{path}\\{file.Substring(file.LastIndexOf("\\") + 1)}");
                    }
                }
                _BackupDirectory = path;
                SaveKeyToUserData("backup-directory", _BackupDirectory);
                return null;
            }

            public static bool CreateNewBackup() {
                var Date = DateTime.Now;
                string Name = $"GDTOOLS_BACKUP_{Date.Year}-{Date.Month}-{Date.Day}";

                int n = 1;
                while (Directory.Exists($"{_BackupDirectory}\\{Name}") || File.Exists($"{_BackupDirectory}\\{Name}.{Ext.Backup}")) {
                    n++;
                    Name = Name.Substring(0, Name.Contains('#') ? Name.IndexOf("#") : Name.Length);
                    Name = $"{Name}#{n}";
                }

                string Path = $"{_BackupDirectory}\\{Name}";

                Directory.CreateDirectory(Path);

                File.Copy(GetCCPath("LocalLevels"), $"{Path}\\CCLocalLevels.dat", true);
                File.Copy(GetCCPath("GameManager"), $"{Path}\\CCGameManager.dat", true);

                if (Settings.CompressBackups) {
                    ZipFile.CreateFromDirectory(Path, $"{Path}.{Ext.Backup}");
                    Directory.Delete(Path, true);
                }

                return true;
            }

            public static bool DeleteBackup(string name) {
                if (name.EndsWith($".{Ext.Backup}")) {
                    File.Delete($"{_BackupDirectory}\\{name}");
                } else {
                    Directory.Delete($"{_BackupDirectory}\\{name}", true);
                }

                return true;
            }

            public static string ImportBackup(string path) {
                if (path.EndsWith($".{Ext.Backup}") || path.EndsWith(".zip")) {
                    File.Move(path, $"{_BackupDirectory}\\{path.Substring(path.LastIndexOf("\\") + 1, path.LastIndexOf(".") - path.LastIndexOf("\\") - 1)}.{Ext.Backup}");

                    return null;
                } else {
                    if (File.Exists($"{path}\\CCLocalLevels.dat") || File.Exists($"{path}\\CCGameManager.dat")) {
                        Directory.Move(path, $"{_BackupDirectory}\\{path.Substring(path.LastIndexOf("\\") + 1)}");

                        return null;
                    } else {
                        return "This appears to not be a backup folder!";
                    }
                }
            }

            public static bool SwitchToBackup(string name) {
                string path = $"{_BackupDirectory}\\{name}";
                
                string rem = "";
                if (path.EndsWith($".{Ext.Backup}")) {
                    rem = $"{_BackupDirectory}\\GDTOOLS_TEMP_{new Random().Next(1000)}";
                    ZipFile.ExtractToDirectory(path, rem);
                }

                if (rem != "") path = rem;

                File.Copy($"{path}\\CCLocalLevels.dat", GetCCPath("LocalLevels"), true);
                File.Copy($"{path}\\CCGameManager.dat", GetCCPath("GameManager"), true);

                if (rem != "") Directory.Delete(rem, true);

                return true;
            }
        }
    }
}