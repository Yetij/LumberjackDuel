public class l_Ethereal : LogicTree
{
    
    public override bool BeingChopped(LogicJack jack)
    {
        return jack == owner;
    }
}
