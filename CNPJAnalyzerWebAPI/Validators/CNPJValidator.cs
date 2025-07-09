using System.Text.RegularExpressions;

namespace CNPJAnalyzerWebAPI.Validators
{
    public static class CnpjValidator
    {
        public static bool IsValid(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            string cleanCNPJ = Regex.Replace(cnpj, @"[^\d]", "");
            
            if (cleanCNPJ.Length != 14 || cleanCNPJ.All(c => c == cleanCNPJ[0]))
                return false;

            return ValidateDigits(cleanCNPJ);
        }

        private static bool ValidateDigits(string cnpj)
        {
            int[] weights1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] weights2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int sum1 = CalculateSum(cnpj, weights1, 12);
            int digit1 = CalculateDigit(sum1);

            int sum2 = CalculateSum(cnpj, weights2, 12) + digit1 * weights2[12];
            int digit2 = CalculateDigit(sum2);

            return digit1 == int.Parse(cnpj[12].ToString()) && 
                   digit2 == int.Parse(cnpj[13].ToString());
        }

        private static int CalculateSum(string cnpj, int[] weights, int length)
        {
            int sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += int.Parse(cnpj[i].ToString()) * weights[i];
            }
            return sum;
        }

        private static int CalculateDigit(int sum)
        {
            return sum % 11 < 2 ? 0 : 11 - (sum % 11);
        }
    }
}