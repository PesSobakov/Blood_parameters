using Blood_parameters.Models.Database;
using Castle.Components.DictionaryAdapter;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Blood_parameters.Models;

public class ClusteringKMean
{
    public class ClusterizationResult
    {
        public List<int> clusters;
        public double sumOfSquares;
        public List<double>? elbow;
        public List<List<double>> normalizedInput;
    }

    public class ElbowResult
    {
        public int bestK;
        public List<double> elbow;
    }

    static public double GetSilhouette(List<List<double>> input, List<int> clusterIndexes)
    {
        if (clusterIndexes.Max() == 0)
        {
            return 0;
        }
        int attributesCount = input[0].Count;

        List<double> min = new List<double>() { };
        List<double> max = new List<double>() { };
        for (int i = 0; i < attributesCount; i++)
        {
            min.Add(input.Min(x => x[i]));
            max.Add(input.Max(x => x[i]));
        }

        List<List<double>> normalized = new List<List<double>>();

        foreach (var item in input)
        {
            List<double> temp = new List<double>();
            for (int i = 0; i < attributesCount; i++)
            {
                temp.Add((item[i] - min[i]) / (max[i] - min[i]));
            }
            normalized.Add(temp);
        }

        List<List<double>> distances = new List<List<double>>();

        for (int i = 0; i < normalized.Count; i++)
        {
            List<double> temp = new List<double>();
            for (int j = 0; j < normalized.Count; j++)
            {
                List<double> differenceInSquare = new List<double>();
                for (int k = 0; k < normalized[j].Count; k++)
                {
                    differenceInSquare.Add(Math.Pow(normalized[i][k] - normalized[j][k], 2.0));
                }
                temp.Add(Math.Sqrt(differenceInSquare.Sum()));
            }
            distances.Add(temp);
        }

        List<List<int>> clusters = new List<List<int>>();
        for (int i = 0; i < clusterIndexes.Max() + 1; i++)
        {
            clusters.Add(new List<int>());
        }
        for (int i = 0; i < clusterIndexes.Count; i++)
        {
            clusters[clusterIndexes[i]].Add(i);
        }

        List<double> ai = new List<double>();
        List<double> bi = new List<double>();
        List<double> si = new List<double>();

        for (int i = 0; i < distances.Count; i++)
        {
            int clusterSize = clusters[clusterIndexes[i]].Count;
            if (clusterSize == 1)
            {
                ai.Add(0);
            }
            else
            {
                double sum = 0;
                for (int j = 0; j < distances[i].Count; j++)
                {
                    if (clusterIndexes[i] == clusterIndexes[j])
                    {
                        sum += distances[i][j];
                    }
                }
                ai.Add(sum / (clusterSize - 1));
            }
        }

        for (int i = 0; i < distances.Count; i++)
        {
            int clusterSize = clusters[clusterIndexes[i]].Count;
            if (clusterSize == 1)
            {
                bi.Add(0);
            }
            else
            {
                List<double> distancesToClusters = new List<double>();
                for (int j = 0; j < clusters.Count; j++)
                {
                    if (clusterIndexes[i] == j || clusters[j].Count == 0)
                    {
                        distancesToClusters.Add(double.PositiveInfinity);
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < distances[i].Count; k++)
                        {
                            if (clusterIndexes[j] == clusterIndexes[k])
                            {
                                sum += distances[i][k];
                            }
                        }

                        distancesToClusters.Add(sum / clusters[j].Count);
                    }
                }
                bi.Add(distancesToClusters.Min());
            }
        }

        for (int i = 0; i < distances.Count; i++)
        {
            int clusterSize = clusters[clusterIndexes[i]].Count;
            if (clusterSize == 1 || (ai[i] == 0 && bi[i] == 0))
            {
                si.Add(0);
            }
            else
            {
                si.Add((bi[i] - ai[i]) / Math.Max(bi[i], ai[i]));
            }
        }

        double silhouette = si.Average();
        return silhouette;
    }

    static public int GetBestNumberOfClustersBySilhouette(List<List<double>> input)
    {
        List<double> Silhouettes = new List<double>();

        for (int i = 1; i < input.Count + 1; i++)
        {
            Silhouettes.Add(GetSilhouette(input, Clusterize(input, i).clusters));
        }

        double max = double.NegativeInfinity;
        int index = -1;
        for (int i = 0; i < Silhouettes.Count; i++)
        {
            if (Silhouettes[i] > max)
            {
                max = Silhouettes[i];
                index = i;
            }
        }

        return index + 1;
    }

    static public ElbowResult GetBestNumberOfClustersByElbow(List<List<double>> input)
    {
        if (input.Count < 3)
        {
            return new ElbowResult() { bestK = input.Count, elbow = new List<double>(new double[input.Count]) };
        }
        int maxK = input.Count < 10 ? input.Count : input.Count < 30 ? 10 : input.Count / 3;
        List<double> sumOfSquares = new List<double>();

        for (int i = 1; i < maxK + 1; i++)
        {
            sumOfSquares.Add(Clusterize(input, i).sumOfSquares);
        }

        List<double> D = new List<double>();
        for (int i = 1; i < sumOfSquares.Count - 1; i++)
        {
            if (sumOfSquares[i + 1] == 0 || sumOfSquares[i] == 0 || sumOfSquares[i - 1] == 0)
            {
                D.Add(double.MaxValue);
            }
            else
            {
                D.Add((sumOfSquares[i + 1] - sumOfSquares[i]) / (sumOfSquares[i] - sumOfSquares[i - 1]));
            }
        }

        double min = double.PositiveInfinity;
        int index = -1;
        for (int i = 0; i < D.Count; i++)
        {
            if (D[i] < min)
            {
                min = D[i];
                index = i;
            }
        }

        return new ElbowResult() { bestK = index + 2, elbow = sumOfSquares };
    }


    static public ClusterizationResult Clusterize1(List<List<double>> input, int numberOfClusters)
    {
        if (numberOfClusters < 1)
        {
            throw new ArgumentOutOfRangeException();
        }
        int attributesCount = input[0].Count;

        List<double> min = new List<double>() { };
        List<double> max = new List<double>() { };
        for (int i = 0; i < attributesCount; i++)
        {
            min.Add(input.Min(x => x[i]));
            max.Add(input.Max(x => x[i]));
        }

        List<List<double>> normalized = new List<List<double>>();

        foreach (var item in input)
        {
            List<double> temp = new List<double>();
            for (int i = 0; i < attributesCount; i++)
            {
                temp.Add((item[i] - min[i]) / (max[i] - min[i]));
            }
            normalized.Add(temp);
        }

        List<List<double>> centroids = new List<List<double>>();
        for (int i = 0; i < numberOfClusters; i++)
        {
            centroids.Add(normalized[i * (normalized.Count - 1) / numberOfClusters]);
        }

        List<List<double>> distances;
        List<int> clusterIndexes;
        List<int> lastClusterIndexes = new List<int>();

        while (true)
        {
            distances = new List<List<double>>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < normalized.Count; j++)
                {
                    List<double> differenceInSquare = new List<double>();
                    for (int k = 0; k < normalized[j].Count; k++)
                    {
                        differenceInSquare.Add(Math.Pow(centroids[i][k] - normalized[j][k], 2.0));
                    }
                    temp.Add(Math.Sqrt(differenceInSquare.Sum()));
                }
                distances.Add(temp);
            }

            clusterIndexes = new List<int>();

            for (int i = 0; i < distances[0].Count; i++)
            {
                double minDistance = double.PositiveInfinity;
                int index = 0;
                for (int j = 0; j < distances.Count; j++)
                {
                    if (distances[j][i] < minDistance)
                    {
                        minDistance = distances[j][i];
                        index = j;
                    }
                }
                clusterIndexes.Add(index);
            }

            if (lastClusterIndexes.SequenceEqual(clusterIndexes))
            {
                break;
            }
            else
            {
                lastClusterIndexes = clusterIndexes;
            }

            centroids = new List<List<double>>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                List<double> averages = new List<double>();
                for (int j = 0; j < normalized[0].Count; j++)
                {
                    double avg = 0;
                    for (int k = 0; k < normalized.Count; k++)
                    {
                        if (clusterIndexes[k] == i)
                        {
                            avg += normalized[k][j];
                        }
                    }
                    avg /= clusterIndexes.Where(x => x == i).Count();
                    averages.Add(avg);
                }
                centroids.Add(averages);

            }
        }
        return new ClusterizationResult() { clusters = clusterIndexes };
    }

    static public ClusterizationResult Clusterize(List<List<double>> input)
    {
        if (input.Count == 0)
        {
            return new ClusterizationResult() { clusters = new List<int>(), sumOfSquares = 0, elbow = new List<double>(), normalizedInput = new List<List<double>>() };
        }
        ElbowResult result = GetBestNumberOfClustersByElbow(input);
        ClusterizationResult clusterization = Clusterize(input, result.bestK);
        clusterization.elbow = result.elbow;
        return clusterization;
    }

    static public ClusterizationResult Clusterize(List<List<double>> input, int numberOfClusters, int repeat = 10)
    {
        if (numberOfClusters < 1)
        {
            throw new ArgumentOutOfRangeException();
        }
        if (input.Count == 0)
        {
            return new ClusterizationResult() { clusters = new List<int>(), sumOfSquares = 0, elbow = new List<double>() };
        }
        List<ClusterizationResult> results = new List<ClusterizationResult>(repeat);
        for (int i = 0; i < repeat; i++)
        {
            results.Add(ClusterizeOnce(input, numberOfClusters));
        }
        double min = results.Min(x => x.sumOfSquares);
        return results.First(x => x.sumOfSquares == min);
    }

    static public ClusterizationResult ClusterizeOnce(List<List<double>> input, int numberOfClusters)
    {
        if (numberOfClusters < 1)
        {
            throw new ArgumentOutOfRangeException();
        }
        if (input.Count == 0)
        {
            return new ClusterizationResult() { clusters = new List<int>() };
        }
        int attributesCount = input[0].Count;

        List<double> min = new List<double>() { };
        List<double> max = new List<double>() { };
        for (int i = 0; i < attributesCount; i++)
        {
            min.Add(input.Min(x => x[i]));
            max.Add(input.Max(x => x[i]));
        }

        List<List<double>> normalized = new List<List<double>>();

        foreach (var item in input)
        {
            List<double> temp = new List<double>();
            for (int i = 0; i < attributesCount; i++)
            {
                temp.Add((item[i] - min[i]) / (max[i] - min[i]));
            }
            normalized.Add(temp);
        }

        //centroids almost k-means++

        List<List<double>> centroids = new List<List<double>>();
        centroids.Add(normalized[Random.Shared.Next(0, normalized.Count)]);
        List<double> distancesToNearestCentroid;
        for (int i = 1; i < numberOfClusters; i++)
        {
            distancesToNearestCentroid = new List<double>(input.Count);
            for (int j = 0; j < normalized.Count; j++)
            {
                List<double> temp = new List<double>();
                for (int k = 0; k < centroids.Count; k++)
                {
                    List<double> differenceInSquare = new List<double>();
                    for (int l = 0; l < normalized[j].Count; l++)
                    {
                        differenceInSquare.Add(Math.Pow(centroids[k][l] - normalized[j][l], 2.0));
                    }
                    temp.Add(Math.Sqrt(differenceInSquare.Sum()));
                }
                distancesToNearestCentroid.Add(temp.Min());
            }
            var (_, index) = distancesToNearestCentroid.Select((n, i) => (n, i)).Max();
            centroids.Add(normalized[index]);
        }

        //

        List<List<double>> distances;
        List<int> clusterIndexes;
        List<int> lastClusterIndexes = new List<int>();

        while (true)
        {
            distances = new List<List<double>>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < normalized.Count; j++)
                {
                    List<double> differenceInSquare = new List<double>();
                    for (int k = 0; k < normalized[j].Count; k++)
                    {
                        differenceInSquare.Add(Math.Pow(centroids[i][k] - normalized[j][k], 2.0));
                    }
                    temp.Add(Math.Sqrt(differenceInSquare.Sum()));
                }
                distances.Add(temp);
            }

            clusterIndexes = new List<int>();

            for (int i = 0; i < distances[0].Count; i++)
            {
                double minDistance = double.PositiveInfinity;
                int index = 0;
                for (int j = 0; j < distances.Count; j++)
                {
                    if (distances[j][i] < minDistance)
                    {
                        minDistance = distances[j][i];
                        index = j;
                    }
                }
                clusterIndexes.Add(index);
            }

            if (lastClusterIndexes.SequenceEqual(clusterIndexes))
            {
                break;
            }
            else
            {
                lastClusterIndexes = clusterIndexes;
            }

            centroids = new List<List<double>>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                List<double> averages = new List<double>();
                for (int j = 0; j < normalized[0].Count; j++)
                {
                    double avg = 0;
                    for (int k = 0; k < normalized.Count; k++)
                    {
                        if (clusterIndexes[k] == i)
                        {
                            avg += normalized[k][j];
                        }
                    }
                    avg /= clusterIndexes.Where(x => x == i).Count();
                    if (double.IsNaN(avg))
                    {
                        avg = 0;
                    }
                    averages.Add(avg);
                }
                centroids.Add(averages);
            }
        }

        distancesToNearestCentroid = new List<double>(input.Count);
        for (int j = 0; j < normalized.Count; j++)
        {
            List<double> temp = new List<double>();
            for (int k = 0; k < centroids.Count; k++)
            {
                List<double> differenceInSquare = new List<double>();
                for (int l = 0; l < normalized[j].Count; l++)
                {
                    differenceInSquare.Add(Math.Pow(centroids[k][l] - normalized[j][l], 2.0));
                }
                temp.Add(differenceInSquare.Sum());
            }
            distancesToNearestCentroid.Add(temp.Min());
        }

        /*///

        distancesToNearestCentroid = new List<double>(input.Count);
        for (int j = 0; j < normalized.Count; j++)
        {
            List<double> temp = new List<double>();
            for (int k = 0; k < centroids.Count; k++)
            {
                List<double> differenceInSquare = new List<double>();
                for (int l = 0; l < normalized[j].Count; l++)
                {
                    differenceInSquare.Add(Math.Pow(centroids[k][l] - normalized[j][l], 2.0));
                }
                temp.Add(Math.Sqrt(differenceInSquare.Sum()));
            }
            distancesToNearestCentroid.Add(temp.Min());
        }

        List<double>  percentDistancesToNearestCentroid = new List<double>(input.Count);
        for (int j = 0; j < normalized.Count; j++)
        {
            List<double> temp = new List<double>();
            for (int k = 0; k < centroids.Count; k++)
            {
                List<double> differenceInSquare = new List<double>();
                for (int l = 0; l < normalized[j].Count; l++)
                {
                    differenceInSquare.Add(Math.Pow(centroids[k][l] - normalized[j][l], 2.0));
                }
                temp.Add(Math.Sqrt(differenceInSquare.Sum()));
            }
            percentDistancesToNearestCentroid.Add(temp.Min());
        }


        double meanAbsoluteError = distancesToNearestCentroid.Average();
        double meanAbsolutePercentageError;
        double meanSquareError = distancesToNearestCentroid.Average(x => Math.Pow(x, 2));



        ///*/

        return new ClusterizationResult() { clusters = clusterIndexes, sumOfSquares = distancesToNearestCentroid.Sum(), normalizedInput = normalized };
    }


}

