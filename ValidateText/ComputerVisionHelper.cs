using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.IO;
using System.Globalization;

namespace ValidateText
{
    public enum MatchType
    {
        FullMatch,
        NoMatch,
        PartialMatch
    }

    public static class ComputerVisionHelper
    {

        private static bool FindAmount(double valueToFind, OcrResults ocrResult)
        {
            foreach (var eachRegion in ocrResult.Regions)
            {
                foreach (var eachLine in eachRegion.Lines)
                {
                    foreach (var eachWord in eachLine.Words)
                    {
                        double dblValue;
                        if (double.TryParse(eachWord.Text.Trim(), NumberStyles.Number | NumberStyles.AllowCurrencySymbol, new CultureInfo("en-US"), out dblValue))
                        {
                            if (Math.Floor(dblValue) == Math.Floor(valueToFind))
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

        public static async Task<MatchType> VerifyText(double valueToCompare, string filePath, string subscriptionKey, bool valueBeside = true)
        {
            bool result = false;
            MatchType mResult = MatchType.NoMatch;

            OcrResults ocrResult = await GetOCRResult(filePath, subscriptionKey);

            var wordsFound = ExtractAmount(ocrResult, filePath, valueBeside);
            foreach (var eachFind in wordsFound)
            {
                double retValue;
                if (Double.TryParse(eachFind.Item2.Text.Trim(), NumberStyles.Number | NumberStyles.AllowCurrencySymbol, new CultureInfo("en-US"), out retValue))
                {
                    //double diff = retValue - valueToCompare;
                    //if (diff < 0.5 && diff > -0.5)
                    if (Math.Floor(retValue) == Math.Floor(valueToCompare))
                    {
                        result = true;
                        mResult = MatchType.FullMatch;
                        break;
                    }
                }
            }
            if (mResult == MatchType.NoMatch)
            {
                if (FindAmount(valueToCompare, ocrResult))
                {
                    mResult = MatchType.PartialMatch;
                }
            }

            

            //return result;
            return mResult;
        }

        private static async Task<OcrResults> GetOCRResult(string filePath, string subscriptionKey)
        {
            VisionServiceClient VisionServiceClient = new VisionServiceClient(subscriptionKey);
            OcrResults ocrResult = null;
            using (Stream imageFileStream = File.OpenRead(filePath))
            {

                ocrResult = await VisionServiceClient.RecognizeTextAsync(imageFileStream);
            }

            return ocrResult;
        }

        private static bool FindWord(Line currentLine, string stringToFind)
        {
            if (currentLine.Words != null)
            {

                var found = currentLine.Words.Any(wo => wo.Text.Trim().ToUpper().Contains(stringToFind.ToUpper()));

                //string.Equals(stringToFind, wo.Text, StringComparison.CurrentCultureIgnoreCase));

                return found;
            }
            return false;
        }


        private static List<Tuple<Word, Word>> ExtractAmount(OcrResults ocrResult, string filePath, bool valueBeside = true)
        {
            List<string> lstStrings = new List<string>();
            List<Word> foundWords = GetTotalWords(ocrResult);

            if (foundWords == null || foundWords.Count == 0)
            {
                var foundWords1 = GetTotalWords(ocrResult, "tota");

                var foundWords2 = GetTotalWords(ocrResult, "otal");

                var foundWords3 = GetTotalWords(ocrResult, "lot");

                foundWords.AddRange(foundWords1);
                foundWords.AddRange(foundWords2);
                foundWords.AddRange(foundWords3);
            }

            List<Tuple<Word, Word>> lstValues = new List<Tuple<Word, Word>>();

            foreach (var eachFoundWord in foundWords)
            {
                foreach (var eachRegion in ocrResult.Regions)
                {

                    foreach (var eachLine in eachRegion.Lines)
                    {
                        List<Word> foundAdjWords;
                        if (valueBeside)
                        {
                            foundAdjWords = eachLine.Words.ToList().FindAll(wo =>
                            ((wo.Rectangle.Top - eachFoundWord.Rectangle.Top <= 10) && (wo.Rectangle.Top - eachFoundWord.Rectangle.Top >= -10))
                            );
                        }
                        else
                        {
                            foundAdjWords = eachLine.Words.ToList().FindAll(wo =>
                           ((wo.Rectangle.Left - eachFoundWord.Rectangle.Left <= 10) && (wo.Rectangle.Left - eachFoundWord.Rectangle.Left >= -10))
                           );
                        }

                        bool foundValue = false;
                        foreach (var eachFoundAdjWord in foundAdjWords)
                        {
                            lstStrings.Add(eachFoundAdjWord.Text);
                            double result;
                            string[] strArray = { "?", "!", "*", "~", "'" };
                            string valueToParse = eachFoundAdjWord.Text.Trim();
                            if (strArray.Any(valueToParse.Contains))
                            {
                                foreach (var eachChar in strArray)
                                {
                                    valueToParse = valueToParse.Replace(eachChar, string.Empty);
                                }
                                eachFoundAdjWord.Text = valueToParse;
                            }
                            if (Double.TryParse(valueToParse, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, new CultureInfo("en-US"), out result))
                            {
                                foundValue = true;
                                lstValues.Add(new Tuple<Word, Word>(eachFoundWord, eachFoundAdjWord));
                            }

                        }
                        //if (foundAdjWords == null || foundAdjWords.Count == 0 || !foundValue)
                        //{
                        //    lstValues.Add(new Tuple<Word, Word>(eachFoundWord, null));
                        //}
                    }
                }
            }

            return lstValues;
        }

        private static List<Word> GetTotalWords(OcrResults ocrResult, string backupText = null)
        {
            const string STR_Total = "total";
            string stringToFind;
            if (backupText != null)
            {
                stringToFind = backupText;
            }
            else
            {
                stringToFind = STR_Total;
            }



            List<Word> foundWords = new List<Word>();

            foreach (var eachRegion in ocrResult.Regions)
            {
                List<Line> lstLines = eachRegion.Lines.ToList();

                List<Line> foundLines = lstLines.FindAll(li => FindWord(li, stringToFind));

                foreach (var eachFoundLine in foundLines)
                {
                    //var words = eachFoundLine.Words.ToList().FindAll(wo => string.Equals("total", wo.Text, StringComparison.CurrentCultureIgnoreCase));
                    var words = eachFoundLine.Words.ToList().FindAll(wo => wo.Text.Trim().ToUpper().Contains(stringToFind.ToUpper()));

                    foundWords.AddRange(words);
                }
            }

            return foundWords;
        }
    }
}
