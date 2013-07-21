using Coob.System.FileEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Coob
{
    class Root
    {
        public static Coob Coob;
        public static IScriptHandler Scripting;
        public static HookHandler Hooks;

        static void Main(string[] args)
        {
            Console.Title = "Coob";
            Console.TreatControlCAsInput = true;
            Log.Info("Starting Coob.");

            DefaultCoobOptions defopt = new DefaultCoobOptions();

            iniFile config = new iniFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
            if (File.Exists("config.ini"))
            {
                Log.Info("Read file config.ini ...");
                defopt.Port = Convert.ToInt32(config.IniReadValue("Settings", "Port"));
                defopt.WorldSeed = Convert.ToInt32(config.IniReadValue("Settings", "WorldSeed"));
            }
            else
            {
                Log.Warning("File config.ini doesn't exist");
                Log.Info("Creating file config.ini ...");
                File.Create("config.ini").Close();
                config.IniWriteValue("Settings", "Port", defopt.Port.ToString());
                config.IniWriteValue("Settings", "WorldSeed", defopt.WorldSeed.ToString());
            }
            Log.Info("Port set to " + defopt.Port);
            Log.Info("WorldSeed set to " + defopt.WorldSeed);

            Coob = new Coob(defopt);

            Scripting = new JavascriptEngine();

            Scripting.Initialize();
            Scripting.Load("Coob.java");

            Scripting.SetParameter("coob", Root.Coob);

            Scripting.SetFunction("LogInfo", (Action<string>)Log.Info);
            Scripting.SetFunction("LogWarning", (Action<string>)Log.Warning);
            Scripting.SetFunction("LogError", (Action<string>)Log.Error);

            Hooks = new HookHandler();
            Hooks.ScriptHandlers.Add(Scripting);

            Scripting.Run();

            if (!Scripting.CallFunction<bool>("onInitialize"))
                return;

            Coob.StartMessageHandler();

            while (Coob.Running)
            {
                var input = Console.ReadLine().ToLower();

                if (input == "exit") // Temporary way to quit server properly. Seems to fuck up because the console hates threading.
                    Coob.StopMessageHandler();
            }

            Log.Info("Stopping server...");
            Scripting.CallFunction("onQuit");

            Log.Display(); // "Needs" to be called here since it normally gets called in the message handler (which isn't called anymore since server stopped).
        }
    }
}
