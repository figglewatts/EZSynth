using EZSynth.Synthesizer;

namespace EZSynth.Sampler
{
    public interface ISampler
    {
        /// <summary>
        /// The root note of this sampler, i.e. the note being played in the sample. Used to calculate which frequency
        /// to play the sample at for different notes.
        /// </summary>
        int RootNote { get; }
    
        /// <summary>
        /// The note that this sampler is currently playing. Result is undefined if the sampler is not currently playing
        /// a note.
        /// </summary>
        int PlayingNote { get; }
    
        /// <summary>
        /// Whether or not to loop the sample when we play a note. If false then the sample is one-shot (for drums etc).
        /// </summary>
        bool LoopSample { get; }

        /// <summary>
        /// Resample the sample to the given sample rate.
        /// </summary>
        /// <param name="sampleRateHz">The sample rate to resample to.</param>
        void ResampleTo(int sampleRateHz);

        /// <summary>
        /// Return a sample from the sampler.
        /// </summary>
        /// <param name="pitchBendSemitones">Pitch bend value in semitones. Can be +ve or -ve.</param>
        /// <returns>Left and right PCM audio samples.</returns>
        (short left, short right) Sample(float pitchBendSemitones);
    }
}
