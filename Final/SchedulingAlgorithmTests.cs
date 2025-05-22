using System;
using System.Collections.Generic;
using System.Linq;
namespace Final
{
    // מערכת מעקב  עבור האלגוריתם הגנטי
    public static class Tests
    {
        // סטטיסטיקות לשמירה
        private static int generationCounter = 0;
        private static double bestFitness = double.MinValue;
        private static double previousBestFitness = double.MinValue;
        private static readonly List<double> fitnessHistory = new List<double>();
        private static DateTime startTime;

        // אתחול מערכת המעקב
        public static void Initialize()
        {
            // איפוס סטטיסטיקות
            generationCounter = 0;
            bestFitness = double.MinValue;
            previousBestFitness = double.MinValue;
            fitnessHistory.Clear();
            startTime = DateTime.Now;
        }
        // מעקב אחר דור באלגוריתם הגנטי
        public static void TrackGeneration(int currentGeneration, Population population)
        {
            if (population?.Chromoshomes == null || population.Chromoshomes.Count == 0)
                return;
            generationCounter = currentGeneration;
            // חישוב סטטיסטיקות הדור הנוכחי
            double maxFitness = population.Chromoshomes.Max(c => c.Fitness);
            // שמירת הערך הטוב ביותר
            if (maxFitness >= bestFitness)
            {
                previousBestFitness = bestFitness;
                bestFitness = maxFitness;
            }
            // שמירת היסטוריית הציונים הטובים ביותר
            fitnessHistory.Add(maxFitness);
            // חישוב שיפור באחוזים מהדור הקודם
            double improvementPercent = 0;
            if (previousBestFitness > double.MinValue)
            {
                improvementPercent = (maxFitness - previousBestFitness) / Math.Abs(previousBestFitness) * 100;
            }
            // הדפסת נתוני הדור הנוכחי
            Console.WriteLine($"דור {currentGeneration}: " +
                $"ציון הכרומזום הטוב ביותר={maxFitness:F2}, " +
                $"שיפור={improvementPercent:F2}%");
        }

        // מעקב אחר הפתרון הטוב ביותר
        public static void TrackBestSolution(Chromosome bestSolution)
        {
            if (bestSolution == null)
                return;
            Console.WriteLine($"פרטי הפתרון הטוב ביותר:");
            Console.WriteLine($"ציון כושר: {bestSolution.Fitness:F2}");

        }
        // הדפסת סיכום ריצת האלגוריתם
        public static void PrintSummary()
        {
            TimeSpan duration = DateTime.Now - startTime;
            Console.WriteLine($"==== סיכום ריצת האלגוריתם ====");
            Console.WriteLine($"משך זמן: {duration.TotalSeconds:F2} שניות");
            Console.WriteLine($"מספר דורות: {generationCounter}");
            if (fitnessHistory.Count > 0)
            {
                Console.WriteLine($"ציון כושר התחלתי: {fitnessHistory.FirstOrDefault():F2}");
                Console.WriteLine($"ציון כושר סופי: {fitnessHistory.LastOrDefault():F2}");
                double improvementPercent = 0;
                if (fitnessHistory.Count >= 2)
                {
                    improvementPercent = (fitnessHistory.Last() - fitnessHistory.First()) / Math.Abs(fitnessHistory.First()) * 100;
                }
                Console.WriteLine($"שיפור כולל: {improvementPercent:F2}%");
            }
            Console.WriteLine("==== סיום ריצת האלגוריתם ====");
        }
    }
}