using UnityEngine;
using System;
using Classes;
public class MicrophoneInput : MonoBehaviour
{
    NAudio.Wave.WaveInEvent[] microphoneInputs = new NAudio.Wave.WaveInEvent[GameState.amountPlayer];
    const int spectrumLength = 8192;
    const int sampleRate = 44100;
    const int bufferMilliseconds = 20;
    const int hzThreshold = 25;
    const int bitDepth = 16;
    double[][] samples = new double[GameState.amountPlayer][];
    float maxSample;
    int maxSampleIndex;
    public double hz = 0;
    float[] hzTest;
    public Node[] nodes;

    void Start()
    {
        // init nodes
        nodes = new Node[GameState.amountPlayer];
        // set microphones
        bool inMicrophones;
        bool alredyHasListener;
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
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
                j = 0;
                while (j < i)
                {
                    if (GameState.settings.microphoneInput[i].name == GameState.settings.microphoneInput[j].name)
                    {
                        alredyHasListener = true;
                        samples[i] = samples[j];
                        j = i;
                    }
                    j++;
                }
                if (!alredyHasListener)
                {
                    // create listeener event
                    microphoneInputs[i] = new NAudio.Wave.WaveInEvent
                    {
                        DeviceNumber = GameState.settings.microphoneInput[i].index,
                        WaveFormat = new NAudio.Wave.WaveFormat(sampleRate, bitDepth, 1),
                        BufferMilliseconds = bufferMilliseconds
                    };
                    microphoneInputs[i].DataAvailable += (object sender, NAudio.Wave.WaveInEventArgs e) =>
                    {
                        samples[iCopy] = new double[sampleRate * bufferMilliseconds / 1000];
                        for (int x = 0; x < e.Buffer.Length / 2; x++)
                        {
                            samples[iCopy][x] = BitConverter.ToInt16(e.Buffer, x * 2);
                        }
                    };
                    microphoneInputs[i].StartRecording();
                }
            }
            else
            {
                samples[i] = null;
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            if (samples[i] != null)
            {
                double[] paddedAudio = FftSharp.Pad.ZeroPad(samples[i]);
                double[] fftMag = FftSharp.Transform.FFTpower(paddedAudio);
                // find the frequency peak
                int peakIndex = 0;
                for (int j = 0; j < fftMag.Length; j++)
                {
                    if (fftMag[j] > fftMag[peakIndex])
                        peakIndex = j;
                }
                double frequenzNumber;
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
                    nodes[i] = NodeFunctions.getNode(nodeNumber - 4);
                }
            }
        }
    }
}
