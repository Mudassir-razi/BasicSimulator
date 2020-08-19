using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Toolset
{
    //For "advanced" mathematical functions
    public static class Mathf
    {

        public static Dictionary<char, double> Suffix = new Dictionary<char, double>()
        {
            {'m', 0.001}, {'M', 1000000}, {'G', 1000000000}, {'u', 0.000001}, {'n', 0.000000001}, {'p', 0.000000000001}
        };

        /// <summary>
        /// Clamps the value in a given range
        /// </summary>
        /// <param name="value"> Value to clamp</param>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <returns></returns>
        public static double Clamp(double value, double min, double max)
        {
            if (value >= max) value = max;
            else if (value <= min) value = min;
            return value;
        }

        /// <summary>
        /// Linearly interpolates between min & max
        /// </summary>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="t">value</param>
        /// <returns></returns>
        public static double Lerp(double min, double max, double t)
        {
            double distance = max - min;
            return (min + t * distance);
        }

        /// <summary>
        /// Returns the sigmoid of the given value
        /// </summary>
        /// <param name="value">the value to be sigmoid</param>
        /// <returns></returns>
        public static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Pow(Math.E, value));
        }

        /// <summary>
        /// Returns the absolute difference between to values
        /// </summary>
        /// <param name="a">first value</param>
        /// <param name="b">second value</param>
        /// <returns></returns>
        public static double Difference(double a, double b)
        {
            return Math.Abs(a - b);
        }

        /// <summary>
        /// Returns a double equivalent of string.Correctly assigns scientific notaions
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns></returns>
        public static double Scientific(string s)
        {
            s.Trim();

            if (!double.TryParse(s, out double val))
            {
                //not a plain number
                //then checks if its like 1.0234m type number
                if (Suffix.ContainsKey(s[s.Length - 1]) && double.TryParse(s.Remove(s.Length - 1, 1), out val))
                {
                    return Suffix[s[s.Length - 1]] * val;
                }
                else throw new InvalidvalueAssertion("Invalid value assigned");
            }
            else return val;
        }


        /// <summary>
        /// Solves a system of equations using Gaussian reduction
        /// Use when the equation matrix is a sparse matrix
        /// </summary>
        /// <param name="Matrix_A">Left hand side 2D matrix</param>
        /// <param name="Matrix_Z">Right hand side collumn matrix</param>
        /// <returns>returns X matirx.Solution of the equation</returns>
        public static double[] GaussianReduction(double[,] Matrix_A, double[] Matrix_Z)
        {
            double[] Matrix_X = new double[Matrix_Z.Length];
            PopulateArray<double>(Matrix_X, 0);
            int ppoint = 0;
            int n = Matrix_Z.Length;

            //making upper triangular matrix
            for (int k = 0; k < n - 1; k++)
            {
                //altering rows if pivot point is zero
                if (Matrix_A[k, ppoint] == 0)
                {
                    double temp;

                    for (int t = 0; t < n; t++)
                    {
                        temp = Matrix_A[k, t];
                        Matrix_A[k, t] = Matrix_A[k + 1, t];
                        Matrix_A[k + 1, t] = temp;
                    }

                    temp = Matrix_Z[k];
                    Matrix_Z[k] = Matrix_Z[k + 1];
                    Matrix_Z[k + 1] = temp;
                }
                for (int row = k + 1; row < n; row++)
                {
                    double factor = (double)Matrix_A[row, ppoint] / Matrix_A[k, ppoint];
                    for (int l = 0; l < n; l++)
                    {
                        Matrix_A[row, l] -= Matrix_A[k, l] * factor;
                    }
                    Matrix_Z[row] -= Matrix_Z[k] * factor;
                }
                ppoint++;
                if (ppoint > n - 2) break;
            }

            //solving equation
            double summation;
            ppoint = n - 1;
            for (int i = n - 1; i >= 0; i--)
            {
                summation = 0;
                for (int j = 0; j < n; j++)
                {
                    if (j == ppoint) continue;
                    summation += Matrix_X[j] * Matrix_A[i, j];
                }
                Matrix_X[ppoint] = (Matrix_Z[i] - summation) / Matrix_A[i, ppoint];

                ppoint--;
            }
            
            return Matrix_X;
        }


        /// <summary>
        /// Populates a new one dimensional array with a given value
        /// </summary>
        /// <typeparam name="T">type of array</typeparam>
        /// <param name="array">array to be populated</param>
        /// <param name="value">value to populate with</param>
        public static void PopulateArray<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Creates a matrix of given type and populates it with a given initial value
        /// </summary>
        /// <typeparam name="T">Type of matrix</typeparam>
        /// <param name="row">Number of rows</param>
        /// <param name="col">Number of collumns</param>
        /// <param name="initValue">Initial value</param>
        /// <returns></returns>
        public static T[,] GenerateMatrix<T>(int row, int col, T initValue )
        {
            T[,] mat = new T[row, col];
            for(int i = 0;i < row;i++)
            {
                for(int j = 0;j < col;j++)
                {
                    mat[i, j] = initValue;
                }
            }
            
            return mat;
        }

        public static T[] GenerateMatrix<T>(int row, T initvalue)
        {
            T[] mat = new T[row];
            for(int i = 0;i < row;i++)
            {
                mat[i] = initvalue;
            }
            return mat;
        }

        /// <summary>
        /// A special function for combining 4 matrix.
        /// g, b, c, d matrices are combined togather to form 'A' matrix needed for MNA algorithm
        /// </summary>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double[,] ConcatenateMatrix(double[,] g, double[,] b, double[,] c, double[,] d)
        {
            int leng = (int)Math.Sqrt(g.Length);
            int lend = (int)Math.Sqrt(d.Length);
            double[,] A = new double[leng+lend, leng+lend];
            //copying g matrix
            for (int i = 0; i < leng;i++)
            {
                for(int j = 0;j < leng;j++)
                {
                    A[i, j] = g[i, j];
                }
            }

            //copying b matrix
            for(int i = 0;i < leng;i++)
            {
                for(int j = 0;j < lend;j++)
                {
                    A[i, leng + j] = b[i, j];
                }
            }

            //copying c matrix
            for(int i = 0;i < lend;i++)
            {
                for(int j = 0;j < leng;j++)
                {
                    A[leng + i, j] = c[i, j];
                }
            }

            //copying d matrix
            for(int i = 0;i < lend;i++)
            {
                for(int j = 0;j < lend;j++)
                {
                    A[leng + i, leng + j] = d[i, j];
                }
            }

            return A;
        }
    }

    public class InvalidvalueAssertion : Exception
    {
        public InvalidvalueAssertion(String message) : base(message) { }

    }

}
