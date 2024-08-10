namespace COG.Utils;

public interface IGetter<out T>
{
    public T GetNext();

    public bool HasNext();

    public int Number();
}