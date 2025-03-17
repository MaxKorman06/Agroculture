public class Crop
{
    public int ID { get; set; }
    public string Name { get; set; }

    // Планова врожайність (ц/га) для даної культури
    public double PlannedYield { get; set; }

    // Питомий винос елементів живлення (кг/ц)
    public NutrientUptake NutrientUptake { get; set; }

    // Коефіцієнти використання з ґрунту
    public SoilUtilization SoilUtilization { get; set; }

    // Коефіцієнти використання з мінеральних добрив
    public FertilizerUtilization FertilizerUtilization { get; set; }

    public string RecomendedNextCrop { get; set; }
}

public class NutrientUptake
{
    public double N { get; set; }
    public double P2O5 { get; set; }
    public double K2O { get; set; }
}

public class SoilUtilization
{
    public double N { get; set; }
    public double P2O5 { get; set; }
    public double K2O { get; set; }
}

public class FertilizerUtilization
{
    public double N { get; set; }
    public double P2O5 { get; set; }
    public double K2O { get; set; }
}
