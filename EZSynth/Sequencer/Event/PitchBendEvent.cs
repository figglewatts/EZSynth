using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class PitchBendEvent : BaseInstrumentEvent
    {
        public float PitchBendAmount;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            synth.PitchBend(InstrumentID, PitchBendAmount);
        }
    }
}
