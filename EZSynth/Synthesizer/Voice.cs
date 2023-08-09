using System;
using EZSynth.Sampler;
using EZSynth.Util;

namespace EZSynth.Synthesizer
{
    public class Voice
    {
        /// <summary>
        /// The sampler this voice is using for sample data.
        /// </summary>
        public ISampler Sampler { get; set; }

        /// <summary>
        /// Whether or not this voice is actively playing audio.
        /// </summary>
        public bool Active => Sampler != null;

        /// <summary>
        /// The volume of this voice (0.0 to 1.0).
        /// </summary>
        public float Volume { get; set; } = 1.0f;

        /// <summary>
        /// The current parameters of this voice.
        /// </summary>
        public VoiceParameters Parameters;

        protected readonly Synth _synth;  // reference to the synth using this voice
        protected int _timesSampled = 0;  // the number of times the sampler has been sampled (used for ADSR envelope)
        protected bool _released = false; // whether or not the note being played has been released yet
        protected float _velocity = 1;    // the velocity the note was first played with
    
        internal Voice(Synth synth)
        {
            _synth = synth;
        }

        /// <summary>
        /// Sample this voice. Gets a sample from the sampler with the given note, and applies pitch bend, volume envelope,
        /// panning, and additional processing.
        /// </summary>
        /// <returns>Left and right channel samples.</returns>
        /// <exception cref="InvalidOperationException">If the Sampler of this voice is null.</exception>
        public (short left, short right) Sample()
        {
            if (Sampler == null) throw new InvalidOperationException("unable to Sample() null Sampler");
        
            var (left, right) = Sampler.Sample(Parameters.Pitch * _synth.MaxPitchBend);
            (left, right) = applyVolumeEnvelope(left, right);
            (left, right) = applyPan(left, right);
            (left, right) = applyVolume(left, right);
            _timesSampled++;
            return (left, right);
        }

        /// <summary>
        /// Start playing this voice's note.
        /// </summary>
        /// <param name="sampler">The sampler to use to play the note.</param>
        /// <param name="voiceParameters">The parameters to use to play the note.</param>
        public void NoteOn(ISampler sampler, VoiceParameters voiceParameters)
        {
            Sampler = sampler;
            Parameters = voiceParameters;
            _velocity = Parameters.Velocity;
            _timesSampled = 0;
        }

        /// <summary>
        /// Release the note we're playing, so the ADSR volume envelope can enter its release phase.
        /// </summary>
        public void NoteOff()
        {
            _released = true;
            _timesSampled = 0; // reset so we can sample the release part of the envelope now
        }

        /// <summary>
        /// Apply the ADSR volume envelope to the given stereo samples.
        /// </summary>
        /// <param name="left">The left sample.</param>
        /// <param name="right">The right sample.</param>
        /// <returns>The left and right samples having had the envelope applied.</returns>
        protected (short left, short right) applyVolumeEnvelope(short left, short right)
        {
            double t = _timesSampled * _synth.TimeStep;
            double envelope = 1;
            if (!_released)
            {
                if (t < Parameters.VolumeEnvelope.AttackTime)
                {
                    // attacking
                    envelope = t / Parameters.VolumeEnvelope.AttackTime;
                }
                else if (t < Parameters.VolumeEnvelope.AttackTime + Parameters.VolumeEnvelope.DecayTime)
                {
                    // decaying
                    double tDecay = t - Parameters.VolumeEnvelope.AttackTime;
                    float sustainLevel = Parameters.VolumeEnvelope.SustainLevel;
                    envelope = sustainLevel + (1 - sustainLevel) * (1 - tDecay / Parameters.VolumeEnvelope.DecayTime);
                }
                else
                {
                    // sustaining
                    envelope = Parameters.VolumeEnvelope.SustainLevel;
                }
            }
            else
            {
                if (t < Parameters.VolumeEnvelope.ReleaseTime)
                {
                    // releasing
                    envelope = Parameters.VolumeEnvelope.SustainLevel * (1 - t / Parameters.VolumeEnvelope.ReleaseTime);
                }
                else
                {
                    // fully released, get rid of our sample to tell the synth this voice is now free
                    envelope = 0;
                    _timesSampled = 0;
                    Sampler = null;
                }
            }

            envelope = MathUtil.Clamp(envelope, 0, 1);
            return ((short)(left * envelope), (short)(right * envelope));
        }

        /// <summary>
        /// Apply note velocity, voice volume, and instrument volume to the sample.
        /// </summary>
        /// <param name="left">The left sample.</param>
        /// <param name="right">The right sample.</param>
        /// <returns>The samples having had volume applied.</returns>
        protected (short left, short right) applyVolume(short left, short right)
        {
            return (
                (short)(left * Volume * _velocity * Parameters.Volume), 
                (short)(right * Volume * _velocity * Parameters.Volume));
        }

        /// <summary>
        /// Apply the voice panning to the given stereo samples.
        /// </summary>
        /// <param name="left">The left sample.</param>
        /// <param name="right">The right sample.</param>
        /// <returns>The samples having had panning applied.</returns>
        protected (short left, short right) applyPan(short left, short right)
        {
            float leftMultiplier, rightMultiplier;
            if (Parameters.Pan < 0)
            {
                // Pan is to the left, so lower volume of right channel
                rightMultiplier = 1 + Parameters.Pan; // pan is negative, so this is less than 1
                leftMultiplier = 1;
            }
            else
            {
                // Pan is to the right, so lower volume of left channel
                leftMultiplier = 1 - Parameters.Pan; // pan is positive, so this is less than 1
                rightMultiplier = 1;
            }
        
            // apply pan
            return ((short)(left * leftMultiplier), (short)(right * rightMultiplier));
        }
    }
}
