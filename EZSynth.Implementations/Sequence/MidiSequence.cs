using System;
using System.Collections.Generic;
using System.Linq;
using EZSynth.Sequencer;
using EZSynth.Sequencer.Event;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using ControlChangeEvent = Melanchall.DryWetMidi.Core.ControlChangeEvent;
using PitchBendEvent = Melanchall.DryWetMidi.Core.PitchBendEvent;
using ProgramChangeEvent = Melanchall.DryWetMidi.Core.ProgramChangeEvent;
using SetTempoEvent = Melanchall.DryWetMidi.Core.SetTempoEvent;

namespace EZSynth.Implementations.Sequence
{
    public class MidiSequence : ISequence
    {
        public int Resolution => ((TicksPerQuarterNoteTimeDivision)_midiFile.TimeDivision).TicksPerQuarterNote;

        protected readonly MidiFile _midiFile;
        protected readonly Dictionary<int, List<MidiEvent>> _midiEvents;

        public MidiSequence(MidiFile midiFile)
        {
            _midiFile = midiFile;
            _midiEvents = getEventsFromMidiFile(_midiFile);
        }
    
        public double GetLengthSeconds(double secondsPerTick)
        {
            return _midiFile.GetDuration<MetricTimeSpan>().TotalSeconds;
        }

        public IEnumerable<BaseSequenceEvent> GetEvents(int tickNumber)
        {
            // return no events if none happened this tick
            if (!_midiEvents.ContainsKey(tickNumber)) return Array.Empty<BaseSequenceEvent>();

            var events = _midiEvents[tickNumber];
            return events.Select(midiEventToSequenceEvent).Where(e => e != null);
        }

        protected BaseSequenceEvent midiEventToSequenceEvent(MidiEvent midiEvent)
        {
            BaseSequenceEvent result;
            switch (midiEvent)
            {
                case ControlChangeEvent controlChangeEvent:
                    result = new Sequencer.Event.ControlChangeEvent
                    {
                        InstrumentID = controlChangeEvent.Channel,
                        Controller = controlChangeEvent.ControlNumber,
                        Value = controlChangeEvent.ControlValue
                    };
                    break;
                case NoteOffEvent noteOffEvent:
                    result = new Sequencer.Event.NoteEvent
                    {
                        InstrumentID = noteOffEvent.Channel,
                        Note = noteOffEvent.NoteNumber,
                        Velocity = noteOffEvent.Velocity
                    };
                    break;
                case NoteOnEvent noteOnEvent:
                    result = new Sequencer.Event.NoteEvent
                    {
                        InstrumentID = noteOnEvent.Channel,
                        Note = noteOnEvent.NoteNumber,
                        Velocity = noteOnEvent.Velocity
                    };
                    break;
                case PitchBendEvent pitchBendEvent:
                    result = new Sequencer.Event.PitchBendEvent
                    {
                        InstrumentID = pitchBendEvent.Channel,
                        PitchBendAmount = pitchBendToFloat(pitchBendEvent.PitchValue)
                    };
                    break;
                case ProgramChangeEvent programChangeEvent:
                    result = new Sequencer.Event.ProgramChangeEvent
                    {
                        InstrumentID = programChangeEvent.Channel,
                        Program = programChangeEvent.ProgramNumber
                    };
                    break;
                case SetTempoEvent setTempoEvent:
                    result = new Sequencer.Event.SetTempoEvent
                    {
                        SecondsPerBeat =
                            microSecondsPerQuarterNoteToSecondsPerBeat(setTempoEvent.MicrosecondsPerQuarterNote)
                    };
                    break;
                default:
                    result = null;
                    break;
            }

            return result;
        }
    
        protected float pitchBendToFloat(int pitchBendValue)
        {
            return (pitchBendValue - 8192f) / 8192f;
        }

        protected float microSecondsPerQuarterNoteToSecondsPerBeat(float tempo)
        {
            return tempo / 1000000.0f;
        }

        protected Dictionary<int, List<MidiEvent>> getEventsFromMidiFile(MidiFile midi)
        {
            Dictionary<int, List<MidiEvent>> result = new Dictionary<int, List<MidiEvent>>();

            void addToResult(int tickNumber, MidiEvent midiEvent)
            {
                if (result.TryGetValue(tickNumber, out List<MidiEvent> events))
                {
                    events.Add(midiEvent);
                }
                else
                {
                    result[tickNumber] = new List<MidiEvent> { midiEvent };
                }
            }
        
            foreach (var trackChunk in midi.GetTrackChunks())
            {
                int currentTick = 0;
                foreach (var midiEvent in trackChunk.Events)
                {
                    addToResult(currentTick, midiEvent);
                    currentTick += (int)midiEvent.DeltaTime;
                }
            }

            return result;
        }
    }
}
