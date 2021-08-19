using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

/// <summary>
/// Reads ELIAS' output and plays it via an AudioSource.
/// </summary>
public class EliasAudioReader
{
	protected EliasHelper elias;
    protected AudioSource audioSource;
    protected GCHandle gcHandle;
    protected float[] dataBuffer;
    protected int currentDataIndex;
    protected int bufferedLength;
    protected uint eliasFramesPerBufferExternal;


    public volatile AudioSpeakerMode unityChannelMode;

	public EliasAudioReader(EliasHelper eliasHelper, uint eliasFramesPerBufferOut, AudioSource audioSourceTarget, bool useProceduralClip)
	{
		elias = eliasHelper;
        eliasFramesPerBufferExternal = eliasFramesPerBufferOut;
        audioSource = audioSourceTarget;
		dataBuffer = new float[eliasFramesPerBufferExternal * elias.ChannelCount];
        //This following part is normally only used when useHighLatencyMode in EliasPlayer is set to true.
        if (useProceduralClip)
        {
            AudioClip clip = AudioClip.Create("elias clip", elias.FramesPerBuffer * elias.ChannelCount, elias.ChannelCount, elias.SampleRate, true, PCMReadCallback);
            audioSource.clip = clip;
        }
        // By not having any audio clip, and making sure ELIAS is the first effect on the Audio Source, ELIAS is treated as the "source".
		audioSource.loop = true;
		gcHandle = GCHandle.Alloc(dataBuffer, GCHandleType.Pinned);
	}

	/// <summary>
	/// Stops the AudioSource and disposes references. DOES NOT stop ELIAS!
	/// </summary>
	public void Dispose()
	{
		audioSource.Stop();
		audioSource = null;
		elias = null;
		gcHandle.Free ();
    }

    protected void PCMReadCallback(float[] data)
    {
        ReadCallback(data, elias.ChannelCount);
    }


    public bool ReadCallback(float[] data, int unityChannelCount)
	{
		currentDataIndex = 0;
		while (currentDataIndex < data.Length)
		{
			if (bufferedLength > 0 && elias.ChannelCount == unityChannelCount)
			{
				int length = Math.Min(data.Length - currentDataIndex, bufferedLength);
				Array.Copy(dataBuffer, 0, data, currentDataIndex, length);
				currentDataIndex += length;
				bufferedLength -= length;
				if (bufferedLength > 0)
				{
					Array.Copy(dataBuffer, length, dataBuffer, 0, bufferedLength);
				}
			}
            else if (bufferedLength > 0)
            {
                copyConvertedData(data, unityChannelCount);
            }
            else
			{
				EliasWrapper.elias_result_codes r = EliasWrapper.elias_read_samples(elias.Handle, Marshal.UnsafeAddrOfPinnedArrayElement(dataBuffer, 0));
				bufferedLength = (int)eliasFramesPerBufferExternal * elias.ChannelCount;
				EliasHelper.LogResult(r, "Failed to render");
                if (r != EliasWrapper.elias_result_codes.elias_result_success)
                {
                    return false;
                }
			}
		}
        return true;
	}

    protected void copyConvertedData(float[] data, int unityChannelCount)
    {
        int lengthInFrames = Math.Min((data.Length - currentDataIndex) / unityChannelCount, bufferedLength / elias.ChannelCount);
        if (elias.ChannelCount == 1 && unityChannelMode != AudioSpeakerMode.Mono)
        {
            upmixMono(data, lengthInFrames, unityChannelCount);
        }
        else if (elias.ChannelCount == 2 && unityChannelMode == AudioSpeakerMode.Mono)
        {
            convertStereoToMono(data, lengthInFrames, unityChannelCount);
        }
        else if (elias.ChannelCount == 2 && elias.ChannelCount != unityChannelCount)
        {
            addSilentChannels(data, lengthInFrames, unityChannelCount);
        }
        else if (elias.ChannelCount > 2 && elias.ChannelCount != unityChannelCount)
        {
            Debug.LogWarning("To downmix more then stereo, please set the Elias Players Channel count to match Unitys.");
        }


        if (bufferedLength > 0)
        {
            Array.Copy(dataBuffer, lengthInFrames * elias.ChannelCount, dataBuffer, 0, Math.Min(dataBuffer.Length, bufferedLength));
        }
    }

    protected void addSilentChannels(float[] data, int lengthInFrames, int unityChannelCount)
    {
        for (int i = 0; i < lengthInFrames; ++i)
        {
            for (int h = 0; h < elias.ChannelCount; h++)
            {
                data[currentDataIndex + h + i * unityChannelCount] = dataBuffer[h + i * elias.ChannelCount];
            }
        }

        currentDataIndex += lengthInFrames * unityChannelCount;
        bufferedLength -= lengthInFrames * elias.ChannelCount;
    }

    protected void upmixMono(float[] data, int lengthInFrames, int unityChannelCount)
    {
        for (int i = 0; i < lengthInFrames; ++i)
        {
            float mono = dataBuffer[i];
            data[currentDataIndex + 0 + i * unityChannelCount] = mono; /* L */
            data[currentDataIndex + 1 + i * unityChannelCount] = mono; /* R */
        }

        currentDataIndex += lengthInFrames * unityChannelCount;
        bufferedLength -= lengthInFrames * elias.ChannelCount;
    }

    protected void convertStereoToMono(float[] data, int lengthInFrames, int unityChannelCount)
    {
        for (int i = 0; i < lengthInFrames; ++i)
        {
            float lf = dataBuffer[0 + i * 2];
            float rf = dataBuffer[1 + i * 2];
            float ce = (lf + rf) * 0.5f;
            data[currentDataIndex + i] = ce; /* MONO */
        }

        currentDataIndex += lengthInFrames * unityChannelCount;
        bufferedLength -= lengthInFrames * elias.ChannelCount;
    }

    //Basics of copy + pastable code if further down/up mixing code is required.
#if false
    float lf = dataBuffer[0 + i * elias.ChannelCount];
    float rf = dataBuffer[1 + i * elias.ChannelCount];
    float ce = dataBuffer[2 + i * elias.ChannelCount];
    float lfe = dataBuffer[3 + i * elias.ChannelCount];
    float lb = dataBuffer[4 + i * elias.ChannelCount];
    float rb = dataBuffer[5 + i * elias.ChannelCount];
    float ls = dataBuffer[6 + i * elias.ChannelCount];
    float rs = dataBuffer[7 + i * elias.ChannelCount];
    data[currentDataIndex + 0 + i * unityChannelCount] = lf; /* FL */
    data[currentDataIndex + 1 + i * unityChannelCount] = rf; /* FR */
    data[currentDataIndex + 2 + i * unityChannelCount] = ce;  /* FC */
    data[currentDataIndex + 3 + i * unityChannelCount] = 0;   /* LFE (only meant for special LFE effects) */
    data[currentDataIndex + 4 + i * unityChannelCount] = lb;  /* BL */
    data[currentDataIndex + 5 + i * unityChannelCount] = rb;  /* BR */
    data[currentDataIndex + 6 + i * unityChannelCount] = ls; ;  /* SL */
    data[currentDataIndex + 7 + i * unityChannelCount] = rs; ;  /* SR */
#endif
}