using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class ProgramChangeEvent : BaseInstrumentEvent
    {
        public int Program;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            synth.UseProgram(InstrumentID, Program);
        }
    }
}
