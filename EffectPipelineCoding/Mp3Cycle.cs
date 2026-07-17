using EffectPipeline;
using EffectPipeline.GameObjects.GUIElements;
using EffectPipeline.types;
using NAudio.Lame;
using NAudio.Wave;
using System.Diagnostics;
using System.Text;

namespace EffectPipelineCoding
{
    internal sealed class GaplessInfo
    {
        public int EncoderDelay { get; init; }
        public int EndPadding { get; init; }

        //Apparently LAME has an internal delay of 528 frames which appears to remain constant over the presets used.
        const int DECODER_DELAY = 529;

        public static byte[] DecodeMp3(Stream mp3)
        {
            using var reader = new Mp3FileReader(mp3);

            using var pcm = new MemoryStream();

            byte[] buffer = new byte[16384];

            int read;

            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                pcm.Write(buffer, 0, read);

            return pcm.ToArray();
        }

        internal static int FindSubstringStart(Stream stream, byte[] str)
        {
            byte[] single_byte_buffer = [0];
            int found_chars = 0;
            while(found_chars < str.Length)
            {
                if (stream.Read(single_byte_buffer, 0, 1) == 0)
                    return -1;
                if (str[found_chars] == single_byte_buffer[0])
                {
                    found_chars++;
                } else
                {
                    found_chars = 0;
                }
            }
            return (int)stream.Position - str.Length;
        }
        public static GaplessInfo? ReadGaplessInfo(Stream stream)
        {
            long originalPos = stream.Position;

            try
            {
                //Look for lame tag
                var lame_tag = FindSubstringStart(stream, [0x4C, 0x41, 0x4D, 0x45]);
                if (lame_tag == -1)
                    return null;
                //Encoder Delay & Padding is at an offset of 0x15 with a length of 3 bytes, allocating 12 bits per field respectively.
                stream.Position = lame_tag + 0x15;
                byte[] gapless_info = new byte[3];
                stream.ReadExactly(gapless_info, 0, 3);
                int delay = (gapless_info[0] << 4) | (gapless_info[1] >> 4);
                int padding = ((gapless_info[1] & 0b0000_1111) << 8) | gapless_info[2];
                return new() { EncoderDelay = delay + DECODER_DELAY, EndPadding = padding - DECODER_DELAY };
            }
            finally
            {
                stream.Position = originalPos;
            }
        }
        public static byte[] TrimGapless(
            byte[] pcm,
            WaveFormat format,
            GaplessInfo info)
        {
            int bytesPerSample =
                format.BitsPerSample / 8 * format.Channels;

            int trimStart = info.EncoderDelay * bytesPerSample;
            int trimEnd = info.EndPadding * bytesPerSample;

            int length = pcm.Length - trimStart - trimEnd;

            if (length < 0)
                length = 0;

            byte[] result = new byte[length];

            Buffer.BlockCopy(
                pcm,
                trimStart,
                result,
                0,
                length);

            return result;
        }
    }
    public class Mp3Cycle : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Audio Input", typeof(MonoAudio))];

        public IEnumerable<(string, Type)> Outputs => [("Audio Output", typeof(MonoAudio))];

        public Property[] Properties => [new DropdownProperty(["Lowest", "Low", "Standard", "High", "Highest"], "Encoding quality")];


        LAMEPreset QualityToPreset(int quality) => quality switch { 
            //Lower settings seem to produce invalid frames/not enough frames
            0 => LAMEPreset.MEDIUM,
            1 => LAMEPreset.ABR_16,
            2 => LAMEPreset.STANDARD,
            3 => LAMEPreset.EXTREME,
            4 => LAMEPreset.INSANE,
            _ => throw new NotImplementedException()
        };

        byte[] ConvertFloatToPcm16(float[] samples)
        {
            byte[] buffer = new byte[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                float clamped = Math.Clamp(samples[i], -1f, 1f);
          
                short value = (short)(clamped * short.MaxValue);
                buffer[i * 2] = (byte)(value & 0xFF);
                buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
            }

            return buffer;
        }
        float[] ConvertPcm16ToFloat(byte[] pcmBytes)
        {
            int sampleCount = pcmBytes.Length / 2;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                int byteIndex = i * 2;

                short pcm = (short)(
                    pcmBytes[byteIndex] |
                    (pcmBytes[byteIndex + 1] << 8)
                );

                samples[i] = (float)pcm / (float)short.MaxValue;
            }

            return samples;
        }
        public async Task<IInstance[]> applyEffect(IInstance?[] inputs, IPropertyState[] properties)
        {
            var audio = (MonoAudio?)inputs[0];
            if (audio == null) {
                return [new MonoAudio(1, [])];
            }
            var encoding_quality = ((DropdownPropertyState)properties[0]).Selected;
            var stream = new MemoryStream();
            var in_pcm = ConvertFloatToPcm16(audio.samples);
            using (var encoder = new LameMP3FileWriter(stream, new(audio.sampleRate, 1), QualityToPreset(encoding_quality)))
            {
                
                encoder.Write(in_pcm);
                encoder.Flush();
            }
            stream.Flush();
            stream.Position = 0;
            var gapless_info = GaplessInfo.ReadGaplessInfo(stream) ?? throw new Exception("Couldn't decode Gapless info, which should be present");
            using var reader = new Mp3FileReader(stream);
            //The xing header is a frame of silence which is prepended to mp3 files as a "directory" of sorts. It contains metadata about the file. This header however changes the number of samples and thus shifts the decoded image.
            //I think that there's also padding on the end of the stream but we dont care cus we only copy the first bytes.
            var out_pcm_stream = new MemoryStream();
            reader.CopyTo(out_pcm_stream);
            reader.Position = 0;
            var out_pcm = out_pcm_stream.ToArray();
            if (out_pcm.Length - (gapless_info.EncoderDelay * 2) - (gapless_info.EndPadding * 2) != in_pcm.Length)
            {
                Console.WriteLine("Mp3 decoding didnt produce enough samples");
                throw new Exception("Mp3 decoding didnt produce enough samples");
            }
            

            
            return [new MonoAudio(audio.sampleRate, ConvertPcm16ToFloat(out_pcm[(gapless_info.EncoderDelay * 2)..(out_pcm.Length - (gapless_info.EndPadding * 2))]))];

        }
    }

    public class Mp3CycleEffectSearch : IEffectSearch
    {
        public string Title => "Mp3 Cycle";

        public IEnumerable<string> Tags => ["codec", "audio", "coding", "distortion"];

        public IEffect CreateEffect() => new Mp3Cycle();
    }
}
