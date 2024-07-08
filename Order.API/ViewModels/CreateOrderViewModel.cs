namespace Order.API.ViewModels
{
    public class CreateOrderViewModel
    {
        public string BuyerId { get; set; }
        public List<CreateOrderItemVM> OrderItems { get; set; }
    }
}
