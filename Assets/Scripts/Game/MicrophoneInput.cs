using UnityEngine;
using System;
using Classes;

public class MicrophoneInput : MonoBehaviour
{
    NAudio.Wave.WaveInEvent[] microphoneInputs = new NAudio.Wave.WaveInEvent[GameState.amountPlayer];
    const int sampleRate = 44100;
    const int bufferMilliseconds = 20;
    const int bitDepth = 16;
    public Node[] nodes;
    readonly double[][][] samples = new double[GameState.amountPlayer][][];

    void Start()
    {
        // init nodes
        nodes = new Node[GameState.amountPlayer];
        // set microphones
        bool inMicrophones;
        bool alredyHasListener;
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            if (!(GameState.settings.microphoneInput[i].isOnline || GameState.profiles[GameState.currentProfileIndex[i]].useOnlineMic))
            {
                samples[i] = new double[2][];
                int iCopy = i;
                // check if microphone exists
                inMicrophones = false;
                int j = 0;
                while (j < NAudio.Wave.WaveInEvent.DeviceCount)
                {
                    if (GameState.settings.microphoneInput[i].name == NAudio.Wave.WaveInEvent.GetCapabilities(j).ProductName)
                    {
                        inMicrophones = true;
                        j = Microphone.devices.Length;
                    }
                    j++;
                }
                if (inMicrophones)
                {
                    // check if microphone alredy has listener
                    alredyHasListener = false;
                    int otherListenerNumber = 0;
                    j = 0;
                    while (j < i)
                    {
                        if(!(GameState.settings.microphoneInput[i].isOnline || GameState.profiles[GameState.currentProfileIndex[j]].useOnlineMic))
                        {
                            if (GameState.settings.microphoneInput[i].name == GameState.settings.microphoneInput[j].name)
                            {
                                alredyHasListener = true;
                                otherListenerNumber = j;
                                samples[i] = samples[j];
                                j = i;
                            }
                        }
                        j++;
                    }
                    if (!alredyHasListener)
                    {
                        // create listeener event
                        microphoneInputs[i] = new NAudio.Wave.WaveInEvent
                        {
                            DeviceNumber = GameState.settings.microphoneInput[i].index,
                            WaveFormat = new NAudio.Wave.WaveFormat(sampleRate, bitDepth, 2),
                            BufferMilliseconds = bufferMilliseconds
                        };
                        microphoneInputs[i].DataAvailable += (object sender, NAudio.Wave.WaveInEventArgs e) =>
                        {
                            samples[iCopy][0] = new double[e.Buffer.Length / 4];
                            samples[iCopy][1] = new double[e.Buffer.Length / 4];
                            for (int x = 0; x < e.Buffer.Length / 2; x++)
                            {
                                if (x % 2 == 0)
                                {
                                    samples[iCopy][0][x / 2] = BitConverter.ToInt16(e.Buffer, x * 2);
                                }
                                else
                                {
                                    samples[iCopy][1][x / 2] = BitConverter.ToInt16(e.Buffer, x * 2);
                                }
                            }
                        };
                        microphoneInputs[i].StartRecording();
                    }
                    else
                    {
                        samples[i][0] = samples[otherListenerNumber][0];
                        samples[i][1] = samples[otherListenerNumber][1];
                    }
                }
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            if (GameState.settings.microphoneInput[i].isOnline || GameState.profiles[GameState.currentProfileIndex[i]].useOnlineMic)
            {
                int index;
                if (GameState.profiles[GameState.currentProfileIndex[i]].useOnlineMic)
                {
                    index = GameState.onlineMicrophones.FindIndex(element => element.id == GameState.profiles[GameState.currentProfileIndex[i]].onlineMicName);
                    if(index == -1)
                    {
                        nodes[i] = Node.None;
                    }
                    else
                    {
                        nodes[i] = GameState.onlineMicrophones[index].node;
                    }
                }
                else
                {
                    index = GameState.onlineMicrophones.FindIndex(element => element.id == GameState.settings.microphoneInput[i].name);
                    nodes[i] = GameState.onlineMicrophones[index].node;
                }
            }
            else
            {
                if (samples[i][GameState.settings.microphoneInput[i].channel] != null)
                {
                    double[] paddedAudio = FftSharp.Pad.ZeroPad(samples[i][GameState.settings.microphoneInput[i].channel]);
                    double[] fftMag = FftSharp.Transform.FFTpower(paddedAudio);
                    // find the frequency peak
                    int peakIndex = 0;
                    for (int j = 0; j < fftMag.Length; j++)
                    {
                        if (fftMag[j] > fftMag[peakIndex])
                            peakIndex = j;
                    }
                    double frequenzNumber;
                    double hz;
                    if (fftMag[peakIndex] > 30)
                    {
                        frequenzNumber = peakIndex;
                        //interpolate if possible
                        double leftNeighbor;
                        double rightNeighbor;
                        if (peakIndex > 0 && peakIndex < fftMag.Length - 1)
                        {
                            leftNeighbor = fftMag[peakIndex - 1] / fftMag[peakIndex];
                            rightNeighbor = fftMag[peakIndex + 1] / fftMag[peakIndex];
                            frequenzNumber += 0.5f * (rightNeighbor * rightNeighbor - leftNeighbor * leftNeighbor);
                        }
                        //calculating hz
                        hz = frequenzNumber * (sampleRate / 2) / fftMag.Length;
                    }
                    else
                    {
                        hz = 0;
                    }
                    //hz to node
                    int nodeNumber;
                    if (hz <= 0)
                    {
                        nodes[i] = Node.None;
                    }
                    else
                    {
                        nodeNumber = (int)Math.Round(Math.Log(hz / 440, 2) * 12 + 49);
                        nodes[i] = NodeFunctions.GetNodeFromInt(nodeNumber - 4);
                    }
                }
            }
        }
    }
}
