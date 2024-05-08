using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace ConsoleApp25
{
    public class Storage
    {
        private readonly ConcurrentDictionary<long, Session> _sessions;

        public Storage()
        {
            _sessions = new ConcurrentDictionary<long, Session>();
        }

        public Session GetSession(long chatId)
        {
            // Возвращаем сессию по ключу, если она существует
            if (_sessions.ContainsKey(chatId))
                return _sessions[chatId];

            // Создаем и возвращаем новую, если такой не было
            var newSession = new Session() { operation = Operation.Длина };
            _sessions.TryAdd(chatId, newSession);
            return newSession;
        }
    }
}
