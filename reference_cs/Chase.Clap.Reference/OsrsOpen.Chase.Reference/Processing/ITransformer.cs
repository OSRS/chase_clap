namespace OsrsOpen.Chase.Reference.Processing
{
    public interface ITransformer<in F, out T>
    {
        T XForm(F item);
    }
}
