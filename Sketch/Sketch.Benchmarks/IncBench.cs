using BenchmarkDotNet.Attributes;
using BitFaster.Caching.Lfu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch.Benchmarks
{
    //| Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
    //|------- |---------:|----------:|----------:|------:|--------:|
    //|  CmInc | 2.943 us | 0.0362 us | 0.0321 us |  1.00 |    0.00 |
    //|  OmInc | 7.396 us | 0.1468 us | 0.2151 us |  2.54 |    0.10 |

    // With stackalloc arrays:
    //| Method |     Mean |     Error |    StdDev | Ratio |
    //|------- |---------:|----------:|----------:|------:|
    //|  CmInc | 2.806 us | 0.0085 us | 0.0079 us |  1.00 |
    //|  OmInc | 5.514 us | 0.0242 us | 0.0214 us |  1.97 |

    // With 1. data dep removal + 2. loop unroll
    //| Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
    //|------- |---------:|----------:|----------:|------:|--------:|
    //|  CmInc | 2.830 us | 0.0540 us | 0.0600 us |  1.00 |    0.00 |
    //|  OmInc | 5.337 us | 0.0480 us | 0.0449 us |  1.88 |    0.04 |

    // With 3 unrolled
    //| Method |     Mean |     Error |    StdDev | Ratio |
    //|------- |---------:|----------:|----------:|------:|
    //|  CmInc | 2.901 us | 0.0177 us | 0.0165 us |  1.00 |
    //|  OmInc | 4.450 us | 0.0089 us | 0.0079 us |  1.53 |

    // defer stackalloc array
    //| Method |     Mean |     Error |    StdDev | Ratio |
    //|------- |---------:|----------:|----------:|------:|
    //|  CmInc | 2.711 us | 0.0098 us | 0.0087 us |  1.00 |
    //|  OmInc | 3.898 us | 0.0214 us | 0.0190 us |  1.44 |
    public class IncBench
    {
        private static CmSketch<int> std = new CmSketch<int>(10, EqualityComparer<int>.Default);
        private static OmSketch<int> om = new OmSketch<int>(10);

        [Benchmark(Baseline = true)]
        public void CmInc()
        {
            for (int i = 0; i < 128; i++)
                std.Increment(i);
        }

        [Benchmark()]
        public void OmInc()
        {
            for (int i = 0; i < 128; i++)
                om.Increment(i);
        }
    }
}
