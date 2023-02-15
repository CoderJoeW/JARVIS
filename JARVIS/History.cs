using System.Collections.Generic;

namespace JARVIS
{
    public class History
    {
        private readonly List<string> _conversation;
        private readonly int _maxHistoryLength;

        public History(int maxHistoryLength)
        {
            _conversation = new List<string>();
            _maxHistoryLength = maxHistoryLength;
        }

        public void Add(string speaker, string message)
        {
            _conversation.Add($"{speaker}: {message}");
            if (_conversation.Count > _maxHistoryLength)
            {
                _conversation.RemoveAt(0);
            }
        }

        public string GetFullPrompt(string prompt)
        {
            return string.Join("", _conversation) + $" Human: {prompt}";
        }
    }
}
