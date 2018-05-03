using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ReplayConverter.KzTimer
{
    [Flags]
    public enum AdditionalField
    {
        TeleportedOrigin = 1,
        TeleportedAngles = 2,
        TeleportedVelocity = 4
    }

    public class AdditionalTeleport
    {
        public Vector3 Origin;
        public Vector3 Angles;
        public Vector3 Velocity;
        public AdditionalField Flags;
    }

    public class TickData
    {
        public Button Buttons;
        public int Impulse;
        public Vector3 AbsoluteVelocity;
        public Vector3 PredictedVelocity;
        public Vector2 PredictedAngles;
        public int NewWeapon;
        public int PlayerSubType;
        public int PlayerSeed;
        public AdditionalField AdditionalFields;
        public bool Pause;

        public AdditionalTeleport AdditionalTeleport;
    }

    public class ReplayFile
    {
        public static readonly uint Magic = 0xBAADF00D;

        public byte FormatVersion;
        public string SteamId;
        public TimeSpan Time;
        public string Name;
        public int Checkpoints;
        public Vector3 InitialPosition;
        public Vector2 InitialAngles;

        public readonly List<TickData> Ticks = new List<TickData>();

        public ReplayFile() { }

        public ReplayFile(Stream stream)
        {
            var reader = new BinaryReader(stream);

            var magic = reader.ReadUInt32();
            if (magic != Magic)
            {
                throw new Exception("Unrecognised replay file format.");
            }

            FormatVersion = reader.ReadByte();
            reader.ReadSmString();
            SteamId = reader.ReadSmString();
            Time = Utils.ParseTime(reader.ReadSmString());
            Name = reader.ReadSmString();
            Checkpoints = reader.ReadInt32();
            InitialPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            InitialAngles = new Vector2(reader.ReadSingle(), reader.ReadSingle());

            var tickCount = reader.ReadInt32();

            Ticks.Clear();

            for (var i = 0; i < tickCount; ++i)
            {
                TickData tick;
                Ticks.Add(tick = new TickData
                {
                    Buttons = (Button) reader.ReadInt32(),
                    Impulse = reader.ReadInt32(),
                    AbsoluteVelocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    PredictedVelocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    PredictedAngles = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    NewWeapon = reader.ReadInt32(),
                    PlayerSubType = reader.ReadInt32(),
                    PlayerSeed = reader.ReadInt32(),
                    AdditionalFields = (AdditionalField) reader.ReadInt32(),
                    Pause = reader.ReadInt32() == 1
                });

                if (tick.AdditionalFields != 0)
                {
                    tick.AdditionalTeleport = new AdditionalTeleport();

                    if ((tick.AdditionalFields & AdditionalField.TeleportedOrigin) != 0)
                    {
                        tick.AdditionalTeleport.Origin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((tick.AdditionalFields & AdditionalField.TeleportedAngles) != 0)
                    {
                        tick.AdditionalTeleport.Angles = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    if ((tick.AdditionalFields & AdditionalField.TeleportedVelocity) != 0)
                    {
                        tick.AdditionalTeleport.Velocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    }

                    tick.AdditionalTeleport.Flags = tick.AdditionalFields;
                }
            }
        }

        public void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
