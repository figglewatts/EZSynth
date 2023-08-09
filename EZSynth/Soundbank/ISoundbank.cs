using EZSynth.Sampler;
using EZSynth.Synthesizer;

namespace EZSynth.Soundbank
{
    public interface ISoundbank
    {
        /// <summary>
        /// Get a sampler and voice params from this soundbank based on the program number.
        /// </summary>
        /// <param name="programNumber"></param>
        /// <param name="note">The MIDI note number.</param>
        /// <param name="velocity">The MIDI note velocity (0-127).</param>
        /// <returns>(ISampler, VoiceParameters) The sampler and voice params of this program.</returns>
        (ISampler, VoiceParameters) GetSampler(int programNumber, int note, int velocity);

        /// <summary>
        /// Tell the soundbank the sample rate of audio we're expecting from samplers returned.
        /// </summary>
        /// <param name="sampleRateHz">The sample rate (in Hz).</param>
        void SetSampleRate(int sampleRateHz);
    }
}
