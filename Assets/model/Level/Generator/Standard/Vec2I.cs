namespace TerrainGenerator
{
    public class Vector2i
    {
        public int X;

        public int Z;

        public Vector2i()
        {
            X = 0;
            Z = 0;
        }

        public Vector2i(int x, int z)
        {
            X = x;
            Z = z;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null)
            {
                return false;
            }

            var other = obj as Vector2i;
            if (other == null)
                return false;

            return this.X == other.X && this.Z == other.Z;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Z.GetHashCode();
        }

        //add this code to class ThreeDPoint as defined previously
        //
        public static bool operator ==(Vector2i a, Vector2i b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.X == b.X && a.Z == b.Z;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a == b);
        }


        public override string ToString()
        {
            return "[" + X + "," + Z + "]";
        }
    }
}