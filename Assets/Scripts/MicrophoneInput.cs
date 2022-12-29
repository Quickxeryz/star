using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    AudioSource audioSource;
    const int sampleLength = 8192;
    float[] samples = new float[sampleLength];
    float maxSample;
    int maxSampleIndex;
    const float firstHarmonic = (1f / 48000f * ((float)sampleLength)) * 2f;
    float hz = 0;
    string node = "";

    // Start is called before the first frame update
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            string selectedDevice = Microphone.devices[0];
            audioSource.clip = Microphone.Start(selectedDevice, true, 10, AudioSettings.outputSampleRate);
            audioSource.Play();
        }

    }

    void Update()
    {
        if (Microphone.devices.Length > 0)
        {
            //get audio spectrum
            audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
            //get max sampe value
            maxSample = 0;
            maxSampleIndex = 0;
            for (int i = 0; i < sampleLength; i++)
            {
                if (maxSample < samples[i])
                {
                    maxSample = samples[i];
                    maxSampleIndex = i;
                }
            }
            //max value to hz
            hz = (((float)maxSampleIndex) / ((float)firstHarmonic));
            //hz to node
            int nodeNumber = (int)Math.Round(Math.Log(hz / 440, 2) * 12 + 49);
            nodeNumber = nodeNumber % 12;
            if (nodeNumber < 0)
            {
                nodeNumber += 12;
            }
            switch (nodeNumber)
            {
                case 0:
                    node = "G#";
                    break;
                case 1:
                    node = "A";
                    break;
                case 2:
                    node = "A#";
                    break;
                case 3:
                    node = "H";
                    break;
                case 4:
                    node = "C";
                    break;
                case 5:
                    node = "C#";
                    break;
                case 6:
                    node = "D";
                    break;
                case 7:
                    node = "D#";
                    break;
                case 8:
                    node = "E";
                    break;
                case 9:
                    node = "F";
                    break;
                case 10:
                    node = "F#";
                    break;
                case 11:
                    node = "G";
                    break;
                default:
                    node = "Error: not able to get node value from hz!";
                    break;
            }
        }
    }
}
