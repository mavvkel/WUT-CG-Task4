using CG_Task3;
using System.Windows;
using System.Windows.Input;

namespace CG_Task3_Tests
{
    public class Tests
    {
        DDALine line1;

        [SetUp]
        public void Setup()
        {
            line1 = new(new Point(1f, 1f), new Point(3f, 7f));
        }

        [Test]
        public void DDALineLinePixelCalculationTest()
        {

            Assert.That(line1.LinePixels, Is.Not.Null);

            StylusPointCollection expectedCollection = new()
            {
                new StylusPoint(1f, 1f),
                new StylusPoint(2f, 4f),
                new StylusPoint(3f, 7f)
            };

            Assert.That(line1.LinePixels, Is.EqualTo(expectedCollection));
        }

        [Test]
        public void DDALineStartPointChange()
        {
            line1.StartPoint = new Point(-1f, -5f);

            StylusPointCollection expectedCollection = new()
            {
                new StylusPoint(-1f, -5f),
                new StylusPoint(0f, -2f),
                new StylusPoint(1f, 1f),
                new StylusPoint(2f, 4f),
                new StylusPoint(3f, 7f)
            };

            Assert.That(line1.LinePixels, Is.EqualTo(expectedCollection));
        }

        [Test]
        public void DDALineEndPointChange()
        {
            line1.StartPoint = new Point(-1f, -5f);
            line1.EndPoint = new Point(1f, -2f);

            StylusPointCollection expectedCollection = new()
            {
                new StylusPoint(-1f, -5f),
                new StylusPoint(0f, -4f),
                new StylusPoint(1f, -2f)
            };

            Assert.That(line1.LinePixels, Is.EqualTo(expectedCollection));
        }
    }

}