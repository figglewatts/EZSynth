using System;
using EZSF2.Soundfont;
using EZSynth.Sampler;
using EZSynth.Soundbank;
using EZSynth.Synthesizer;

namespace EZSynth.Implementations.Soundbank
{
    public class SoundfontSoundbank : ISoundbank
    {
        protected readonly SF2 _soundfont;

        public SoundfontSoundbank(SF2 soundfont)
        {
            _soundfont = soundfont;
        }
    
        public (ISampler, VoiceParameters) GetSampler(int programNumber, int note, int velocity)
        {
            var noteData = _soundfont.GetNote(programNumber, note, velocity);

            var samplePcmData = noteData.Sample.GetPCMData();
            var sampleRate = (int)noteData.Sample.Header.SampleRate;
            var sampler = new PCMSampler(samplePcmData, sampleRate)
            {
                PlayingNote = noteData.Settings.KeyOverride ?? note,
                RootNote = noteData.Settings.RootKeyOverride ?? noteData.Sample.Header.OriginalPitch,
                LoopSample = false,
            };

            var pan = noteData.Settings.Pan.Value / 500f; // pan is -500 to 500
            var volume = centibelsToLinearGain(noteData.Settings.Attenuation.Value);
            var pitchBend = noteData.Settings.PitchModificationCents.Value / 200f; // +/-2 semitones, i.e. 200 cents
            var voiceVelocity = (noteData.Settings.VelocityOverride ?? velocity) / 127f; // velocity is 0-127
            var attackSeconds = timeCentsToSeconds(noteData.Settings.Attack.Value);
            var decaySeconds = timeCentsToSeconds(noteData.Settings.Decay.Value);
            var sustainLevel = centibelsToLinearGain(noteData.Settings.Sustain.Value);
            var releaseSeconds = timeCentsToSeconds(noteData.Settings.Release.Value);
        
            // if ADSR not set, set defaults that sound nice
            if (noteData.Settings.Release <= -12000) releaseSeconds = 0.5f;

            var voiceParams = new VoiceParameters
            {
                Pan = pan,
                Volume = volume,
                Pitch = pitchBend,
                Velocity = voiceVelocity,
                VolumeEnvelope = new EnvelopeADSR
                {
                    AttackTime = attackSeconds,
                    DecayTime = decaySeconds,
                    SustainLevel = sustainLevel,
                    ReleaseTime = releaseSeconds,
                }
            };

            return (sampler, voiceParams);
        }

        public void SetSampleRate(int sampleRateHz) { }

        protected float centibelsToLinearGain(int centibels)
        {
            return (float)Math.Pow(10, -centibels / 2000f);
        }

        protected float timeCentsToSeconds(int timeCents)
        {
            return (float)Math.Pow(2, (timeCents - 1200) / 1200f);
        }
    }
}
