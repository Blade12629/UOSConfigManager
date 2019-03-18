using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Dynamic;
using System.Threading;
using System.Runtime.InteropServices;

namespace UOSConfigManager
{
    class Program
    {
        public static CFG.Config Config;
        public static readonly string UOSStandardPath = @"C:\Program Files (x86)\UOS\Profiles";
        public static readonly string ConfigFile = Directory.GetCurrentDirectory() + @"\config.json";
        #region delegates
        public delegate void ConsoleMessage(string Message, params string[] args);
        public delegate UOS.XML.Profile.Profile LoadProfileDelegate(int ProfileID);
        public delegate UOS.XML.Launcher.Launcher LoadLauncherDelegate();
        public delegate void VoidIntDelegate(int value);
        public delegate void VoidStrDelegate(string value);
        public delegate void SaveFileDelegate(string path, string content);
        public delegate Task TaskStrDelegate(string value);
        public delegate KeyValuePair<System.Windows.Forms.DialogResult, string> DialogDelegate(DialogType type);
        public delegate void OnExceptionDel1(object sender, ThreadExceptionEventArgs args);
        public delegate void OnExceptionDel2(object sender, UnhandledExceptionEventArgs args);
        #endregion
        public static FileInfo[] profiles;
        private static Thread m_formMainThread;
        public static Dictionary<string, string> SpecialKeys;

        #region const vars
        private const string UOSInstaller = "http://uos-update.github.io/UOS_Latest.exe";
        public const string VM = "https://vetus-mundus.de/forum/";
        public const string UOGuide = "http://www.uoguide.com/Main_Page";
        public const string EasyUO = "http://www.easyuo.com/"; //Download only if registered
        public const string LAA = "https://vetus-mundus.de/index.php?attachment/89-large-address-aware-rar/";
        public const string Author = "Skyfly";
        public const string AuthorDiscord = "??????#0284";
        public const string VMHost = "shard.vetus-mundus.de";
        public const int VMPort = 2594;
        #endregion

        /* ToDo:
         * 1 Large Address Aware einbauen (source cutten als one click?)
         * 2 UOS starten ohne launcher form?
         * 3 UOS start button
         * 4 VM verbindungsdaten unter client
         * 5 increment code on macro
         */
        #region consoleEvents
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                    OnExit().Wait();
                    break;
            }
            return true;
        }
        #endregion

        #region main
        static void Main(string[] args)
        {
            using (Sentry.SentrySdk.Init("https://d0723681d2df4184904a44e8aa4a9b1a@sentry.io/1410815"))
            {
                MainTask().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        
        private static Main mainForm;
        
        private static void OnException(object sender, ThreadExceptionEventArgs args)
        {
            CMSG(args.ToString());
            Sentry.SentrySdk.CaptureException(args.Exception);
        }

        private static void OnException(object sender, UnhandledExceptionEventArgs args)
        {
            CMSG(args.ToString());
            Sentry.SentrySdk.CaptureException(new Exception("UnhandledException", new Exception($"IsTerminating: {args.IsTerminating}, Object: {args.ExceptionObject}")));
        }

        private static async Task MainTask()
        {
            try
            {
                ///*Error catching*/
                //System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(OnException);
                //System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
                //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnException);
                /**/

                //Start registering console events so we can catch the close event
                SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

                CMSG("Initializing");

                #region config
                CMSG("Looking for config");
                if (!File.Exists(ConfigFile))
                {
                    CMSG("Config not found");
                    CMSG("Please create one!");
                    /*ToDo: Setup Form*/
                    CMSG("Creating config");
                    Config = new CFG.Config();

                    string awaitPath = null;

                    while (true)
                    {
                        CMSG("Please enter your UOS profile path (e.x.: " + UOSStandardPath + ")");
                        awaitPath = Console.ReadLine();
                        CMSG("Are you sure that ''" + awaitPath + "'' is the correct path?");
                        CMSG("Press 'y' to continue or 'n' to change your path");
                        char keyChar = Console.ReadKey().KeyChar;
                        while (!keyChar.Equals('n') && !keyChar.Equals('N') && !keyChar.Equals('y') && !keyChar.Equals('Y'))
                        {
                            keyChar = Console.ReadKey().KeyChar;
                            await Task.Delay(50);
                        }

                        if (keyChar.Equals('y') || keyChar.Equals('Y'))
                        {
                            if (!Directory.Exists(awaitPath))
                                continue;

                            Config.UOSPath = awaitPath;
                            Console.WriteLine();
                            CMSG("UOS path set to " + awaitPath);
                            CMSG("Saving config");
                            SaveConfig();
                            CMSG("Config saved");
                            break;
                        }
                    }

                    CMSG("Config created");
                }
                CMSG("Loading config");
                LoadConfig();
                CMSG("Config loaded");
                #endregion

                #region specialKeys
                CMSG("Looking for special keys");
                FileInfo specialKeysFI = new FileInfo("sk.txt");
                SpecialKeys = new Dictionary<string, string>();

                if (specialKeysFI.Exists)
                {
                    CMSG("Special keys found, loading");
                    using (StreamReader sr = new StreamReader(specialKeysFI.OpenRead()))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] split = line.Split('|');

                            if (split.Length <= 1)
                                continue;

                            SpecialKeys.Add(split[0], split[1]);
                        }
                    }
                    CMSG("Done loading special keys");
                }
                else
                    CMSG("Special keys not found");
                #endregion

                #region Profiles
                CMSG("Loading profiles");
                loadProfilesInfos(Config.UOSPath);
                Console.WriteLine("————————————————————————");
                CMSG($"Found {profiles.Count()} profiles");
                for (int i = 0; i < profiles.Count(); i++)
                    CMSG($"ID: {i}, profile: {profiles[i].Name}");

                Console.WriteLine("————————————————————————");
                CMSG("Loaded profiles");
                #endregion

                #region UserInterface
                ThreadStart ts = new ThreadStart(() =>
                {
                    CMSG("Initializing user interface");
                    GUIHelper_Initialize().Wait();
                    mainForm = new Main();
                    mainForm.FormClosed += (arg, sender) =>
                    {
                        OnExit().Wait();
                    };
                    mainForm.Shown += (sender, arg) => Task.Run(async () => await OnFormShown());
                    System.Windows.Forms.Application.Run(mainForm);

                    System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
                    System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(new OnExceptionDel1(OnException));
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(new OnExceptionDel2(OnException));

                    CMSG($"User interface initialized!{Environment.NewLine}Done loading, ready to go ({DateTime.Now.ToString()})");
                });
                m_formMainThread = new Thread(ts);

                m_formMainThread.SetApartmentState(ApartmentState.STA);
                m_formMainThread.Start();
                #endregion
            }
            catch (Exception ex)
            {
                CMSG(ex.ToString());
                Sentry.SentrySdk.CaptureException(ex);
            }
            
            await Task.Delay(-1);
        }

        public static async Task OnExit()
        {
            CMSG("Closing GUIHelper thread");
            m_GuiHelper.Abort();

            //CMSG("Exiting in 3 seconds");
            //await Task.Delay(3000);
            Environment.Exit(0);
        }

        private static async Task OnFormShown()
        {
            ConsoleMessage c = new ConsoleMessage(CMSG);
            c.Invoke($"Loading profile {Config.DefaultProfileID}");
        }
        #endregion

        #region methods

        public static void SaveFile(string path, string content)
            => File.WriteAllText(path, content);

        public static void LoadConfig()
        {
            string json = File.ReadAllText(ConfigFile);
            Config = JsonConvert.DeserializeObject<CFG.Config>(json);
        }

        public static void SaveConfig()
        {
            string json = JValue.Parse(JsonConvert.SerializeObject(Config)).ToString(Newtonsoft.Json.Formatting.Indented);

            if (File.Exists(ConfigFile))
                File.Delete(ConfigFile);

            File.WriteAllText(ConfigFile, json);
        }

        private static void loadProfilesInfos(string path)
            => profiles = GetProfiles(path);

        public static void StartUOS()
        {
            //Set uos path
            CMSG("Starting UOS");
            DirectoryInfo dinfo = null;
            DirectoryInfo dinfos = dinfo.Parent;
            System.Diagnostics.Process.Start(dinfo.FullName + @"\UOS.exe");
        }

        public static async Task InstallUOS(string path)
        {
            CMSG("Downloading UOS");
            using (System.Net.WebClient wc = new System.Net.WebClient())
                wc.DownloadFile(UOSInstaller, path);

            CMSG("Starting UOS Installer");
            System.Diagnostics.Process.Start(path);
        }

        public static UOS.XML.Profile.Profile LoadProfile(int ProfileID)
        {
            UOS.UOSReader reader = new UOS.UOSReader(profiles.ElementAt(ProfileID));
            return reader.Read().Result as UOS.XML.Profile.Profile;
        }
        
        public static UOS.XML.Launcher.Launcher LoadLauncher()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Config.UOSPath).Parent;

            UOS.UOSReader reader = new UOS.UOSReader(new FileInfo(dirInfo.FullName + @"\launcher.xml"), UOS.UOSReader.ConfigType.Launcher);
            return reader.Read().Result as UOS.XML.Launcher.Launcher;
        }

        public static string ConvertVirtualKey(string Val)
        {
            string result = null;


            if (SpecialKeys.TryGetValue(Val, out string value))
                result = value;
            else
                result = new System.Windows.Forms.KeysConverter().ConvertToString(Program.ParseHex(Val));

            return result;
        }

        public static void CMSG(object obj)
            => CMSG(obj.ToString(), null);
        public static void CMSG(string message, params string[] args)
        {
            Task.Run(() =>
            {
                File.AppendAllText(Directory.GetCurrentDirectory() + $@"\Log_{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}.txt", $"{DateTime.Now}: {message}{Environment.NewLine}");
            });
            Console.WriteLine($"{DateTime.Now}: {message}", args);
        }

        public static int ParseHex(string Val)
        {
            int newVal = 0;
            try
            {

            if (Val.StartsWith("0x"))
            {
                string ValNew = Val.Remove(0, 2);
                newVal = int.Parse(ValNew, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            else
                newVal = int.Parse(Val, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (Exception ex)
            { 
                Console.WriteLine("Val:" + Val);
                Sentry.SentrySdk.CaptureException(ex);
            }
            return newVal;
        }
        public static string ParseHex(int Val)
        {
            string result = Val.ToString("X");

            return result;
        }

        private static FileInfo[] GetProfiles(string path)
            => new DirectoryInfo(path).EnumerateFiles().Where(f => f.Extension.Contains("xml")).ToArray();
        private static FileInfo[] GetBackups(string path)
            => new DirectoryInfo(path).EnumerateFiles().Where(f => f.Extension.Contains("backup")).ToArray();


        public enum DialogType
        {
            File,
            Folder
        }
        
        public static KeyValuePair<System.Windows.Forms.DialogResult, string> Dialog(DialogType type)
        {
            DialogState ds = new DialogState();

            bool done = false;
            Action ac = new Action(() =>
            {
                ds.ThreadProcShowDialog(type);
                done = true;
            });

            EnqueueAndAwait(ac, ref done);

            System.Windows.Forms.DialogResult dr = ds.result;
            string result = ds.return_;

            return new KeyValuePair<System.Windows.Forms.DialogResult, string>(dr, result);
        }

        public class DialogState
        {
            public System.Windows.Forms.DialogResult result;
            public object ShownDialog;
            public DialogType DialogType;
            public string return_;

            public void ThreadProcShowDialog(DialogType dtype)
            {
                DialogType = dtype;

                switch (dtype)
                {
                    case DialogType.Folder:
                        System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                        result = dialog.ShowDialog();
                        ShownDialog = dialog;
                        return_ = dialog.SelectedPath;
                        break;
                    case DialogType.File:
                        System.Windows.Forms.SaveFileDialog dialog2 = new System.Windows.Forms.SaveFileDialog();
                        result = dialog2.ShowDialog();
                        ShownDialog = dialog2;
                        return_ = dialog2.FileName;
                        break;
                }
            }

            public void Dispose()
            {
                switch (DialogType)
                {
                    case DialogType.Folder:
                        if (ShownDialog == null)
                            break;
                        System.Windows.Forms.FolderBrowserDialog fbd = ShownDialog as System.Windows.Forms.FolderBrowserDialog;
                        fbd.Dispose();
                        break;
                    case DialogType.File:
                        if (ShownDialog == null)
                            break;
                        System.Windows.Forms.SaveFileDialog sfd = ShownDialog as System.Windows.Forms.SaveFileDialog;
                        sfd.Dispose();
                        break;
                }
            }

        }

        #endregion

        #region guiHelperThread
        private static Thread m_GuiHelper;
        public static Queue<Action> GuiHelper_Queue;
        private static ThreadState m_GuiHelper_State { get { return m_GuiHelper == null ? ThreadState.Unstarted : m_GuiHelper.ThreadState; } }

        private static async Task GUIHelper_Initialize()
        {
            CMSG("Initializing GUIHelper thread");
            GuiHelper_Queue = new Queue<Action>();

            ThreadStart ts = GUIHelper_Main();

            m_GuiHelper = new Thread(ts)
            {
                IsBackground = true,
                Name = "UOSConfigManager_GuiHelper",
            };

            //So we can do STA/OLE calls like ShowDialog();
            m_GuiHelper.SetApartmentState(ApartmentState.STA);

            m_GuiHelper.Start();

            CMSG("GUIHelper thread initialized and running");
        }
        
        private static ThreadStart GUIHelper_Main()
        {
            ThreadStart ts = new ThreadStart(() =>
            {
                while (true)
                {
                    if (GuiHelper_Queue.Count > 0)
                    {
                        Action toExecute = GuiHelper_Queue.Dequeue();
                        toExecute.Invoke();
                    }

                    Task.Delay(25).Wait();
                }
            });
            
            return ts;
        }
        public static void EnqueueAndAwait(Action ac, ref bool waitFor)
        {
            GuiHelper_Queue.Enqueue(ac);

            while (!waitFor)
                Task.Delay(5);
        }
        #endregion
    }

    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null || action == null)
                return;

            foreach (T ienum in enumerable)
                action(ienum);
        }
    }
}
