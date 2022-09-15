using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch
{
    // https://yangtonghome.github.io/uploads/OM-Globecom-Yang.pdf
    // https://github.com/zhouyangpkuer/OMsketch
    // http://burtleburtle.net/bob/hash/evahash.html
    public class OmSketch<T>
    {
        const int LOW_COUNTER_SIZE = 4;
        const int HIGH_COUNTER_SIZE = 16;
        const int MAX_HASH_NUM = 20;

        int w_low, d_low;
        int w_high, d_high;
        int word_num_low, word_num_high;

        int MAX_CNT_LOW, MAX_CNT_HIGH;
        int word_index_size, counter_index_size;

        ulong[] word_low;
        ulong[] word_high;

        int overflow_cnt_query;
        int overflow_cnt_insert;

        public OmSketch(int max)
        {
            max = BitOps.CeilingPowerOfTwo(max);

            // LOW_PROPORTION = .67
            int high = max / 3;
            int low = max - high;

            Init(low, high);
        }

        public OmSketch(int _w_low, int _w_high)
        {
            Init(_w_low, _w_high);
        }

        private void Init(int _w_low, int _w_high)
        {
            overflow_cnt_query = 0;
            overflow_cnt_insert = 0;

            //w_low is the number of counters (16 * #words) in the lower layer.
            //_w_low is the number of words in the lower layer.
            w_low = (_w_low << 4);
            word_num_low = _w_low;

            //w_high is the numeber of counters in the higher layer.
            w_high = (_w_high << 2);
            word_num_high = _w_high;

            d_low = 4;
            d_high = 4;

            //for the low and the high;
            word_index_size = 18;
            counter_index_size = 4;

            word_low = new ulong[word_num_low];
            word_high = new ulong[word_num_high];
            //memset(word_low, 0, sizeof(ulong) * word_num_low);
            //memset(word_high, 0, sizeof(ulong) * word_num_high);

            MAX_CNT_LOW = (1 << LOW_COUNTER_SIZE) - 1;
            MAX_CNT_HIGH = (1 << HIGH_COUNTER_SIZE) - 1;

            //for (int i = 0; i < d_low + d_high; i++)
            //{
            //    bobhash[i] = new BOBHash64(i + 1000);
            //}
        }

        private static ulong Mix64(ulong z)
        {
            z = (z ^ z >> 30) * 0xbf58476d1ce4e5b9L;
            z = (z ^ z >> 27) * 0x94d049bb133111ebL;
            return z ^ z >> 31;
        }

        public unsafe void Increment(T value)
        {
            int* low_offset = stackalloc int[d_low];

            int min_value = 1 << 30;
            ulong temp;

            ulong hash_value = Mix64((ulong)value.GetHashCode());//(bobhash[0]->run(str, strlen(str)));

            int wordIndex0 = ((int)hash_value & 0x3FFFF) % word_num_low;
            hash_value >>= 18;

            // 1. remove data dependency
            //   low_offset[0] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[1] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[2] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[3] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;

            int hash0 = (int)(hash_value);
            int hash1 = (int)(hash_value >> 4);
            int hash2 = (int)(hash_value >> 8);
            int hash3 = (int)(hash_value >> 12);

            low_offset[0] = ((int)hash0 & 0xF) % 14;
            low_offset[1] = ((int)hash1 & 0xF) % 14;
            low_offset[2] = ((int)hash2 & 0xF) % 14;
            low_offset[3] = ((int)hash3 & 0xF) % 14;

            hash_value >>= 16;

            // 2. unroll loop
            //for (int i = 0; i < d_low; i++)
            //{
            //    temp = (word_low[word_index[0]] >> (low_offset[i] << 2)) & 0xF;
            //    min_value = (int)temp < min_value ? (int)temp : min_value;
            //}

            ulong wordLowIndex0 = word_low[wordIndex0];
            ulong temp0 = (wordLowIndex0 >> (low_offset[0] << 2)) & 0xF;
            int min0 = (int)temp0 < min_value ? (int)temp0 : min_value;

            ulong temp1 = (wordLowIndex0 >> (low_offset[1] << 2)) & 0xF;
            int min1 = (int)temp1 < min_value ? (int)temp1 : min_value;

            ulong temp2 = (wordLowIndex0 >> (low_offset[2] << 2)) & 0xF;
            int min2 = (int)temp2 < min_value ? (int)temp2 : min_value;

            ulong temp3 = (wordLowIndex0 >> (low_offset[3] << 2)) & 0xF;
            int min3 = (int)temp2 < min_value ? (int)temp2 : min_value;

            min0 = min0 < min1 ? min0 : min1;
            min2 = min2 < min3 ? min2 : min3;

            min_value = min0 < min2 ? min0 : min2;




            if (min_value != MAX_CNT_LOW)
            {
                // 3. unroll, remove data dependencies
                //      for (int i = 0; i<d_low; i++)
                //{
                // temp = (word_low[word_index[0]] >> (low_offset[i] << 2)) & 0xF;
                // if((int)temp == min_value)
                // {
                //  word_low[word_index[0]] += ((ulong)1 << (low_offset[i] << 2));	
                // }
                //}

                ulong a = 0, b = 0, c = 0, d = 0;

                if ((int)temp0 == min_value)
                {
                    a = ((ulong)1 << (low_offset[0] << 2));
                }

                if ((int)temp1 == min_value)
                {
                    b += ((ulong)1 << (low_offset[1] << 2));
                }

                if ((int)temp2 == min_value)
                {
                    c += ((ulong)1 << (low_offset[2] << 2));
                }

                if ((int)temp3 == min_value)
                {
                    d += ((ulong)1 << (low_offset[3] << 2));
                }

                word_low[wordIndex0] += (a + b) + (c + d);

                return;
            }

            // 4. defer stackalloc array
            int* word_index = stackalloc int[MAX_HASH_NUM];
            int* high_offset = stackalloc int[MAX_HASH_NUM];
            word_index[0] = wordIndex0;


            /*********************carry into the high counters!****************************/

            temp = (ulong)((low_offset[0] ^ low_offset[1]) | ((low_offset[2] ^ low_offset[3]) << 4));
            ulong finger = (word_low[word_index[0]] >> 56) & 0x7F;

            if (finger != 0 && (finger & (temp & 0x7F)) != finger)
                word_low[word_index[0]] |= ((ulong)1 << 63);

            word_low[word_index[0]] |= ((temp & 0x7F) << 56);

            overflow_cnt_insert++;

            for (int i = 0; i < d_low; i++)
                word_low[word_index[0]] &= (~((ulong)0xF << (low_offset[i] << 2)));


            min_value = 1 << 30;

            hash_value = Mix64(hash_value);//(bobhash[d_low]->run(str, strlen(str)));

            word_index[0] = word_index[1] = ((int)hash_value & 0x3FFFF) % word_num_high;
            hash_value >>= 18;

            high_offset[0] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;
            high_offset[1] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;

            word_index[2] = word_index[3] = ((int)hash_value & 0x3FFFF) % word_num_high;
            hash_value >>= 18;

            high_offset[2] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;
            high_offset[3] = ((int)hash_value & 3) % 4;
            //hash_value >>= 2;


            for (int i = 0; i < d_high; i++)
            {
                temp = (word_high[word_index[i]] >> (high_offset[i] << 4)) & 0xFFFF;
                min_value = (int)temp < min_value ? (int)temp : min_value;
            }

            for (int i = 0; i < d_high; i++)
            {
                temp = (word_high[word_index[i]] >> (high_offset[i] << 4)) & 0xFFFF;
                if ((int)temp == min_value)
                {
                    word_high[word_index[i]] += ((ulong)1 << (high_offset[i] << 4));
                }
            }
            return;
        }

        public unsafe int EstimateFrequency(T value)
        {

            int* low_offset = stackalloc int[d_low];
            int wordIndex0 = 0;

            ulong hash_value;

            ulong temp;


            int min_value = 1 << 30;

            hash_value = Mix64((ulong)value.GetHashCode()); //(bobhash[0]->run(str, strlen(str)));

            //word_index[0] = ((int)hash_value & 0x3FFFF) % word_num_low;
            wordIndex0 = ((int)hash_value & 0x3FFFF) % word_num_low;
            hash_value >>= 18;

            int hash0 = (int)(hash_value);
            int hash1 = (int)(hash_value >> 4);
            int hash2 = (int)(hash_value >> 8);
            int hash3 = (int)(hash_value >> 12);

            low_offset[0] = ((int)hash0 & 0xF) % 14;
            low_offset[1] = ((int)hash1 & 0xF) % 14;
            low_offset[2] = ((int)hash2 & 0xF) % 14;
            low_offset[3] = ((int)hash3 & 0xF) % 14;

            hash_value >>= 16;

            //   low_offset[0] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[1] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[2] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;
            //low_offset[3] = ((int)hash_value & 0xF) % 14;
            //hash_value >>= 4;

            //for(int i = 0; i < d_low; i++)
            //{
            // temp = (word_low[wordIndex0] >> (low_offset[i] << 2)) & 0xF;

            // min_value = (int)temp < min_value ? (int)temp : min_value;
            //}

            ulong wordLowIndex0 = word_low[wordIndex0];

            ulong temp0 = (wordLowIndex0 >> (low_offset[0] << 2)) & 0xF;
            int min0 = (int)temp0 < min_value ? (int)temp0 : min_value;

            ulong temp1 = (wordLowIndex0 >> (low_offset[1] << 2)) & 0xF;
            int min1 = (int)temp1 < min_value ? (int)temp1 : min_value;

            ulong temp2 = (wordLowIndex0 >> (low_offset[2] << 2)) & 0xF;
            int min2 = (int)temp2 < min_value ? (int)temp2 : min_value;

            ulong temp3 = (wordLowIndex0 >> (low_offset[3] << 2)) & 0xF;
            int min3 = (int)temp2 < min_value ? (int)temp2 : min_value;

            min0 = min0 < min1 ? min0 : min1;
            min2 = min2 < min3 ? min2 : min3;

            min_value = min0 < min2 ? min0 : min2;


            ulong finger = (wordLowIndex0 >> 56) & 0x7F;
            int flag = (int)wordLowIndex0 >> 63;

            temp = (ulong)((low_offset[0] ^ low_offset[1]) | ((low_offset[2] ^ low_offset[3]) << 4));

            if (flag == 1)
            {
                if ((finger & (temp & 0x7F)) != finger)
                    return min_value;
            }

            else
            {
                if (finger != (temp & 0x7F))
                    return min_value;
            }

            int* word_index = stackalloc int[MAX_HASH_NUM];
            int* high_offset = stackalloc int[MAX_HASH_NUM];
            word_index[0] = wordIndex0;


            overflow_cnt_query++;

            int backup = min_value;
            min_value = 1 << 30;


            hash_value = Mix64(hash_value);//(bobhash[d_low]->run(str, strlen(str)));

            word_index[0] = word_index[1] = ((int)hash_value & 0x3FFFF) % word_num_high;
            hash_value >>= 18;

            high_offset[0] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;
            high_offset[1] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;

            word_index[2] = word_index[3] = ((int)hash_value & 0x3FFFF) % word_num_high;
            hash_value >>= 18;

            high_offset[2] = ((int)hash_value & 3) % 4;
            hash_value >>= 2;
            high_offset[3] = ((int)hash_value & 3) % 4;
            // hash_value >>= 2;

            for (int i = 0; i < d_high; i++)
            {
                temp2 = (word_high[word_index[i]] >> (high_offset[i] << 4)) & 0xFFFF;
                min_value = (int)temp2 < min_value ? (int)temp2 : min_value;
            }

            return min_value * (MAX_CNT_LOW + 1) + backup;
        }
    }
}
