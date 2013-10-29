namespace Toolbox.Geometry
{
	public struct Location
	{
		public readonly Orientation Orientation;
		public readonly Translation Translation;

		public static readonly Location Identity =
			new Location(Orientation.Identity, Vector.Zero);

		public Location(Orientation orientation, Translation translation)
		{
			Orientation = orientation;
			Translation = translation;
		}

		#region Regular Object Matrices

		// Note application is right to left!, so first orient then translate

		public Matrix Matrix
		{
			get
			{
				return Translation.Matrix * Orientation.Matrix;
			}
		}

		public Matrix InverseMatrix
		{
			get
			{
				return Orientation.InverseMatrix * Translation.InverseMatrix;
			}
		}

		#endregion

		#region Camera Matrices

		/// When all model transformations are finished, the world is translated using a negative translation
		/// Application is right to left, first inverse translate the world, then inverse orientate the world

		public Matrix CameraMatrix
		{
			get
			{
				return Orientation.InverseMatrix * Translation.InverseMatrix;
			}
		}

		public Matrix InverseCameraMatrix
		{
			get
			{
				return Translation.Matrix * Orientation.Matrix;
			}
		}

		#endregion
	}
}
