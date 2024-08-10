namespace COG.Utils;

public interface IGetter<T>
{
    public T GetNext();

    public bool HasNext();

    public int Number();

    public void PutBack(T value);
}