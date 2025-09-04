using System.Collections.Generic;
using System;

public class PacketDispatcher
{
    private readonly Dictionary<PacketType, Action<string>> handlers = new();

    public void Register(PacketType type, Action<string> handler)
    {
        if (handlers.ContainsKey(type))
            handlers[type] += handler;
        else
            handlers[type] = handler;
    }

    public void Unregister(PacketType type, Action<string> handler)
    {
        if (handlers.ContainsKey(type))
        {
            handlers[type] -= handler;
            if (handlers[type] == null)
                handlers.Remove(type);
        }
    }

    public void Dispatch(PacketType type, string msg)
    {
        if (handlers.TryGetValue(type, out var action))
            action?.Invoke(msg);
    }
}
