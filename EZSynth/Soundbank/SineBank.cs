using EZSynth.Sampler;
using EZSynth.Synthesizer;

namespace EZSynth.Soundbank
{
    public class SineBank : ISoundbank
    {
        protected int _sampleRate;
    
        public (ISampler, VoiceParameters) GetSampler(int programNumber, int note, int velocity)
        {
            var sampler = new SineSampler(_sampleRate);
            sampler.PlayingNote = note;

            var voiceParams = new VoiceParameters
            {
                Volume = 0.7f,
                VolumeEnvelope = new EnvelopeADSR
                {
                    AttackTime = 0.5f,
                    DecayTime = 1.0f,
                    SustainLevel = 0.75f,
                    ReleaseTime = 1.0f
                },
            };
            return (sampler, voiceParams);
        }

        public void SetSampleRate(int sampleRateHz)
        {
            _sampleRate = sampleRateHz;
        }
    }
}
