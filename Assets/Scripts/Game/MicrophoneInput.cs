using UnityEngine;
using System;
using Classes;

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
            hz = (AudioSettings.outputSampleRate / sampleLength) * maxSampleIndex + (AudioSettings.outputSampleRate / sampleLength) / 2;//TEST (((float)maxSampleIndex) / ((float)firstHarmonic));
            //hz to node
            if (maxSample < 0.001) //TEST if (hz == 0)
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
