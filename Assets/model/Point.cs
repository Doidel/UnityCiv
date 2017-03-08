using System;

//struct holding x and y coordinates
public struct Point
{
    public int X, Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
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

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Point p = (Point)obj;
        return X == p.X && Y == p.Y;
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        return Y ^ X;
    }
}