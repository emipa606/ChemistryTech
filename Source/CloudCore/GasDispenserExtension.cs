using Verse;

namespace CloudCore;

public class GasDispenserExtension : DefModExtension
{
    public static readonly GasDispenserExtension defaultValues = new GasDispenserExtension();

    public string useGas = "Helium_Gas";
}