using UnityEngine;
using System;
using System.IO;
using System.Text;

public static class WavUtility
{
    // Helper to read 4-byte chunk IDs safely
    private static string ReadChunkID(BinaryReader reader)
    {
        return Encoding.ASCII.GetString(reader.ReadBytes(4));
    }

    // Convert WAV byte array to AudioClip
    public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
    {
        using (MemoryStream stream = new MemoryStream(fileBytes))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            string riff = ReadChunkID(reader);
            if (riff != "RIFF") throw new Exception("Invalid WAV file. Missing RIFF header.");

            int chunkSize = reader.ReadInt32();
            string wave = ReadChunkID(reader);
            if (wave != "WAVE") throw new Exception("Invalid WAV file. Missing WAVE header.");

            string fmt = ReadChunkID(reader);
            if (fmt != "fmt ") throw new Exception("Invalid WAV file. Missing fmt header.");

            int subchunk1Size = reader.ReadInt32();
            ushort audioFormat = reader.ReadUInt16();
            ushort numChannels = reader.ReadUInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            ushort blockAlign = reader.ReadUInt16();
            ushort bitsPerSample = reader.ReadUInt16();

            // Skip any extra fmt bytes
            if (subchunk1Size > 16)
                reader.ReadBytes(subchunk1Size - 16);

            // Find data chunk
            string dataID = ReadChunkID(reader);
            while (dataID != "data")
            {
                int chunkDataSize = reader.ReadInt32();
                reader.ReadBytes(chunkDataSize);
                dataID = ReadChunkID(reader);
            }

            int dataSize = reader.ReadInt32();
            byte[] data = reader.ReadBytes(dataSize);

            float[] samples;

            if (bitsPerSample == 16)
            {
                int sampleCount = data.Length / 2;
                samples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(data, i * 2);
                    samples[i] = sample / 32768f;
                }
            }
            else if (bitsPerSample == 8)
            {
                int sampleCount = data.Length;
                samples = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    samples[i] = (data[i] - 128) / 128f;
                }
            }
            else
            {
                throw new Exception($"Unsupported WAV bit depth: {bitsPerSample}");
            }

            AudioClip clip = AudioClip.Create(name, samples.Length / numChannels, numChannels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }

    // Optional: Save AudioClip as WAV
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int samplesCount = clip.samples * clip.channels;
            float[] samples = new float[samplesCount];
            clip.GetData(samples, 0);

            ushort bitDepth = 16;
            byte[] wavData = ConvertToWav(samples, clip.channels, clip.frequency, bitDepth);

            writer.Write(wavData);
            return stream.ToArray();
        }
    }

    private static byte[] ConvertToWav(float[] samples, int channels, int sampleRate, ushort bitDepth)
    {
        int byteRate = sampleRate * channels * (bitDepth / 8);
        int fileSize = 44 + samples.Length * (bitDepth / 8) - 8;

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            // RIFF header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1); // PCM
            writer.Write((ushort)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((ushort)(channels * (bitDepth / 8)));
            writer.Write(bitDepth);

            // data subchunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(samples.Length * (bitDepth / 8));

            // Write samples
            foreach (float sample in samples)
            {
                if (bitDepth == 16)
                {
                    short val = (short)(sample * 32767);
                    writer.Write(val);
                }
                else if (bitDepth == 8)
                {
                    byte val = (byte)(sample * 127 + 128);
                    writer.Write(val);
                }
            }

            return stream.ToArray();
        }
    }
}
