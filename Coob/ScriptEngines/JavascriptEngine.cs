﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint;
using Jint.Native;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Net;

namespace Coob
{
    public class JavascriptEngine : IScriptHandler
    {
        private JintEngine engine;
        private string source;

        public JavascriptEngine()
        {
            engine = new JintEngine();
            source = "";
        }

        public void Initialize()
        {
            engine.AddPermission(new UIPermission(PermissionState.Unrestricted));
            engine.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            engine.AddPermission(new WebPermission(PermissionState.Unrestricted));

            engine.SetFunction("CreateThread", new Func<JsFunction, Thread>(CreateThread));
        }

        public void Load(string sourceFile)
        {
            source += File.ReadAllText(sourceFile);
        }

        public void LoadPlugin(string pluginName, string sourceFile)
        {
            Log.Info("Loading Javascript plugin \"{0}\"", pluginName);

            string pluginSource = File.ReadAllText(sourceFile);
            pluginSource = PreprocessIncludes(pluginSource, Path.GetDirectoryName(sourceFile));

            source += pluginSource;
        }

        public string PreprocessIncludes(string source, string directory)
        {
            foreach (var line in source.Split('\n'))
            {
                string trimmed = line.Trim();
                if (!trimmed.StartsWith("#include \"")) continue;
                if (trimmed.Length < 11) continue;

                string includeName = trimmed.Substring(10, trimmed.Length - 10 - 1);

                string includePath = Path.Combine(directory, includeName);
                if (File.Exists(includePath))
                {
                    source = source.Replace(trimmed, PreprocessIncludes(File.ReadAllText(includePath), directory));
                }
            }
            return source;
        }

        public void SetParameter(string name, object value)
        {
            engine.SetParameter(name, value);
        }

        public void SetFunction(string name, Delegate function)
        {
            engine.SetFunction(name, function);
        }

        public void RunString(string code)
        {
            try
            {
                engine.Run(code);
            }
            catch (JintException ex)
            {
                Log.Error(ex.InnerException != null ? (ex.Message + ": " + ex.InnerException.Message) : ex.Message);
                Log.Display();
                //Environment.Exit(1);
            }
        }

        public T CallFunction<T>(object function, params object[] arguments)
        {
            try
            {
                if (function is JsFunction)
                    return (T)engine.CallFunction((JsFunction)function, arguments);
                else
                    return (T)engine.CallFunction((string)function, arguments);
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }

            return default(T);
        }

        public void CallFunction(object function, params object[] arguments)
        {
            try
            {
                if (function is JsFunction)
                    engine.CallFunction((JsFunction)function, arguments);
                else
                    engine.CallFunction((string)function, arguments);
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }
        }

        public string GetScriptDirectoryName()
        {
            return "Javascript";
        }

        public string GetScriptExtension()
        {
            return ".js";
        }

        public string GetEntryFileName()
        {
            return "main";
        }

        private void WriteException(Exception exception)
        {
            Log.Error(exception);
        }

        public void Run()
        {
            RunString(source);
        }

        Thread CreateThread(JsFunction func)
        {
            return new Thread(() => engine.CallFunction(func));
        }
    }
}
