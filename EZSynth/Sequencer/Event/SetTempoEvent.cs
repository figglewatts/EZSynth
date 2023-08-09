using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class SetTempoEvent : BaseSequenceEvent
    {
        public double SecondsPerBeat;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            sequencer.SecondsPerBeat = SecondsPerBeat;
        }
    }
}
