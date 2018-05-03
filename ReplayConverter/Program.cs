using System;
using System.IO;
using System.Linq;
using ReplayConverter.Gokz;
using ReplayConverter.KzTimer;

namespace ReplayConverter
{
    class Program
    {
        static void Main( string[] args )
        {
            foreach ( var arg in args )
            {
                var fullPath = new FileInfo( arg ).FullName;
                var directory = Path.GetDirectoryName( fullPath );
                var fileName = Path.GetFileNameWithoutExtension( fullPath );

                var dest = Path.Combine( directory, $"{fileName}.replay" );

                ConvertReplay( arg, dest, "kz_reach_v2" );
            }
        }

        static void ConvertReplay( string srcPath, string dstPath, string mapName )
        {
            KzTimer.ReplayFile srcReplay;
            using ( var srcStream = File.OpenRead( srcPath ) )
            {
                srcReplay = new KzTimer.ReplayFile( srcStream );
            }

            var dstReplay = new Gokz.ReplayFile();

            dstReplay.FormatVersion = 1;
            dstReplay.PluginVersion = "";
            dstReplay.MapName = mapName;
            dstReplay.Course = -1;
            dstReplay.Mode = GlobalMode.KzTimer;
            dstReplay.Style = GlobalStyle.Normal;
            dstReplay.Time = srcReplay.Time;
            dstReplay.TeleportsUsed = srcReplay.Ticks.Count( x => (x.AdditionalFields & AdditionalField.TeleportedOrigin) != 0 );
            dstReplay.SteamId = -1;
            dstReplay.SteamId2 = srcReplay.SteamId;
            dstReplay.PlayerName = srcReplay.Name;

            var pos = srcReplay.InitialPosition;
            var dt = 1f / 128f;

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

            using ( var dstStream = File.Create( dstPath ) )
            {
                dstReplay.Write( dstStream );
            }
        }
    }
}
