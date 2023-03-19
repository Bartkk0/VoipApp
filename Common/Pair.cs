namespace Common;

public class Pair<TA, TB> {

    public Pair(TA first, TB second) {
        First = first;
        Second = second;
    }

    public TA First { get; set; }
    public TB Second { get; set; }
}