using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ReplayConverter
{
    public static class Utils
    {
        public static string ReadSmString(this BinaryReader reader)
        {
            var length = reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static TimeSpan ParseTime(string time)
        {
            var minuteSplit = time.IndexOf( ':' );
            var minutes = int.Parse( time.Substring( 0, minuteSplit ) );
            var seconds = double.Parse( time.Substring( minuteSplit + 1 ) );

            return TimeSpan.FromSeconds( minutes * 60 + seconds );
        }
    }
}
