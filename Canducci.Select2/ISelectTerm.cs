namespace Canducci.Select2
{
    public interface ISelectTerm
    {
        int Page { get; set; }
        string Q { get; set; }
        string Term { get; set; }
        string Type { get; set; }
    }
}