# EZSynth

[![NuGet](https://img.shields.io/nuget/v/EZSynth.svg)](https://www.nuget.org/packages/EZSynth/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

C# synthesizer designed to be extensible and generic in terms of formats of audio and sequence data.

## Installation

Install via [NuGet](https://www.nuget.org/):

```sh
Install-Package EZSynth -Version x.x.x
Install-Package EZSynth.Implementations -Version x.x.x
```

Or via the .NET CLI:

```sh
dotnet add package EZSynth --version x.x.x
dotnet add package EZSynth.Implementations --version x.x.x
```

## Usage

EZSynth was designed with a 'bring your own data' approach, though comes with basic default implementations.

Implement `ISequence` to provide your own sequence data (in a format similar to MIDI events) for the synthesizer.

Implement `ISoundbank` to provide an interface for the synth to get sample/note data based on preset, note, and velocity.

Implement `ISampler` to provide an interface for getting sample data.

Built in to `EZSynth` are samplers `PCMSampler` and `SineSampler` for sampling PCM and sine data. There is
also `SineBank` that is an `ISoundbank` interface providing `SineSampler` instances to play notes.

There is also `EZSynth.Implementations` which contains `MidiSequence` and `SoundfontSoundbank`, to render
MIDI data from MIDI files with samples from SoundFont 2 files. This is in a separate package because it
brings in a couple of extra dependencies (DryWetMidi and EZSF2).

```csharp
// open soundfont with EZSF2 and create a soundbank
using BinaryReader br = new BinaryReader(File.OpenRead("test.sf2"));
SF2 soundFont = new SF2(br);
var soundbank = new SoundfontSoundbank(soundFont);

// create synth with the soundbank
int sampleRate = 44100;
var synth = new Synth(soundbank, sampleRate);

// open midi file with DryWetMidi and create a sequence
using var fs = File.OpenRead("midi.mid");
MidiFile midiFile = MidiFile.Read(fs);
MidiSequence sequence = new MidiSequence(midiFile);

// create sequencer with the sequence
Sequencer sequencer = new Sequencer(sequence, synth);

// render the sequence into PCM data as short[]
var sampleData = sequencer.Render();

// write the data to a WAV file with NAudio
using var writer = new WaveFileWriter("out.wav", new WaveFormat(sampleRate, 2));
writer.WriteSamples(sampleData, 0, sampleData.Length);
```