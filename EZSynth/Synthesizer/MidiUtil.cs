using System;
using EZSynth.Util;

namespace EZSynth.Synthesizer
{
    public static class MidiUtil
    {
        public static float VelocityToFloat(int velocity)
        {
            return MathUtil.Clamp(velocity / 127f, 0, 1);
        }

        public static double NoteToFrequency(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0f);
        }

        public static int FrequencyToNote(float frequency)
        {
            return (int)Math.Round(69 + 12 * MathUtil.Log2(frequency / 440));
        }
    }
}
