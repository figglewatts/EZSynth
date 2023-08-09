using System;
using EZSynth.Synthesizer;

namespace EZSynth.Sampler
{
    public class PCMSampler : ISampler
    {
        protected short[] _waveTable;
        protected int _sampleRate;
        protected double _phase = 0;

        public int RootNote { get; set; }
        public int PlayingNote { get; set; }
        public bool LoopSample { get; set; } = true;
    
        public PCMSampler(short[] waveTable, int sampleRateHz)
        {
            _waveTable = waveTable;
            _sampleRate = sampleRateHz;
        }

        public void ResampleTo(int sampleRateHz)
        {
            _waveTable = resample(sampleRateHz);
            _sampleRate = sampleRateHz;
        }

        public (short left, short right) Sample(float pitchBendSemitones)
        {
            // handle non-looping samples
            if (!LoopSample && _phase > _waveTable.Length) return (0, 0); // return silence after we've played the sample

            // calculate how much to advance the phase of the sample by, based on our note frequency and pitch bend
            double phaseIncrement = calculatePhaseIncrement(pitchBendSemitones);

            // sample the wavetable, linearly interpolating if phase lies between samples so we don't get aliasing
            short sample = lerpSample(_phase);

            // increment the phase for the next sample
            _phase += phaseIncrement;
        
            // return the sample
            return (sample, sample);
        }

        protected double calculatePhaseIncrement(float pitchBendSemitones)
        {
            // first figure out what frequency we're currently playing at:
            // pitch bend is given in semitones, 12 semitones are in an octave, and an octave doubles the frequency
            // therefore we can calculate how much to multiply the frequency by by figuring out how many octaves we
            // are up or down, and doubling the frequency by this amount
            double pitchBendOctaves = pitchBendSemitones / 12;
            double freqBendRatio = Math.Pow(2, pitchBendOctaves);
            double playingFrequency = MidiUtil.NoteToFrequency(PlayingNote) * freqBendRatio;
        
            // now relate this frequency to the root note of the sample:
            // we know the sample plays at a frequency of RootNote, so a phase of 1 will sample in lockstep
            // with that frequency - if we sample at a phase greater than 1 it will be a higher frequency (and vice versa
            // for lower frequencies), so we can calculate the phase we should sample at by getting the ratio
            // between the current note and the root note
            return playingFrequency / MidiUtil.NoteToFrequency(RootNote);
        }

        protected short lerpSample(double phase)
        {
            // calculate how far between samples we are
            double amountBetween = phase - Math.Floor(phase);
        
            // get the samples we should interpolate between
            int sampleAIndex = (int)phase % _waveTable.Length;
            short sampleA = _waveTable[sampleAIndex];
            int sampleBIndex = (sampleAIndex + 1) % _waveTable.Length;
            short sampleB = _waveTable[sampleBIndex];
        
            // linearly interpolate between the samples
            return (short)(sampleA * (1 - amountBetween) + sampleB * amountBetween);
        }

        protected short[] resample(int newSampleRate)
        {
            // Calculate new length of sample
            int newLength = (int)(_waveTable.Length * (newSampleRate / (double)_sampleRate));
            short[] resampled = new short[newLength];

            for (int i = 0; i < newLength; i++)
            {
                // Find position in old samples
                double position = i * (_sampleRate / (double)newSampleRate);
                resampled[i] = lerpSample(position);
            }

            return resampled;
        }
    }
}
