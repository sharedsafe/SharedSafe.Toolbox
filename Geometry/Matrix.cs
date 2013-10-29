#define OPTIMIZE_MULT

using System.Diagnostics;

namespace Toolbox.Geometry
{

	public sealed class Matrix
	{
		public readonly double[] M;

		public Matrix()
		{
			M = new double[16];
		}
		
		public Matrix(double[] matrix)
			: this()
		{
			Debug.Assert(matrix.Length == 16);

			for (int i = 0; i != 16; ++i)
				M[i] = matrix[i];
		}

		public double this[uint column, uint row]
		{
			get
			{
				return M[row * 4 + column];
			}

			set
			{
				M[row * 4 + column] = value;
			}
		}

		public Matrix Transposed
		{
			get
			{
				// simple transpose matrix

				return new Matrix(new[]
				{
					this[0, 0], this[0, 1], this[0, 2], this[0, 3],
					this[1, 0], this[1, 1], this[1, 2], this[1, 3],
					this[2, 0], this[2, 1], this[2, 2], this[2, 3],
					this[3, 0], this[3, 1], this[3, 2], this[3, 3]
				});
			}
		}

		public static Matrix operator *(Matrix l, Matrix r)
		{
#if !OPTIMIZE_MULT
			Matrix m = new Matrix();

			for (int column = 0; column != 4; ++column)
				for (int row = 0; row != 4; ++row)
				{
					m[column, row] =
						l[column, 0] * r[0, row] +
						l[column, 1] * r[1, row] +
						l[column, 2] * r[2, row] +
						l[column, 3] * r[3, row];
				}

			return m;
#else
			var matrix = new Matrix();
			double[] m = matrix.M;
			double[] lm = l.M;
			double[] rm = r.M;

			for (int column = 0; column != 4; ++column)
			{
				double l0 = lm[column];
				double l1 = lm[column + 4];
				double l2 = lm[column + 8];
				double l3 = lm[column + 12];

				m[column] = l0 * rm[0] + l1 * rm[1] + l2 * rm[2] + l3 * rm[3];
				m[column + 4] = l0 * rm[4] + l1 * rm[5] + l2 * rm[6] + l3 * rm[7];
				m[column + 8] = l0 * rm[8] + l1 * rm[9] + l2 * rm[10] + l3 * rm[11];
				m[column + 12] = l0 * rm[12] + l1 * rm[13] + l2 * rm[14] + l3 * rm[15];
			}

			return matrix;
#endif
		}

		/// Translate the matrix.
		public static Matrix operator +(Matrix m, Vector v)
		{
			var nm = new Matrix(m.M);
			nm[0, 3] += v.X;
			nm[1, 3] += v.Y;
			nm[2, 3] += v.Z;

			return nm;
		}

		public static Matrix operator -(Matrix m, Vector v)
		{
			return m + (-v);
		}

		public static readonly Matrix Identity = new Matrix(new[] 
			{
			1.0, 0.0, 0.0, 0.0,
			0.0, 1.0, 0.0, 0.0,
			0.0, 0.0, 1.0, 0.0,
			0.0, 0.0, 0.0, 1.0
			}
			);

		#region Vector * Matrix

		static double mRow(Matrix m, Vector v, uint r)
		{
			return v.X * m[0, r] + v.Y * m[1, r] + v.Z * m[2, r] + /* w == 1 */ m[3,r]; 
		}

		public static Vector operator * (Vector v, Matrix m)
		{
			return new Vector(mRow(m, v, 0), mRow(m, v, 1), mRow(m, v, 2));
		}




		#endregion

	}
}
