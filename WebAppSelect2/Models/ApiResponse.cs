namespace WebAppSelect2.Models
{
    public class Response<T>
    {
        public string Query { get; set; }
        public int Total { get; set; }
        public List<T> Results { get; set; }
    }
}
