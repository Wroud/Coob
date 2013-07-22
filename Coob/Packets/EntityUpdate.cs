using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coob.Structures;
using System.IO;

namespace Coob.Packets
{
    public partial class Packet
    {
        public class EntityUpdate : Base
        {
            public Entity Entity;
            public Entity Changes;
            public long UpdateBitmask;
            public bool IsJoin;

            public EntityUpdate(Entity entity, Entity changes, bool join, Client client)
                : base(client)
            {
                this.Entity = entity;
                this.Changes = changes;
                this.IsJoin = join;
            }

            public static Base Parse(Client client)
            {
                int length = client.Reader.ReadInt32();

                byte[] compressedData = client.Reader.ReadBytes(length);
                byte[] maskedData = ZlibHelper.UncompressBuffer(compressedData);

                Entity entity;

                using (var ms = new MemoryStream(maskedData))
                using (var br = new BinaryReader(ms))
                {
                    ulong id = br.ReadUInt64();

                    if (id != client.ID)
                        throw new NotImplementedException();

                    if (client.Entity == null)
                    {
                        entity = new Entity();

                        client.Entity = entity;
                        entity.ID = client.ID;
                        Root.Coob.Entities[client.ID] = client.Entity;

                        entity.ReadByMask(br);
                        using (var mss = new MemoryStream())
                        using (var bw = new BinaryWriter(mss))
                        {
                            client.Entity.AddPlayer(bw);
                            bool enditly;
                            for (int i = 0; i < mss.Length; i++)
                                if (mss.ToArray()[i] != maskedData[i])
                                    enditly = maskedData.SequenceEqual(mss.ToArray());
                        }

                        return new EntityUpdate(client.Entity, client.Entity, true, client);
                    }
                    else
                    {
                        entity = Root.Coob.Entities[id];

                        Entity changes = new Entity();
                        changes.ReadByMask(br);

                        return new EntityUpdate(client.Entity, changes, false, client);
                    }
                }
            }

            public override bool CallScript()
            {
                if (IsJoin)
                {
                    bool joined = Root.Scripting.CallFunction<bool>("onClientJoin", Sender);

                    if (joined)
                        Sender.Joined = true;

                    foreach (var client in Root.Coob.GetClients())
                    {
                        if (client.ID == Entity.ID)
                            continue;
                        byte[] compressed;

                        using (var mss = new MemoryStream())
                        using (var bw = new BinaryWriter(mss))
                        {
                            client.Entity.AddPlayer(bw);
                            compressed = ZlibHelper.CompressBuffer(mss.ToArray());
                        }
                        Sender.Writer.Write(0);
                        Sender.Writer.Write(compressed.Length);
                        Sender.Writer.Write(compressed);
                    }

                    return joined;
                }
                else
                    return Root.Scripting.CallFunction<bool>("onEntityUpdate", Entity, Changes, Sender);
            }

            public override void Process()
            {
                Entity.CopyByMask(Changes);

                byte[] compressed;

                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Entity.ID);
                    bw.Write(Changes.LastBitmask);
                    Entity.WriteByMask(Changes.LastBitmask, bw);

                    compressed = ZlibHelper.CompressBuffer(ms.ToArray());
                }

                foreach (var client in Root.Coob.GetClients())
                {
                    //if (client.ID == Entity.ID)
                    continue;
                    try
                    {
                        client.Writer.Write(0);
                        client.Writer.Write(compressed.Length);
                        client.Writer.Write(compressed);
                    }
                    catch { }
                }
            }
        }
    }
}
