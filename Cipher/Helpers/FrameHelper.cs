// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Cipher.Helpers
{
    public class FrameHelper
    {
        public class FrameListReader
        {
            private readonly List<object> frames;
            private int addIndex;
            private int readIndex;

            public FrameListReader()
            {
                frames = new List<object>();
            }

            public void AddFrame(object frame)
            {
                frames.Add(frame);
                addIndex++;
            }

            public object ReadFrame()
            {
                if (readIndex >= frames.Count)
                    return null;

                return frames[readIndex++];
            }

            public bool HasNext()
            {
                return readIndex < frames.Count;
            }
        }
    }
}
