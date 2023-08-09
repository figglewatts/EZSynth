using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class ProgramChangeEvent : BaseInstrumentEvent
    {
        public int Program;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            // handle drum kits
            if (InstrumentID == 9)
            {
                synth.UseProgram(InstrumentID, 128);
            }
            else
            {
                synth.UseProgram(InstrumentID, Program);
            }
        }
    }
}
