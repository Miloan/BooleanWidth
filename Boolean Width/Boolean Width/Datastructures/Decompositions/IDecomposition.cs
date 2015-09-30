namespace BooleanWidth.Datastructures.Decompositions
{
    interface IDecomposition
    {
        Graph Graph { get; }

        double BooleanWidth { get; }

        long MaxNeighborhoodSize { get; }
    }
}
