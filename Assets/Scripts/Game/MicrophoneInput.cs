using UnityEngine;
using System;
using Classes;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    AudioSource audioSource;
    const int spectrumLength = 8192;
    float[] spectrum = new float[spectrumLength];
    float maxSample;
    int maxSampleIndex;
    const float firstHarmonic = (1f / 48000f * ((float)spectrumLength)) * 2f;
    public float hz = 0;
    public Node node;

    // Start is called before the first frame update
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            audioSource = GetComponent<AudioSource>();
            string selectedDevice = Microphone.devices[0];
            audioSource.clip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (Microphone.devices.Length > 0)
        {
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            // get maximum index
            float maxValue = 0;
            int maxValueIndex = 0;
            for (int i = 0; i < spectrumLength; i++)
            {
                if (maxValue < spectrum[i])
                {
                    maxValue = spectrum[i];
                    maxValueIndex = i;
                }
            }
            float frequenzNumber = maxValueIndex;
            float leftNeighbor;
            float rightNeighbor;
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
                node = Node.None;
            }
            else
            {
                int nodeNumber = (int)Math.Round(Math.Log(hz / 440, 2) * 12 + 49);
                node = NodeFunctions.getNode(nodeNumber - 4);
            }
        }
    }
}
