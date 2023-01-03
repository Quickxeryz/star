using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public enum Node
    {
        C = 11,
        CH = 10,
        D = 9,
        DH = 8,
        E = 7,
        F = 6,
        FH = 5,
        G = 4,
        GH = 3,
        A = 2,
        AH = 1,
        B = 0
    }

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
                    node = Node.GH;
                    break;
                case 1:
                    node = Node.A;
                    break;
                case 2:
                    node = Node.AH;
                    break;
                case 3:
                    node = Node.B;
                    break;
                case 4:
                    node = Node.C;
                    break;
                case 5:
                    node = Node.CH;
                    break;
                case 6:
                    node = Node.D;
                    break;
                case 7:
                    node = Node.DH;
                    break;
                case 8:
                    node = Node.E;
                    break;
                case 9:
                    node = Node.F;
                    break;
                case 10:
                    node = Node.FH;
                    break;
                case 11:
                    node = Node.G;
                    break;
                default:
                    node = Node.C;
                    break;
            }
        }
    }
}
