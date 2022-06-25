// By https://github.com/Tr-st-n
namespace GitProfileSwitcher
{
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Text;

    internal class Program
    {
        private const string DATA_FILE_NAME = "data.json";

        static void Main()
        {
            var provider = new DataProvider(DATA_FILE_NAME);
            bool exit = false;
            do
            {
                bool invalidInput = false;
                do
                {
                    if (invalidInput)
                    {
                        Console.WriteLine("\ninvalid input...\n");
                    }
                    invalidInput = false;
                    WriteProfileInfoToConsole(provider);
                    Console.WriteLine(
                        "\nSelect an option:\n" +
                        "1) Update current global git config user (by #).\n" +
                        "2) ADD new saved user profile.\n" +
                        "3) REMOVE saved user profile (by #).\n" +
                        "4) UPDATE saved user profile (by #).\n" +
                        "Press ESC to exit program.\n");
                    var key = Console.ReadKey().Key;
                    switch (key)
                    {
                        // UPDATE CURRENT GLOBAL GIT CONFIG USER
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            {
                                if (provider.Profiles.Count < 1)
                                {
                                    Console.WriteLine("\nYou have no profiles to select.\n");
                                    break;
                                }
                                Console.WriteLine("\nSelect profile by # to set as global git config user...\n");
                                var profile = GetProfileByNumber(provider);
                                if (profile == null)
                                {
                                    Console.WriteLine("\nNo profile selected.\n");
                                }
                                else
                                {
                                    GitConfigProviderWindows.CurrentGlobalUser = profile;
                                    Console.WriteLine("\nSet Current Global Git Config Profile.\n");
                                }
                                break;
                            }
                        // ADD NEW SAVED USER PROFILE
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.D2:
                            {
                                provider.Profiles.Add(
                                    new Profile(
                                        GetProfileStrIpt("name"),
                                        GetProfileStrIpt("email")));
                                provider.WriteData();
                                Console.WriteLine("\nNew Profile Saved.\n");
                                break;
                            }
                        // REMOVE A SAVED USER PROFILE
                        case ConsoleKey.NumPad3:
                        case ConsoleKey.D3:
                            {
                                if (provider.Profiles.Count < 1)
                                {
                                    Console.WriteLine("\nYou have no profiles to remove.\n");
                                    break;
                                }
                                Console.WriteLine("\nRemove profile by #...\n");
                                var profile = GetProfileByNumber(provider);
                                if (profile == null)
                                {
                                    Console.WriteLine("\nNo profile selected.\n");
                                }
                                else
                                {
                                    provider.Profiles.Remove(profile);
                                    provider.WriteData();
                                    Console.WriteLine("\nProfile Removed.\n");
                                }
                                break;
                            }
                        // UPDATE A SAVED USER PROFILE
                        case ConsoleKey.NumPad4:
                        case ConsoleKey.D4:
                            {
                                if (provider.Profiles.Count < 1)
                                {
                                    Console.WriteLine("\nYou have no profiles to update.\n");
                                    break;
                                }
                                Console.WriteLine("\nUpdate profile by #...\n");
                                var profile = provider.Profiles
                                    .Where(x => x.Same(GetProfileByNumber(provider))).FirstOrDefault();
                                if (profile == null)
                                {
                                    Console.WriteLine("\nNo profile found.\n");
                                }
                                else
                                {
                                    profile = new Profile(
                                        GetProfileStrIpt("name"),
                                        GetProfileStrIpt("email"));
                                    provider.WriteData();
                                    Console.WriteLine("\nProfile Updated.\n");
                                }
                                break;
                            }
                        case ConsoleKey.Escape:
                            {
                                exit = true;
                                break;
                            }
                        // INVALID INPUT
                        default:
                            {
                                invalidInput = true;
                                break;
                            }
                    }
                } while (invalidInput);
            } while (!exit);
        }

        private static string GetProfileStrIpt(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException($"Argument {fieldName} should not be null or white space.");
            }
            bool iptErr = false;
            do
            {
                iptErr = false;
                Console.Write($"\nEnter a profile {fieldName}: ");
                var ipt = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ipt))
                {
                    Console.WriteLine("Invalid input: cannot be null or white space.");
                    iptErr = true;
                }
                else
                {
                    return ipt.Trim();
                }
            } while (iptErr);
            throw new Exception("WTF!? This should never get hit.");
        }

        private static Profile GetProfileByNumber(DataProvider provider)
        {
            bool iptErr = false;
            do
            {
                iptErr = false;
                Console.Write($"\nEnter a # (or type \"cancel\"): ");
                var ipt = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ipt))
                {
                    Console.WriteLine("Invalid input: cannot be null or white space.");
                    iptErr = true;
                }
                else
                {
                    if (ipt.ToLower() == "cancel")
                    {
                        return null;
                    }
                    if (int.TryParse(ipt, out int iptNum))
                    {
                        if (iptNum < 1 || iptNum > provider.Profiles.Count)
                        {
                            Console.WriteLine($"Invalid input: number \"{iptNum}\" is not a valid profile number.");
                            iptErr = true;
                        }
                        else
                        {
                            return provider.Profiles[iptNum - 1];
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid input: \"{ipt}\" is not an integer value.");
                        iptErr = true;
                    }
                }
            } while (iptErr);
            throw new Exception("WTF!? This should never get hit.");
        }

        private static void WriteProfileInfoToConsole(DataProvider provider)
        {
            var strBldr = new StringBuilder("\nUsers to select from:\n");
            for (int i = 0; i < provider.Profiles.Count; i++)
            {
                var profile = provider.Profiles[i];
                strBldr.AppendLine($"{i+1})\n name = \"{profile.Name}\" \n email = \"{profile.Email}\"\n");
            }
            Console.WriteLine(strBldr.ToString());
            var currentUser = GitConfigProviderWindows.CurrentGlobalUser;
            Console.WriteLine($"\nCurrent user is:\n name = \"{currentUser.Name}\" \n email = \"{currentUser.Email}\"\n");
        }
    }

    public static class GitConfigProviderWindows
    {
        private const string NAME_CMD = "git config --global user.name";

        private const string EMAIL_CMD = "git config --global user.email";

        public static Profile CurrentGlobalUser
        {
            get
            {
                return new Profile(
                    CmdExeIO(NAME_CMD),
                    CmdExeIO(EMAIL_CMD));
            }
            set
            {
                CmdExeIO(NAME_CMD + " " + value.Name);
                CmdExeIO(EMAIL_CMD + " " + value.Email);
            }
        }

        private static string CmdExeIO(string cmdStr)
        {
            var p = new Process();
            var strBldr = new StringBuilder();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = @"/c " + cmdStr;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.OutputDataReceived += (a, b) =>
            {
                if (!string.IsNullOrWhiteSpace(b?.Data))
                {
                    strBldr.Append(b.Data?.Trim());
                }
            };
            p.ErrorDataReceived += (a, b) =>
            {
                if (!string.IsNullOrWhiteSpace(b?.Data))
                {
                    Console.WriteLine($"cmd.exe ERROR: {b.Data}");
                }
            };
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            return strBldr.ToString();
        }
    }

    public class DataProvider
    {
        public DataProvider(string dataFileName)
        {
            if (string.IsNullOrWhiteSpace(dataFileName))
            {
                throw new ArgumentException($"Argument {nameof(dataFileName)} cannot be null or empty");
            }
            _dataFileName = dataFileName;
            ReadData();
        }

        private string _dataFileName;

        public List<Profile> Profiles { get; set; }

        public void ReadData()
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(_dataFileName);
            }
            catch (FileNotFoundException)
            {
                Profiles = new List<Profile>();
                WriteData();
                reader = new StreamReader(_dataFileName);
            }
            using var strmRdr = reader;
            using var jsonRdr = new JsonTextReader(strmRdr);
            var jsonSer = new JsonSerializer();
            var data = jsonSer.Deserialize<List<Profile>>(jsonRdr);
            Profiles = data!;
        }

        public void WriteData()
        {
            using var strmWtr = new StreamWriter(_dataFileName);
            using var jsonWtr = new JsonTextWriter(strmWtr);
            var jsonSer = new JsonSerializer();
            jsonSer.Serialize(jsonWtr, Profiles);
            jsonWtr.Flush();
        }
    }

    public class Profile
    {
        public Profile(string name, string email)
        {
            Name = name;
            Email = email;
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Same(Profile profile) => profile.Name == Name && profile.Email == Email;
    }
}