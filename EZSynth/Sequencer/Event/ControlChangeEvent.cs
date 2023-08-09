using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class ControlChangeEvent : BaseInstrumentEvent
    {
        public int Controller;
        public int Value;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            if (Controller == 10) // pan
            {
                synth.AdjustPan(InstrumentID, panToFloat(Value));
            }
        }
    
        protected float panToFloat(int pan)
        {
            return (pan - 64f) / 64f;
        }
    }
}
