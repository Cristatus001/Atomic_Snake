using UnityEngine;

namespace AtomicSnake.Runtime
{
    public static class UiFontUtil
    {
        static Font _font;

        public static Font Default
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }
    }
}
