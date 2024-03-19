using System.Text;

namespace APBD3;

public abstract class Container
{
    protected double Mass { get; set; } //kg
    private double TareMass { get; set; }
    protected double MaxPayload { get; private set; }
    private int Height { get; set; } //cm
    private int Depth { get; set; }
    public string SerialNum { get; private set; }

    private static readonly List<int> SerialNumbers = new List<int>();


    protected class OverfillException : Exception
    {
        public OverfillException() 
        {
            Console.WriteLine("Container is overfilled.");
        }
    }

    protected Container(double mass, double tareMass, double maxPayload, int height, int depth)
    {
        Mass = mass;
        TareMass = tareMass;
        MaxPayload = maxPayload;
        Height = height;
        Depth = depth;
        SerialNum = GenerateSerialNumber(); 
    }
    
    private string GenerateSerialNumber() 
    {
       Random rand = new Random();
       int randomNumber = rand.Next();
       if (!SerialNumbers.Contains(randomNumber))
       {
           SerialNumbers.Add(randomNumber);
           return "KON-" + GetType().Name + "-" + randomNumber;
       }
       return GenerateSerialNumber();
    }
    
    public virtual void Empty()
    {
        this.Mass = 0;
    }

    public virtual void Load(double m)
    {
        if (Mass + m < MaxPayload)
            Mass += m;
        else
            throw new OverfillException();
    }

    public double GetWeight()
    {
        return Mass + TareMass;
    }
    
    public override string ToString()
    {
        return $"Container information: mass: {Mass} kg, tare mass: {TareMass} kg, max capacity: {MaxPayload} kg, " +
               $"height: {Height} cm, depth: {Depth} cm, serial number: {SerialNum}";
    }
}

public interface IHazardNotifier
{
    void SendNotification();
}

public class LiquidContainer(
    double mass,
    double tareMass,
    double maxPayload,
    int height,
    int depth,
    LiquidContainer.CargoType type)
    : Container(mass, tareMass, maxPayload, height, depth), IHazardNotifier
{
    public enum CargoType
    {
        Ordinary,
        Hazardous
    }

    private CargoType Type { get; set; } = type;

    public void SendNotification()
    {
        Console.WriteLine($"Occured a dangerous situation for container {SerialNum}");
    }

    public override void Load(double m)
    {
        double maxCapacity = MaxPayload * (Type == CargoType.Hazardous ? 0.5 : 0.9);

        if (m <= maxCapacity)
        {
            base.Load(m);
        }
        else SendNotification();
    }

    public override string ToString()
    {
        return base.ToString() + $", \ncargo type: {Type}";
    }
}

public class GasContainer(double mass, double tareMass, double maxPayload, int height, int depth, double pressure)
    : Container(mass, tareMass, maxPayload, height, depth), IHazardNotifier
{
    private double _pressure = pressure;

    public override void Empty()
    {
        Mass*= 0.05;
    }
    public void SendNotification() 
    {
        Console.WriteLine($"Mass of cargo exceeds maximum capacity for container {SerialNum}");
    }
    
    public override void Load(double m)
    {
        if (Mass + m < MaxPayload)
            Mass += m;
        else
            SendNotification();
    }
    
    public override string ToString()
    {
        return base.ToString() + $", \npressure: {_pressure}";
    }
    
}

public class ProductType(string name, double requiredTemp)
{
    public string Name { get; } = name;
    public double RequiredTemp { get; } = requiredTemp;
}

public class RefrigeratedContainer : Container
{
    private ProductType Type { get; }
    private double Temp { get; set; }
    
    public RefrigeratedContainer(double mass, double tareMass, double maxPayload, int height, int depth, ProductType productType) : base(mass, tareMass, maxPayload, height, depth)
    {
        Type = productType;
        Temp = Type.RequiredTemp;
    }

    public void SetTemperature(double temperature)
    {
        if (temperature > Type.RequiredTemp)
        {
            Console.WriteLine($"Error: Set temperature cannot be higher than the required temperature for {Type.Name}");
        }
        else
        {
            Temp = temperature;
            Console.WriteLine($"Temperature set to {temperature}°C");
        }
    }
    
    public override string ToString()
    {
        return base.ToString() + $", \nproduct type: {Type.Name}, current temperature: {Temp}";
    }
}

public class ContainerShip(int maxSpeed, int maxContainers, double maxWeight)
{
    private readonly List<Container> _containers = new List<Container>();
    private int MaxSpeed { get; } = maxSpeed; //knots
    private int MaxContainers { get; } = maxContainers; 
    private double MaxWeight { get; } = maxWeight; //tons

    public void LoadContainer(Container container)
    {
        if (_containers.Count < MaxContainers && GetTotalWeight() + container.GetWeight() <= MaxWeight*1000)
        {
            _containers.Add(container);
            Console.WriteLine($"Container loaded: {container.GetType().Name}");
        }
        else
        {
            Console.WriteLine("Ship cannot add this container: overload.");
        }
    }
    
    public void LoadContainers(List<Container> containers)
    {
        foreach (var container in containers)
        {
            LoadContainer(container);
        }
    }

    public void RemoveContainer(Container container)
    {
        _containers.Remove(container);
        Console.WriteLine($"Container unloaded: {container.SerialNum}");
    }
    
    public void ReplaceContainer(int index, Container newContainer)
    {
        if (index >= 0 && index < _containers.Count)
        {
            _containers[index] = newContainer;
            Console.WriteLine($"Container replaced at index {index}: with {newContainer.SerialNum}");
        }
        else
        {
            Console.WriteLine($"Invalid index: {index}");
        }
    }
    
    public void TransferContainer(Container container, ContainerShip destinationShip)
    {
        if (_containers.Contains(container))
        {
            destinationShip.LoadContainer(container);
            _containers.Remove(container);
            Console.WriteLine("Container transferred to target ship.");
        }
    }
    
    private double GetTotalWeight() //in kg
    {
        double totalWeight = 0;
        foreach (var container in _containers) 
        {
            totalWeight += container.GetWeight();
        }
        return totalWeight;
    }
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Container Ship Information:");
        sb.AppendLine($"Maximum speed: {MaxSpeed} knots, maximum containers: {MaxContainers}, " +
                      $"maximum weight capacity: {MaxWeight} kg");
        sb.AppendLine("Containers on board:");
        foreach (var container in _containers)
        {
            sb.AppendLine($"- {container}");
        }

        return sb.ToString();
    }
    
}

public class Apbd3
{
    static void Main(string[] args) { 
        ContainerShip ship = new ContainerShip(25, 20, 30);
        
        LiquidContainer container1 = new LiquidContainer(20, 100, 200, 200, 
            100, LiquidContainer.CargoType.Hazardous);
        GasContainer container2 = new GasContainer(10, 10, 30, 10, 50, 2);
        ProductType pt1 = new ProductType("bananas", 13.3);
        
        RefrigeratedContainer container3 = new RefrigeratedContainer(10, 10, 20, 30, 30, pt1);
        container3.SetTemperature(20);
        Console.WriteLine(container3);
        
        ship.LoadContainer(container1);

        List<Container> containers =
        [
            container2,
            container3

        ];

        ship.LoadContainers(containers);
        Console.WriteLine(ship);
        
        container2.Empty();
        Console.WriteLine(container2.GetWeight());
        container2.Load(20);
        
        ship.RemoveContainer(container1);
        
        ship.ReplaceContainer(1, container1);

        ContainerShip ship2 = new ContainerShip(20, 30, 100);
        
        ship.TransferContainer(container1, ship2);
        
        Console.WriteLine(ship2);

    }
}
