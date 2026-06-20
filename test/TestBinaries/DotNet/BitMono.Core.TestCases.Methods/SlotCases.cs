namespace BitMono.Core.TestCases.Methods;

// Fixtures for MethodSlotGrouper. Real compiled IL so MethodImpl rows and newslot/reuseslot flags
// are exactly what the C# compiler emits.

public interface ISlotShape
{
    void SlotDraw();
}

public class SlotCircle : ISlotShape
{
    public void SlotDraw() { } // implicit implementation
}

public class SlotSquare : ISlotShape
{
    void ISlotShape.SlotDraw() { } // explicit implementation (.override row)
}

public abstract class SlotAnimal
{
    public abstract void SlotSpeak();
}

public class SlotDog : SlotAnimal
{
    public override void SlotSpeak() { } // override chain
}

public class SlotWidget
{
    public override string ToString() => "widget"; // overrides external object.ToString
}

public interface ISlotBox<T>
{
    T SlotGet();
}

public class SlotIntBox : ISlotBox<int>
{
    public int SlotGet() => 0; // generic implicit implementation
}
