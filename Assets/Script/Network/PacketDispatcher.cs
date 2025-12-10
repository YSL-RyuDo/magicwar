using System;
using System.Collections.Generic;

public class PacketDispatcher
{
    private Dictionary<PacketType, Action<byte[]>> handlers = new();

    public void Register(PacketType type, Action<byte[]> callback)
    {
        handlers[type] = callback;
    }

    public void Unregister(PacketType type, Action<byte[]> callback)
    {
        if (handlers.ContainsKey(type))
        {
            if (handlers[type] == callback)
                handlers.Remove(type);
        }
    }

    public void Dispatch(PacketType type, byte[] body)
    {
        if (handlers.TryGetValue(type, out var cb))
            cb(body);
    }
}
