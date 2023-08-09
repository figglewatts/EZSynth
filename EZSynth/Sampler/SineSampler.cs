using System;
using EZSynth.Synthesizer;

namespace EZSynth.Sampler
{
    public class SineSampler : PCMSampler
    {
        protected const int SINE_NOTE = 69; // A4, as it's an exact number of Hz (440), for a full sine cycle
    
        public SineSampler(int sampleRateHz) : base(null, sampleRateHz)
        {
            _waveTable = createWaveTable();
            RootNote = SINE_NOTE;
        }

        public new void ResampleTo(int sampleRateHz)
        {
            _sampleRate = sampleRateHz;
            _waveTable = createWaveTable();
        }

        protected short[] createWaveTable()
        {
            var result = new short[_sampleRate];
            for (int i = 0; i < _sampleRate; i++)
            {
                result[i] = (short)(Math.Sin(
                    2 * Math.PI * MidiUtil.NoteToFrequency(SINE_NOTE) * ((double)i / _sampleRate)) * 32767);
            }
            return result;
        }
    }
}
