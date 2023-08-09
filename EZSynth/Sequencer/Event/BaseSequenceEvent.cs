using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public abstract class BaseSequenceEvent
    {
        /// <summary>
        /// Handle the event this object represents by modifying the synth/sequencer state.
        /// </summary>
        /// <param name="synth">The synth that is being used to render this sequence.</param>
        /// <param name="sequencer">The sequencer rendering this sequence.</param>
        public abstract void Handle(Synth synth, Sequencer sequencer);
    }
}
