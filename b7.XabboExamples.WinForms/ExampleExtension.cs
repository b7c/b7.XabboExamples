﻿using System;
using System.Reflection;

using Xabbo.GEarth;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.XabboExamples.WinForms
{
    public class ExampleExtension : GEarthExtension
    {
        private static readonly GEarthOptions _options = new GEarthOptions
        {
            Title = "Xabbo.GEarth WinForms",
            Description = "example extension using the Xabbo framework",
            Author = "b7",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?",
            ShowEventButton = true
        };

        public event Action<string> LogMessage;

        public bool EnablePacketManipulation { get; set; }
        public bool EnablePacketBlocking { get; set; }

        public ExampleExtension(int port)
            : base(_options, port)
        { }

        private void Log(string message) => LogMessage?.Invoke(message);

        protected override void OnInterceptorConnected(object sender, EventArgs e)
        {
            base.OnInterceptorConnected(sender, e);
            Log("Connected to G-Earth.");
        }

        protected override void OnInterceptorInitialized(object sender, EventArgs e)
        {
            base.OnInterceptorInitialized(sender, e);
            Log("Extension initialized by G-Earth.");
        }

        protected override void OnClicked(object sender, EventArgs e)
        {
            base.OnClicked(sender, e);
            Log("Extension was clicked in G-Earth.");
        }

        protected override void OnGameConnected(object sender, GameConnectedEventArgs e)
        {
            /*
                The base call is where this ExampleExtension instance
                gets bound to the InterceptDispatcher, which routes
                packets to methods decorated with InterceptIn/Out attributes (see below).
            */
            base.OnGameConnected(sender, e);

            Log("Game connection established.\r\n\r\n"
                + $"               Host: {e.Host}\r\n"
                + $"               Port: {e.Port}\r\n"
                + $"  Client identifier: {e.Port}\r\n"
                + $"        Client type: {e.ClientType}\r\n"
                + $"     Client version: {e.ClientVersion}\r\n"
                + $"      Message infos: {e.Messages.Count:N0}\r\n"
            );
        }

        protected override void OnIntercepted(object sender, InterceptArgs e)
        {
            base.OnIntercepted(sender, e);

            // Do something with all intercepted packets here.
        }

        protected override void OnGameDisconnected(object sender, EventArgs e)
        {
            base.OnGameDisconnected(sender, e);
            Log("Game connection ended.");
        }

        protected override void OnInterceptorDisconnected(object sender, EventArgs e)
        {
            base.OnInterceptorDisconnected(sender, e);
            Log("Connection with G-Earth lost.");
        }

        public void InjectPacketClient()
        {
            /*
                Sends a Chat packet to the client.
                Use the Send method to send packets to the server or client.
                The destination is determined by the message header; as we are using an
                incoming message header (In.Chat), the packet will be sent to the client.
            */

            Send(In.Chat, -1, "Hello from the Xabbo.GEarth example!", 0, 2, 0, 0);
            Log("Sent chat packet to client.");
        }

        public void InjectPacketServer()
        {
            // Sends a Chat packet to the server.
            Send(Out.Chat, "Hello, world", 0, -1);
            Log("Sent chat packet to server.");
        }

        [InterceptIn(nameof(Incoming.Chat), nameof(Incoming.Shout))]
        private void OnInterceptChat(InterceptArgs e)
        {
            // Changes incoming messages to upper-case if enabled.
            if (EnablePacketManipulation)
            {
                // Replaces a string after the first int (4 bytes)
                // in the packet using a transform function.
                e.Packet.ReplaceString(s => s.ToUpper(), 4);
            }
        }

        [InterceptOut(nameof(Outgoing.Move))]
        private void OnInterceptMove(InterceptArgs e)
        {
            if (EnablePacketBlocking)
            {
                // Flags the packet to be blocked, this cannot be reversed.
                e.Block();

                int x = e.Packet.ReadInt(),
                    y = e.Packet.ReadInt();

                Log($"Blocked move packet to ({x}, {y}).");
            }
        }
    }
}
