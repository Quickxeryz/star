using UnityEngine;
using System;
using Classes;

public class MicrophoneInput : MonoBehaviour
{
    AudioSource[] audioSources = new AudioSource[GameState.amountPlayer];
    const int spectrumLength = 8192;
    float[] spectrum = new float[spectrumLength];
    float maxSample;
    int maxSampleIndex;
    float hz = 0;
    public Node[] nodes;

    void Start()
    {
        // init nodes
        nodes = new Node[GameState.amountPlayer];
        // set microphones
        if (Microphone.devices.Length > 0)
        {
            GameObject audioSorceObject;
            string selectedDevice;
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                if (i < Microphone.devices.Length)
                {
                    selectedDevice = Microphone.devices[i];
                    audioSorceObject = new GameObject("AudioSource" + (i + 1).ToString());
                    audioSources[i] = audioSorceObject.AddComponent<AudioSource>();
                    audioSources[i].clip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
                    audioSources[i].loop = true;
                    audioSources[i].Play();
                }
                else
                {
                    audioSources[i] = audioSources[0];
                }
            }
        }
    }

    void Update()
    {
        if (Microphone.devices.Length > 0)
        {
            float maxValue;
            int maxValueIndex;
            float frequenzNumber;
            float leftNeighbor;
            float rightNeighbor;
            int nodeNumber;
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                audioSources[i].GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                // get maximum index
                maxValue = 0;
                maxValueIndex = 0;
                for (int j = 0; j < spectrumLength; j++)
                {
                    if (maxValue < spectrum[j])
                    {
                        maxValue = spectrum[j];
                        maxValueIndex = j;
                    }
                }
                frequenzNumber = maxValueIndex;
                // interpolate frequenz number
                if (maxValueIndex > 0 && maxValueIndex < spectrumLength - 1)
                {
                    leftNeighbor = spectrum[maxValueIndex - 1] / spectrum[maxValueIndex];
                    rightNeighbor = spectrum[maxValueIndex + 1] / spectrum[maxValueIndex];
                    frequenzNumber += 0.5f * (rightNeighbor * rightNeighbor - leftNeighbor * leftNeighbor);
                }
                hz = frequenzNumber * (AudioSettings.outputSampleRate / 2) / spectrumLength;
                //hz to node
                if (hz == 0)
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
