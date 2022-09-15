using BenchmarkDotNet.Attributes;
using BitFaster.Caching.Lfu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch.Benchmarks
{
    //|         Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
    //|--------------- |----------:|----------:|----------:|------:|--------:|
    //| EstimateFreqCm |  4.585 us | 0.0249 us | 0.0233 us |  1.00 |    0.00 |
    //| EstimateFreqOm | 10.905 us | 0.1088 us | 0.1018 us |  2.38 |    0.03 |

    //|         Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
    //|--------------- |---------:|----------:|----------:|------:|--------:|
    //| EstimateFreqCm | 5.280 us | 0.0708 us | 0.0662 us |  1.00 |    0.00 |
    //| EstimateFreqOm | 5.732 us | 0.0929 us | 0.0869 us |  1.09 |    0.02 |

    //|         Method |     Mean |     Error |    StdDev | Ratio |
    //|--------------- |---------:|----------:|----------:|------:|
    //| EstimateFreqCm | 5.025 us | 0.0188 us | 0.0176 us |  1.00 |
    //| EstimateFreqOm | 4.750 us | 0.0213 us | 0.0188 us |  0.95 |
    public class FreqBench
    {
        private static CmSketch<int> std = new CmSketch<int>(10, EqualityComparer<int>.Default);
        private static OmSketch<int> om = new OmSketch<int>(10);

        [GlobalSetup]
        public void Setup()
        {
            for (int i = 0; i < 128; i++)
            {
                if (i % 3 == 0)
                {
                    std.Increment(i);
                    om.Increment(i);
                }
            }
        }

        [Benchmark(Baseline = true)]
        public int EstimateFreqCm()
        {
            int count = 0;
            for (int i = 0; i < 128; i++)
            {
                if (std.EstimateFrequency(i) > std.EstimateFrequency(i + 1))
                    count++;
            }

            return count;
        }

        [Benchmark()]
        public int EstimateFreqOm()
        {
            int count = 0;
            for (int i = 0; i < 128; i++)
            {
                if (om.EstimateFrequency(i) > om.EstimateFrequency(i + 1))
                    count++;
            }

            return count;
        }
    }
}
