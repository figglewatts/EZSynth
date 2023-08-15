using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EZSynth.Soundbank;
using EZSynth.Util;

namespace EZSynth.Synthesizer
{
    public class Synth
    {
        public const int DEFAULT_SAMPLE_RATE = 44100;
        public const int DEFAULT_MAX_VOICES = 28;
        public const int DEFAULT_MAX_PITCH_BEND_SEMITONES = 2;

        /// <summary>
        /// The timestep we're rendering samples at. Each sample represents this unit of time in seconds.
        /// </summary>
        public double TimeStep => 1f / SampleRate;
    
        /// <summary>
        /// The sample rate we're rendering samples at.
        /// </summary>
        public int SampleRate { get; protected set; } = DEFAULT_SAMPLE_RATE;
    
        /// <summary>
        /// The maximum number of voices that can be playing notes at once.
        /// </summary>
        public int MaxVoices { get; protected set; } = DEFAULT_MAX_VOICES;
    
        /// <summary>
        /// The maximum pitch bend in semitones.
        /// </summary>
        public float MaxPitchBend { get; set; } = DEFAULT_MAX_PITCH_BEND_SEMITONES;
    
        /// <summary>
        /// The soundbank we're using to synthesize the sound.
        /// </summary>
        public ISoundbank Soundbank { get; set; }

        /// <summary>
        /// The bank of voices available for playing instrument-notes on. Limited by MaxVoices.
        /// </summary>
        protected readonly Voice[] _voices;
    
        /// <summary>
        /// Mapping of instrument IDs to Soundbank programs. Used with UseProgram() to remember what instrument each
        /// ID is playing.
        /// </summary>
        protected readonly Dictionary<int, InstrumentData> _instrumentData;

        /// <summary>
        /// Mapping of instrument-notes to voices playing the note. This is what enables voice polyphony.
        /// Samples from active voices in this mapping are mixed together to form the synth output.
        /// </summary>
        protected readonly Dictionary<(int id, int note), Voice> _activeVoices;

        public Synth(ISoundbank soundbank, int sampleRateHz = DEFAULT_SAMPLE_RATE, int maxVoices = DEFAULT_MAX_VOICES)
        {
            Soundbank = soundbank;
            Soundbank.SetSampleRate(sampleRateHz);
        
            SampleRate = sampleRateHz;
            MaxVoices = maxVoices;

            _voices = createVoiceBank();
            _instrumentData = new Dictionary<int, InstrumentData>();
            _activeVoices = new Dictionary<(int id, int note), Voice>();
        }

        /// <summary>
        /// Switch an instrument to a different program number. Used in combination with the soundbank to set samples
        /// to be played for each instrument ID.
        /// </summary>
        /// <param name="id">The instrument ID.</param>
        /// <param name="programNumber">The program number to use.</param>
        public void UseProgram(int id, int programNumber)
        {
            ensureInstrumentExists(id);
            _instrumentData[id].ProgramNumber = programNumber;
        }

        public void PitchBend(int id, float pitch)
        {
            // update pitch of active voices
            foreach (var (instrumentId, note) in _activeVoices.Keys)
            {
                if (instrumentId == id) _activeVoices[(id, note)].Parameters.Pitch = pitch;
            }
        
            // update pitch of instrument data
            _instrumentData[id].Pitch = pitch;
        }

        public void AdjustPan(int id, float pan)
        {
            // update panning of active voices
            foreach (var (instrumentId, note) in _activeVoices.Keys)
            {
                if (instrumentId == id) _activeVoices[(id, note)].Parameters.Pan = pan;
            }
        
            // update panning of instrument data
            _instrumentData[id].Pan = pan;
        }

        /// <summary>
        /// Turn a note on with a given velocity. The ID is to identify different instruments. It can be considered
        /// equivalent to a MIDI channel, but without any limit in terms of how many there are.
        ///
        /// NoteOn/NoteOff events for the same instrument should have matching IDs.
        /// </summary>
        /// <param name="note">The MIDI note to play.</param>
        /// <param name="velocity">The velocity to play the note with (0-127, >127 clamped).</param>
        /// <param name="id">The ID of the instrument. Essentially a MIDI channel.</param>
        public void NoteOn(int id, int note, int velocity)
        {
            // sometimes people use a NoteOn event with a velocity of 0 to indicate a NoteOff event, so let's handle that
            if (velocity <= 0)
            {
                NoteOff(id, note);
                return;
            }

            // get the instrument's program, then use that to get sample and voice information from the soundbank
            ensureInstrumentExists(id);
            InstrumentData instrumentData = _instrumentData[id];
            var (sampler, voiceParams) = Soundbank.GetSampler(instrumentData.ProgramNumber, note, velocity);
            if (sampler == null) return; // sometimes the soundbank will want to not play certain sounds
            voiceParams.Velocity = MidiUtil.VelocityToFloat(velocity);
            voiceParams.Pitch = instrumentData.Pitch;
            voiceParams.Pan = instrumentData.Pan;
        
            // handle if this note is already being played, restart it
            if (_activeVoices.TryGetValue((id, note), out Voice existingVoice))
            {
                existingVoice.NoteOn(sampler, voiceParams);
                return;
            }

            // get an inactive voice to use to play this instrument-note
            Voice voice = getInactiveVoice();
            if (voice == null) return; // silently fail if we've run out of voices, it probably won't matter over all the racket
            Debug.Assert(voice.Active == false);

            // play the note
            voice.NoteOn(sampler, voiceParams);
            _activeVoices[(id, note)] = voice;
        }

        /// <summary>
        /// Turn a note off on a given instrument ID.
        /// </summary>
        /// <param name="note">The note to turn off.</param>
        /// <param name="id">The ID of the instrument. Essentially a MIDI channel.</param>
        public void NoteOff(int id, int note)
        {
            // silently fail if we're not actually playing this note on this instrument, because who really cares
            if (!_activeVoices.ContainsKey((id, note))) return;

            // tell the voice the note is off now, so it can start decaying
            _activeVoices[(id, note)].NoteOff();
        }

        /// <summary>
        /// Render a single sample of the synth into PCM audio.
        /// </summary>
        /// <returns>Left and right samples of PCM audio.</returns>
        public (short left, short right) Sample()
        {
            int activeVoices = _activeVoices.Count;
            if (activeVoices == 0) return (0, 0); // silence
        
            int mixedLeft = 0, mixedRight = 0;
            foreach (var voice in _activeVoices.Values)
            {
                Debug.Assert(voice.Active == true);
            
                // iterate through the active voices, sample them, and mix the audio
                (short left, short right) = voice.Sample();
                mixedLeft += left;
                mixedRight += right;
            }
            mixedLeft = clip(mixedLeft);
            mixedRight = clip(mixedRight);
        
            // clean up (i.e. remove from active) voices with completed volume envelopes
            cleanupInactiveVoices();
        
            return ((short)mixedLeft, (short)mixedRight);
        }

        /// <summary>
        /// Clip the audio using tanh() to smoothly attenuate samples that would be too loud.
        /// </summary>
        /// <param name="sample">The sample we're clipping.</param>
        /// <returns>The clipped sample (-1 -> 1)</returns>
        protected int clip(int sample)
        {
            double sampleDouble = sample / 32768d;
            const double fixedAttenuation = 0.5; // gonna be honest, just tweaked this until it sounded good
            double clippedSample = Math.Tanh(sampleDouble * fixedAttenuation); // use tanh to attenuate loud samples back within -1->1
            clippedSample = MathUtil.Clamp(clippedSample, -1, 1);
            return (int)(clippedSample * 32767);
        }

        /// <summary>
        /// Run on synth creation, creates the bank of voices to be used to play instrument-notes.
        /// </summary>
        /// <returns>The array containing the bank of voices.</returns>
        protected Voice[] createVoiceBank()
        {
            var result = new Voice[MaxVoices];
            for (int i = 0; i < MaxVoices; i++)
            {
                result[i] = new Voice(this);
            }
            return result;
        }

        /// <summary>
        /// To be called after all voices have been sampled, to remove voices whose volume envelopes have completed
        /// from the list of active voices (as they are no longer audible).
        /// </summary>
        protected void cleanupInactiveVoices()
        {
            var nonActiveVoices = _activeVoices.Where(kvp => !kvp.Value.Active).ToList();
            foreach (var kvp in nonActiveVoices)
            {
                _activeVoices.Remove(kvp.Key);
            }
        }

        /// <summary>
        /// Get a voice that isn't currently playing a note.
        /// </summary>
        /// <returns>The voice, or null if there were no voices available.</returns>
        protected Voice getInactiveVoice()
        {
            try
            {
                return _voices.First(v => !v.Active);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// To be called before an instrument is used, to configure default values if no instrument with the given ID
        /// exists, so we can default to 'always playing something', hopefully making the synth easier to use.
        /// </summary>
        /// <param name="id">The instrument ID.</param>
        protected void ensureInstrumentExists(int id)
        {
            // we could throw an error if there's no program associated with this instrument, but we want to be
            // easy to use, so instead let's just ensure unknown IDs have a program number of zero
            _instrumentData.TryAdd(id, new InstrumentData { ProgramNumber = id });
        }
    }
}
