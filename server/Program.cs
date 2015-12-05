﻿using common;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace server
{
    internal class Program
    {
        private static readonly List<HttpListenerContext> currentRequests = new List<HttpListenerContext>();
        private static HttpListener listener;

        internal static SimpleSettings Settings { get; set; }
        internal static XmlData GameData { get; set; }
        internal static Database Database { get; set; }
        internal static string InstanceId { get; set; }

        private static ILog logger { get; } = LogManager.GetLogger("Server");

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Name = "Entry";

            using (Settings = new SimpleSettings("server"))
            using (Database = new Database(
                    Settings.GetValue<string>("db_host", "127.0.0.1"),
                    Settings.GetValue<int>("db_port", "6379"),
                    Settings.GetValue<string>("db_auth", "")))
            {
                GameData = new XmlData();
                InstanceId = Guid.NewGuid().ToString();

                int port = Settings.GetValue<int>("port", "8888");

                listener = new HttpListener();
                listener.Prefixes.Add("http://*:" + port + "/");
                listener.Start();

                listener.BeginGetContext(ListenerCallback, null);
                Console.CancelKeyPress += (sender, e) => e.Cancel = true;
                logger.Info("Listening at port " + port + "...");

                ISManager manager = new ISManager();
                manager.Run();

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

                logger.Info("Terminating...");
                while (currentRequests.Count > 0) ;
                manager.Dispose();
                listener.Stop();
                GameData.Dispose();
            }
        }

        private static void ListenerCallback(IAsyncResult ar)
        {
            try
            {
                if (!listener.IsListening) return;
                var context = listener.EndGetContext(ar);
                listener.BeginGetContext(ListenerCallback, null);
                ProcessRequest(context);
            }
            catch { }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                logger.Info($"Request \"{context.Request.Url.LocalPath}\" from: {context.Request.RemoteEndPoint}");

                if (context.Request.Url.LocalPath.Contains("sfx") || context.Request.Url.LocalPath.Contains("music"))
                {
                    new Sfx().HandleRequest(context);
                    context.Response.Close();
                    return;
                }

                string s;
                if (context.Request.Url.LocalPath.IndexOf(".") == -1)
                    s = "server" + context.Request.Url.LocalPath.Replace("/", ".");
                else
                    s = "server" + context.Request.Url.LocalPath.Remove(context.Request.Url.LocalPath.IndexOf(".")).Replace("/", ".");

                Type t = Type.GetType(s);
                if (t != null)
                {
                    var handler = Activator.CreateInstance(t, null, null);
                    if (!(handler is RequestHandler))
                    {
                        if (handler == null)
                            using (var wtr = new StreamWriter(context.Response.OutputStream))
                                wtr.Write("<Error>Class \"{0}\" not found.</Error>", t.FullName);
                        else
                            using (var wtr = new StreamWriter(context.Response.OutputStream))
                                wtr.Write("<Error>Class \"{0}\" is not of the type RequestHandler.</Error>", t.FullName);
                    }
                    else
                        (handler as RequestHandler).HandleRequest(context);
                }
            }
            catch (Exception e)
            {
                currentRequests.Remove(context);
                using (var writer = new StreamWriter(context.Response.OutputStream))
                    writer.Write(e.ToString());
                logger.Error(e);
            }

            context.Response.Close();
        }
    }
}
