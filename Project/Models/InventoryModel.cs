namespace Project.Models
{
    public class InventoryModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int ProductCost { get; set; }
        public int Quantity { get; set; }
        public byte[] ProductImage { get; set; }
    }
}
