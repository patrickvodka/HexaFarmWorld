public class BuyItemCommand : ICommand
{
    private SO_ShopSlot slot;

    public BuyItemCommand(SO_ShopSlot slot)
    {
        this.slot = slot;
    }

    public void Execute()
    {
        ShopSystem.Instance.SelectItem(slot);
    }
}