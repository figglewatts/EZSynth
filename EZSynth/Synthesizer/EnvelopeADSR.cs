namespace EZSynth.Synthesizer
{
    public struct EnvelopeADSR
    {
        public EnvelopeADSR CreateDefault()
        {
            return new EnvelopeADSR
            {
                AttackTime = 0,
                DecayTime = 0,
                SustainLevel = 1,
                ReleaseTime = 0,
            };
        }

        /// <summary>
        /// Attack time in seconds.
        /// </summary>
        public float AttackTime { get; set; }

        /// <summary>
        /// Decay time in seconds.
        /// </summary>
        public float DecayTime { get; set; }

        /// <summary>
        /// Sustain level (from 0.0 to 1.0).
        /// </summary>
        public float SustainLevel { get; set; }

        /// <summary>
        /// Release time in seconds.
        /// </summary>
        public float ReleaseTime { get; set; }
    }
}
