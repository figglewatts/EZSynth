namespace EZSynth.Synthesizer
{
    public struct VoiceParameters
    {
        /// <summary>
        /// The volume of this voice (0 to 1).
        /// </summary>
        public float Volume { get; set; }
    
        /// <summary>
        /// The pan of this voice (-1.0 to 1.0, left to right).
        /// </summary>
        public float Pan { get; set; }

        /// <summary>
        /// The pitch modifier of this voice (-1.0 to 1.0, low to high).
        /// This will be multiplied by the pitch bend amount in the synth.
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// The velocity of the note played with this voice.
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// The ADSR envelope to apply to this voice's volume.
        /// </summary>
        public EnvelopeADSR VolumeEnvelope;

        public VoiceParameters CreateDefault()
        {
            return new VoiceParameters
            {
                Volume = 1,
                Pan = 0,
                Pitch = 0,
                Velocity = 1
            };
        }
    }
}
