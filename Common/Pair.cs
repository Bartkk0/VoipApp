namespace Common;

public class Pair<A, B> {
    public Pair() {
    }

    public Pair(A first, B second) {
        First = first;
        Second = second;
    }

    public A First { get; set; }
    public B Second { get; set; }
}