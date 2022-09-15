using FluentAssertions;

namespace Sketch.UnitTests
{
    public class SketchTests
    {
        [Fact]
        public void WhenIncrementSmallFrequencyIsCorrect()
        {
            var omasketch = new OmSketch<int>(10);

            omasketch.Increment(1);
            omasketch.Increment(1);
            omasketch.Increment(1);

            omasketch.EstimateFrequency(1).Should().Be(3);
        }

        [Fact]
        public void WhenIncrementBigFrequencyIsCorrect()
        {
            var omasketch = new OmSketch<int>(10);

            for (int i = 0; i < 20; i++)
            {
                omasketch.Increment(666);
            }

            omasketch.EstimateFrequency(666).Should().Be(20);
        }
    }
}