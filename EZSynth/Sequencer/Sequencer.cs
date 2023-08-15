using System;
using EZSynth.Synthesizer;

namespace EZSynth.Sequencer
{
    public class Sequencer
    {
        public ISequence Sequence { get; set; }
        public Synth Synth { get; set; }

        public int BufferSize { get; set; } = 2048;

        /// <summary>
        /// The amount of time it takes in seconds for the sequencer play a single beat.
        /// </summary>
        public double SecondsPerBeat { get; set; } = 0.5d; // defaults to 120bpm

        /// <summary>
        /// The tempo in beats per minute. This uses SecondsPerBeat under the hood, so any tempo values here
        /// will be derived from SecondsPerBeat.
        /// This is because SecondsPerBeat is easier to use internally when rendering a sequence.
        /// </summary>
        public double Tempo
        {
            get
            {
                double minutesPerBeat = SecondsPerBeat / 60;
                return 1d / minutesPerBeat;
            }
            set => SecondsPerBeat = (1 / value) * 60;
        }

        public double SecondsPerTick => 1d / (Sequence.Resolution / SecondsPerBeat);

        protected double _lastTickProcessedTime = -1;
    
        public Sequencer(ISequence sequence, Synth synth)
        {
            Sequence = sequence;
            Synth = synth;
        }

        public short[] Render()
        {
            Synth.Soundbank.SetSampleRate(Synth.SampleRate);
            _lastTickProcessedTime = -1;
            int numSamples = (int)Math.Ceiling(Sequence.GetLengthSeconds(SecondsPerTick) * Synth.SampleRate);
            short[] resultSamples = new short[numSamples * 2]; // stereo, so 2 channels
            short[] buffer = new short[BufferSize * 2];
            for (int i = 0; i < numSamples; i+=BufferSize)
            {
                int samplesToRender = Math.Min(BufferSize, numSamples - i);
                renderSamples(i, buffer, samplesToRender);
                Array.Copy(buffer, 0, resultSamples, i * 2, samplesToRender * 2);
            }

            return resultSamples;
        }

        protected void renderSamples(int sampleNum, short[] buffer, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)sampleNum / Synth.SampleRate;

                // figure out if it's time to tick the sequence
                if (shouldProcessTick(t, out int tickNumber))
                {
                    // handle any events the sequence is raising this tick
                    foreach (var sequenceEvent in Sequence.GetEvents(tickNumber))
                    {
                        sequenceEvent.Handle(Synth, this);
                    }
                    _lastTickProcessedTime = t;
                }

                var (left, right) = Synth.Sample();
                buffer[i * 2] = left;
                buffer[i * 2 + 1] = right;

                sampleNum++;
            }
        }

        /// <summary>
        /// Returns true if we should process a tick of the sequence this sample.
        /// </summary>
        /// <param name="timeSeconds">The current time in seconds we are into the sequence.</param>
        /// <param name="tickNumber">The tick number this time maps to.</param>
        /// <returns>True if we should process a tick the sequence this sample, false otherwise.</returns>
        protected bool shouldProcessTick(double timeSeconds, out int tickNumber)
        {
            tickNumber = (int)(timeSeconds / SecondsPerTick);
            return timeSeconds - _lastTickProcessedTime > SecondsPerTick;
        }
    }
}
