public class IDGenerator
{
    private List<int> UsedIDS = new List<int>();
    private Random rnd = new Random();
    public int Min {get;private set;}
    public int Max {get;private set;}

    public IDGenerator(int _Min, int _Max){
        Min = _Min;
        Max = _Max;
        rnd = new Random();
        UsedIDS.Clear();
    }

    public void DeleteID(int ID){
        UsedIDS.Remove(ID);
    }
    /// <summary>
    /// Generates an unique ID based on the interval.
    /// </summary>
    /// <returns>Returns -1 if error</returns>
    public int GenerateID()
    {
        int ID = rnd.Next(Min, Max);
        var start = DateTime.Now;
        while (!(ID > Min && ID < Max) && UsedIDS.Contains(ID))
        {
            ID = rnd.Next(Min, Max);
            if ((DateTime.Now - start).Milliseconds > 100)
            {//Too many attempts. Return due to error.
                return -1;
            }
        }
        return ID;
    }

    public bool Contains(int ID){
        return UsedIDS.Contains(ID);
    }

}