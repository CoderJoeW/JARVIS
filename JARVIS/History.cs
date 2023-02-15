using System.Collections.Generic;

namespace JARVIS
{
    public class History
    {
        private readonly List<string> _history;
        private readonly int _maxHistoryLength;

        public History(int maxHistoryLength)
        {
            _history = new List<string>();
            _maxHistoryLength = maxHistoryLength;
        }

        public void Add(string prompt)
        {
            _history.Add(GetFullPrompt(prompt));
            if (_history.Count > _maxHistoryLength)
            {
                _history.RemoveAt(0);
            }
        }

        public string GetFullPrompt(string prompt)
        {
            return string.Join("", _history) + prompt;
        }
    }
}
