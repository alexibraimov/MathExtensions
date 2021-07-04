using Diff;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace TestDiff
{
    [TestFixture]
    public class MathExtensionsTests
    {
        void TestDerivative(Expression<Func<double, double>> function, double n = 0)
        {
            var f = function.Compile();
            double eps = 1e-8;
            var dfunction = MathExtensions.Derivative(function);

            var df = dfunction.Compile();
            for (double x = n; x < 5; x += 0.08)
            {
                x = Math.Round(x, 5);
                Assert.AreEqual(df(x), (f(x + eps) - f(x)) / eps, 1e-5, $"Error on function {function.Body}");
            }
        }

        [Test]
        public void Constant()
        {
            TestDerivative(z => 1);
        }

        [Test]
        public void Parameter()
        {
            TestDerivative(z => z);
        }

        [Test]
        public void Product1()
        {
            TestDerivative(z => z * 5);
        }

        [Test]
        public void Product2()
        {
            TestDerivative(z => z * z * 5);
        }

        [Test]
        public void Sum1()
        {
            TestDerivative(z => z + z);
        }

        [Test]
        public void Sum2()
        {
            TestDerivative(z => 5 * z + z * z);
        }

        [Test]
        public void Sin1()
        {
            TestDerivative(z => Math.Sin(z));
        }

        [Test]
        public void Sin2()
        {
            TestDerivative(z => Math.Sin(z * z + z));
        }

        [Test]
        public void Cos1()
        {
            TestDerivative(z => Math.Cos(z));
        }

        [Test]
        public void Cos2()
        {
            TestDerivative(z => Math.Cos(z * z + z));
        }

        [Test]
        public void Cos3()
        {
            TestDerivative(z => Math.Cos(z * z + z) + Math.Sin(z + 5) + Math.Sin(1));
        }

        [Test]
        public void Pow()
        {
            TestDerivative(z => Math.Pow(z, 2));
        }

        [Test]
        public void Pow2()
        {
            TestDerivative(z => Math.Pow(2, z));
        }

        [Test]
        public void Exp()
        {
            TestDerivative(z => Math.Exp(Math.Sin(z)));
        }

        [Test]
        public void Log1()
        {
            TestDerivative(z => Math.Log(z, 6));
        }

        [Test]
        public void Log2()
        {
            TestDerivative(z => Math.Log(z * z), 1);
        }

        [Test]
        public void Div1()
        {
            TestDerivative(z => z / z);
        }

        [Test]
        public void Div2()
        {
            TestDerivative(z => z / 1);
        }

        [Test]
        public void Div3()
        {
            TestDerivative(z => Math.Sin(z) / 0);
        }

        [Test]
        public void Asin()
        {
            TestDerivative(z => 5 * Math.Asin(z));
        }

        [Test]
        public void Acos()
        {
            TestDerivative(z => Math.Acos(z));
        }

        [Test]
        public void Atan()
        {
            TestDerivative(z => Math.Atan(z));
        }

        [Test]
        public void Tanh()
        {
            TestDerivative(z => Math.Tanh(z));
        }

        [Test]
        public void Sinh()
        {
            TestDerivative(z => Math.Sinh(z));
        }

        [Test]
        public void Cosh()
        {
            TestDerivative(z => Math.Cosh(z));
        }
    }
}
