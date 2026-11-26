namespace Canducci.Select2
{
    public class SelectItem<T>
    {
        public SelectItem(T id, string text, bool selected = false, bool disabled = false)
        {
            Id = id;
            Text = text;
            Selected = selected;
            Disabled = disabled;
        }
        public T Id { get; private set; }
        public string Text { get; private set; }
        public bool Selected { get; private set; }
        public bool Disabled { get; private set; }
    }
}
