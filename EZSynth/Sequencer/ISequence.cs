using System.Collections.Generic;
using EZSynth.Sequencer.Event;

namespace EZSynth.Sequencer
{
    public interface ISequence
    {
        /// <summary>
        /// The resolution of the sequence is the number of ticks in the sequence per beat played.
        /// </summary>
        int Resolution { get; }
    
        /// <summary>
        /// Get the length of this sequence in seconds.
        /// </summary>
        /// <param name="secondsPerTick">The length of a tick in seconds.</param>
        /// <returns>The length of the entire sequence in seconds.</returns>
        double GetLengthSeconds(double secondsPerTick);

        /// <summary>
        /// For the given tick of the sequence, return an enumerable of the events we should process this
        /// tick.
        /// </summary>
        /// <param name="tickNumber">The tick of the sequence to get events for.</param>
        /// <returns>IEnumerable of events.</returns>
        IEnumerable<BaseSequenceEvent> GetEvents(int tickNumber);
    }
}