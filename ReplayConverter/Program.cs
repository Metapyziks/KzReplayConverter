using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReplayConverter.Gokz;
using ReplayConverter.KzTimer;

namespace ReplayConverter
{
    class Program
    {
        private static readonly Regex _sOptionRegex = new Regex(@"^-(-(?<option>[a-z0-9_-]+)|(?<option>[a-z]))\s*(=(?<value>.*))?");

        static void Main( string[] args )
        {
            var mapName = "[unknown]";
            string outdir = null;

            foreach ( var arg in args )
            {
                var match = _sOptionRegex.Match( arg );
                if ( match.Success )
                {
                    var value = match.Groups["value"].Success ? match.Groups["value"].Value : null;
                    switch ( match.Groups["option"].Value )
                    {
                        case "m":
                        case "map":
                            mapName = value;
                            break;
                        case "o":
                        case "outdir":
                            outdir = value;
                            break;
                    }

                    continue;
                }

                var fullPath = new FileInfo( arg ).FullName;
                var directory = Path.GetDirectoryName( fullPath );
                var fileName = Path.GetFileNameWithoutExtension( fullPath );

                var dest = Path.Combine( outdir ?? directory, $"{fileName}.replay" );

                ConvertReplay( arg, dest, mapName );
            }
        }

        static void ConvertReplay( string srcPath, string dstPath, string mapName )
        {
            KzTimer.ReplayFile srcReplay;
            using ( var srcStream = File.OpenRead( srcPath ) )
            {
                srcReplay = new KzTimer.ReplayFile( srcStream );
            }

            var dstReplay = new Gokz.ReplayFile
            {
                FormatVersion = 1,
                PluginVersion = "",
                MapName = mapName,
                Course = -1,
                Mode = GlobalMode.KzTimer,
                Style = GlobalStyle.Normal,
                Time = srcReplay.Time,
                TeleportsUsed = srcReplay.Ticks.Count( x => (x.AdditionalFields & AdditionalField.TeleportedOrigin) != 0 ),
                SteamId = -1,
                SteamId2 = srcReplay.SteamId,
                PlayerName = srcReplay.Name
            };

            var pos = srcReplay.InitialPosition;

            const int tickRate = 128;
            const float dt = 1f / tickRate;

            foreach ( var srcTick in srcReplay.Ticks )
            {
                var dstTick = new Gokz.TickData();

                if ( (srcTick.AdditionalFields & AdditionalField.TeleportedOrigin) != 0)
                {
                    pos = srcTick.AdditionalTeleport.Origin;
                }

                dstTick.Buttons = srcTick.Buttons;
                dstTick.Angles = srcTick.PredictedAngles;
                dstTick.Position = pos;

                pos += srcTick.AbsoluteVelocity * dt;

                dstReplay.Ticks.Add( dstTick );
            }

            var dir = Path.GetDirectoryName( dstPath );
            if ( !Directory.Exists( dir ) ) Directory.CreateDirectory( dir );

            using ( var dstStream = File.Create( dstPath ) )
            {
                dstReplay.Write( dstStream );
            }
        }
    }
}
