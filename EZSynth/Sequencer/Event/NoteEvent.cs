using EZSynth.Synthesizer;

namespace EZSynth.Sequencer.Event
{
    public class NoteEvent : BaseInstrumentEvent
    {
        public int Note;
        public int Velocity;
    
        public override void Handle(Synth synth, Sequencer sequencer)
        {
            synth.NoteOn(InstrumentID, Note, Velocity);
        }
    }
}
