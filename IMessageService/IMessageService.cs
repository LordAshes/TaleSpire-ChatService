using Bounce.Unmanaged;
using System;

namespace Talespire
{
    public enum MessageSource
    {
        gm = 0,
        player = 1,
        creature = 2,
        anonymous = 999
    }

    public interface IMessageService
    {
        void SendMessage(string message, NGuid source);
        void AddHandler(string key, Func<string, string, MessageSource, string> callback);
        void RemoveHandler(string key);
    }
}