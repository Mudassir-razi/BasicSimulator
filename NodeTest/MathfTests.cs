using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolset;
using Xunit;

namespace NodeTest
{
    public class MathfTests
    {
        [Fact]
        public void Clamp_ShouldRestricsValue()
        {
            //arange
            double expected = 10;

            //act
            double actual = Mathf.Clamp(12, 1, 10);

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Lerp_ShoulgInterpolate()
        {
            //arange
            double expected = 18.5;

            //act
            double actual = Mathf.Lerp(1, 36, 0.5);

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Scientific_ShouldConverStringToDouble()
        {
            //arange 
            double expected = 1200000;

            //act
            double actual = Mathf.Scientific(" 1.2M");

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GaussianReduction_SouldSolveEQ()
        {
            double[] expected = { 100, 0.002, -1, 1};

            double[,] a = { { 1.5, -0.5, 1 }, { -0.5, 1.5, -1 }, { 1, -1, 0 } };
            double[] z = { 1, 0, 12 };

            double[] actual = Mathf.GaussianReduction(a, z);
            
            Assert.Equal<double>(expected, actual);
        }

        [Fact]
        public void MatrixCombination_SouldCombineMatrix()
        {
            double[,] expexted = { { 1,2,3,10,14 },
                                    { 4,5,6,11,15 },
                                    {7,8,9,12,16 },
                                    {10,11,12,0,0 },
                                    {14,15,16,0,0 } };

            double[,] g = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            double[,] b = { { 10, 14 }, { 11, 15 }, { 12, 16 } };
            double[,] c = { { 10, 11, 12 }, { 14, 15, 16 } };
            double[,] d = { { 0, 0 }, { 0, 0 } };

            double[,] actual = Mathf.ConcatenateMatrix(g, b, c, d);
            Assert.Equal(expexted, actual);
        }
    }
}
