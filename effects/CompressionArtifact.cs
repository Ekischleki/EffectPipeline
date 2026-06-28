using EffectPipeline.gameObjects.GUI_Elements;
using EffectPipeline.types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EffectPipeline.effects
{
    internal class CompressionArtifact : IEffect
    {
        public IEnumerable<(string, Type)> Inputs => [("Image", Type.GreyscaleImage)];

        public IEnumerable<(string, Type)> Outputs => [("Image", Type.GreyscaleImage)];

        public Property[] Properties => [new DropdownProperty(["Preserve Pixel Position", "Shift Pixels"], ""), new FloatInputProperty("Glitch Probability") { Min = 0, Max = 1, Value = 0.05f }, new NumberInputProperty("Seed") { Min = 0, Max = int.MaxValue }];


        private (List<byte>, List<int>) EncodeLzw(IEnumerable<byte> in_stream)
        {
            List<int> lengths = [];
            var dict = new Dictionary<string, int>();
            for (int i = 0; i <= 0xFF; i++)
            {
                dict.Add(i.ToString(), i);
            }
            var bit_list = new BitList();
            string p = "";
            var l = 0;
            foreach (var b in in_stream)
            {
                string new_p;
                if (p == "")
                {
                    new_p = b.ToString();
                }
                else
                {
                    new_p = $"{p};{b}";
                }
                if (dict.ContainsKey(new_p))
                {
                    l++;
                    p = new_p;
                }
                else
                {
                    var bits_per_idx = int.Log2(dict.Count) + 1;
                    bit_list.Add(dict[p], bits_per_idx);
                    lengths.Add(l);
                    dict.Add(new_p, dict.Count);
                    p = b.ToString();
                    l = 1;
                }
            }
            if(p != "")
            {
                lengths.Add(l);
                bit_list.Add(dict[p], int.Log2(dict.Count) + 1);
            }

            return (bit_list.packed, lengths);
        }

        public IEnumerable<byte> DecodeLzw(List<byte> input, List<int>? lengths = null)
        {
            var length_i = 1;
            BitReader reader = new(input);
            List<byte[]> dict = [];
            for (int i = 0; i <= 0xFF; i++)
            {
                dict.Add([(byte)i]);
            }

            var last = reader.ReadBytes(int.Log2(dict.Count) + 1);
            
            while (last > 255)
            {
                last -= 255;
            }
            yield return (byte)last;
            var count = dict.Count + 1;
            while(true)
            {
                var bits_per_idx = int.Log2(count) + 1;
                int next;
                int? length;
                try
                {
                    next = reader.ReadBytes(bits_per_idx);
                    length = lengths?[length_i];
                    length_i += 1;
                } catch(Exception) 
                {
                    yield break;
                }
                if (next < dict.Count)
                {
                    dict.Add(dict[last].Append(dict[next][0]).ToArray());  
                } else
                {
                    next = dict.Count;
                    dict.Add(dict[last].Append(dict[last][0]).ToArray());
                }
                count++;
                var add = dict[next].ToList();
                if (length != null)
                {
                    while(add.Count > length)
                    {
                        add.RemoveAt(add.Count - 1);
                    }
                    while (add.Count < length)
                    {
                        add.Add(add[add.Count - 1]);
                    }
                }
                foreach (var b in add)
                {
                    yield return b;
                }
                last = next;

            }
        }
        public IInstance[] applyEffect(IInstance?[] inputs, Property[] properties)
        {
            var preserve_length = ((DropdownProperty)properties[0]).Selected == 0;
            var glitch_prob = ((FloatInputProperty)properties[1]).Value;
            var seed = ((NumberInputProperty)properties[2]).Value;
            var rand = new Random(seed);
            var image = (GreyscaleImage?)inputs[0] ?? throw new ArgumentNullException();
            var (encoded, lengths) = EncodeLzw(image.image.Select(v => (byte)float.Round(float.Clamp(v, 0, 1) * byte.MaxValue)));
            if (!preserve_length)
            {
                lengths = null;
            }
            for (var i = 0; i < encoded.Count; i++)
            {
                if (rand.NextDouble() > glitch_prob)
                {
                    continue;
                }
                encoded[i] ^= (byte)(1 << rand.Next(7));
            }
            var decoded = DecodeLzw(encoded, lengths).ToList();
            while (decoded.Count < image.image.Length)
            {
                decoded.Add(0);
            }
            while(decoded.Count > image.image.Length)
            {
                decoded.RemoveAt(decoded.Count - 1);
            }
            return [
                new GreyscaleImage(image.width, image.height, decoded.Select(x => ((float)x) / byte.MaxValue).ToArray())
                ];
        }


        private class BitReader(List<byte> b)
        {
            public List<byte> stream = b;
            private int sub_index = 0;
            private int index = 0;
            public bool Read()
            {
                var val = ((stream[index] >> (7 - sub_index)) & 1) == 1;
                sub_index++;
                if(sub_index >= 8)
                {
                    sub_index = 0;
                    index++;
                }
                return val;
            }

            public int ReadBytes(int amount)
            {
                int res = 0;

                for(int i = 0; i < amount; i++)
                {
                    res = res << 1;
                    if (Read())
                    {
                        res |= 1;
                    }
                }
                return res;
            }
        }

        private class BitList
        {
            public List<byte> packed = [0];
            private int sub_index = 0;
            public void Add(int v, int bits)
            {
                for(int i = 0; i < bits; i++)
                {
                    Add(((v >> (bits - i - 1)) & 1) == 1);
                }
            }
            public void Add(bool b)
            {
                if(b)
                {
                    packed[^1] = (byte)(packed[^1] | (0x80 >> sub_index));
                }
                sub_index++;
                if(sub_index >= 8)
                {
                    sub_index = 0;
                    packed.Add(0);
                }
            }
        }
    }

    internal class CompressionArtifactSearch : IEffectSearch
    {
        public string Title => "LZW Glitches";

        public IEnumerable<string> Tags => ["compression", "glitch"];

        public IEffect CreateEffect() => new CompressionArtifact();
    }
}
